using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Timers;

namespace imageStacker.Core
{
    public enum Verbosity
    {
        Verbose,
        Info,
        Warning,
        Error
    }
    public class Logger
    {
        public static Logger loggerInstance = new Logger();
        private readonly ConcurrentDictionary<string, int> fillStates = new ConcurrentDictionary<string, int>();

        public Logger()
        {
            var t = new Timer(1000);
            t.Elapsed += (o, e) => this.WriteLine("", Verbosity.Error);
            t.Start();
        }

        public void WriteLine(string text, Verbosity verbosity)
        {
            Console.Out.Write("\r " + string.Join(" ", fillStates.ToList().Select(x => $"{x.Key}:{x.Value:d4}").ToArray()));
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

    }
}
