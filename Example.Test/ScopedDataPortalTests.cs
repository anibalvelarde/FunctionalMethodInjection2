using Autofac;
using Csla.DataPortalClient;
using Example.DalConcrete;
using Example.Lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.Test
{

    [TestClass]
    public class ScopedDataPortalTests
    {


        static IContainer clientContainer;
        ILifetimeScope scope;


        [TestInitialize]
        public void TestInitialize()
        {

            if (clientContainer == null)
            {

                ContainerBuilder builder = new ContainerBuilder();

                builder.RegisterGeneric(typeof(ObjectPortal_DPWrapper<>)).As(typeof(IObjectPortal<>));
                builder.RegisterGeneric(typeof(HandleRegistrations<>)).As(typeof(IHandleRegistrations<>)).SingleInstance();
                builder.RegisterModule<LibModule>();

                clientContainer = builder.Build();

                Csla.ApplicationContext.AuthenticationType = "Windows";
                Csla.ApplicationContext.DataPortalProxy = "ObjectPortal.BasicHttpBindingWcfProxy, ObjectPortal";
                Csla.ApplicationContext.DataPortalUrlString = "http://localhost:62686/WcfPortal.svc";

            }

            scope = clientContainer.BeginLifetimeScope();

        }

        [TestMethod]
        public void Root_Create()
        {

            var portal = scope.Resolve<IObjectPortal<IRoot>>();

            var result = portal.Create();

            Assert.AreEqual(1, result.BusinessItemList.Count);
            Assert.IsTrue(result.IsNew);
        }


        [TestMethod]
        public void Root_CreateCriteria()
        {

            var portal = scope.Resolve<IObjectPortal<IRoot>>();
            var criteria = Guid.NewGuid();
            var result = portal.Create(criteria);

            Assert.IsTrue(result.IsNew);
            Assert.AreEqual(1, result.BusinessItemList.Count);
            Assert.AreEqual(criteria, result.BusinessItemList[0].Criteria);

        }

        [TestMethod]
        public void Root_Fetch()
        {


            var portal = scope.Resolve<IObjectPortal<IRoot>>();

            var result = portal.Fetch();

            Assert.IsFalse(result.IsNew);
            Assert.IsFalse(result.IsDirty);
            Assert.IsNotNull(result.BusinessItemList);
            Assert.AreEqual(2, result.BusinessItemList.Count);
        }

        [TestMethod]
        public void Root_Fetch_Criteria()
        {
            var portal = scope.Resolve<IObjectPortal<IRoot>>();
            var criteria = Guid.NewGuid();

            var result = portal.Fetch(criteria);

            Assert.IsFalse(result.IsNew);
            Assert.IsFalse(result.IsDirty);
            Assert.AreEqual(criteria, result.BusinessItemList[0].Criteria);
            Assert.AreEqual(Guid.Empty, result.BusinessItemList[0].UpdatedID);

        }

    }

}
