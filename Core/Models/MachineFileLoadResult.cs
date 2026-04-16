using System.Collections.Generic;

namespace Tools.Core.Models
{
    public class MachineFileLoadResult
    {
        public bool Success { get; set; }
        public bool TemplateCreated { get; set; }
        public Macchine Machines { get; set; }
        public List<string> Messages { get; } = new List<string>();
    }
}
