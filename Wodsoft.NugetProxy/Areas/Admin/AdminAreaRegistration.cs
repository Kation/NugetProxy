using System.Web.Mvc;
using Wodsoft.NugetProxy.Models;

namespace Wodsoft.NugetProxy.Areas.Admin
{
    public class AdminAreaRegistration : AreaRegistration 
    {
        public override string AreaName 
        {
            get 
            {
                return "Admin";
            }
        }

        public override void RegisterArea(AreaRegistrationContext context) 
        {
            var route = context.Routes.MapRoute<Member>(
                "Admin_default",
                "Admin/{controller}/{action}/{id}",
                new { action = "Index", id = UrlParameter.Optional }
            );
            route.DataTokens["area"] = "Admin";
            route.DataTokens["authArea"] = "Admin";
        }
    }
}