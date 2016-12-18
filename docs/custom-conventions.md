# Handling Custom Conventions

While the ViewLocator and ViewModelLocator classes were designed to support non-standard conventions by giving public access to each class' instance of NameTransformer, adding new name transformation rules based on regular expressions can be quite a daunting task for those unfamiliar with regular expression syntax. Furthermore, because the NameTransformer is designed to perform generic name transformations, it doesn't allow for the customization of name and namespace transformation independently of each other. In other words, there's no simple way of adding support for a custom View name suffix while maintaining the standard transformations for namespaces, and there's no simply way of changing namespace transformations while maintaining the standard transformations for type names.
In recognition of these limitations, we have added configurability and several high-level methods to each of the locator classes. These new features allow for building custom transformation rules for common use cases without requiring knowledge of regular expressions. In addition, the methods are domain-aware (i.e. they contemplate the concepts of both namespaces and type names) rather than being geared toward generic name transformation.

### Terminology

Before introducing these new methods of the locator classes, it would be appropriate to discuss terminology.

**Name transformation** is a general term used to describe how type resolution is done. A type's fully qualified name is taken as the source and then "transformed" into the name of the output type. At the lowest level, the NameTransfomer class is responsible for this action and performs the transformation using regular expression-based "transformation rules". 

**Type mapping** is a term to describe the new capabilities that were added to the locator classes. Creating a type mapping is considered a higher-level operation because type mappings contemplate the two facets of type resolution: resolving the type's name and resolving the type's namespace. Although a type mapping is ultimately expressed as a transformation rule for the NameTransformer, the methods for creating type mappings were intended to relieve you of having to understand regular expressions in addition to being more domain-specific.

### Configuration of Type Mappings

Both of the locator classes can be configured by calling the new ConfigureTypeMappings() method, which takes an instance of the TypeMappingConfiguration class as an argument.

#### TypeMappingConfiguration Class

This class has various properties whose values are used as global settings required by the locator classes for configuring how the various high-level type mapping methods behave.

##### Properties

 - DefaultSubNamespaceForViews: The subnamespace comprising the application's Views (i.e. "Views" in the namespace"MyProject.Views"). This value is used to create a default mapping with subnamespace for ViewModels. The default value is "Views".
 - DefaultSubNamespaceForViewModels: The subnamespace comprising the application's ViewModels (i.e. "ViewModels" in the namespace"MyProject.ViewModels"). This value is used to create a default mapping with subnamespace for Views. The default value is "ViewModels".
 - UseNameSuffixesInMappings: Flag indicating if mappings should account for name suffixes in the type's name to distinguish between Views and ViewModels. This value can be set false if Views and ViewModels can be distinguished by namespace or subnamespace.The default value is true.
 - NameFormat: Format string used to construct a type's name using a base name (or entity name) and the View or ViewModel suffix. The format items are as follows:
	 - {0}: base name
	 - {1}: name suffix
	 - As only two arguments will be used with the specified format string, NameFormat can contain any combination of the format items listed above but must not contain any more (i.e. {2}, {3}, etc.). The default value is "{0}{1}" (i.e. &lt;basename&gt;&lt;suffix&gt;).
 - IncludeViewSuffixInViewModelNames: Flag indicating if mappings should account for name suffixes like "Page" or "Form" as part of the name of the companion ViewModel (e.g. CustomerPageViewModel vs. CustomerViewModel or CustomerFormViewModel vs. CustomerViewModel). Note that by convention, regardless of this property's value, if the View suffix is part of the ViewModel suffix, the View suffix is assumed to be omitted (i.e. CustomerViewModel rather than CustomerViewViewModel). The default value is true.
 - ViewSuffixList: List of View suffixes for which to create default type mappings during configuration. The default values are "View" and "Page".
 - ViewModelSuffix: The ViewSuffix to use for all type mappings, both the default type mappings added during configuration as well as custom type mappings added after configuration. The default value is "ViewModel".

##### ViewLocator.ConfigureTypeMapping(), ViewModelLocator.ConfigureTypeMapping() Methods

