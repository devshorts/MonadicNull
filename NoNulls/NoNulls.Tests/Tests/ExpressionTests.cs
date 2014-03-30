using Devshorts.MonadicNull;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoNulls.Tests.SampleData;

namespace NoNulls.Tests.Tests
{
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void Foo()
        {
            var x = Option.Safe(() => Get().Get().Get());

            Assert.IsFalse(x.ValidChain());
        }

        public Test Get()
        {
            return null;
        }
    }

    [TestClass]
    internal  class ExpressionTests
    {
        [TestMethod]
        [ExpectedException(typeof(NoValueException))]
        public void ShouldThrow()
        {
            var user = new User();

            var name = Option.Safe(() => user.School.District.Street.Number);

            var v = name.Value;
        }

        [TestMethod]
        public void TestWithValueTypeTargetNull()
        {
            User user = null;

            var name = Option.Safe(() => user.Number);

            Assert.IsFalse(name.ValidChain());
        }

        [TestMethod]
        public void TestWithValueTypeTargetNullField()
        {
            User user = null;

            MethodValue<User> name = Option.Safe(() => user.Field.Field.Field.Field.Field);

            Assert.IsFalse(name.ValidChain());
        }

        [TestMethod]
        public void TestWithValueTypeTarget()
        {
            var user = new User();

            user.Number = 0;

            var name = Option.Safe(() => user.Number);

            Assert.IsTrue(name.ValidChain());
        }

        [TestMethod]
        public void TestBasicNullWithValueTypeTarget()
        {
            var user = new User();

            var name = Option.Safe(() => user.School.District.Street.Number);

            Assert.IsFalse(name.ValidChain());
        }


        [TestMethod]
        public void TestGet()
        {
            var user = new User();

            var name = Option.Safe(() => user.GetSchool().District.Street.Name);

            Assert.IsFalse(name.ValidChain()); 
        }


        [TestMethod]
        public void TestNullWithReferenceTypeTarget()
        {
            var user = new User
            {
                School = new School()
            };

            var name = Option.Safe(() => user.School.District.Street);

            Assert.IsFalse(name.ValidChain());
        }

        [TestMethod]
        public void TestNonNullWithMethods()
        {
            var user = new User
            {
                School = new School
                         {
                             District = new District
                                        {
                                            Street = new Street
                                                     {
                                                         Name = "foo"
                                                     }
                                        }
                         }
            };

            var name = Option.Safe(() => user.GetSchool().GetDistrict().GetStreet().Name);

            Assert.AreEqual(name.Value, "foo");
        }

        [TestMethod]
        public void TestNonNullsWithMethodCalls()
        {
            var user = new User
            {
                School = new School
                {
                    District = new District
                    {
                        Street = new Street
                                 {
                                     Name = "foo"
                                 }
                    }
                }
            };

            var name = Option.Safe(() => user.GetSchool().GetDistrict().GetStreet().GetName(1));

            Assert.AreEqual(name.Value, "foo1");
        }
    }
}
