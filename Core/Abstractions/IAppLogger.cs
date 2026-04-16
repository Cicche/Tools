namespace Tools.Core.Abstractions
{
    public interface IAppLogger
    {
        void Info(string category, string message);
        void Warn(string category, string message);
        void Error(string category, string message);
    }
}
