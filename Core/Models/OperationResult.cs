namespace Tools.Core.Models
{
    public class OperationResult
    {
        public bool Success { get; set; }
        public OperationErrorCode Code { get; set; }
        public string Message { get; set; }
        public string MachineName { get; set; }
        public string MachineIp { get; set; }

        public static OperationResult Ok(string message = "", PC machine = null)
        {
            return new OperationResult
            {
                Success = true,
                Code = OperationErrorCode.None,
                Message = message ?? string.Empty,
                MachineName = machine?.Nome ?? string.Empty,
                MachineIp = machine?.Ip ?? string.Empty
            };
        }

        public static OperationResult Fail(string message, OperationErrorCode code = OperationErrorCode.Unexpected, PC machine = null)
        {
            return new OperationResult
            {
                Success = false,
                Code = code,
                Message = message ?? "Operazione fallita.",
                MachineName = machine?.Nome ?? string.Empty,
                MachineIp = machine?.Ip ?? string.Empty
            };
        }
    }
}
