namespace TreeToString.PowerShellExtensions {
    public interface IPowerShellVariables {
        dynamic GetPsVariable (string varName);
        dynamic GetPsVariable (string varName, bool valueOnly);
        dynamic GetPsVariable (string varName, bool valueOnly, string scope);
        void RemovePsVariable (string varName);
        void RemovePsVariable (string varName, string scope);
        void SetPsVariable (string varName, dynamic varValue);
        void SetPsVariable (string varName, dynamic varValue, string scope);
    }
}