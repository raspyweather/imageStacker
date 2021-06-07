using System;

namespace imageStacker.Core.Abstraction
{
    public interface ILogger : IDisposable
    {
        void NotifyFillstate(int count, string name);
        void WriteLine(string text, Verbosity verbosity, bool newLine = true);

        void LogException(Exception e);
        void AddQueue(Func<string> getInfo, string name);
    }
}
