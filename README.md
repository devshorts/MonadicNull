Devshorts.MonadicNull
====

Do you hate writing this?

```csharp
if (user != null)
{
    if (user.School != null)
    {
        if (user.School.District != null)
        {
            return user.School.District.Street;
        }

		log.debug("user.school.disctrict is null");
		return null;
    }

	log.debug("user.school is null");
	return null;
}

log.debug("user is null");
return null;
```

Because I do.  

Can't wait till C# 6 for the `?.` operator? Me neither.  

Until then, wouldn't it be nice if you could do this?

```csharp
var result = Option.Safe(() => user.School.District.Street);

if(result.HasValue()){
   // do stuff with result.Value
}
else{
  Log.error("Null found at {0}", result.Failure);
}
```

This is a monadic binder that leverages expression trees to let you evaluate long expression chains without fear of null references.  There are a lot of ways to do this, but the biggest complaint people have is not knowing *what* is null in the chain if the chain fails. 

This project solves not only the long nested if statement problem (using a "monadic" null shortciruit mechanism) but also gives you metadata about the chain! Now you can know

1. If there is a value
2. If there isn't a value, what in the chain failed with a null

And you will be guaranteed to get a non-null result from the chain, which is a wrapped object that gives you the target metadata you want.  

Installation
====

Install version 0.2.1 via [Nuget](https://www.nuget.org/packages/Devshorts.MonadicNull/0.2.1)

```
> Install-Package Devshorts.MonadicNull
```

Usage
=== 

For example, here is a basic usage:

```csharp
[TestMethod]
public void TestWithValueTypeTargetNullField()
{
    User user = null;

    MethodValue<User> field = Option.Safe(() => user.Field.Field.Field.Field.Field);

    Assert.IsFalse(field.ValidChain());
}
```

The `MethodValue<T>` type contains several pieces of information:

1. Whether the chain is valid (i.e. the `user.Field.Field.Field...` succeeded without null's
2. What is the final result, via the `.Value` property (which throws a `NoValueException` if the chain was invalid
3. If the chain was invalid, which part of it was invalid. This is captured leveraging expression trees and will look something like this (depending on your base objects)

Lets look at a more complicated example:

```csharp
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
```
                      
Precompiling for performance
====
                            
If you need to run the if check a bunch of times, precompile the expression. The expression now takes an argument:

```csharp
[TestMethod]
public void TestGet()
{
    var user = new User();

    var name = Option.CompileChain(u => u.GetSchool().District.Street.Name)(user);

    Console.WriteLine(name.Failure); 
}
```
                                                                                                                  
Since this chain failed, we can print the failure:

```
"value(NoNulls.Tests.Tests.ExpressionTests+<>c__DisplayClassa).user.GetSchool()"
```

A fun example
===

Let's say there is an IEnumerable extension method that collects failed items into a list, and successful items into a list

```csharp
public class Split<T>
{
    public IList<T> Success { get; private set; }
    public IList<T> Failure { get; private set; }

    public Split(IList<T> success, IList<T> failure)
    {
        Success = success;
        Failure = failure;
    }
}

public static class Extensions
{
    public static Split<T> Protect<T>(this IEnumerable<T> source, Func<T, bool> predicate)
    {
        var split = new Split<T>(new List<T>(), new List<T>());

        foreach (var item in source)
        {
            if (predicate(item))
            {
                split.Success.Add(item);
            }
            else
            {
                split.Failure.Add(item);
            }
        }

        return split;
    } 
}
```

And a random user object generator

```csharp
private static Random _random = new Random((int) DateTime.Now.Ticks);

private static T Next<T>() where T: class, new()
{
    return _random.Next(0, 2) == 0 ? null : (T)Activator.CreateInstance(typeof (T));
}

public static User GetUser()
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
```

Let's safely collect all failures and all successes

```csharp
[TestMethod]
public void Split()
{
    var chain = Option.CompileChain<User, Street>(u => u.School.District.Street);

    var split = Enumerable.Repeat(0, 1000)
                               .Select(i => GetUser())
                               .Select(chain)
                               .Protect(item => item.ValidChain());

    Console.WriteLine("Successful {0}", split.Success.Count);

    Console.WriteLine("Failure {0}", split.Failure.Count);
}
```
Performance
====

A question was raised whether this is slower than regular null checks. The answer is not really. Because the expression is precompiled (you can re-use it) many times over, so you only pay an expression tree compilation cost once.  If you are going to check on things in a loop, precompile it and run it. If you are only going to run the check once, feel free to use the basic `Safe` method.

In initial benchmarks precompiled expression monadic null took the same amount of time as regular if checks.

Caveats
====
The chain only works for method invocations, property/field accessors, or list indexing.

You cannot do anything other than that or the expression tree parsing will fail.

Internals
====

Internally, the lambda is decomposed and transformed to an expression where the if checks are automatically built out. For the previous failure example, the underlying lambda after transformation actually looks like this:

```csharp
.Lambda #Lambda1<System.Func`2[NoNulls.Tests.SampleData.User,Devshorts.MonadicNull.MethodValue`1[System.String]]>(NoNulls.Tests.SampleData.User $u)
{
    .Block() {
        .Block(NoNulls.Tests.SampleData.User $var1) {
            $var1 = $u;
            .If ($var1 == null) {
                .New Devshorts.MonadicNull.MethodValue`1[System.String](
                    null,
                    "u",
                    False)
            } .Else {
                .Block(NoNulls.Tests.SampleData.School $var2) {
                    $var2 = .Call $var1.GetSchool();
                    .If ($var2 == null) {
                        .New Devshorts.MonadicNull.MethodValue`1[System.String](
                            null,
                            "u.GetSchool()",
                            False)
                    } .Else {
                        .Block(NoNulls.Tests.SampleData.District $var3) {
                            $var3 = $var2.District;
                            .If ($var3 == null) {
                                .New Devshorts.MonadicNull.MethodValue`1[System.String](
                                    null,
                                    "u.GetSchool().District",
                                    False)
                            } .Else {
                                .Block(NoNulls.Tests.SampleData.Street $var4) {
                                    $var4 = $var3.Street;
                                    .If ($var4 == null) {
                                        .New Devshorts.MonadicNull.MethodValue`1[System.String](
                                            null,
                                            "u.GetSchool().District.Street",
                                            False)
                                    } .Else {
                                        .Block(System.String $var5) {
                                            $var5 = $var4.Name;
                                            .New Devshorts.MonadicNull.MethodValue`1[System.String](
                                                $var5,
                                                "u.GetSchool().District.Street.Name",
                                                True)
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
```

