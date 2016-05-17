using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Microsoft.Practices.Unity;
using Wodsoft.NugetProxy.Models;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Wodsoft.NugetProxy.App_Start.UnityControllerFactory), "Start")]
namespace Wodsoft.NugetProxy.App_Start
{
    public class UnityControllerFactory : EntityControllerFactory
    {
        public static void Start()
        {
            ControllerBuilder.Current.SetControllerFactory(new UnityControllerFactory(UnityConfig.GetConfiguredContainer()));
        }
    
        IUnityContainer _container;

        public UnityControllerFactory(IUnityContainer container)
        {
            _container = container;

            //Change EntityContextBuilder to your class that inherit IEntityContextBuilder interface.
            //If your entity context builder has constructor with arguments, continue register types that you need.
            container.RegisterType<DbContext,DataContext>(new PerRequestLifetimeManager());
            container.RegisterType<IEntityContextBuilder, EntityContextBuilder>(new PerRequestLifetimeManager());

            //Register your entity here:
            //RegisterController<EntityType>();
            //...
        }

        protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null)
            {
                throw new HttpException(404, "Controller Not Found.");
            }
            return _container.Resolve(controllerType) as IController;
        }
    }
}