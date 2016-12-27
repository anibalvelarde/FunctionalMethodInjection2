//using Autofac;
//using Csla.Core;
//using Csla.Server;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ObjectPortal
//{
//    public static class DataPortalContextExtensions
//    {

//        private static string key = $"Scope_{Guid.NewGuid().ToString()}";

//        public static void AddScope(this DataPortalContext context, ILifetimeScope scope)
//        {

//            var clientContextProperty =
//                context.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
//                .Where(x => x.Name == "ClientContext").SingleOrDefault();

//            var clientContext = (ContextDictionary)clientContextProperty.GetValue(context);

//            if(clientContext == null)
//            {
//                clientContext = new ContextDictionary();
//                clientContextProperty.SetValue(context, clientContext);
//            }

//            clientContext.Add(key, scope);

//        }

//        public static ILifetimeScope GetScope(this DataPortalContext context)
//        {
//            var clientContextProperty =
//                context.GetType().GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
//                .Where(x => x.Name == "ClientContext").SingleOrDefault();

//            var clientContext = (ContextDictionary)clientContextProperty.GetValue(context);

//            if (clientContext == null || !clientContext.Contains(key))
//            {
//                return null;
//            }

//            return (ILifetimeScope) clientContext[key];
//        }

//    }
//}
