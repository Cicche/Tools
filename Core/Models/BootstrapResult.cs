using System.Collections.Generic;

namespace Tools.Core.Models
{
    public class BootstrapResult
    {
        public bool Loaded { get; set; }
        public bool BlockedByStrictMode { get; set; }
        public bool TemplateCreated { get; set; }
        public bool CredentialStoreMissing { get; set; }
        public List<string> Messages { get; } = new List<string>();
        public List<PC> Machines { get; } = new List<PC>();
        public int UnresolvedCredentials { get; set; }
    }
}
