﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace StatusPlaceholder {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "17.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("StatusPlaceholder.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Status placeholder needs to be decorated with [StatusPlaceholder] attribute.
        /// </summary>
        internal static string MissingAttribute_AnalyzerDescription {
            get {
                return ResourceManager.GetString("MissingAttribute_AnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type name &apos;{0}&apos; doesn&apos;t have [StatusPlaceholder] attribute.
        /// </summary>
        internal static string MissingAttribute_AnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("MissingAttribute_AnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Status placeholder needs to be decorated with [StatusPlaceholder] attribute.
        /// </summary>
        internal static string MissingAttribute_AnalyzerTitle {
            get {
                return ResourceManager.GetString("MissingAttribute_AnalyzerTitle", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Status placeholder needs to implement IStatusPlaceholder interface.
        /// </summary>
        internal static string MissingInterface_AnalyzerDescription {
            get {
                return ResourceManager.GetString("MissingInterface_AnalyzerDescription", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Type name &apos;{0}&apos; doesn&apos;t implement IStatusPlaceholder.
        /// </summary>
        internal static string MissingInterface_AnalyzerMessageFormat {
            get {
                return ResourceManager.GetString("MissingInterface_AnalyzerMessageFormat", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to Status placeholder needs to implement IStatusPlaceholder interface.
        /// </summary>
        internal static string MissingInterface_AnalyzerTitle {
            get {
                return ResourceManager.GetString("MissingInterface_AnalyzerTitle", resourceCulture);
            }
        }
    }
}