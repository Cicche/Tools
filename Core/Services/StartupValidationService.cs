using System;
using System.Collections.Generic;
using System.Net;
using Tools.Core.Models;

namespace Tools.Core.Services
{
    internal class StartupValidationService
    {
        internal StartupValidationResult Validate(IEnumerable<PC> machines)
        {
            var result = new StartupValidationResult();
            if (machines == null) return result;

            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var seenIps = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            int row = 0;

            foreach (PC machine in machines)
            {
                row++;
                if (machine == null)
                {
                    result.RejectedItems.Add($"Riga {row}: macchina nulla.");
                    continue;
                }

                string type = (machine.Type ?? string.Empty).Trim();
                string name = (machine.Nome ?? string.Empty).Trim();
                string ip = (machine.Ip ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(type) || string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(ip))
                {
                    result.RejectedItems.Add($"Riga {row}: campi obbligatori mancanti (Type/Nome/Ip).");
                    continue;
                }

                string normalizedType = type.ToUpperInvariant();
                if (!IsAllowedType(normalizedType))
                {
                    result.RejectedItems.Add($"Riga {row}: tipo non valido '{type}' per macchina '{name}'.");
                    continue;
                }

                if (!seenNames.Add(name))
                {
                    result.RejectedItems.Add($"Riga {row}: Nome duplicato '{name}'.");
                    continue;
                }

                if (!seenIps.Add(ip))
                {
                    result.RejectedItems.Add($"Riga {row}: IP duplicato '{ip}' (macchina '{name}').");
                    continue;
                }

                if (!IPAddress.TryParse(ip, out _))
                {
                    result.RejectedItems.Add($"Riga {row}: IP non valido '{ip}' (macchina '{name}').");
                    continue;
                }

                machine.Type = normalizedType;
                machine.Nome = name;
                machine.Ip = ip;
                result.ValidMachines.Add(machine);
            }

            return result;
        }

        private static bool IsAllowedType(string type)
        {
            return type == "CMP" || type == "TRD" || type == "DOK" ||
                   type == "SERVER" || type == "GW" || type == "MFC";
        }
    }
}
