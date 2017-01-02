using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using ObjectPortal;
using Example.Dal;

namespace Example.Lib
{
    public class LibModule : Autofac.Module
    {

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
            builder.RegisterType<Root>().As<IRoot>().AsSelf();
            builder.RegisterType<BusinessItem>().As<IBusinessItem>().AsSelf();
            builder.RegisterType<BusinessItemList>().As<IBusinessItemList>().AsSelf();

            // TODO Discuss
            // Try to use as little reflection as possible
            // Let DI do the heavy lifting
            builder.Register<LoadHandleRegistrations<Root>>(cc => (r) => Root.Handle(r));
            builder.Register<LoadHandleRegistrations<BusinessItemList>>(cc => (r) => BusinessItemList.Handle(r));
            builder.Register<LoadHandleRegistrations<BusinessItem>>(cc => (r) => BusinessItem.Handle(r));

            // Need to find a way to make this generic
            // Delegates and generics do not play nice together!!

            //builder.Register<FetchRoot>((c) =>
            //{
            //    var portal = c.Resolve<IObjectPortal<IRoot>>();
            //    return () => portal.Fetch(); // C# lets you implicitly convert a lamda to a delegate...can't do this anywhere else!
            //});

            //builder.Register<Func<BusinessItemDto, IBusinessItem>>(c =>
            //{
            //    var portal = c.Resolve<IObjectPortal<IBusinessItem>>();

            //    return (d) => portal.Fetch(d);

            //});

            //// Update - Best I came up with
            //builder.RegisterObjectPortalFetch(typeof(FetchRoot));
            //builder.RegisterObjectPortalFetch(typeof(FetchRootGuid));

            // Update Nov 21st
            // You no longer need to register all of the factory delegates
            // ObjectPortal will automatically recognize and create them for you on the fly
            // However I like the approach of registering them
            // Because I think it will be better peformance

            // With both cases there's a catch
            // if you forget to register them you won't get an error
            // Autofac will automatically create a delegate for you
            // but it will not call the ObjectPortal method for you

            //builder.RegisterObjectPortalFetchChild(typeof(IObjectPortal<IBusinessItem>));
            //builder.RegisterObjectPortalFetchChild(typeof(IObjectPortal<IBusinessItem>));
            ////builder.RegisterObjectPortalFetchChild(typeof(IObjectPortal<IBusinessItemList>));
            //builder.RegisterObjectPortalFetchChild(typeof(FetchChildBusinessItemListGuid));

            ///// I don't think registering these are neccessary
            ///// Can't ObjectPortal realize that the dependency you are asking for is a Delegate
            ///// and construct the delegate on the fly??
            //builder.RegisterObjectPortalCreateChild(typeof(IObjectPortal<IBusinessItem>List));
            //builder.RegisterObjectPortalCreateChild(typeof(IObjectPortal<IBusinessItem>));
            //builder.RegisterObjectPortalCreateChild(typeof(IObjectPortal<IBusinessItem>ListGuid));
            //builder.RegisterObjectPortalCreateChild(typeof(IObjectPortal<IBusinessItem>Guid));

            //builder.RegisterObjectPortalUpdate(typeof(IRoot));
            //builder.RegisterObjectPortalUpdateChild(typeof(IBusinessItemList));
            //builder.RegisterObjectPortalUpdateChild(typeof(IObjectPortal<IBusinessItem>));


            //builder.Register<System.Tuple<IObjectPortal<IBusinessItem>, IBusinessItemDal>>(cc =>
            //{
            //    return System.Tuple.Create<IObjectPortal<IBusinessItem>, IBusinessItemDal>(cc.Resolve<IObjectPortal<IBusinessItem>>(), cc.Resolve<IBusinessItemDal>());
            //});


            // TODO : Review..Cool??? Or not??
            // Finds the constructor of System.Tuple and resolves each dependency!
            builder.RegisterGeneric(typeof(System.Tuple<,>));
            builder.RegisterGeneric(typeof(System.Tuple<,,>));
            builder.RegisterGeneric(typeof(System.Tuple<,,,>));
            builder.RegisterGeneric(typeof(System.Tuple<,,,,>));
            builder.RegisterGeneric(typeof(System.Tuple<,,,,,>));
            builder.RegisterGeneric(typeof(System.Tuple<,,,,,,>));
            builder.RegisterGeneric(typeof(System.Tuple<,,,,,,,>));

        }

    }

}
