using imageStacker.Core.Abstraction;
using System;

namespace imageStacker.Core.Abstraction
{
    public interface ILogger : IDisposable
    {
        void NotifyFillstate(int count, string name);
        void ShowFillStates(Verbosity verbosity);
        void ShowQueueStates(Verbosity verbosity);
        void WriteLine(string text, Verbosity verbosity, bool newLine = true);

        void LogException(Exception e);
        void AddQueue<T>(SemaphoreBoundedQueue<T> queue);
    }
}
