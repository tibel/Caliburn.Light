---
layout: page
title: Using the Name Transformer
---

The NameTransformer was introduced in Caliburn.Micro v1.1 and is an integral part of how both the ViewLocator and ViewModelLocator map class names to their companion role. While you can override various funcs on these services to replace the underlying behavior, most of your needs should be met by simply configuring rules with the appropriate NameTransformer which describe your unique mapping strategies.

Name transformation is based on rules that use regular expression pattern-matching. When a transformation is executed, all registered rules are evaluated in sequence. By default, the resultant names yielded by all matching rules are returned by the NameTransformer. The ViewLocator and ViewModelLocator classes will use the resultant name list to check, in order, the existence of a matching type in the AssemblySource.Instance collection. Once a type is found, the remaining names in the list are ignored. While the locator classes will always return at most one type regardless of the number of names returned by the NameTransformer, it's important to be able to specify how the NameTransformer constructs that list of names to provide better control over which type will be located. The primary means of control is through sequence. Because the locator classes need to support some type naming conventions out-of-the-box, some default name transformation rules are automatically added. However, to be able to support custom rules and allow them to take precedence over the default rules, the NameTransformer evaluates the rules in reverse order from which they are added (LIFO). In general, you will want more-general rules to be evaluated after more-specific rules. Therefore, when adding rules to the NameTransfomer it is necessary to add the more-general rules first and the more-specific rules last. To limit the names returned by the NameTransformer to those yielded by the first matching rule, the UseEagerRuleSelection property on the NameTransformer can be set to false. By default, UseEagerRuleSelection is set to true.

Custom rules are added by calling the AddRule() method of the NameTransformer object maintained by the ViewLocator and ViewModelLocator classes. Both classes reference their own separate static instance of NameTransformer, so each class maintains its own set of rules.

The calling convention is as follows:

``` csharp
public void AddRule(string replacePattern, IEnumerable<string> replaceValueList, string globalFilterPattern = null)
```

 - replacePattern: a regular expression pattern for replacing all or parts of the input string
 - replaceValueList: a collection of strings to apply to the replacePattern
 - globalFilterPattern: a regular expression pattern used for determining if rule should be evaluated. Optional.

``` csharp
public void AddRule(string replacePattern, string replaceValue, string globalFilterPattern = null)
```

 - replacePattern: a regular expression pattern for replacing all or parts of the input string
 - replaceValue: a string to apply to the replacePattern
 - globalFilterPattern: a regular expression pattern used for determining if rule should be evaluated. Optional.

To show how this method is used, we can take a look at one of the built-in rules added by the ViewLocator class:

``` csharp
NameTransformer.AddRule("Model$", string.Empty);
```

This transformation rule looks for the substring "Model" terminating the ViewModel name and strips out that substring (i.e. replace with string.Empty or "null string"). The "$" in the first argument indicates that the pattern must match at the end of the source string. If "Model" exists anywhere else, the pattern is not matched. Because this call didn't include the optional "globalFilterPattern" argument, this rule applies to all ViewModel names.

This rule yields the following results:

| Input | Result |
|------------|------|
| MainViewModel | MainView |
| ModelAirplaneViewModel | ModelAirplaneView |
| CustomerViewModelBase | CustomerViewModelBase |

The corresponding built-in rule added by the ViewModelLocator is:

``` csharp
NameTransformer.AddRule(@"(?<fullname>^.*$)", @"${fullname}Model");
```

This rule takes whatever the input is and adds "Model" to the end. This rule uses a regular expression capture group, which is extremely useful in complex transformations. The "replacePattern" assigns the full name of the View to a capture group called "fullname" and the "replaceValue" transforms it into <fullname> "Model".

To demonstrate how the "globalFilterPattern" applies, we can take a look at two other built-in rules for the ViewModelLocator:

