using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Management;
using System.Management.Automation;
using System.Management.Automation.Host;
using System.Management.Automation.Internal;
using System.Management.Automation.Runspaces;
using System.Text.Json.Serialization;
using System.Threading;
using Microsoft.PowerShell.Commands;
using Microsoft.PowerShell.Commands.Utility;
using Microsoft.VisualBasic;
using Newtonsoft.Json;

namespace TreeToString {

    public class PSOutputPreferences {
        public string ConfirmPreference;
        public string DebugPreference;
        public string ErrorActionPreference;
        public string InformationPreference;
        public string ProgressPreference;
        public string VerbosePreference;
        public string WarningPreference;
        public string WhatIfPreference;

        public PSOutputPreferences () { }

        public PSOutputPreferences (dynamic inputObject) {
            inputObject = inputObject as Dictionary<string, string>;
            this.ConfirmPreference = inputObject["ConfirmPreference"];
            this.DebugPreference = inputObject["DebugPreference"];
            this.ErrorActionPreference = inputObject["ErrorActionPreference"];
            this.InformationPreference = inputObject["InformationPreference"];
            this.ProgressPreference = inputObject["ProgressPreference"];
            this.VerbosePreference = inputObject["VerbosePreference"];
            this.WarningPreference = inputObject["WarningPreference"];
            this.WhatIfPreference = inputObject["WhatIfPreference"];
        }
        public PSOutputPreferences (Dictionary<string, string> inputObject) {
            this.ConfirmPreference = inputObject["ConfirmPreference"];
            this.DebugPreference = inputObject["DebugPreference"];
            this.ErrorActionPreference = inputObject["ErrorActionPreference"];
            this.InformationPreference = inputObject["InformationPreference"];
            this.ProgressPreference = inputObject["ProgressPreference"];
            this.VerbosePreference = inputObject["VerbosePreference"];
            this.WarningPreference = inputObject["WarningPreference"];
            this.WhatIfPreference = inputObject["WhatIfPreference"];
        }
        public PSOutputPreferences (string confirmPreference, string debugPreference, string errorActionPreference, string informationPreference, string progressPreference, string verbosePreference, string warningPreference, string whatIfPreference) {
            this.ConfirmPreference = confirmPreference;
            this.DebugPreference = debugPreference;
            this.ErrorActionPreference = errorActionPreference;
            this.InformationPreference = informationPreference;
            this.ProgressPreference = progressPreference;
            this.VerbosePreference = verbosePreference;
            this.WarningPreference = warningPreference;
            this.WhatIfPreference = whatIfPreference;
        }
    }

    public class PowerShellLogging {
        public PSOutputPreferences OutputPreferences = new PSOutputPreferences ();
        private List<string> outputSettings = new List<string> {
            "Break",
            "Continue",
            "Ignore",
            "Inquire",
            "SilentlyContinue",
            "Stop",
            "Suspend"
        };
        private List<string> toHostSettings = new List<string> {
            "Break",
            "Continue",
            "Inquire",
            "Stop",
            "Suspend"
        };

        public dynamic ConvertToJson (object objectToProcess) {
            JsonObject.ConvertToJsonContext context = new JsonObject.ConvertToJsonContext (
                15,
                false,
                true
            );
            return JsonObject.ConvertToJson (objectToProcess, context);
        }

        public dynamic GetPsVariable (string varName) {
            return GetPsVariable (varName, true);
        }
        public dynamic GetPsVariable (string varName, bool valueOnly) {
            var psRunspace = PowerShell.Create (RunspaceMode.CurrentRunspace);
            psRunspace.AddCommand ("Get-Variable");
            psRunspace.AddParameter ("Name", varName);
            if (valueOnly) {
                psRunspace.AddParameter ("ValueOnly", true);
            }
            var outVar = psRunspace.Invoke ();
            return outVar;
        }
        public void SetPsVariable (string varName, dynamic varValue) {
            var psRunspace = PowerShell.Create (RunspaceMode.CurrentRunspace);
            psRunspace.AddCommand ("Set-Variable");
            psRunspace.AddParameter ("Name", varName);
            psRunspace.AddParameter ("Value", varValue);
            psRunspace.Invoke ();
        }
        public void UpdatePsOutputPreferences () {
            var psRunspace = PowerShell.Create (RunspaceMode.CurrentRunspace);
            ScriptBlock scriptBlock = ScriptBlock.Create ("$Global:NewObject = [System.Collections.Generic.Dictionary[[string],[string]]]::new(); (Get-Variable *Preference).ForEach{ $NewObject.Add($PSItem.Name, $PSItem.Value)}");
            var psVar = "";
            try {
                psRunspace.AddScript (scriptBlock.ToString ());
            } catch (System.Exception) {
                throw new Exception ("Couldn't Add Script...");
            }
            try {
                psRunspace.Invoke ();
            } catch (System.Exception) {
                throw new Exception ("Couldn't Invoke...");
            }
            try {
                var rawObject = GetPsVariable ("NewObject");
                psVar = ConvertToJson (rawObject);
            } catch (System.Exception ex) {
                throw new Exception ("Couldn't Serialize NewObject...", ex);
            }
            try {
                //FIXME: This is dumb... why is it a list? Why do I have to do this? WHYYYY!?
                OutputPreferences = JsonConvert.DeserializeObject<List<PSOutputPreferences>> (psVar) [0];
            } catch (System.Exception ex) {
                throw new Exception ("Couldn't Set PSOutputPreferences...", ex);
            }
        }

        public void WritePsDebug (string message) {
            var psRunspace = PowerShell.Create (RunspaceMode.CurrentRunspace);
            UpdatePsOutputPreferences ();
            string currentSetting = OutputPreferences.DebugPreference;
            if (this.toHostSettings.Contains (currentSetting)) {
                psRunspace.AddCommand ("Write-Debug");
                psRunspace.AddParameter ("Message", message);
                psRunspace.Invoke ();
            }
        }
        public void WritePsVerbose (string message) {
            var psRunspace = PowerShell.Create (RunspaceMode.CurrentRunspace);
            UpdatePsOutputPreferences ();
            string currentSetting = OutputPreferences.DebugPreference;
            if (this.toHostSettings.Contains (currentSetting)) {
                psRunspace.AddCommand ("Write-Verbose");
                psRunspace.AddParameter ("Message", message);
                psRunspace.Invoke ();
            }
        }
        public void WritePsError (ErrorRecord errorRecord) {
            var psRunspace = PowerShell.Create (RunspaceMode.CurrentRunspace);
            UpdatePsOutputPreferences ();
            string currentSetting = OutputPreferences.DebugPreference;
            if (this.toHostSettings.Contains (currentSetting)) {
                psRunspace.AddCommand ("Write-Error");
                psRunspace.AddParameter ("ErrorRecord", errorRecord);
                psRunspace.Invoke ();
            }
        }
        public void WritePsError (string message) {
            var psRunspace = PowerShell.Create (RunspaceMode.CurrentRunspace);
            UpdatePsOutputPreferences ();
            string currentSetting = OutputPreferences.DebugPreference;
            if (this.toHostSettings.Contains (currentSetting)) {
                psRunspace.AddCommand ("Write-Error");
                psRunspace.AddParameter ("Message", message);
                psRunspace.Invoke ();
            }
        }

    }
}