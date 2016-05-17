using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Routing;
using Wodsoft.NugetProxy.Models;

namespace Wodsoft.NugetProxy
{
    public class NugetPackageHandler : IHttpAsyncHandler
    {
        private string _id, _version;
        public NugetPackageHandler(string id, string version)
        {
            _id = id.ToLower();
            _version = version.ToLower();
        }

        public bool IsReusable { get { return false; } }

        public void ProcessRequest(HttpContext context)
        {
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            var task = Process(context);
            task.ContinueWith(t => cb(t));
            return task;
        }

        public void EndProcessRequest(IAsyncResult result) { }

        private async Task Process(HttpContext context)
        {
            context.Response.ContentType = "binary/octet-stream";
            context.Response.AppendHeader("Content-Disposition", "attachment; filename=\"" + context.Server.UrlEncode(_id + "." + _version + ".nupkg") + "\"");

            string filename = context.Server.MapPath("~/packages/" + _id + "." + _version + ".nupkg");
            await SynchronizationHelp.Enter(filename);
            Stream stream = null;
            if (!File.Exists(filename))
            {
                try
                {
                    NugetDownloader downloader = new NugetDownloader(MvcApplication.NugetSource + context.Request.Url.PathAndQuery);
                    downloader.Progress += async (sender, buffer, length) =>
                    {
                        if (stream == null)
                            stream = File.Open(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.Read);
                        await stream.WriteAsync(buffer, 0, length);
                        await context.Response.OutputStream.WriteAsync(buffer, 0, length);
                    };
                    await downloader.Download();
                    if (downloader.StatusCode != HttpStatusCode.OK)
                    {
                        if (stream != null)
                        {
                            stream.Close();
                            stream.Dispose();
                            File.Delete(filename);
                        }
                        context.Response.StatusCode = (int)downloader.StatusCode;
                        return;
                    }
                    else
                    {
                        stream.Close();
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
                await stream.CopyToAsync(context.Response.OutputStream);
            }
        }
    }
}