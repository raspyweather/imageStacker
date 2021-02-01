using imageStacker.Core.Abstraction;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Timers;

namespace imageStacker.Core
{

    public enum Verbosity
    {
        Debug = 0,
        Verbose,
        Info,
        Warning,
        Error,
    }

    public class Logger : ILogger, IDisposable
    {
        private readonly ConcurrentDictionary<string, int> fillStates = new ConcurrentDictionary<string, int>();
        private readonly TextWriter output;
        private readonly Timer printTimer;
        private readonly Verbosity selectedLevel;
        private List<IBoundedQueue> queues = new List<IBoundedQueue>();

        public Logger(TextWriter output, Verbosity level = Verbosity.Info)
        {
            this.selectedLevel = level;

            printTimer = new Timer(2000);
            printTimer.Elapsed += (o, e) =>
            {
                this.ShowFillStates(Verbosity.Info);
                this.ShowQueueStates(Verbosity.Debug);
            };
            printTimer.Start();
            this.output = output;
        }

        public void ShowQueueStates(Verbosity verbosity)
            => this.WriteLine(string.Join("\n", queues.Select(x => $"{x.Name}:{x.Count:d4}:{x.AddedCount} {(x.IsAddingCompleted ? "ADc" : "ADn")} {(x.IsCompleted ? "iCc" : "iCn")}").ToArray()), verbosity, true);

        public void ShowFillStates(Verbosity verbosity)
            => this.WriteLine(string.Join(" ", fillStates.ToList().Select(x => $"{x.Key}:{x.Value:d4}").ToArray()), verbosity, false);

        public void NotifyFillstate(int count, string name)
        {
            fillStates[name] = count;
        }

        public void WriteLine(string text, Verbosity verbosity, bool newLine = true)
        {
            if (verbosity < selectedLevel) { return; }

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

        public void AddQueue<T>(SemaphoreBoundedQueue<T> queue)
        {
            this.queues.Add(queue);
        }
    }
}
