using Autofac;
using Csla.Reflection;
using Csla.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPortal
{

    public interface IObjectPortalActivator : IDataPortalActivator
    {

    }

    public class ObjectPortalActivator : IObjectPortalActivator
    {

        public ObjectPortalActivator(IContainer c)
        {
            this.Container = c;
        }

        IContainer Container { get; set; }

        public object CreateInstance(Type requestedType)
        {

            if (requestedType.IsGenericType && typeof(IMobileObjectWrapper).IsAssignableFrom(requestedType))
            {
                var scope = Container.BeginLifetimeScope();
                var func = scope.Resolve(typeof(Func<,>).MakeGenericType(new Type[] { typeof(ILifetimeScope), requestedType }));

                var mow = ((Func<ILifetimeScope, IMobileObjectWrapper>)func)(scope);

                return mow;

            }
            else
            {
                return MethodCaller.CreateInstance(requestedType);
            }


        }

        public void FinalizeInstance(object obj)
        {
            if (typeof(IMobileObjectWrapper).IsAssignableFrom(obj.GetType()))
            {
                var mow = (IMobileObjectWrapper)obj;
                mow.scope.Dispose();
            }
        }

        public void InitializeInstance(object obj)
        {
            /* Nothing needed */
        }
    }
}