This method configures or reconfigures how type mappings are added by the locator class. 

``` csharp
public static void ConfigureTypeMapping(TypeMappingConfiguration config)
```

The config parameter is the configuration object whose property values are used as settings to configure the locator class for type mapping.

This method is called internally by the locator class using the default property values of the TypeMappingConfiguration class.

Each time this method is called, the existing name transformation rules are cleared and new default type mappings are added automatically.

The settings of the configuration object are applied globally to the default type mappings that are automatically added at configuration and to any type mappings added after configuration. For instance, if the NameFormat is customized to specify a naming convention that places name suffixes before the base name, thereby turning the name suffixes to prefixes (e.g. ViewCustomer and ViewModelCustomer), this convention will be used as the standard type naming convention for any subsequent calls to methods that add a type mapping.

``` csharp
//Override the default subnamespaces
var config = new TypeMappingConfiguration
{
    DefaultSubNamespaceForViewModels = "MyViewModels",
    DefaultSubNamespaceForViews = "MyViews"
};
 
ViewLocator.ConfigureTypeMappings(config);
ViewModelLocator.ConfigureTypeMappings(config);
 
//Resolves:
//MyProject.MyViewModels.CustomerViewModel -> MyProject.MyViews.CustomerView
//MyProject.MyViewModels.CustomerPageViewModel -> MyProject.MyViews.CustomerPage
//MyProject.MyViews.CustomerView -> MyProject.MyViewModels.CustomerViewModel
//MyProject.MyViews.CustomerPage -> MyProject.MyViewModels.CustomerPageViewModel

//Change ViewModel naming convention to always exclude View suffix
var config = new TypeMappingConfiguration
{
    DefaultSubNamespaceForViewModels = "MyViewModels",
    DefaultSubNamespaceForViews = "MyViews",
    IncludeViewSuffixInViewModelNames = false
};
 
ViewLocator.ConfigureTypeMappings(config);
ViewModelLocator.ConfigureTypeMappings(config);
 
//Resolves:
//MyProject.MyViewModels.CustomerViewModel -> MyProject.MyViews.CustomerPage, MyProject.MyViews.CustomerView
//MyProject.MyViews.CustomerView -> MyProject.MyViewModels.CustomerViewModel
//MyProject.MyViews.CustomerPage -> MyProject.MyViewModels.CustomerViewModel

//Change naming conventions to place name suffixes before the base name (i.e. name prefix)
var config = new TypeMappingConfiguration
{
    NameFormat = "{1}{0}",
    IncludeViewSuffixInViewModelNames = false
};
 
ViewLocator.ConfigureTypeMappings(config);
ViewModelLocator.ConfigureTypeMappings(config);
 
//Resolves:
//MyProject.ViewModels.ViewModelCustomer -> MyProject.Views.PageCustomer, MyProject.Views.ViewCustomer
//MyProject.Views.ViewCustomer -> MyProject.ViewModels.ViewModelCustomer
//MyProject.Views.PageCustomer -> MyProject.ViewModels.ViewModelCustomer

//Change naming conventions to omit name suffixes altogether (i.e. distinguish View and ViewModel types by namespace alone)
var config = new TypeMappingConfiguration
{
    UseNameSuffixesInMappings = false
};
 
ViewLocator.ConfigureTypeMappings(config);
ViewModelLocator.ConfigureTypeMappings(config);
 
//Resolves:
//MyProject.ViewModels.Customer -> MyProject.Views.Customer
//MyProject.Views.Customer -> MyProject.ViewModels.Customer

//Configure for Spanish language and semantics
var config = new TypeMappingConfiguration()
{
    DefaultSubNamespaceForViewModels = "ModelosDeVistas",
    DefaultSubNamespaceForViews = "Vistas",
    ViewModelSuffix = "ModeloDeVista",
    ViewSuffixList = new List<string>(new string[] { "Vista", "Pagina" }),
    NameFormat = "{1}{0}",
    IncludeViewSuffixInViewModelNames = false
};
 
ViewLocator.ConfigureTypeMappings(config);
ViewModelLocator.ConfigureTypeMappings(config);
 
//Resolves:
//MiProyecto.ModelosDeVistas.ModeloDeVistaCliente -> MiProyecto.Vistas.VistaCliente, MiProyecto.Vistas.PaginaCliente
//MiProyecto.Vistas.VistaCliente -> MiProyecto.ModelosDeVistas.ModeloDeVistaCliente
//MiProyecto.Vistas.PaginaCliente -> MiProyecto.ModelosDeVistas.ModeloDeVistaCliente
```

