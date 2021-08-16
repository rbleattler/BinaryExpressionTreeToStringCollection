using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
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

namespace TreeToString.PowerShellExtensions {

    public class PowerShellLogging : IPowerShellVariables {
        private PowerShellVariables PowerShellVariables = new PowerShellVariables ();
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
                RemovePsVariable ("NewObject");
            } catch (System.Exception ex) {
                throw new Exception ("Couldn't Serialize NewObject...", ex);
            }
            try {
                //FIXME: This is dumb... why is it a list? Why do I have to do this? WHY!?
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
            string currentSetting = OutputPreferences.VerbosePreference;
            if (this.toHostSettings.Contains (currentSetting)) {
                psRunspace.AddCommand ("Write-Verbose");
                psRunspace.AddParameter ("Message", message);
                psRunspace.Invoke ();
            }
        }
        public void WritePsError (ErrorRecord errorRecord) {
            var psRunspace = PowerShell.Create (RunspaceMode.CurrentRunspace);
            UpdatePsOutputPreferences ();
            string currentSetting = OutputPreferences.ErrorActionPreference;
            if (this.toHostSettings.Contains (currentSetting)) {
                psRunspace.AddCommand ("Write-Error");
                psRunspace.AddParameter ("ErrorRecord", errorRecord);
                psRunspace.Invoke ();
            }
        }
        public void WritePsError (string message) {
            var psRunspace = PowerShell.Create (RunspaceMode.CurrentRunspace);
            UpdatePsOutputPreferences ();
            string currentSetting = OutputPreferences.ErrorActionPreference;
            if (this.toHostSettings.Contains (currentSetting)) {
                psRunspace.AddCommand ("Write-Error");
                psRunspace.AddParameter ("Message", message);
                psRunspace.Invoke ();
            }
        }

        public dynamic GetPsVariable (string varName) {
            return PowerShellVariables.GetPsVariable (varName, true);
        }

        public dynamic GetPsVariable (string varName, bool valueOnly) {
            return PowerShellVariables.GetPsVariable (varName, valueOnly);
        }

        public void RemovePsVariable (string varName) {
            PowerShellVariables.RemovePsVariable (varName);
        }

        public void RemovePsVariable (string varName, string scope) {
            PowerShellVariables.RemovePsVariable (varName, scope);
        }

        public void SetPsVariable (string varName, dynamic varValue) {
            PowerShellVariables.SetPsVariable (varName, varValue);
        }

        public dynamic GetPsVariable (string varName, bool valueOnly, string scope) {
            return PowerShellVariables.GetPsVariable (varName, valueOnly, scope);
        }

        public void SetPsVariable (string varName, dynamic varValue, string scope) {
            PowerShellVariables.SetPsVariable (varName, varValue, scope);
        }
    }
}