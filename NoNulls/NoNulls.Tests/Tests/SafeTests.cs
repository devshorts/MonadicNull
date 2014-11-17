using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using Devshorts.MonadicNull;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoNulls.Tests.Extensions;
using NoNulls.Tests.SampleData;

namespace NoNulls.Tests.Tests
{
    [TestClass]
    public class SafeTests
    {
        [TestMethod]
        public void TestFuncInAnotherClass()
        {
            var user = new User();

            var containerClass = new UserAuditorThatTakesAFunc();
            containerClass.GetAPropertyOfUserToAudit = x => x.School.ToString();

            // This throws an "Expression cannot be invoked exception"
            var name = Option.Safe(() => containerClass.GetAPropertyOfUserToAudit(user));

            Assert.IsFalse(name.ValidChain());
        }

        [TestMethod]
        public void TestExpressionInAnotherClass()
        {
            var user = new User();

            var containerClass = new UserAuditorThatTakesAnExpression();
            containerClass.GetAPropertyOfUserToAudit = x => x.School.ToString();

            // This throws a null reference exception
            var name = Option.Safe(() => containerClass.GetAPropertyOfUserToAudit.Compile().Invoke(user));

            Assert.IsFalse(name.ValidChain());
        }
    }

    public class UserAuditorThatTakesAFunc
    {
        public Func<User, string> GetAPropertyOfUserToAudit { get; set; } 
    }

    public class UserAuditorThatTakesAnExpression
    {
        public Expression<Func<User, string>> GetAPropertyOfUserToAudit { get; set; } 
    }
}
