using Autofac;
using Csla.Core;
using Csla.Serialization.Mobile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPortal
{
    internal interface IMobileDependency : IMobileObject
    {
        void ResetDependency(ILifetimeScope scope);
    }

    public interface IMobileDependency<T>
    {
        T Dependency { get; }
    }

    [Serializable]
    [KnownType(typeof(IObjectPortal<>))]
    internal class MobileDependency<T> : Csla.Core.MobileObject, IMobileDependency<T>, IMobileDependency
    {

        [NonSerialized]
        Lazy<T> _dependency;

        public T Dependency { get { return _dependency.Value; } }


        void IMobileDependency.ResetDependency(ILifetimeScope scope)
        {
            _dependency = scope.Resolve<Lazy<T>>();
        }

        public MobileDependency() { }

        public MobileDependency(Lazy<T> dependency, MobileDependencyList mobileDependencies)
        {
            this._dependency = dependency;
            mobileDependencies.Add(this);
        }


    }

    [Serializable]
    internal class MobileDependencyList : MobileList<IMobileDependency>, IMobileObject
    {
     
    }
}
