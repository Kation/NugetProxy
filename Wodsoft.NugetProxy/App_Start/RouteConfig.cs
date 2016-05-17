using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace Wodsoft.NugetProxy
{
    public class RouteConfig2
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Home",
                url: "Home/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );

            routes.Add(new Route("api/v2/package/{id}/{version}", new NugetPackageRouteHandler()));
            routes.Add(new Route("api/v2", new NugetRouteHandler()));
            routes.Add(new Route("api/v2/{action}", new NugetRouteHandler())
            { Constraints = new RouteValueDictionary(new { action = ".*" }) });

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Index", id = UrlParameter.Optional }
            );
        }
    }
}

