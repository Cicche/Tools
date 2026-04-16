using System.Collections.Generic;

namespace Tools.Core.Models
{
    internal class StartupValidationResult
    {
        public List<PC> ValidMachines { get; } = new List<PC>();
        public List<string> RejectedItems { get; } = new List<string>();
        public int RejectedCount => RejectedItems.Count;
    }
}
