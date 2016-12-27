using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Autofac.Core;
using Autofac.Builder;
using Autofac;
using System.Reflection;
using System.Linq.Expressions;
using Csla.Core;
using Csla.Serialization.Mobile;

namespace ObjectPortal
{

    /// <summary>
    /// Abstract BO object creating, fetching and updating each other
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObjectPortal<T> : Csla.Server.ObjectFactory, IObjectPortal<T>
        where T : class, ITrackStatus, IMobileObject
    {

        Func<T> createT;


        private IHandleRegistrations CreateHandleRegistrations(T instance)
        {
            return (IHandleRegistrations)scope.Resolve(typeof(IHandleRegistrations<>).MakeGenericType(instance.GetType()));
        }

        [NonSerialized]
        ILifetimeScope scope;

        public ObjectPortal(Func<T> createT, ILifetimeScope scope)
        {
            this.createT = () =>
            {
                var newT = createT();

                // Tag on the scope
                var dp = newT as IDPBusinessObject; // In the actual implementation we would not use a service locator. Limited by CSLA
                dp.scope = scope;

                return newT;
            };

            this.scope = scope;
        }



        public T Create()
        {

            T result = createT();

            MarkNew(result);

            var regs = CreateHandleRegistrations(result);

            regs.TryExecuteMethod(result, ObjectPortalMethod.Create, scope);

            return result;
        }


        public T Create<C>(C criteria)
        {

            T result = createT();

            MarkNew(result);

            var regs = CreateHandleRegistrations(result);

            if (!regs.TryExecuteMethod(result, ObjectPortalMethod.Create, scope, criteria))
            {
                throw new ObjectPortalOperationNotSupportedException($"CreateChild with criteria {criteria.GetType().FullName} not supported on {result.GetType().FullName}");
            }

            return result;

        }

        public T CreateChild()
        {

            T result = createT();

            MarkNew(result);
            MarkAsChild(result);

            var regs = CreateHandleRegistrations(result);

            regs.TryExecuteMethod(result, ObjectPortalMethod.CreateChild, scope);

            return result;
        }

        public T CreateChild<C>(C criteria)
        {

            T result = createT();

            MarkNew(result);
            MarkAsChild(result);

            var regs = CreateHandleRegistrations(result);

            if (!regs.TryExecuteMethod(result, ObjectPortalMethod.CreateChild, scope, criteria))
            {
                throw new ObjectPortalOperationNotSupportedException($"CreateChild with criteria {criteria.GetType().FullName} not supported on {result.GetType().FullName}");
            }

            return result;


        }

        public T Fetch()
        {

            T result = createT();

            MarkOld(result);

            var regs = CreateHandleRegistrations(result);

            regs.TryExecuteMethod(result, ObjectPortalMethod.Fetch, scope);

            return result;
        }


        public T Fetch<C>(C criteria)
        {

            T result = createT();

            MarkOld(result);

            var regs = CreateHandleRegistrations(result);

            if (!regs.TryExecuteMethod(result, ObjectPortalMethod.Fetch, scope, criteria))
            {
                throw new ObjectPortalOperationNotSupportedException($"FetchChild with criteria {criteria.GetType().FullName} not supported on {result.GetType().FullName}");
            }

            return result;

        }

        public T FetchChild()
        {

            T result = createT();

            MarkOld(result);
            MarkAsChild(result);

            var regs = CreateHandleRegistrations(result);

            regs.TryExecuteMethod(result, ObjectPortalMethod.FetchChild, scope);

            return result;
        }

        public T FetchChild<C>(C criteria)
        {

            T result = createT();

            MarkOld(result);
            MarkAsChild(result);

            var regs = CreateHandleRegistrations(result);

            if (!regs.TryExecuteMethod(result, ObjectPortalMethod.FetchChild, scope, criteria))
            {
                throw new ObjectPortalOperationNotSupportedException($"FetchChild with criteria {criteria.GetType().FullName} not supported on {result.GetType().FullName}");
            }

            return result;


        }


        public T Update(T bo)
        {

            if (bo.IsDirty)
            {
                var regs = CreateHandleRegistrations(bo);
                ObjectPortalMethod? method = null;

                if (bo.IsNew)
                {
                    method = ObjectPortalMethod.Insert;
                }
                else
                {
                    method = ObjectPortalMethod.Update;
                }

                if (!regs.TryExecuteMethod(bo, method.Value, scope))
                {
                    throw new ObjectPortalOperationNotSupportedException($"Update (no criteria) not supported on {bo.GetType().FullName}");
                }

                MarkOld(bo);

            }

            return bo;
        }


        public void UpdateChild(T bo)
        {

            if (bo.IsDirty)
            {
                var regs = CreateHandleRegistrations(bo);
                ObjectPortalMethod? method = null;

                if (bo.IsNew)
                {
                    method = ObjectPortalMethod.InsertChild;
                }
                else
                {
                    method = ObjectPortalMethod.UpdateChild;
                }

                if (!regs.TryExecuteMethod(bo, method.Value, scope))
                {
                    throw new ObjectPortalOperationNotSupportedException($"Update (no criteria) not supported on {bo.GetType().FullName}");
                }

                MarkOld(bo);

            }
            

        }

        public void UpdateChild<C>(T bo, C criteria)
        {
            if (bo.IsDirty)
            {
                var regs = CreateHandleRegistrations(bo);
                ObjectPortalMethod? method = null;

                if (bo.IsNew)
                {
                    method = ObjectPortalMethod.InsertChild;
                }
                else
                {
                    method = ObjectPortalMethod.UpdateChild;
                }

                if (!regs.TryExecuteMethod(bo, method.Value, scope, criteria))
                {
                    throw new ObjectPortalOperationNotSupportedException($"UpdateChild {criteria.GetType().FullName} not supported on {bo.GetType().FullName}");
                }

                MarkOld(bo);

            }
            
        }
    }


    [Serializable]
    public class ObjectPortalOperationNotSupportedException : Exception
    {
        public ObjectPortalOperationNotSupportedException() { }
        public ObjectPortalOperationNotSupportedException(string message) : base(message) { }
        public ObjectPortalOperationNotSupportedException(string message, Exception inner) : base(message, inner) { }
        protected ObjectPortalOperationNotSupportedException(
          System.Runtime.Serialization.SerializationInfo info,
          System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
    }
}
