using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Wodsoft.NugetProxy.Models;
using System.Net;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Wodsoft.NugetProxy
{
    public static class NugetProxyMiddleware
    {
        private static string _Source, _Replace, _Path;

        private static async Task PageHandler(HttpContext httpContext)
        {
            string action = httpContext.GetRouteValue("action") as string;
            string path = httpContext.Request.Path.Value + httpContext.Request.QueryString.Value;
            var host = httpContext.Request.Host;
            var schema = httpContext.Request.Scheme;
            string apiPath = path.Substring(_Path.Length);

            await SynchronizationHelp.Enter(path);
            Page page;
            DataContext dataContext;
            try
            {
                //使用httpContext.RequestServices.GetRequiredService获取DataContext会导致后台下载线程保存Page时报错
                //因为DataContext已经被释放
                dataContext = new DataContext(new DbContextOptionsBuilder<DataContext>().UseSqlServer(_ConnectionString).Options);
                page = dataContext.Page.SingleOrDefault(t => t.Path == path);
            }
            catch (Exception ex)
            {
                SynchronizationHelp.Exit(path);
                httpContext.Response.StatusCode = 500;
                await httpContext.Response.WriteAsync(ex.Message);
                ExceptionLog(ex, httpContext);
                return;
            }
            if (page == null)
            {
                page = new Page();
                page.Path = path;
                page.Id = Guid.NewGuid();
                dataContext.Page.Add(page);
            }
            else
            {
                SynchronizationHelp.Exit(path);
            }
            byte[] pageContent = page.Content;
            if (page.ExpiredDate < DateTime.Now)
            {
                string key = path;
                bool download = true;
                if (pageContent != null)
                {
                    key = "Download/" + path;
                    download = await SynchronizationHelp.TryEnter(key);
                }
                if (download)
                {
                    NugetDownloader downloader = new NugetDownloader(_Source + apiPath);
                    Task downloadTask = downloader.Download().ContinueWith(async t =>
                    {
                        if (downloader.StatusCode == HttpStatusCode.OK)
                        {
                            var reader = new StreamReader(downloader.Stream);
                            var content = await reader.ReadToEndAsync();
                            string replaceTo = schema + "://" + host + _Path;
                            content = content.Replace(_Replace, replaceTo);
                            page.Content = Encoding.UTF8.GetBytes(content);
                            page.ExpiredDate = DateTime.Now.AddMinutes(GetCacheTime(action));
                            page.ContentType = downloader.ContentType;
                            await dataContext.SaveChangesAsync();
                        }
                        else
                        {
                            page.Content = new byte[0];
                            page.ExpiredDate = DateTime.Now.AddSeconds(10);
                        }
                        if (pageContent != null)
                            SynchronizationHelp.Exit(key);
                    }).Unwrap();
                    if (pageContent == null)
                    {
                        try
                        {
                            await downloadTask;
                        }
                        catch (Exception ex)
                        {
                            httpContext.Response.StatusCode = 500;
                            await httpContext.Response.WriteAsync(ex.Message);
                            ExceptionLog(ex, httpContext);
                            return;
                        }
                        finally
                        {
                            SynchronizationHelp.Exit(key);
                        }
                        pageContent = page.Content;
                    }

                    if (page.Content == null || page.Content.Length == 0)
                    {
                        httpContext.Response.StatusCode = 400;
                        return;
                    }
                }
            }
            httpContext.Response.ContentType = page.ContentType;
            //httpContext.Response.Cache.SetExpires(page.ExpiredDate);
            if (pageContent.Length > 0)
            {
                await httpContext.Response.Body.WriteAsync(pageContent, 0, pageContent.Length);
                await httpContext.Response.Body.FlushAsync();
            }
        }

        private static async Task PackageHandler(HttpContext httpContext)
        {

            string id = httpContext.GetRouteValue("id") as string;
            string version = httpContext.GetRouteValue("version") as string;
            string filename = id + "." + version + ".nupkg";

            httpContext.Response.ContentType = "binary/octet-stream";
            httpContext.Response.Headers.Add("Content-Disposition", "attachment; filename=\"" + filename + "\"");

            filename = "Packages/" + filename;
            await SynchronizationHelp.Enter(filename);
            Stream stream = null;
            if (!File.Exists(filename))
            {
                try
                {
                    NugetDownloader downloader = new NugetDownloader(_Source + "/package/" + id + "/" + version);
                    downloader.Progress += async (sender, buffer, length) =>
                    {
                        if (stream == null)
                            stream = File.Open(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                        await stream.WriteAsync(buffer, 0, length);
                        await httpContext.Response.Body.WriteAsync(buffer, 0, length);
                    };
                    await downloader.Download();
                    if (downloader.StatusCode != HttpStatusCode.OK)
                    {
                        if (stream != null)
                        {
                            stream.Dispose();
                            File.Delete(filename);
                        }
                        httpContext.Response.StatusCode = (int)downloader.StatusCode;
                        return;
                    }
                    else
                    {
                        stream.Dispose();
                    }
                }
                finally
                {
                    SynchronizationHelp.Exit(filename);
                }
            }
            else
            {
                SynchronizationHelp.Exit(filename);
                //context.Request.Headers["Range"]
                stream = File.Open(filename, FileMode.Open, FileAccess.Read, FileShare.Read);
                try
                {
                    await stream.CopyToAsync(httpContext.Response.Body);
                }
                finally
                {
                    stream.Dispose();
                }
            }
        }

        private static int _CacheMetadata, _CacheList, _CacheDetail, _CacheDefault;
        private static int GetCacheTime(string action)
        {
            switch (action)
            {
                case "":
                case "/":
                case "$metadata":
                    return _CacheMetadata;
                case "search()":
                    return _CacheList;
                case "findpackagesbyid()":
                    return _CacheDetail;
                default:
                    return _CacheDefault;
            }
        }

        private static void ExceptionLog(Exception ex, HttpContext context)
        {
            ILoggerFactory loggerFactory = context.RequestServices.GetService<ILoggerFactory>();
            var log = loggerFactory.CreateLogger("Page handler");
            log.LogError(new EventId(), ex, "Nuget handle error");
        }

        private static string _ConnectionString;
        public static IApplicationBuilder UseNugetProxyMiddleware(this IApplicationBuilder builder, IConfigurationRoot config)
        {
            _ConnectionString = config.GetConnectionString("DataContext");

            if (!Directory.Exists("Packages"))
                Directory.CreateDirectory("Packages");
            
            var cacheSection = config.GetSection("Cache");
            _CacheMetadata = int.Parse(cacheSection.GetSection("Metadata").Value);
            _CacheList = int.Parse(cacheSection.GetSection("List").Value);
            _CacheDetail = int.Parse(cacheSection.GetSection("Detail").Value);
            _CacheDefault = int.Parse(cacheSection.GetSection("Default").Value);
            var proxySection = config.GetSection("Proxy");
            _Source = proxySection.GetSection("Source").Value;
            _Replace = proxySection.GetSection("Replace").Value;
            _Path = proxySection.GetSection("Path").Value;

            var routeBuilder = new RouteBuilder(builder);
            routeBuilder.MapGet(_Path + "/{action}", PageHandler);
            routeBuilder.MapGet(_Path, PageHandler);
            routeBuilder.MapGet(_Path + "/package/{id}/{version}", PackageHandler);

            _Path = "/" + _Path;

            var router = routeBuilder.Build();
            return builder.UseRouter(router);
        }
    }
}
