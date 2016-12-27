using Csla.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using Csla.Core;
using Csla.Serialization.Mobile;

namespace ObjectPortal
{

    public interface IServerObjectPortal
    {
        Task<DataPortalResult> Create<C>(C criteria, DataPortalContext context, bool isSync);
        Task<DataPortalResult> Fetch<C>(C criteria, DataPortalContext context, bool isSync);
    }

    public class CslaServerObjectPortal<T> : IServerObjectPortal
        where T:class, ITrackStatus, IMobileObject
    {
        ILifetimeScope scope;
        public CslaServerObjectPortal(ILifetimeScope scope)
        {
            this.scope = scope;
        }

        public Task<DataPortalResult> Create<C>(C criteria, DataPortalContext context, bool isSync)
        {

            IObjectPortal<T> portal = scope.Resolve<IObjectPortal<T>>();
            object result = null;

            if(criteria != null && criteria.GetType() != typeof(EmptyCriteria))
            {
                result = portal.Create(criteria);
            } else
            {
                result = portal.Create();
            }


            return Task.FromResult(new DataPortalResult(result));

        }

        public Task<DataPortalResult> Delete(object criteria, DataPortalContext context, bool isSync)
        {
            throw new NotImplementedException();
        }

        public Task<DataPortalResult> Fetch<C>(C criteria, DataPortalContext context, bool isSync)
        {

            object result = null;

            IObjectPortal<T> portal = scope.Resolve<IObjectPortal<T>>();

            if (criteria != null && criteria.GetType() != typeof(EmptyCriteria))
            {
                result = portal.Fetch(criteria);
            }
            else
            {
                result = portal.Fetch();
            }


            return Task.FromResult(new DataPortalResult(result));
        }

        public Task<DataPortalResult> Update(object obj, DataPortalContext context, bool isSync)
        {
            throw new NotImplementedException();
        }
    }
}
