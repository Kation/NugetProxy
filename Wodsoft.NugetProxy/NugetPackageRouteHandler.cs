using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Wodsoft.NugetProxy
{
    public class NugetPackageRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            object id, version;
            if (!requestContext.RouteData.Values.TryGetValue("id", out id))
                return null;
            if (!requestContext.RouteData.Values.TryGetValue("version", out version))
                return null;
            return new NugetPackageHandler(id.ToString(), version.ToString());
        }
    }
}