//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ObjectPortal
//{

//    public class FunctionalStatic
//    {
//        public static List<Action<FunctionalStatic>> Functions()
//        {
//            var l = new List<Action<FunctionalStatic>>();

//            l.Add((FunctionalStatic i) => i.Create()) ;
//            l.Add((FunctionalStatic i, object dal) => i.Fetch(dal));

//            return l;

//        }

//        Guid _create;
//        Guid _fetch;

//        public void Create()
//        {
//            _create = Guid.NewGuid();
//        }

//        public void Fetch(object dal)
//        {
//            _fetch = Guid.NewGuid();
//        }

//    }
    
//    [TestClass]
//    public class FunctionalStaticTest
//    {

//        [TestMethod]
//        public void CallFunctions()
//        {
//            var a = new FunctionalStatic();
//            var b = new FunctionalStatic();

//            var l = FunctionalStatic.Functions();

//            l.ForEach(f => f(a));
//            l.ForEach(f => f(b));


//        }

//    }
//}