### New Type Mapping Methods

##### ViewLocator.AddDefaultTypeMapping(), ViewModelLocator.AddDefaultTypeMapping()

This method is used to add a type mapping that supports the standard type and namespace naming conventions for a given View name suffix.

``` csharp
public static void AddDefaultTypeMapping(string viewSuffix = "View")
```

The viewSuffix parameter is the suffix for type name. Should be "View" or synonym of "View". (Optional)

This method is primarily used to add support of types that have custom synonyms (e.g. "Form", "Screen", "Tab") but otherwise use the standard naming conventions.

While the viewSuffix argument is optional, defaulting to "View", it is unnecessary to call this method in such a way since the locator classes already add type mappings for both the "View" and "Page" View name suffixes, although this might not be the case if the locator classes were configured differently using the ConfigureTypeMappings() method and modifying ViewSuffixList property of the TypeMappingConfiguration object. However, modifying the ViewSuffixList property of the configuration object and reconfiguring the locator class would obviate the need to call this method after the fact.

Keep in mind that this method does not add any type mappings if the UseNameSuffixesInMappings property of the configuration object is set to false. When this is the case, there is no default type naming convention for which to add mappings.

``` csharp
//Add support for "Form" as a synonym of "View" using the standard type and namespace naming conventions
ViewLocator.AddDefaultTypeMapping("Form");
//Resolves: MyProject.ViewModels.MainFormViewModel -> MyProject.Views.MainForm


ViewModelLocator.AddDefaultTypeMapping("Form");
//Resolves: MyProject.Views.MainForm -> MyProject.ViewModels.MainFormViewModel
```

##### ViewLocator.RegisterViewSuffix()

This method is used to indicate to the ViewLocator that a transformation rule to support a custom View suffix was added using the NameTransformer.AddRule(). This method is automatically called internally by the ViewLocator class when the AddNamespaceMapping(), AddTypeMapping() and the AddDefaultTypeMapping() type mapping methods are called.

``` csharp
public static void RegisterViewSuffix(string viewSuffix)
```

The viewSuffix parameter is the suffix for type name. Should be "View" or synonym of "View". (Optional)

In order for multi-View support to work properly, the ViewLocator needs to track all of the View suffixes that an application may use. Although this is managed automatically when name transformation rules are added using the new type mapping methods, transformation rules that are added directly through the NameTransformer instance of the ViewLocator class will bypass this registration step. Therefore, the ViewLocator.RegisterViewSuffix() will need to be called in such cases. The method will perform a check for existing entries prior to adding new ones.

``` csharp
//Manually add a rule to the NameTransformer to do simple text replacement independent of namespace
ViewLocator.NameTransformer.AddRule("ScreenViewModel$", "Screen");
//However, we need to treat "Screen" as a synonym of "View" somehow in order to
//enable multi-view support for View types with names ending in "Screen"
ViewLocator.RegisterViewSuffix("Screen");
//Resolves: MyProject.ViewModels.CustomerScreenViewModel -> MyProject.Views.Customer.Master
//when the context is "Master"
```

##### ViewLocator.AddNamespaceMapping(), ViewModelLocator.AddNamespaceMapping()

This method is used to add a type mapping between a source namespace and one or more target namespaces . The resultant type mapping creates a transformation rule supporting the standard type naming conventions but with a custom namespace naming convention. Optionally, a custom View suffix may be specified for this mapping.

``` csharp
public static void AddNamespaceMapping(string nsSource, string[] nsTargets, string viewSuffix = "View")
```

 - nsSource: Namespace of source type
 - nsTargets: Namespaces of target type as an array
 - viewSuffix: Suffix for type name. Should be "View" or synonym of "View". (Optional)

