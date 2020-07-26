using imageStacker.Core;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace imageStacker.Exploration.Cli
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var folders = System.IO.Directory.GetDirectories(@"L:\Canada\timelapses");
            foreach (var item in folders)
            {
                try
                {
                    var filename = Path.GetFileNameWithoutExtension(item);
                    if (!(File.Exists(filename + "-MaxFilter-.jpg") || File.Exists(filename + "-MaxFilter-.png")))
                    {
                        var zargs = $"stackImage --inputFolder={item} --outputFile={filename} --outputFolder=.".Split(' ');
                        await imageStacker.Cli.Program.Main(zargs);
                    }
                    else
                    {
                        Console.WriteLine($"Skipped {filename + "-MaxFilter-.jpg"}");
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}