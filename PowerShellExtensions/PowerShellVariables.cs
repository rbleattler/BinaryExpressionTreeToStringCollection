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

    public class PowerShellVariables : IPowerShellVariables {
        public void RemovePsVariable (string varName) {
            RemovePsVariable (varName, "global");
        }
        public void RemovePsVariable (string varName, string scope) {
            string[] strings = {
                "GLOBAL",
                "LOCAL",
                "SCRIPT",
                "PRIVATE",
                "USING",
                "WORKFLOW"
            };
            Contract.Requires (
                strings.Contains (scope.ToUpper ())
            );
            var psRunspace = PowerShell.Create (RunspaceMode.CurrentRunspace);
            psRunspace.AddCommand ("Remove-Variable");
            psRunspace.AddParameter ("Name", varName);
            psRunspace.AddParameter ("Scope", scope);
            psRunspace.AddParameter ("Force", true);
            psRunspace.Invoke ();
        }
        public dynamic GetPsVariable (string varName) {
            return GetPsVariable (varName, true);
        }
        public dynamic GetPsVariable (string varName, bool valueOnly) {
            return GetPsVariable (varName, valueOnly, "global");
        }
        public void SetPsVariable (string varName, dynamic varValue) {
            SetPsVariable (varName, varValue, "global");
        }
        public dynamic GetPsVariable (string varName, bool valueOnly, string scope) {
            string[] strings = {
                "GLOBAL",
                "LOCAL",
                "SCRIPT",
                "PRIVATE",
                "USING",
                "WORKFLOW"
            };
            Contract.Requires (
                strings.Contains (scope.ToUpper ())
            );
            var psRunspace = PowerShell.Create (RunspaceMode.CurrentRunspace);
            psRunspace.AddCommand ("Get-Variable");
            psRunspace.AddParameter ("Name", varName);
            if (valueOnly) {
                psRunspace.AddParameter ("ValueOnly", true);
            }
            psRunspace.AddParameter ("Scope", scope);
            var outVar = psRunspace.Invoke ();
            return outVar;
        }
        public void SetPsVariable (string varName, dynamic varValue, string scope) {
            string[] strings = {
                "GLOBAL",
                "LOCAL",
                "SCRIPT",
                "PRIVATE",
                "USING",
                "WORKFLOW"
            };
            Contract.Requires (
                strings.Contains (scope.ToUpper ())
            );
            var psRunspace = PowerShell.Create (RunspaceMode.CurrentRunspace);
            psRunspace.AddCommand ("Set-Variable");
            psRunspace.AddParameter ("Name", varName);
            psRunspace.AddParameter ("Value", varValue);
            psRunspace.AddParameter ("Scope", scope);
            psRunspace.Invoke ();
        }
    }
}