``` csharp
public static void AddNamespaceMapping(string nsSource, string nsTarget, string viewSuffix = "View")
```

 - nsSource: Namespace of source type
 - nsTarget: Namespace of target type
 - viewSuffix: Suffix for type name. Should be "View" or synonym of "View". (Optional)

This method supports the use of wildcards (indicated with *) in the nsSource argument. When a null string (or String.Empty) is used for the nsSource argument, the namespace(s) passed as the nsTarget/nsTargets argument will be appended to the namespace of the source type. Refer to the example for more details.

An array can be passed as an argument for the target namespaces to indicate that the target type could exist in multiple namespaces ("one-to-many" mapping). Because the locator classes are designed to pick up the first occurrence of a type that matches the name transformation rule, it does not matter if a type doesn't actually exist in one of the target namespaces or if there exist several types in different namespaces that share the same name. One possible use case for this mechanism would be to map a ViewModel namespace to an assembly for custom Views and to another assembly for standard Views. If the assembly for custom Views doesn't exist or if the particular View doesn't exist in the custom View assembly, the ViewLocator would pick up the View from the standard View assembly.

``` csharp
//"Append target to source" mapping
//Null string or String.Empty passed as source namespace is special case to allowe this
//Note the need to prepend target namespace with "." to make this work
ViewLocator.AddNamespaceMapping("", ".Views");
//Resolves: MyProject.Customers.CustomerViewModel -> MyProject.Customers.Views.CustomerView
 
//Standard explicit namespace mapping
ViewLocator.AddNamespaceMapping("MyProject.ViewModels.Customers", "MyClient1.Views");
//Resolves: MyProject.ViewModels.CustomerViewModel -> MyClient1.Views.CustomerView
 
//One to many explicit namespace mapping
ViewLocator.AddNamespaceMapping("MyProject.ViewModels.Customers", new string[] { "MyClient1.Views", "MyProject.Views" } );
//Resolves: MyProject.ViewModels.CustomerViewModel -> {MyClient1.Views.CustomerView, MyProject.Views.CustomerView }
 
//Wildcard mapping
ViewLocator.AddNamespaceMapping("*.ViewModels.Customers.*", "MyClient1.Customers.Views");
//Resolves: MyProject.ViewModels.Customers.CustomerViewModel -> MyClient1.Customers.Views.CustomerView
//          MyProject.More.ViewModels.Customers.MasterViewModel -> MyClient1.Customers.Views.MasterView
//          MyProject.ViewModels.Customers.More.OrderHistoryViewModel -> MyClient1.Customers.Views.OrderHistoryView 
```

##### ViewLocator.AddSubNamespaceMapping(), ViewModelLocator.AddSubNamespaceMapping()

This method is used to add a type mapping between a source namespace and one or more target namespaces by substituting a given subnamespace for another. The resultant type mapping creates a transformation rule supporting the standard type naming conventions but with a custom namespace naming convention. Optionally, a custom View suffix may be specified for this mapping.

``` csharp
public static void AddSubNamespaceMapping(string nsSource, string[] nsTargets, string viewSuffix = "View")
```

 - nsSource: Subnamespace of source type
 - nsTargets: Subnamespaces of target type as an array
 - viewSuffix: Suffix for type name. Should be "View" or synonym of "View". (Optional)

``` csharp
public static void AddSubNamespaceMapping(string nsSource, string nsTarget, string viewSuffix = "View")
```

 - nsSource: Subnamespace of source type
 - nsTarget: Subnamespace of target type
 - viewSuffix: Suffix for type name. Should be "View" or synonym of "View". (Optional)

This method supports the use of a wildcards (indicated with *) in the nsSource argument. When a null string (or String.Empty) is used for the nsSource argument, the namespace(s) passed as the nsTarget/nsTargets argument will be appended to the namespace of the source type. When nsSource is the null string or if it begins and terminates with a wild-card, the behavior is identical to that of AddNamespaceMapping(). As with AddNamespaceMapping(), the AddSubNamespaceMapping() method supports one-to-many mapping. Refer to the notes on AddNamespaceMapping() for an explanation.

