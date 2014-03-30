Devshorts.MonadicNull
====

Free yourself from the endless chains of if statements!

This is a monadic binder that leverages expression trees to let you evaluate long expression chains without fear of null references. 

Installation
====

Install version 0.1.1 via [Nuget](https://www.nuget.org/packages/Devshorts.MonadicNull/0.1.1)

```
> Install-Package Devshorts.MonadicNull
```

Usage
===
For example, here is a usage:

```csharp
[TestMethod]
public void TestWithValueTypeTargetNullField()
{
    User user = null;

    MethodValue<User> name = Option.Safe(() => user.Field.Field.Field.Field.Field);

    Assert.IsFalse(name.ValidChain());
}
```

The `MethodValue<T>` type contains several pieces of information:

1. Whether the chain is valid (i.e. the `user.Field.Field.Field...` succeeded without null's
2. What is the final result, via the `.Value` property (which throws a `NoValueException` if the chain was invalid
3. If the chain was invalid, which part of it was invalid. This is captured leveraging expression trees and will look something like this (depending on your base objects)


```csharp
[TestMethod]
public void TestGet()
{
    var user = new User();

    var name = Option.Safe(() => user.GetSchool().District.Street.Name);

    Console.WriteLine(name.Failure); 
}

```
```
"value(NoNulls.Tests.Tests.ExpressionTests+<>c__DisplayClassa).user.GetSchool()"
```

Caveats
====
The chain only works for method invocations and property/field accessors. You cannot do anything other than that or the expression tree parsing will fail.

Internals
====

Internally, the lambda is decomposed and iterated over and if checks are automatically built out. For the previous failure example, the underlying lambda after transformation actually looks like this:

```csharp
.Lambda #Lambda1<System.Func`1[Devshorts.MonadicNull.MethodValue`1[System.String]]>() {
    .Block() {
        .If (.Constant<NoNulls.Tests.Tests.ExpressionTests+<>c__DisplayClassa>(NoNulls.Tests.Tests.ExpressionTests+<>c__DisplayClassa).user ==
        null) {
            .New Devshorts.MonadicNull.MethodValue`1[System.String](
                null,
                "value(NoNulls.Tests.Tests.ExpressionTests+<>c__DisplayClassa).user",
                False)
        } .Else {
            .If (.Call (.Constant<NoNulls.Tests.Tests.ExpressionTests+<>c__DisplayClassa>(NoNulls.Tests.Tests.ExpressionTests+<>c__DisplayClassa).user).GetSchool()
            == null) {
                .New Devshorts.MonadicNull.MethodValue`1[System.String](
                    null,
                    "value(NoNulls.Tests.Tests.ExpressionTests+<>c__DisplayClassa).user.GetSchool()",
                    False)
            } .Else {
                .If ((.Call (.Constant<NoNulls.Tests.Tests.ExpressionTests+<>c__DisplayClassa>(NoNulls.Tests.Tests.ExpressionTests+<>c__DisplayClassa).user).GetSchool()
                ).District == null) {
                    .New Devshorts.MonadicNull.MethodValue`1[System.String](
                        null,
                        "value(NoNulls.Tests.Tests.ExpressionTests+<>c__DisplayClassa).user.GetSchool().District",
                        False)
                } .Else {
                    .If (((.Call (.Constant<NoNulls.Tests.Tests.ExpressionTests+<>c__DisplayClassa>(NoNulls.Tests.Tests.ExpressionTests+<>c__DisplayClassa).user).GetSchool()
                    ).District).Street == null) {
                        .New Devshorts.MonadicNull.MethodValue`1[System.String](
                            null,
                            "value(NoNulls.Tests.Tests.ExpressionTests+<>c__DisplayClassa).user.GetSchool().District.Street",
                            False)
                    } .Else {
                        .New Devshorts.MonadicNull.MethodValue`1[System.String](
                            (((.Call (.Constant<NoNulls.Tests.Tests.ExpressionTests+<>c__DisplayClassa>(NoNulls.Tests.Tests.ExpressionTests+<>c__DisplayClassa).user).GetSchool()
                            ).District).Street).Name,
                            "value(NoNulls.Tests.Tests.ExpressionTests+<>c__DisplayClassa).user.GetSchool().District.Street",
                            True)
                    }
                }
            }
        }
    }
}
```

s