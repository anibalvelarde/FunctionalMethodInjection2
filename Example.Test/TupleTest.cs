using Autofac;
using Autofac.Core;
using Example.Dal;
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
    public class TupleTest
    {

        static IContainer container;
        ILifetimeScope scope;


        [TestInitialize]
        public void TestInitialize()
        {

            if (container == null)
            {

                ContainerBuilder builder = new ContainerBuilder();

                builder.RegisterModule<ObjectPortal.AutofacModuleServer>();


                builder.RegisterType<RootDal>().AsImplementedInterfaces();
                builder.RegisterType<BusinessItemDal>().AsImplementedInterfaces();

                builder.RegisterModule<LibModule>();

                container = builder.Build();

            }

            scope = container.BeginLifetimeScope();

        }

        [TestMethod]
        public void Tuple_2()
        {
            var result = scope.Resolve<System.Tuple<IRootDal, IBusinessItemDal>>();

            Assert.IsNotNull(result.Item1);
            Assert.IsInstanceOfType(result.Item1, typeof(RootDal));

            Assert.IsNotNull(result.Item2);
            Assert.IsInstanceOfType(result.Item2, typeof(BusinessItemDal));

        }

        [TestMethod]
        public void Tuple_3()
        {
            var result = scope.Resolve<System.Tuple<IRootDal, IBusinessItemDal, IObjectPortal<IBusinessItem>>>();

            Assert.IsNotNull(result.Item1);
            Assert.IsInstanceOfType(result.Item1, typeof(RootDal));

            Assert.IsNotNull(result.Item2);
            Assert.IsInstanceOfType(result.Item2, typeof(BusinessItemDal));

            Assert.IsNotNull(result.Item3);
            Assert.IsInstanceOfType(result.Item3, typeof(ObjectPortal<IBusinessItem>));

        }

        [TestMethod]
        public void Tuple_NotRegistered()
        {

            try
            {
                var result = scope.Resolve<System.Tuple<IRootDal, IBusinessItemDal, IObjectPortal<IBusinessItem>, IBusinessObject>>();

                Assert.Fail("Should throw an exception");
            }
            catch (DependencyResolutionException ex)
            {

            }
            catch (Exception)
            {
                Assert.Fail("Unexpected exception type");
            }


        }
    }
}
