using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Routing;

namespace Wodsoft.NugetProxy
{
    public class NugetRouteHandler : IRouteHandler
    {
        public IHttpHandler GetHttpHandler(RequestContext requestContext)
        {
            object action;
            requestContext.RouteData.Values.TryGetValue("action", out action);
            if (action == null)
                action = "";
            return new NugetHandler(action.ToString());
        }
    }
}