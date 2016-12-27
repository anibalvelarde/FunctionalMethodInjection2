using Autofac;
using Autofac.Core;
using Csla;
using Csla.Core;
using Csla.Serialization.Mobile;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectPortal
{
    public class ObjectPortal_DPWrapper<T> : IObjectPortal<T>
        where T : class, IMobileObject, ITrackStatus
    {
        #region Fields

        private readonly Type _concreteType;

        #endregion

        #region ctor

        public ObjectPortal_DPWrapper(ILifetimeScope scope)
        {
            //first check T and see if we are getting and abstract or interface type - if we are - we can use the scope to resolve T
            //if T is not an interface or abstract - then we don't even need to check the container for it - just use the type directly
            Type genericType = typeof(T);

            if (genericType.IsInterface || genericType.IsAbstract)
            {
                IComponentRegistration registration = scope.ComponentRegistry.RegistrationsFor(new TypedService(genericType)).FirstOrDefault();
                if (registration != null)
                {
                    IInstanceActivator activator = registration.Activator as IInstanceActivator;

                    if (activator != null)
                    {
                        _concreteType = activator.LimitType;
                    }
                }

                if (_concreteType == null)
                {
                    throw new Exception($"Cannot find registration for {_concreteType.FullName}");
                }
            }
            else
            {
                //if we were given a non-abstract type already - then just use it
                _concreteType = typeof(T);
            }
        }

        #endregion

        public T Execute(T command)
        {
            T retObj = default(T);

            Type type = typeof(T);
            if (type.IsInterface || type.IsAbstract)
            {
                retObj = (T)typeof(DataPortal).GetMethod("Execute")
                                              .MakeGenericMethod(_concreteType)
                                              .Invoke(null, new object[] { command });
            }
            else
            {
                retObj = DataPortal.Execute<T>(command);
            }

            return retObj;
        }

        public async Task<T> ExecuteAsync(T command)
        {
            Type type = typeof(T);
            T retObj = default(T);

            if (type.IsInterface || type.IsAbstract)
            {
                Task asyncTsk = typeof(DataPortal).GetMethod("ExecuteAsync")
                                                  .MakeGenericMethod(_concreteType)
                                                  .Invoke(null, new object[] { command }) as Task;

                await asyncTsk;

                retObj = VerifyAsyncResultFromTask(asyncTsk);
            }
            else
            {
                retObj = await DataPortal.ExecuteAsync<T>(command);
            }

            return retObj;
        }

        public T Create()
        {
            Type type = typeof(T);
            T retObj = default(T);

            if (type.IsInterface || type.IsAbstract)
            {
                retObj = (T)typeof(DataPortal).GetMethod("Create", Type.EmptyTypes)
                                              .MakeGenericMethod(_concreteType)
                                              .Invoke(null, null);
            }
            else
            {
                retObj = DataPortal.Create<T>();
            }

            return retObj;
        }

        public T Create<C>(C criteria)
        {
            Type type = typeof(T);
            T retObj = default(T);

            if (type.IsInterface || type.IsAbstract)
            {
                retObj = (T)typeof(DataPortal).GetMethod("Create", new Type[] { typeof(object) })
                                              .MakeGenericMethod(_concreteType)
                                              .Invoke(null, new object[] { criteria });
            }
            else
            {
                retObj = DataPortal.Create<T>(criteria);
            }

            return retObj;
        }

        public async Task<T> CreateAsync<C>(C criteria)
        {
            Type type = typeof(T);
            T retObj = default(T);

            if (type.IsInterface || type.IsAbstract)
            {
                Task result = typeof(DataPortal).GetMethod("CreateAsync", new Type[] { typeof(object) })
                                                .MakeGenericMethod(_concreteType)
                                                .Invoke(null, new object[] { criteria }) as Task;
                await result;

                retObj = VerifyAsyncResultFromTask(result);
            }
            else
            {
                retObj = await DataPortal.CreateAsync<T>(criteria);
            }

            return retObj;
        }

        public async Task<T> CreateAsync()
        {
            Type type = typeof(T);
            T retObj = default(T);

            if (type.IsInterface || type.IsAbstract)
            {
                Task result = typeof(DataPortal).GetMethod("CreateAsync", Type.EmptyTypes)
                                                .MakeGenericMethod(_concreteType)
                                                .Invoke(null, null) as Task;

                await result;

                retObj = VerifyAsyncResultFromTask(result);
            }
            else
            {
                retObj = await DataPortal.CreateAsync<T>();
            }

            return retObj;
        }

        public void Delete(object criteria)
        {
            Type type = typeof(T);

            if (type.IsInterface || type.IsAbstract)
            {
                typeof(DataPortal).GetMethod("Delete", new Type[] { typeof(object) })
                                  .MakeGenericMethod(_concreteType)
                                  .Invoke(null, new object[] { criteria });
            }
            else
            {
                DataPortal.Delete<T>(criteria);
            }
        }

        public async Task DeleteAsync(object criteria)
        {
            Type type = typeof(T);

            if (type.IsInterface || type.IsAbstract)
            {
                Task asyncTask = (Task)typeof(DataPortal).GetMethod("DeleteAsync", new Type[] { typeof(object) })
                                                         .MakeGenericMethod(_concreteType)
                                                         .Invoke(null, new object[] { criteria });
                await asyncTask;

                if (asyncTask.Exception != null)
                {
                    throw asyncTask.Exception;
                }
            }
            else
            {
                await DataPortal.DeleteAsync<T>(criteria);
            }
        }

        public T Fetch()
        {
            Type type = typeof(T);
            T retObj = default(T);

            if (type.IsInterface || type.IsAbstract)
            {
                retObj = (T)typeof(DataPortal).GetMethod("Fetch", new Type[] { })
                                              .MakeGenericMethod(_concreteType)
                                              .Invoke(null, null);
            }
            else
            {
                retObj = DataPortal.Fetch<T>();
            }

            return retObj;
        }

        public T Fetch<C>(C criteria)
        {
            Type type = typeof(T);
            T retObj = default(T);

            if (type.IsInterface || type.IsAbstract)
            {
                retObj = (T)typeof(DataPortal).GetMethod("Fetch", new Type[] { typeof(object) })
                                              .MakeGenericMethod(_concreteType)
                                              .Invoke(null, new object[] { criteria });
            }
            else
            {
                retObj = DataPortal.Fetch<T>(criteria);
            }

            return retObj;
        }

        public async Task<T> FetchAsync(object criteria)
        {
            Type type = typeof(T);
            T retObj = default(T);

            if (type.IsInterface || type.IsAbstract)
            {
                Task asyncTask = typeof(DataPortal).GetMethod("FetchAsync", new Type[] { typeof(object) })
                                                   .MakeGenericMethod(_concreteType)
                                                   .Invoke(null, new object[] { criteria }) as Task;

                await asyncTask;

                retObj = VerifyAsyncResultFromTask(asyncTask);
            }
            else
            {
                retObj = await DataPortal.FetchAsync<T>(criteria);
            }

            return retObj;
        }

        public async Task<T> FetchAsync()
        {
            Type type = typeof(T);
            T retObj = default(T);

            if (type.IsInterface || type.IsAbstract)
            {
                Task asyncTsk = typeof(DataPortal).GetMethod("FetchAsync", Type.EmptyTypes)
                                                  .MakeGenericMethod(_concreteType)
                                                  .Invoke(null, null) as Task;

                await asyncTsk;

                retObj = VerifyAsyncResultFromTask(asyncTsk);
            }
            else
            {
                retObj = await DataPortal.FetchAsync<T>();
            }

            return retObj;
        }

        public T Update(T busObjInstance)
        {
            Type type = typeof(T);
            T retObj = default(T);

            if (type.IsInterface || type.IsAbstract)
            {
                retObj = (T)typeof(DataPortal).GetMethod("Update")
                                              .MakeGenericMethod(_concreteType)
                                              .Invoke(null, new object[] { busObjInstance });
            }
            else
            {
                retObj = DataPortal.Update<T>(busObjInstance);
            }

            return retObj;
        }

        public async Task<T> UpdateAsync(T busObjInstance)
        {
            Type type = typeof(T);
            T retObj = default(T);

            if (type.IsInterface || type.IsAbstract)
            {
                Task asyncTsk = typeof(DataPortal).GetMethod("UpdateAsync")
                                                  .MakeGenericMethod(_concreteType)
                                                  .Invoke(null, new object[] { busObjInstance }) as Task;

                await asyncTsk;

                retObj = VerifyAsyncResultFromTask(asyncTsk);
            }
            else
            {
                retObj = await DataPortal.UpdateAsync<T>(busObjInstance);
            }

            return retObj;
        }

        //** NOTE: we do not need to do the concrete type resolution on the 
        //Child DataPortal_XYZ methods because DataPortal_CreateChild - etc.. do not use the attributes for runlocal etc...
        public T CreateChild()
        {
            return DataPortal.CreateChild<T>();
        }

        //** NOTE: we do not need to do the concrete type resolution on the 
        //Child DataPortal_XYZ methods because DataPortal_CreateChild - etc.. do not use the attributes for runlocal etc...
        public T CreateChild<C>(C criteria)
        {
            return DataPortal.CreateChild<T>(criteria);
        }

        //** NOTE: we do not need to do the concrete type resolution on the 
        //Child DataPortal_XYZ methods because DataPortal_CreateChild - etc.. do not use the attributes for runlocal etc...
        public T FetchChild()
        {
            return DataPortal.FetchChild<T>();
        }

        //** NOTE: we do not need to do the concrete type resolution on the 
        //Child DataPortal_XYZ methods because DataPortal_CreateChild - etc.. do not use the attributes for runlocal etc...
        public T FetchChild<C>(C criteria)
        {
            return DataPortal.FetchChild<T>(criteria);
        }

        public void UpdateChild(T child)
        {
            DataPortal.UpdateChild(child);
        }

        public void UpdateChild<C>(T child, C criteria)
        {
            DataPortal.UpdateChild(child, criteria);
        }

        public ContextDictionary GlobalContext
        {
            get { return ApplicationContext.GlobalContext; }
        }

        #region Helper Methods

        //private Lazy<PropertyInfo> _taskResultProperty = new Lazy<PropertyInfo>(()=>
        //{
        //    PropertyInfo inf = typeof(Task<>).MakeGenericType(typeof(T)).GetProperty("Result");
        //    return inf;
        //});

        //private PropertyInfo AsyncResult { get { return _taskResultProperty.Value; } }
        private T VerifyAsyncResultFromTask(Task dpMethodAsyncTask)
        {
            if (dpMethodAsyncTask.Exception != null)
            {
                throw dpMethodAsyncTask.Exception;
            }
            else
            {
                return dpMethodAsyncTask.GetType().GetProperty("Result").GetValue(dpMethodAsyncTask) as T;
            }
        }

        #endregion
    }
}
