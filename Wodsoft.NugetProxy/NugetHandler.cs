using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Wodsoft.NugetProxy.Models;

namespace Wodsoft.NugetProxy
{
    public class NugetHandler : IHttpHandler, IHttpAsyncHandler
    {
        private string _action;

        public NugetHandler(string action)
        {
            _action = action.ToLower();
        }

        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context) { }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            var task = Process(context);
            task.ContinueWith(t => cb(t));
            return task;
        }

        public void EndProcessRequest(IAsyncResult result) { }

        private async Task Process(HttpContext context)
        {
            string path = context.Request.Url.PathAndQuery.ToLower();
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
                context.Response.StatusCode = 500;
                context.Response.Write(ex.Message);
                ExceptionLog(ex, context);
                return;
            }
            if (page == null)
            {
                page = dataContext.Page.Create();
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
                    NugetDownloader downloader = new NugetDownloader(MvcApplication.NugetSource + context.Request.Url.PathAndQuery.Substring(7));
                    Task<Task> downloadTask = downloader.Download().ContinueWith(async t =>
                    {
                        if (downloader.StatusCode == HttpStatusCode.OK)
                        {
                            var reader = new StreamReader(downloader.Stream);
                            var content = await reader.ReadToEndAsync();
                            string replaceTo = context.Request.Url.Scheme + "://" + context.Request.Url.Host;
                            if (context.Request.Url.Port != 80)
                                replaceTo += ":" + context.Request.Url.Port;
                            replaceTo += "/api/v2";
                            content = content.Replace(MvcApplication.UrlReplace, replaceTo);
                            page.Content = Encoding.UTF8.GetBytes(content);
                            page.ExpiredDate = DateTime.Now.AddSeconds(GetCacheTime(_action));
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
                            context.Response.StatusCode = 500;
                            context.Response.Write(ex.Message);
                            ExceptionLog(ex, context);
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
                        context.Response.StatusCode = 400;
                        return;
                    }
                }
            }
            context.Response.ContentType = page.ContentType;
            context.Response.Cache.SetExpires(page.ExpiredDate);
            if (pageContent.Length > 0)
                await context.Response.OutputStream.WriteAsync(pageContent, 0, pageContent.Length);
            await context.Response.OutputStream.FlushAsync();
        }

        private int GetCacheTime(string action)
        {
            switch (action)
            {
                case "":
                case "/":
                case "$metadata":
                    return MvcApplication.MetadataCacheTime;
                case "search()":
                    return MvcApplication.ListCacheTime;
                case "findpackagesbyid()":
                    return MvcApplication.DetailCacheTime;
                default:
                    return MvcApplication.DefaultCacheTime;
            }
        }

        private void ExceptionLog(Exception ex, HttpContext context)
        {
            var path = context.Server.MapPath("~/Logs/");
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
            var filename = context.Server.MapPath("~/Logs/" + DateTime.Now.ToString("yyyy-MM-dd HHmmssfff") + ".log");
            var stream = File.Open(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.None);
            var writer = new StreamWriter(stream);
            writer.WriteLine(ex.Data);
            writer.WriteLine(ex.Message);
            writer.WriteLine(ex.Source);
            writer.WriteLine(ex.StackTrace);
            writer.Flush();
            writer.Close();
        }
    }
}