using System.Collections.Generic;
using Tools.Core.Models;

namespace Tools.Core.Abstractions
{
    public interface IMachineRepository
    {
        MachineFileLoadResult LoadMachines(string path);
        Dictionary<string, CategoryCredential> LoadCategoryCredentials(string path);
        void SaveSanitized(string path, IEnumerable<PC> machines);
    }
}
