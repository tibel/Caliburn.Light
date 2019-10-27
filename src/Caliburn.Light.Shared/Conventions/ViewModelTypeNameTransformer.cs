using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Collections.Generic;
#if NETFX_CORE
using Windows.ApplicationModel;
#endif

namespace Caliburn.Light
{
    /// <summary>
    /// A strategy for determining which view to use for a given model.
    /// </summary>
    public static class ViewModelTypeNameTransformer
    {
        //These fields are used for configuring the default type mappings. They can be changed using ConfigureTypeMappings().
        private static string _defaultSubNsViews;
        private static string _defaultSubNsViewModels;
        private static bool _useNameSuffixesInMappings;
        private static string _nameFormat;
        private static string _viewModelSuffix;
        private static readonly List<string> ViewSuffixList = new List<string>();
        private static bool _includeViewSuffixInVmNames;

        ///<summary>
        /// Used to transform names.
        ///</summary>
        public static NameTransformer NameTransformer = new NameTransformer();

        /// <summary>
        ///   Separator used when resolving View names for context instances.
        /// </summary>
        public static string ContextSeparator = ".";

        static ViewModelTypeNameTransformer()
        {
            ConfigureTypeMappings(new TypeMappingConfiguration());
        }

        /// <summary>
        /// Specifies how type mappings are created, including default type mappings. Calling this method will
        /// clear all existing name transformation rules and create new default type mappings according to the
        /// configuration.
        /// </summary>
        /// <param name="config">An instance of TypeMappingConfiguration that provides the settings for configuration</param>
        public static void ConfigureTypeMappings(TypeMappingConfiguration config)
        {
            if (string.IsNullOrEmpty(config.DefaultSubNamespaceForViews))
            {
                throw new ArgumentException("DefaultSubNamespaceForViews field cannot be blank.");
            }

            if (string.IsNullOrEmpty(config.DefaultSubNamespaceForViewModels))
            {
                throw new ArgumentException("DefaultSubNamespaceForViewModels field cannot be blank.");
            }

            if (string.IsNullOrEmpty(config.NameFormat))
            {
                throw new ArgumentException("NameFormat field cannot be blank.");
            }

            NameTransformer.Clear();
            ViewSuffixList.Clear();

            _defaultSubNsViews = config.DefaultSubNamespaceForViews;
            _defaultSubNsViewModels = config.DefaultSubNamespaceForViewModels;
            _nameFormat = config.NameFormat;
            _useNameSuffixesInMappings = config.UseNameSuffixesInMappings;
            _viewModelSuffix = config.ViewModelSuffix;
            ViewSuffixList.AddRange(config.ViewSuffixList);
            _includeViewSuffixInVmNames = config.IncludeViewSuffixInViewModelNames;

            SetAllDefaults();
        }


        private static void SetAllDefaults()
        {
            if (_useNameSuffixesInMappings)
            {
                //Add support for all view suffixes
                ViewSuffixList.ForEach(AddDefaultTypeMapping);
            }
            else
            {
                AddSubNamespaceMapping(_defaultSubNsViewModels, _defaultSubNsViews);
            }
        }

        /// <summary>
        /// Adds a default type mapping using the standard namespace mapping convention
        /// </summary>
        /// <param name="viewSuffix">Suffix for type name. Should  be "View" or synonym of "View". (Optional)</param>
        public static void AddDefaultTypeMapping(string viewSuffix = "View")
        {
            if (!_useNameSuffixesInMappings)
            {
                return;
            }

            //Check for <Namespace>.<BaseName><ViewSuffix> construct
            AddNamespaceMapping(string.Empty, string.Empty, viewSuffix);

            //Check for <Namespace>.ViewModels.<NameSpace>.<BaseName><ViewSuffix> construct
            AddSubNamespaceMapping(_defaultSubNsViewModels, _defaultSubNsViews, viewSuffix);
        }

        /// <summary>
        /// This method registers a View suffix or synonym so that View Context resolution works properly.
        /// It is automatically called internally when calling AddNamespaceMapping(), AddDefaultTypeMapping(),
        /// or AddTypeMapping(). It should not need to be called explicitly unless a rule that handles synonyms
        /// is added directly through the NameTransformer.
        /// </summary>
        /// <param name="viewSuffix">Suffix for type name. Should  be "View" or synonym of "View".</param>
        public static void RegisterViewSuffix(string viewSuffix)
        {
            if (ViewSuffixList.Count(s => s == viewSuffix) == 0)
            {
                ViewSuffixList.Add(viewSuffix);
            }
        }

