using System.Collections.Generic;

namespace TreeToString.PowerShellExtensions {
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
}