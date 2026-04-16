namespace Tools.Core.Models
{
    internal class MachineContract
    {
        public string Type { get; set; }
        public string Name { get; set; }
        public string Ip { get; set; }

        public static MachineContract FromPc(PC pc)
        {
            if (pc == null) return null;
            return new MachineContract
            {
                Type = pc.Type,
                Name = pc.Nome,
                Ip = pc.Ip
            };
        }
    }
}