        /// <summary>
        /// Adds a standard type mapping based on namespace RegEx replace and filter patterns
        /// </summary>
        /// <param name="nsSourceReplaceRegEx">RegEx replace pattern for source namespace</param>
        /// <param name="nsSourceFilterRegEx">RegEx filter pattern for source namespace</param>
        /// <param name="nsTargetsRegEx">Array of RegEx replace values for target namespaces</param>
        /// <param name="viewSuffix">Suffix for type name. Should  be "View" or synonym of "View". (Optional)</param>
        public static void AddTypeMapping(string nsSourceReplaceRegEx, string nsSourceFilterRegEx,
            string[] nsTargetsRegEx, string viewSuffix = "View")
        {
            RegisterViewSuffix(viewSuffix);

            var repsuffix = _useNameSuffixesInMappings ? viewSuffix : string.Empty;
            const string basegrp = "${basename}";

            var rxbase = RegExHelper.GetNameCaptureGroup("basename");
            var suffix = string.Empty;
            if (_useNameSuffixesInMappings)
            {
                suffix = _viewModelSuffix;
                if (!_viewModelSuffix.Contains(viewSuffix) && _includeViewSuffixInVmNames)
                {
                    suffix = viewSuffix + suffix;
                }
            }
            var rxsrcfilter = string.IsNullOrEmpty(nsSourceFilterRegEx)
                ? null
                : string.Concat(nsSourceFilterRegEx, string.Format(_nameFormat, RegExHelper.NameRegEx, suffix), "$");
            var rxsuffix = RegExHelper.GetCaptureGroup("suffix", suffix);

            NameTransformer.AddRule(
                string.Concat(nsSourceReplaceRegEx, string.Format(_nameFormat, rxbase, rxsuffix), "$"),
                nsTargetsRegEx.Select(t => t + string.Format(_nameFormat, basegrp, repsuffix)).ToArray(),
                rxsrcfilter
                );
        }

        /// <summary>
        /// Adds a standard type mapping based on namespace RegEx replace and filter patterns
        /// </summary>
        /// <param name="nsSourceReplaceRegEx">RegEx replace pattern for source namespace</param>
        /// <param name="nsSourceFilterRegEx">RegEx filter pattern for source namespace</param>
        /// <param name="nsTargetRegEx">RegEx replace value for target namespace</param>
        /// <param name="viewSuffix">Suffix for type name. Should  be "View" or synonym of "View". (Optional)</param>
        public static void AddTypeMapping(string nsSourceReplaceRegEx, string nsSourceFilterRegEx, string nsTargetRegEx,
            string viewSuffix = "View")
        {
            AddTypeMapping(nsSourceReplaceRegEx, nsSourceFilterRegEx, new[] {nsTargetRegEx}, viewSuffix);
        }

        /// <summary>
        /// Adds a standard type mapping based on simple namespace mapping
        /// </summary>
        /// <param name="nsSource">Namespace of source type</param>
        /// <param name="nsTargets">Namespaces of target type as an array</param>
        /// <param name="viewSuffix">Suffix for type name. Should  be "View" or synonym of "View". (Optional)</param>
        public static void AddNamespaceMapping(string nsSource, string[] nsTargets, string viewSuffix = "View")
        {
            //need to terminate with "." in order to concatenate with type name later
            var nsencoded = RegExHelper.NamespaceToRegEx(nsSource + ".");

            //Start pattern search from beginning of string ("^")
            //unless original string was blank (i.e. special case to indicate "append target to source")
            if (!string.IsNullOrEmpty(nsSource))
            {
                nsencoded = "^" + nsencoded;
            }

            //Capture namespace as "origns" in case we need to use it in the output in the future
            var nsreplace = RegExHelper.GetCaptureGroup("origns", nsencoded);

            var nsTargetsRegEx = nsTargets.Select(t => t + ".").ToArray();
            AddTypeMapping(nsreplace, null, nsTargetsRegEx, viewSuffix);
        }

        /// <summary>
        /// Adds a standard type mapping based on simple namespace mapping
        /// </summary>
        /// <param name="nsSource">Namespace of source type</param>
        /// <param name="nsTarget">Namespace of target type</param>
        /// <param name="viewSuffix">Suffix for type name. Should  be "View" or synonym of "View". (Optional)</param>
        public static void AddNamespaceMapping(string nsSource, string nsTarget, string viewSuffix = "View")
        {
            AddNamespaceMapping(nsSource, new[] {nsTarget}, viewSuffix);
        }