This method is called internally at configuration for each View suffix in the ViewSuffixList of the configuration object. The subnames specified by DefaultSubNamespaceForViews and DefaultSubNamespaceForViewModels are used for the mapping. If the default mapping between the "Views" and "ViewModels" subnamespaces is not required, the appropriate configuration settings can be used to eliminate the need to call AddSubNamespaceMapping() directly.

``` csharp
//Add support for Spanish namespaces
ViewLocator.AddSubNamespaceMapping("ModelosDeVistas", "Vistas");
//Resolves: MiProyecto.ModelosDeVistas.Clientes.ClienteViewModel -> MiProyecto.Vistas.Clientes.ClienteView
 
ViewModelLocator.AddSubNamespaceMapping("Vistas", "ModelosDeVistas");
//Resolves: MiProyecto.Vistas.Clientes.ClienteView -> MiProyecto.ModelosDeVistas.Clientes.ClienteViewModel
 
//Wildcard subnamespace mapping
ViewLocator.AddSubNamespaceMapping("*.ViewModels", "ExtLib.Views");
//Resolves: MyCompany.MyApp.SomeNamespace.ViewModels.CustomerViewModel -> ExtLib.Views.CustomerView
 
ViewLocator.AddSubNamespaceMapping("ViewModels.*", "Views");
//Resolves: MyApp.ViewModels.Some.Name.Space.CustomerViewModel -> MyApp.Views.CustomerView
 
ViewLocator.AddSubNamespaceMapping("MyApp.*.ViewModels", "ExtLib.Views");
//Resolves: MyCompany.MyApp.SomeNamespace.ViewModels.CustomerViewModel -> MyCompany.ExtLib.Views.CustomerView
```

##### ViewLocator.AddTypeMapping(), ViewModelLocator.AddTypeMapping()

This method is used to add a type mapping expressed as regular expression-based transformations. The resultant type mapping creates a transformation rule supporting the standard type naming conventions but with a custom namespace naming convention. Optionally, a custom View suffix may be specified for this mapping.

``` csharp
public static void AddTypeMapping(string nsSourceReplaceRegEx, string nsSourceFilterRegEx, string[] nsTargetsRegEx, string viewSuffix = "View")
```

 - nsSourceReplaceRegEx: RegEx replace pattern for source namespace
 - nsSourceFilterRegEx: RegEx filter pattern for source namespace
 - nsTargetsRegEx: Array of RegEx replace values for target namespaces
 - viewSuffix: Suffix for type name. Should be "View" or synonym of "View". (Optional)

``` csharp
public static void AddTypeMapping(string nsSourceReplaceRegEx, string nsSourceFilterRegEx, string nsTargetRegEx, string viewSuffix = "View")
```

 - nsSourceReplaceRegEx: RegEx replace pattern for source namespace
 - nsSourceFilterRegEx: RegEx filter pattern for source namespace
 - nsTargetsRegEx: RegEx replace value for target namespace
 - viewSuffix: Suffix for type name. Should be "View" or synonym of "View". (Optional)

Refer to the documentation on the NameTransformer for details on creating regular expression-based transformation rules. Unlike adding a transformation rule through the NameTransformer class, this method separates out the namespace transformations from the type name transformation. In addition, it supports one-to-many namespace mapping. Refer to the notes on AddNamespaceMapping() for an explanation.

``` csharp
//Capture namespace fragment preceding "ViewModels." as "nsbefore"
string subns = RegExHelper.NamespaceToRegEx("ViewModels.");
string rxrep = RegExHelper.GetNamespaceCaptureGroup("nsbefore") + subns;
            
//Output the namespace fragment after "Views." in the target namespace
string rxtgt = @"Views.${nsbefore}";
ViewLocator.AddTypeMapping(rxrep, null, rxtgt);
//Resolves: MyApp.Some.Name.Space.ViewModels.TestViewModel -> Views.MyApp.Some.Name.Space.TestView
```