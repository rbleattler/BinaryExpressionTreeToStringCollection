using System.Management;
using System.Management.Automation;
using System.Management.Automation.Runspaces;

namespace TreeToString {
    public class PowerShellLogging {

        protected void SetPsVariable (string varName, dynamic varValue) {
            var psRunspace = PowerShell.Create (RunspaceMode.CurrentRunspace);
            psRunspace.AddCommand ("Set-Variable");
            psRunspace.AddParameter ("Name", varName);
            psRunspace.AddParameter ("Value", varValue);
            psRunspace.Invoke ();
        }

        public void WritePsDebug (string message) {
            var psRunspace = PowerShell.Create (RunspaceMode.CurrentRunspace);
            psRunspace.AddCommand ("Write-Debug");
            psRunspace.AddParameter ("Message", message);
            psRunspace.Invoke ();
        }
        public void WritePsVerbose (string message) {
            var psRunspace = PowerShell.Create (RunspaceMode.CurrentRunspace);
            psRunspace.AddCommand ("Write-Verbose");
            psRunspace.AddParameter ("Message", message);
            psRunspace.Invoke ();
        }
        public void WritePsError (ErrorRecord errorRecord) {
            var psRunspace = PowerShell.Create (RunspaceMode.CurrentRunspace);
            psRunspace.AddCommand ("Write-Error");
            psRunspace.AddParameter ("ErrorRecord", errorRecord);
            psRunspace.Invoke ();
        }
        public void WritePsError (string message) {
            var psRunspace = PowerShell.Create (RunspaceMode.CurrentRunspace);
            psRunspace.AddCommand ("Write-Error");
            psRunspace.AddParameter ("Message", message);
            psRunspace.Invoke ();
        }

    }
}