        /// <summary>
        /// Adds a standard type mapping by substituting one subnamespace for another
        /// </summary>
        /// <param name="nsSource">Subnamespace of source type</param>
        /// <param name="nsTargets">Subnamespaces of target type as an array</param>
        /// <param name="viewSuffix">Suffix for type name. Should  be "View" or synonym of "View". (Optional)</param>
        public static void AddSubNamespaceMapping(string nsSource, string[] nsTargets, string viewSuffix = "View")
        {
            //need to terminate with "." in order to concatenate with type name later
            var nsencoded = RegExHelper.NamespaceToRegEx(nsSource + ".");

            string rxbeforetgt, rxaftersrc, rxaftertgt;
            var rxbeforesrc = rxbeforetgt = rxaftersrc = rxaftertgt = string.Empty;

            if (!string.IsNullOrEmpty(nsSource))
            {
                if (!nsSource.StartsWith("*"))
                {
                    rxbeforesrc = RegExHelper.GetNamespaceCaptureGroup("nsbefore");
                    rxbeforetgt = @"${nsbefore}";
                }

                if (!nsSource.EndsWith("*"))
                {
                    rxaftersrc = RegExHelper.GetNamespaceCaptureGroup("nsafter");
                    rxaftertgt = "${nsafter}";
                }
            }

            var rxmid = RegExHelper.GetCaptureGroup("subns", nsencoded);
            var nsreplace = string.Concat(rxbeforesrc, rxmid, rxaftersrc);

            var nsTargetsRegEx = nsTargets.Select(t => string.Concat(rxbeforetgt, t, ".", rxaftertgt)).ToArray();
            AddTypeMapping(nsreplace, null, nsTargetsRegEx, viewSuffix);
        }

        /// <summary>
        /// Adds a standard type mapping by substituting one subnamespace for another
        /// </summary>
        /// <param name="nsSource">Subnamespace of source type</param>
        /// <param name="nsTarget">Subnamespace of target type</param>
        /// <param name="viewSuffix">Suffix for type name. Should  be "View" or synonym of "View". (Optional)</param>
        public static void AddSubNamespaceMapping(string nsSource, string nsTarget, string viewSuffix = "View")
        {
            AddSubNamespaceMapping(nsSource, new[] {nsTarget}, viewSuffix);
        }

        /// <summary>
        /// Modifies the name of the type to be used at design time.
        /// </summary>
        public static Func<string, string> ModifyModelTypeAtDesignTime = modelTypeName =>
        {
            if (modelTypeName.StartsWith("_"))
            {
                var index = modelTypeName.IndexOf('.');
                modelTypeName = modelTypeName.Substring(index + 1);
                index = modelTypeName.IndexOf('.');
                modelTypeName = modelTypeName.Substring(index + 1);
            }

            return modelTypeName;
        };

        /// <summary>
        /// Transforms a ViewModel type name into all of its possible View type names. Optionally accepts an instance
        /// of context object
        /// </summary>
        /// <returns>Enumeration of transformed names</returns>
        /// <remarks>Arguments:
        /// typeName = The name of the ViewModel type being resolved to its companion View.
        /// context = An instance of the context or null.
        /// </remarks>
        public static Func<string, object, IEnumerable<string>> TransformName = (typeName, context) =>
        {
            if (DesignMode.DesignModeEnabled)
            {
                typeName = ModifyModelTypeAtDesignTime(typeName);
            }

            typeName = typeName.Substring(
                0,
                typeName.IndexOf('`') < 0
                    ? typeName.Length
                    : typeName.IndexOf('`')
                );

            Func<string, string> getReplaceString;
            if (context is null)
            {
                getReplaceString = r => r;
                return NameTransformer.Transform(typeName, getReplaceString);
            }

            var contextstr = ContextSeparator + context;
            string grpsuffix = string.Empty;
            if (_useNameSuffixesInMappings)
            {
                //Create RegEx for matching any of the synonyms registered
                var synonymregex = "(" + string.Join("|", ViewSuffixList.ToArray()) + ")";
                grpsuffix = RegExHelper.GetCaptureGroup("suffix", synonymregex);
            }

            const string grpbase = @"\${basename}";
            var patternregex = string.Format(_nameFormat, grpbase, grpsuffix) + "$";

            //Strip out any synonym by just using contents of base capture group with context string
            var replaceregex = "${basename}" + contextstr;

            //Strip out the synonym
            getReplaceString = r => Regex.Replace(r, patternregex, replaceregex);

            //Return only the names for the context
            return NameTransformer.Transform(typeName, getReplaceString).Where(n => n.EndsWith(contextstr));
        };
    }
}
