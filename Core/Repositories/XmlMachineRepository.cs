using System.Collections.Generic;
using Tools.Core.Abstractions;
using Tools.Core.Models;

namespace Tools.Core.Repositories
{
    public class XmlMachineRepository : IMachineRepository
    {
        public MachineFileLoadResult LoadMachines(string path)
        {
            return Macchine.DeserializeWithResult(path);
        }

        public Dictionary<string, CategoryCredential> LoadCategoryCredentials(string path)
        {
            return Macchine.LoadCategoryCredentials(path);
        }

        public void SaveSanitized(string path, IEnumerable<PC> machines)
        {
            Macchine.SaveSanitized(path, machines);
        }
    }
}
