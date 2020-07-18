using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Timers;

namespace imageStacker.Core
{
    public interface ILogger
    {
        void NotifyFillstate(int count, string name);
        void ShowFillStates(string text, Verbosity verbosity);
        void WriteLine(string text, Verbosity verbosity);

        void LogException(Exception e);
    }

    public enum Verbosity
    {
        Verbose,
        Info,
        Warning,
        Error
    }

    public class Logger : ILogger
    {
        private readonly ConcurrentDictionary<string, int> fillStates = new ConcurrentDictionary<string, int>();
        private readonly TextWriter output;

        public Logger(TextWriter output)
        {
            var t = new Timer(1000);
            t.Elapsed += (o, e) => this.ShowFillStates("", Verbosity.Info);
            t.Start();
            this.output = output;
        }

        public void ShowFillStates(string text, Verbosity verbosity)
        {
            this.output.Write("\r");
            this.WriteLine(string.Join(" ", fillStates.ToList().Select(x => $"{x.Key}:{x.Value:d4}").ToArray()), verbosity);
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

        public void WriteLine(string text, Verbosity verbosity)
        {
            var prefix = verbosity == Verbosity.Error ? "ERROR" :
                         verbosity == Verbosity.Warning ? "WARN" :
                         verbosity == Verbosity.Info ? "INFO:" : "";
            output.WriteLine($"[{prefix}] {text}");
            // TODO add colors for funzies
        }

        public void LogException(Exception e) => WriteLine(e.ToString(), Verbosity.Error);
    }
}
