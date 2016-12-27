using Autofac;
using Autofac.Integration.Wcf;
using Csla.Server;
using Example.DalConcrete;
using Example.Lib;
using ObjectPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;

namespace Example.Test.Service
{
    public class Global : System.Web.HttpApplication
    {
        private IContainer container;

        protected void Application_Start(object sender, EventArgs e)
        {

            var builder = new ContainerBuilder();

            builder.RegisterGeneric(typeof(ObjectPortal<>)).As(typeof(IObjectPortal<>));
            builder.RegisterGeneric(typeof(HandleRegistrations<>)).As(typeof(IHandleRegistrations<>)).SingleInstance();
            builder.RegisterType<RootDal>().AsImplementedInterfaces();
            builder.RegisterType<BusinessItemDal>().AsImplementedInterfaces();
            builder.RegisterModule<LibModule>();
            builder.RegisterType<ObjectPortal.AutofacWcfPortal>();
            builder.RegisterGeneric(typeof(CslaServerObjectPortal<>));

            container = builder.Build();

            //            Csla.Server.DataPortalBroker.DataPortalServer = new CslaServerObjectPortal();

            AutofacHostFactory.Container = container;

            Csla.ApplicationContext.AuthenticationType = "Windows";

        }

        protected void Session_Start(object sender, EventArgs e)
        {

        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {

        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        protected void Application_Error(object sender, EventArgs e)
        {

        }

        protected void Session_End(object sender, EventArgs e)
        {

        }

        protected void Application_End(object sender, EventArgs e)
        {

        }
    }
}