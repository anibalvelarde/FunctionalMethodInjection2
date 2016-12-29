using Autofac;
using Csla.DataPortalClient;
using Csla.Reflection;
using Csla.Serialization;
using Example.DalConcrete;
using Example.Lib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ObjectPortal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
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

                builder.RegisterModule<ObjectPortal.AutofacModuleClient>();
                builder.RegisterGeneric(typeof(HandleRegistrations<>)).As(typeof(IHandleRegistrations<>)).SingleInstance();
                builder.RegisterModule<LibModule>();

                clientContainer = builder.Build();

                Csla.ApplicationContext.AuthenticationType = "Windows";
                //Csla.ApplicationContext.DataPortalProxy = "Csla.DataPortalClient.WcfProxy, Csla";
                Csla.ApplicationContext.DataPortalProxy = "ObjectPortal.BasicHttpBindingWcfProxy, ObjectPortal";
                Csla.ApplicationContext.DataPortalUrlString = "http://localhost/Example.Test.Service/WcfPortal.svc";

                // OverrideCSLAConstructor();

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

        [TestMethod]
        public void Fetch_AddChild()
        {

            var portal = scope.Resolve<IObjectPortal<IRoot>>();

            var result = portal.Fetch();

            var count = result.BusinessItemList.Count;

            var newBo = result.BusinessItemList.CreateAddChild();

            Assert.IsNotNull(newBo);
            Assert.AreEqual(count + 1, result.BusinessItemList.Count);

        }

        [TestMethod]

        public void OverrideCSLAConstructor()
        {

            // TODO : Discuss - Only being used by Clone
            // WCF always uses NetDataContractAttribute

            var fields = typeof(Csla.Reflection.MethodCaller)
                .GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
                .Where(x => x.Name == @"_ctorCache").Single();

            var dict = (Dictionary<Type, DynamicCtorDelegate>)fields.GetValue(null);

            ConstructorInfo info = typeof(Csla.Serialization.BinaryFormatterWrapper)
                .GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, Type.EmptyTypes, null);

            Func<BinaryFormatterWrapper> func = () =>
            {
                var f = new BinaryFormatterWrapper();
                f.Formatter.Context = new StreamingContext(StreamingContextStates.All, @"Keith");
                return f;
            };

            MethodCallExpression method = Expression.Call(Expression.Constant(func.Target), func.Method);

            var exp = Expression.Lambda<DynamicCtorDelegate>(method).Compile();

            dict[typeof(BinaryFormatterWrapper)] = exp;

            var formatter = Csla.Serialization.SerializationFormatterFactory.GetFormatter();

        }

    }

}
