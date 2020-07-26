using System;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Timers;

namespace imageStacker.Core
{
    public interface ILogger : IDisposable
    {
        void NotifyFillstate(int count, string name);
        void ShowFillStates(string text, Verbosity verbosity);
        void WriteLine(string text, Verbosity verbosity, bool newLine = true);

        void LogException(Exception e);
    }

    public enum Verbosity
    {
        Verbose,
        Info,
        Warning,
        Error
    }

    public class Logger : ILogger, IDisposable
    {
        private readonly ConcurrentDictionary<string, int> fillStates = new ConcurrentDictionary<string, int>();
        private readonly TextWriter output;
        private readonly Timer printTimer;

        public Logger(TextWriter output)
        {
            printTimer = new Timer(200);
            printTimer.Elapsed += (o, e) => this.ShowFillStates("", Verbosity.Info);
            printTimer.Start();
            this.output = output;
        }

        public void ShowFillStates(string text, Verbosity verbosity)
        {
            this.WriteLine(string.Join(" ", fillStates.ToList().Select(x => $"{x.Key}:{x.Value:d4}").ToArray()), verbosity, false);
        }
        public void NotifyFillstate(int count, string name)
        {
            if (!fillStates.ContainsKey(name))
            {
                fillStates.TryAdd(name, count);
            }
            else
            {
                fillStates[name] = count;
            }
        }

        public void WriteLine(string text, Verbosity verbosity, bool newLine = true)
        {
            var prefix = verbosity == Verbosity.Error ? "ERROR" :
                         verbosity == Verbosity.Warning ? "WARN" :
                         verbosity == Verbosity.Info ? "INFO:" : "";
            output.Write($"{(newLine ? ' ' : '\r')}[{prefix}] {text}");
            if (newLine)
            {
                output.WriteLine();
            }
            // TODO add colors for funzies
        }

        public void LogException(Exception e) => WriteLine(e.ToString(), Verbosity.Error);

        public void Dispose()
        {
            printTimer.Stop();
            printTimer.Dispose();
        }
    }
}
