using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Devshorts.MonadicNull;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NoNulls.Tests.Extensions;
using NoNulls.Tests.SampleData;

namespace NoNulls.Tests.Tests
{
    [TestClass]
    public class TestSplits
    {
        [TestMethod]
        public void Split()
        {
            var chain = Option.CompileChain<User, Street>(u => u.School.District.Street);

            var split = Enumerable.Repeat(0, 1000)
                                       .Select(i => ExpressionTests.Getuser())
                                       .Select(chain)
                                       .Protect(item => item.ValidChain());

            Console.WriteLine("Successful {0}", split.Success.Count);

            Console.WriteLine("Failure {0}", split.Failure.Count);
        }
    }
    [TestClass]
    public class Test
    {
        [TestMethod]
        public void NestedChecksOnThisObject()
        {
            var x = Option.CompileChain<Test, Test>(item => item.Get().Get().Get().Get().Get())(this);

            Assert.IsFalse(x.ValidChain());
        }

        private Test Get()
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
            var usr = new User();

            var name = Option.CompileChain<User, int>(user => user.School.District.Street.Number)(usr);

            var v = name.Value;
        }

        [TestMethod]
        public void TestWithValueTypeTargetNull()
        {
            User user = null;

            var name = Option.CompileChain<User, int>(usr => usr.Number)(user);

            Assert.IsFalse(name.ValidChain());
        }

        [TestMethod]
        public void TestWithValueTypeTargetNullField()
        {            
            var name = Option.CompileChain<User, User>(usr => usr.Field.Field.Field.Field.Field)(null);

            Assert.IsFalse(name.ValidChain());
        }

        [TestMethod]
        public void TestWithValueTypeTarget()
        {
            var user = new User();

            user.Number = 0;

            var name = Option.CompileChain<User, int>(u => u.Number)(user);

            Assert.IsTrue(name.ValidChain());
        }

        [TestMethod]
        public void TestBasicNullWithValueTypeTarget()
        {
            var user = new User();

            var name = Option.CompileChain<User, int>(u => u.School.District.Street.Number)(user);

            Assert.IsFalse(name.ValidChain());
        }


        [TestMethod]
        public void TestGet()
        {
            var user = new User();

            var name = Option.CompileChain<User, string>(u => u.GetSchool().District.Street.Name)(user);

            Assert.IsFalse(name.ValidChain()); 
        }

        [TestMethod]
        public void TestGetSafeWithList()
        {
            var user = new User
                       {
                           School = new School()
                       };

            var name = Option.Safe(() => user.GetSchool().ClassMatesList[0].School.District.Street.Name);

            Assert.IsFalse(name.ValidChain());
        }

        [TestMethod]
        public void TestGetSafeWithListNonNullExtensions()
        {
            var user = new User
            {
                School = new School
                {
                    ClassMatesList = new List<User>
                                              {
                                                  new User
                                                  {
                                                      School = new School
                                                               {
                                                                    District   = new District
                                                                                 {
                                                                                     Street = new Street
                                                                                              {
                                                                                                  Name = "test"
                                                                                              }
                                                                                 }
                                                               }
                                                  }
                                              }
                }
            };

            var name = Option.Safe(() => user.GetSchool().ClassMatesList.First().School.District.Street.Name);

            Assert.IsTrue(name.ValidChain());
            Assert.AreEqual(name.Value, "test");
        }

        [TestMethod]
        public void TestGetSafeWithListNonNull()
        {
            var user = new User
            {
                School = new School
                         {
                             ClassMatesList = new List<User>
                                              {
                                                  new User
                                                  {
                                                      School = new School
                                                               {
                                                                    District   = new District
                                                                                 {
                                                                                     Street = new Street
                                                                                              {
                                                                                                  Name = "test"
                                                                                              }
                                                                                 }
                                                               }
                                                  }
                                              }
                         }
            };

            var name = Option.Safe(() => user.GetSchool().ClassMatesList[0].School.District.Street.Name);

            Assert.IsTrue(name.ValidChain());
            Assert.AreEqual(name.Value, "test");
        }

        [TestMethod]
        public void TestGetSafe()
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

            var name = Option.CompileChain<User, Street>(u => u.School.District.Street)(user);

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

            var name = Option.CompileChain<User, string>(u => u.GetSchool().GetDistrict().GetStreet().Name)(user);

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

            var name = Option.CompileChain<User, string>(u => u.GetSchool().GetDistrict().GetStreet().GetName(1))(user);

            Assert.AreEqual(name.Value, "foo1");
        }

        [TestMethod]
        public void TestNonNullsWithMethodCalls2()
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

            var name = Option.CompileChain<User, string>(u => u.GetSchool().GetDistrict().GetStreet().GetName(() => 1))(user);

            Assert.AreEqual(name.Value, "foo1");
        }

        [TestMethod]
        public void TestNonNullsWithMethodCalls3()
        {
            Func<int> action = () => 1;

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

            var name = Option.CompileChain<User, string>(u => u.GetSchool().GetDistrict().GetStreet().GetName(action))(user);

            Assert.AreEqual(name.Value, "foo1");
        }

        private static Random _random = new Random((int) DateTime.Now.Ticks);

        private static T Next<T>() where T: class, new()
        {
            return _random.Next(0, 2) == 0 ? null : (T)Activator.CreateInstance(typeof (T));
        }

        public static User Getuser()
        {
            var u = Next<User>();

            if (u != null)
            {
                u.School = Next<School>();

                if (u.School != null)
                {
                    u.School.District = Next<District>();

                    if (u.School.District != null)
                    {
                        u.School.District.Street = Next<Street>();
                    }
                }
            }

            return u;
        }

        [TestMethod]
        public void Benchmark()
        {
            Func<Street> nullChecks = () =>
            {
                var user = Getuser();

                if (user != null)
                {
                    if (user.School != null)
                    {
                        if (user.School.District != null)
                        {
                            if (user.School.District.Street != null)
                            {
                                return user.School.District.Street;
                            }
                        }
                    }
                }

                return null;
            };

            var rawNull = Time(nullChecks);

            Console.WriteLine("raw: {0}", rawNull);

            var chain = Option.CompileChain<User, Street>(u => u.School.District.Street);

            var chainedNull = Time(() =>
            {
                var user = Getuser();
                return chain(user);
            });

            Console.WriteLine("chained: {0}", chainedNull);

            Assert.IsTrue(true);
        }

        private double Time<T>(Func<T> action)
        {
            var stopWatch = new Stopwatch();

            var count = 10000;

            stopWatch.Start();

            for (int i = 0; i < count; i++)
            {
                action();
            }

            stopWatch.Stop();

            return stopWatch.ElapsedMilliseconds/(double)count;
        }
         
    }
}
