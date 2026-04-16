using System;
using System.Collections.Generic;
using System.Linq;
using Tools.Core.Models;

namespace Tools.Core.Services
{
    internal class MachineQueryService
    {
        internal IReadOnlyList<MachineContract> ToMachineContracts(IEnumerable<PC> machines)
        {
            if (machines == null) return new List<MachineContract>();
            return machines
                .Where(m => m != null)
                .Select(MachineContract.FromPc)
                .Where(m => m != null)
                .ToList();
        }

        internal IReadOnlyList<CategoryContract> BuildCategorySummary(IEnumerable<PC> machines)
        {
            if (machines == null) return new List<CategoryContract>();

            return machines
                .Where(m => m != null)
                .GroupBy(m => (m.Type ?? string.Empty).Trim().ToUpperInvariant())
                .Select(g => new CategoryContract
                {
                    Name = g.Key,
                    MachineCount = g.Count()
                })
                .OrderBy(c => c.Name, StringComparer.OrdinalIgnoreCase)
                .ToList();
        }
    }
}
