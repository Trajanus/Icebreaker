﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace Icebreaker {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Icebreaker : global::System.Configuration.ApplicationSettingsBase {
        
        private static Icebreaker defaultInstance = ((Icebreaker)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Icebreaker())));
        
        public static Icebreaker Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string DefaultMessage {
            get {
                return ((string)(this["DefaultMessage"]));
            }
            set {
                this["DefaultMessage"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int MinimumMatchPercentage {
            get {
                return ((int)(this["MinimumMatchPercentage"]));
            }
            set {
                this["MinimumMatchPercentage"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int MaximumMatchPercentage {
            get {
                return ((int)(this["MaximumMatchPercentage"]));
            }
            set {
                this["MaximumMatchPercentage"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int NumMessagesToSend {
            get {
                return ((int)(this["NumMessagesToSend"]));
            }
            set {
                this["NumMessagesToSend"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int MinAge {
            get {
                return ((int)(this["MinAge"]));
            }
            set {
                this["MinAge"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("0")]
        public int MaxAge {
            get {
                return ((int)(this["MaxAge"]));
            }
            set {
                this["MaxAge"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool KidsAllowed {
            get {
                return ((bool)(this["KidsAllowed"]));
            }
            set {
                this["KidsAllowed"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string ProfileNameRunningUnder {
            get {
                return ((string)(this["ProfileNameRunningUnder"]));
            }
            set {
                this["ProfileNameRunningUnder"] = value;
            }
        }
    }
}
