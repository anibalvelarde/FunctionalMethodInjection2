using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csla.Rules;
using Autofac;
using Csla.Core;
using System.Reflection;

namespace ObjectPortal
{
    public abstract class BusinessRuleDIBase : Csla.Rules.BusinessRule
    {

        public BusinessRuleDIBase() : base()
        {
            ExecuteMethod = (rc) => Execute_(rc);
        }

        public BusinessRuleDIBase(IPropertyInfo pi) : base(pi)
        {
            ExecuteMethod = (rc) => Execute_(rc);
        }


        protected sealed override void Execute(RuleContext context)
        {
            base.Execute(context);

            var db = context.Target as IBusinessObjectScope;
            var scope = db.scope; // In the actual implementation we would not use a service locator. Limited by CSLA

            if(ExecuteMethodDI != null)
            {
                ExecuteMethodDI(context, scope);
            } else
            {
                ExecuteMethod(context);
            }

        }

        protected Action<RuleContext> ExecuteMethod { get; set; }
        protected Action<RuleContext, ILifetimeScope> ExecuteMethodDI { get; set; }

        protected virtual void Execute_(RuleContext context)
        {

        }

    }
}
