using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Wodsoft.NugetProxy.App_Start.ComBoostConfig), "Start")]
namespace Wodsoft.NugetProxy.App_Start
{
    public class ComBoostConfig
    {
        public static void Start()
        {            
            System.Web.Security.ComBoostPrincipal.Resolver = Resolver;
        }
    
        private static IRoleEntity Resolver(Type entityType, string username)
        {
            //Todo:
            //Return user entity by username.
            return null;
        }
    }
}