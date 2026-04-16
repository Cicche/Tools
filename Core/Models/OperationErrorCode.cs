namespace Tools.Core.Models
{
    public enum OperationErrorCode
    {
        None = 0,
        InvalidInput = 1,
        InvalidMachineConfig = 2,
        CredentialsMissing = 3,
        NetworkUnreachable = 4,
        ExternalProcessError = 5,
        Unauthorized = 6,
        Timeout = 7,
        Unexpected = 99
    }
}
