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
            if (user.School.District.Street != null)
            {
                return user.School.District.Street;
            }
        }
    }
}

return null;
```

Because I do.  

Wouldn't it be nicer if you could do this?

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

Install version 0.2.0 via [Nuget](https://www.nuget.org/packages/Devshorts.MonadicNull/0.2.0)

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

Performance
====

A question was raised whether this is slower than regular null checks. The answer is not really. Because the expression is precompiled (you can re-use it) many times over, so you only pay an expression tree compilation cost once.  If you are going to check on things in a loop, precompile it and run it. If you are only going to run the check once, feel free to use the basic `Safe` method.

In initial benchmarks precompiled expression monadic null took the same amount of time as regular if checks.

Caveats
====
The chain only works for method invocations and property/field accessors. You cannot do anything other than that or the expression tree parsing will fail.

Internals
====

Internally, the lambda is decomposed and transformed to an expression where the if checks are automatically built out. For the previous failure example, the underlying lambda after transformation actually looks like this:

```csharp
.Lambda #Lambda1<System.Func`2[NoNulls.Tests.SampleData.User,Devshorts.MonadicNull.MethodValue`1[NoNulls.Tests.SampleData.Street]]>(NoNulls.Tests.SampleData.User $u)
{
    .Block() {
        .If ($u == null) {
            .New Devshorts.MonadicNull.MethodValue`1[NoNulls.Tests.SampleData.Street](
                null,
                "u",
                False)
        } .Else {
            .If ($u.School == null) {
                .New Devshorts.MonadicNull.MethodValue`1[NoNulls.Tests.SampleData.Street](
                    null,
                    "u.School",
                    False)
            } .Else {
                .If (($u.School).District == null) {
                    .New Devshorts.MonadicNull.MethodValue`1[NoNulls.Tests.SampleData.Street](
                        null,
                        "u.School.District",
                        False)
                } .Else {
                    .New Devshorts.MonadicNull.MethodValue`1[NoNulls.Tests.SampleData.Street](
                        (($u.School).District).Street,
                        "u.School.District",
                        True)
                }
            }
        }
    }
}
```

