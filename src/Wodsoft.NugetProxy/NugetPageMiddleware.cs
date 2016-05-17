using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Wodsoft.NugetProxy.Models;
using System.Net;
using System.IO;
using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace Wodsoft.NugetProxy
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    //public class NugetPageMiddleware
    //{
    //    private readonly RequestDelegate _next;

    //    public NugetPageMiddleware(RequestDelegate next)
    //    {
    //        _next = next;
    //    }

    //    public async Task Invoke(HttpContext httpContext)
    //    {
    //        //Microsoft.AspNetCore.Routing.RequestDelegateRouteBuilderExtensions
    //        string path = httpContext.Request.Path.Value;
    //        if (path.StartsWith("/api/v2"))
    //        {
    //            await SynchronizationHelp.Enter(path);
    //            Page page;
    //            DataContext dataContext;
    //            try
    //            {
    //                dataContext = new DataContext();
    //                page = dataContext.Page.SingleOrDefault(t => t.Path == path);
    //            }
    //            catch (Exception ex)
    //            {
    //                SynchronizationHelp.Exit(path);
    //                httpContext.Response.StatusCode = 500;
    //                await httpContext.Response.WriteAsync(ex.Message);
    //                ExceptionLog(ex, httpContext);
    //                return;
    //            }
    //            if (page == null)
    //            {
    //                page = new Page();
    //                page.Path = path;
    //                page.Id = Guid.NewGuid();
    //                dataContext.Page.Add(page);
    //            }
    //            else
    //            {
    //                SynchronizationHelp.Exit(path);
    //            }
    //            byte[] pageContent = page.Content;
    //            if (page.ExpiredDate < DateTime.Now)
    //            {
    //                string key = path;
    //                bool download = true;
    //                if (pageContent != null)
    //                {
    //                    key = "Download/" + path;
    //                    download = await SynchronizationHelp.TryEnter(key);
    //                }
    //                if (download)
    //                {
    //                    NugetDownloader downloader = new NugetDownloader("https://www.nuget.org" + httpContext.Request.Path.Value + httpContext.Request.QueryString.Value);
    //                    Task<Task> downloadTask = downloader.Download().ContinueWith(async t =>
    //                    {
    //                        if (downloader.StatusCode == HttpStatusCode.OK)
    //                        {
    //                            var reader = new StreamReader(downloader.Stream);
    //                            var content = await reader.ReadToEndAsync();
    //                            string replaceTo = httpContext.Request.Scheme + "://" + httpContext.Request.Host.Host;
    //                            content = content.Replace("https://www.nuget.org/api/v2", replaceTo + "/api/v2");
    //                            page.Content = Encoding.UTF8.GetBytes(content);
    //                            page.ExpiredDate = DateTime.Now.AddSeconds(GetCacheTime(_action));
    //                        }
    //                        else
    //                        {
    //                            if (pageContent == null)
    //                                page.Content = new byte[0];
    //                            page.ExpiredDate = DateTime.Now.AddSeconds(10);
    //                        }
    //                        page.ContentType = downloader.ContentType;
    //                        await dataContext.SaveChangesAsync();
    //                    });
    //                    if (pageContent == null)
    //                    {
    //                        try
    //                        {
    //                            await await downloadTask;
    //                        }
    //                        catch (Exception ex)
    //                        {
    //                            httpContext.Response.StatusCode = 500;
    //                            await httpContext.Response.WriteAsync(ex.Message);
    //                            ExceptionLog(ex, httpContext);
    //                            return;
    //                        }
    //                        finally
    //                        {
    //                            SynchronizationHelp.Exit(key);
    //                        }
    //                        pageContent = page.Content;
    //                    }

    //                    if (page.Content == null || page.Content.Length == 0)
    //                    {
    //                        httpContext.Response.StatusCode = 400;
    //                        return;
    //                    }
    //                }
    //            }
    //            httpContext.Response.ContentType = page.ContentType;
    //            //httpContext.Response.Cache.SetExpires(page.ExpiredDate);
    //            if (pageContent.Length > 0)
    //            {
    //                await httpContext.Response.Body.WriteAsync(pageContent, 0, pageContent.Length);
    //                await httpContext.Response.Body.FlushAsync();
    //            }
    //        }
    //        else
    //            await _next(httpContext);
    //    }

    //    private int GetCacheTime(string action)
    //    {
    //        switch (action)
    //        {
    //            case "":
    //            case "/":
    //            case "$metadata":
    //                return 60 * 24 * 7;
    //            case "search()":
    //                return 60;
    //            case "findpackagesbyid()":
    //                return 60 * 12;
    //            default:
    //                return 30;
    //        }
    //    }

    //    private void ExceptionLog(Exception ex, HttpContext context)
    //    {
    //        ILoggerFactory loggerFactory = context.RequestServices.GetService<ILoggerFactory>();
    //        var log = loggerFactory.CreateLogger<NugetPageMiddleware>();
    //        log.LogError(new EventId(), ex, "Nuget handle error");
    //    }
    //}

    // Extension method used to add the middleware to the HTTP request pipeline.
    public static class NugetPageMiddlewareExtensions
    {
        public static IApplicationBuilder UseNugetPageMiddleware(this IApplicationBuilder builder)
        {
            var routeBuilder = new RouteBuilder(builder);
            routeBuilder.MapGet("api/v2/{action}", Invoke);
            routeBuilder.MapGet("api/v2", Invoke);
            var router = routeBuilder.Build();
            return builder.UseRouter(router);
        }

        public static async Task Invoke(HttpContext httpContext)
        {
            //Microsoft.AspNetCore.Routing.RequestDelegateRouteBuilderExtensions
            string action = httpContext.GetRouteValue("action") as string;
            string path = httpContext.Request.Path.Value;

            await SynchronizationHelp.Enter(path);
            Page page;
            DataContext dataContext;
            try
            {
                dataContext = new DataContext();
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
                    NugetDownloader downloader = new NugetDownloader("https://www.nuget.org" + httpContext.Request.Path.Value + httpContext.Request.QueryString.Value);
                    Task<Task> downloadTask = downloader.Download().ContinueWith(async t =>
                    {
                        if (downloader.StatusCode == HttpStatusCode.OK)
                        {
                            var reader = new StreamReader(downloader.Stream);
                            var content = await reader.ReadToEndAsync();
                            string replaceTo = httpContext.Request.Scheme + "://" + httpContext.Request.Host.Host;
                            content = content.Replace("https://www.nuget.org/api/v2", replaceTo + "/api/v2");
                            page.Content = Encoding.UTF8.GetBytes(content);
                            page.ExpiredDate = DateTime.Now.AddSeconds(GetCacheTime(action));
                        }
                        else
                        {
                            if (pageContent == null)
                                page.Content = new byte[0];
                            page.ExpiredDate = DateTime.Now.AddSeconds(10);
                        }
                        page.ContentType = downloader.ContentType;
                        await dataContext.SaveChangesAsync();
                    });
                    if (pageContent == null)
                    {
                        try
                        {
                            await await downloadTask;
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

        private static int GetCacheTime(string action)
        {
            switch (action)
            {
                case "":
                case "/":
                case "$metadata":
                    return 60 * 24 * 7;
                case "search()":
                    return 60;
                case "findpackagesbyid()":
                    return 60 * 12;
                default:
                    return 30;
            }
        }

        private static void ExceptionLog(Exception ex, HttpContext context)
        {
            ILoggerFactory loggerFactory = context.RequestServices.GetService<ILoggerFactory>();
            var log = loggerFactory.CreateLogger("Page handler");
            log.LogError(new EventId(), ex, "Nuget handle error");
        }
    }
}
