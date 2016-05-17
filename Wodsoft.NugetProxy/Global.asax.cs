using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Configuration;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Wodsoft.NugetProxy
{
    public class MvcApplication : System.Web.HttpApplication
    {
        private static StreamWriter Log;

        public static string NugetSource { get; private set; }

        public static int DefaultCacheTime { get; private set; }

        public static int ListCacheTime { get; private set; }

        public static int DetailCacheTime { get; private set; }

        public static int MetadataCacheTime { get; private set; }

        protected void Application_Start()
        {
            string packagePath = HttpContext.Current.Server.MapPath("~/Packages");
            if (!Directory.Exists(packagePath))
                Directory.CreateDirectory(packagePath);

            NugetSource = WebConfigurationManager.AppSettings["source"];
            DefaultCacheTime = int.Parse(WebConfigurationManager.AppSettings["defaultCacheTime"]);
            ListCacheTime = int.Parse(WebConfigurationManager.AppSettings["listCacheTime"]);
            DetailCacheTime = int.Parse(WebConfigurationManager.AppSettings["detailCacheTime"]);
            MetadataCacheTime = int.Parse(WebConfigurationManager.AppSettings["metadataCacheTime"]);

            //Log = new StreamWriter(File.Open(HttpContext.Current.Server.MapPath("~/log.log"), FileMode.Create, FileAccess.ReadWrite, FileShare.Read));
            //Log.AutoFlush = true;

            AreaRegistration.RegisterAllAreas();
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig2.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);            
        }

        //protected void Application_BeginRequest()
        //{
        //    string path = HttpContext.Current.Request?.Url?.PathAndQuery;
        //    if (Log != null && path != null)
        //        lock (Log)
        //        {
        //            Log.WriteLine(path);
        //        }
        //}
    }
}
