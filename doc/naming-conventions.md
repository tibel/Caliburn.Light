---
layout: page
title: View / View Model Naming Conventions
---

After receiving feedback about View and ViewModel resolution in Caliburn Micro, we have added new capabilities to make type resolution simpler while maintaining the robust regular expression-based name transformation mechanism that drives it. To get a better understanding about these new capabilities and how type resolution, in general, works in the framework, it's an appropriate time to describe in detail the naming conventions that the framework supports out-of-the-box. As you should already know by now, the framework depends considerably on naming conventions, and in type resolution specifically there are two different naming conventions to consider: the convention for naming the type itself and the convention for naming the namespace of the type.

### Naming Conventions for Name of a Type

As mentioned briefly in other areas of the documentation, the most common naming convention for a View and its companion ViewModel can be described as follows:

| &nbsp; | View Model | View |
|-|------------|------|
| Convention | &lt;EntityName&gt;ViewModel | &lt;EntityName&gt;View |
| Example 1 | ShellViewModel | ShellView |
| Example 2 | TabViewModel | TabView |

Because we recognize that "View" is an abstract term and that the primary "View" of most applications is actually some sort "Page", we believe that it's important for the framework to consider "Page" as a synonym for "View". Therefore, the framework has built-in support for this use case:

| &nbsp; | View Model | View |
|-|------------|------|
| Convention | &lt;EntityName&gt;PageViewModel | &lt;EntityName&gt;Page |
| Example 1 | MainPageViewModel | MainPage |
| Example 2 | OrderPageViewModel | OrderPage |

If you examine closely, you'll see that there is a subtle difference between the two conventions above. "ViewModel" is simply added to a "Page"-suffixed name to yield the name of its ViewModel. However, only "Model" is added to a "View"-suffixed name to yield the name of its companion ViewModel. This difference primarily stems from the semantic awkwardness of naming something "MainViewViewModel" as opposed to "MainPageViewModel". Therefore, the naming convention for ViewModels derived from "View"-suffixed View names avoids the redundancy by naming the ViewModel as "MainViewModel".

One limitation of the standard naming conventions supported by the framework is that there isn't consideration for different languages or even different terminologies within English. Although "View" and "ViewModel" can be assumed to be universally understood, as they are both important aspects of the MVVM design pattern that Caliburn Micro is dedicated to, a word like "Page" is not. Therefore, a robust framework would at least allow for supporting additional "View name suffixes" (e.g. "Pagina", "Seite", "Form", "Screen") through customization.

### Naming Convention for Multi-View Support

As mentioned in the Conventions section of the documentation, the framework was designed to handle a one-to-many relationship between ViewModel and View. The standard convention supported by the framework is as follows:

| &nbsp; | View Model | View |
|-|------------|------|
| Convention | &lt;EntityName&gt;[&lt;ViewSuffix&gt;]ViewModel | &lt;EntityName&gt;.&lt;Context&gt; |
| Example 1 | TabViewModel | Tab.First |
| Example 2 | CustomerViewModel | Customer.Master |

As explained in the previous section, the ViewModel's name may or may not include a "View" suffix. That is why the <ViewSuffix> is indicated as optional.

### Naming Conventions for Namespace of a Type

In .NET development, all assemblies must have a single default namespace. So the most basic use case has both the View and ViewModel component layers residing together in the same one. This convention can be described as follows:

| &nbsp; | View Model | View |
|-|------------|------|
| Convention | &lt;RootNS&gt;.&lt;ViewModelTypeName&gt; | &lt;RootNS&gt;.&lt;ViewTypeName&gt; |
| Example 1 | MyProject.ShellViewModel | MyProject.ShellView |
| Example 2 | MyProject.MainPageViewModel | MyProject.MainPage |

While all the Views and ViewModels of many applications may reside in a single assembly, it's common practice to organize Views and ViewModels in separate folders within a project. As a consequence, Visual Studio will, by default, place the components into separate namespaces corresponding to those folders. Since project folders are similar to operating system folders, project subfolders may be nested several layers deep as well. The namespace naming convention for this common use case can be described as follows:

| &nbsp; | View Model | View |
|-|------------|------|
| Convention | &lt;RootNS&gt;.ViewModels.&lt;ChildNS&gt;.&lt;ViewModelTypeName&gt; | &lt;RootNS&gt;.Views.&lt;ChildNS&gt;.&lt;ViewTypeName&gt; |
| Example 1 | MyProject.ViewModels.ShellViewModel | MyProject.Views.ShellView |
| Example 2 | MyProject.ViewModels.Utilities.SettingsViewModel | MyPoject.Views.Utitlities.SettingsView |

While the convention above covers many possibilities in terms of how deeply-nested namespaces may be, it does, however, assume a parallel structure in the organizational scheme of both the Views and ViewModels. Furthermore, it is also quite common to place Views and ViewModels into separate assemblies, which makes the likelihood of parallel organization across different assemblies even less likely.