``` csharp
//Check for <Namespace>.<BaseName>View construct
NameTransformer.AddRule
    (
        @"(?<namespace>(.*\.)*)(?<basename>[A-Za-z_]\w*)(?<suffix>View$)",
        new[] {
            @"${namespace}${basename}ViewModel",
            @"${namespace}${basename}",
            @"${namespace}I${basename}ViewModel",
            @"${namespace}I${basename}"
        },
        @"(.*\.)*[A-Za-z_]\w*View$"
    );
 
//Check for <Namespace>.Views.<BaseName>View construct
NameTransformer.AddRule
    (
        @"(?<namespace>(.*\.)*)Views\.(?<basename>[A-Za-z_]\w*)(?<suffix>View$)",
        new[] {
            @"${namespace}ViewModels.${basename}ViewModel",
            @"${namespace}ViewModels.${basename}",
            @"${namespace}ViewModels.I${basename}ViewModel",
            @"${namespace}ViewModels.I${basename}"
        },
        @"(.*\.)*Views\.[A-Za-z_]\w*View$"
    );
```

The "globalFilterPattern" arguments for the two calls are identical with the exception of the addition of "Views\." to the argument in the second method call. This indicates that the rule should be applied only if the namespace name terminates with "Views." (dot inclusive). If the pattern is matched, the result is an array of ViewModel names with namespace terminating with "ViewModels.".

The first rule, which echoes back the original namespace unchanged, would cover all other cases. As mentioned earlier, the least specific rule is added first. It covers the fall-through case when the namespace doesn't end with "Views.".

When adding custom application-specific transformation rules, the following replace pattern should prove quite useful. The replace pattern takes a fully-qualified ViewModel name and breaks it into capture groups that should cover almost any transformation:

```
(?<nsfull>((?<nsroot>[A-Za-z_]\w*\.)(?<nsstem>([A-Za-z_]\w*\.)*))?(?<nsleaf>[A-Za-z_]\w*\.))?(?<basename>[A-Za-z_]\w*)(?<suffix>ViewModel$)
```

For example, adding the following rule:

``` csharp
ViewLocator.NameTransformer.AddRule
(
    @"(?<nsfull>((?<nsroot>[A-Za-z_]\w*\.)(?<nsstem>([A-Za-z_]\w*\.)*))?(?<nsleaf>[A-Za-z_]\w*\.))?(?<basename>[A-Za-z_]\w*)(?<suffix>ViewModel$)",
    @"nsfull=${nsfull}, nsroot=${nsroot}, nsstem=${nsstem}, nsleaf=${nsleaf}, basename=${basename}, suffix=${suffix}"
);
```

will yield the following results:

| Input | Output |
|------------|------|
| MainViewModel | nsfull=, nsroot=, nsstem=, nsleaf=, basename=Main, suffix=ViewModel |
| ViewModels.MainViewModel | nsfull=ViewModels., nsroot=, nsstem=, nsleaf=ViewModels., basename=Main, suffix=ViewModel |
| Root.ViewModels.MainViewModel | nsfull=Root.ViewModels., nsroot=Root., nsstem=, nsleaf=ViewModels., basename=Main, suffix=ViewModel |
| Root.Stem.ViewModels.MainViewModel | nsfull=Root.Stem.ViewModels., nsroot=Root., nsstem=Stem., nsleaf=ViewModels., basename=Main, suffix=ViewModel |

##### Notes:

The same replace pattern above can be used for the ViewModelLocator by changing the "ViewModel$" to "View$".

You wouldn't ever construct a replace value like in the example above since it would yield an illegal type name. It's merely a replace value that will echo back all of the capture groups for demonstration purposes.

You might notice that the capture groups are not mutually exclusive. Capture groups can be nested as in the example such that "nsfull" captures a full namespace while "nsroot", "nsstem", and "nsleaf" capture individual components of that namespace. You would use the individual components if you needed to "swap out" any one of the individual components.

The capture group "suffix" in the example above does a pattern match on names that end with "ViewModels". The main purpose of this capture group isn't so that it could be used as part of the transformation, since the purpose of the ViewLocator is to resolve a View name. The main reason to have this capture group is to prevent the substring "ViewModels" from being captured in the "basename" group, which in most cases would be part of the string transformation.