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
            try
            {
                /*
                 Functionalities:
                 - Create video from filtered input
                 - Create stacked image from filtered input
                 - Specify filter options 
                 - take input from ffmpeg and feed ffmpeg
                 
                 */
                /*       if (args.Length == 0)
                       {
                           Console.Error.WriteLine("No path defined");
                           return;
                       }*/
                Console.WriteLine(string.Join(',', args));


                string input = @"C:\Users\armbe\OneDrive\Dokumente\PlatformIO\Projects\imageStacker\imageStacker.Exploration.Cli\d2";// @"H:\DCIM\109_FUJI";
                string output = input;// "\\..\\..\\";

                var filters = new IFilter[] { new MaxFilter(), new MinFilter() };
                var files = System.IO.Directory.GetFiles(input);


                var processingData = new ProcessingData
                {
                    Filename = Path.GetFileNameWithoutExtension(input),
                    Files = Directory.GetFiles(input),
                    Filters = filters,
                    OutputDirectory = output
                };

                Directory.CreateDirectory(output);

                // output pipeable
                //   await ProceduralConsumer.Stack(processingData);

                // stack all
                await Consumer.StackImages(processingData);
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.ToString());
            }
        }
    }
}
class Consumer
{
    /// <summary>
    /// Applies all files on all filters
    /// </summary>
    /// <param name="filters"></param>
    /// <param name="files"></param>
    /// <param name="i"></param>
    /// <returns></returns>
    public async static Task StackImages(ProcessingData data)
    {
        var buffer = new BufferBlock<IProcessableImage>(new DataflowBlockOptions()
        {
            EnsureOrdered = true,
            BoundedCapacity = 4
        });
        var t = Task.Run(() => FileReaderHelpers.Produce(buffer, data.Files.ToArray()));
        var firstData = MutableImage.FromProcessableImage(buffer.Receive());
        var filterData = data.Filters.AsParallel().Select((filter, i) => (filter, image: firstData.Clone(), i));

        int i = 0;
        while (await buffer.OutputAvailableAsync() && !buffer.Completion.IsCompleted)
        {
            Console.Write(i++);
            var inputData = buffer.Receive();
            filterData.ForAll(data => data.filter.Process(data.image, inputData));
        }
        // Start the consumer. The Consume method runs asynchronously. 
        await buffer.Completion;
        Console.Error.WriteLine("Processed {0} files", data.Files.Count);
        System.IO.Directory.CreateDirectory(data.OutputDirectory);
        await t;
        filterData.ForAll(fdata => fdata.image.Save(Path.Combine(data.OutputDirectory, data.Filename + fdata.filter.Name + ".jpg")));
    }


    public async static Task ProceduralStack(List<IFilter> filters, List<string> files)
    {
        var buffer = new BufferBlock<IProcessableImage>(new DataflowBlockOptions()
        {
            EnsureOrdered = true,
            BoundedCapacity = 3,
        });

        var image = Image.FromFile(files.First());
        FileReaderHelpers.Produce(buffer, files.ToArray());
        await buffer.OutputAvailableAsync();
        var firstData = MutableImage.FromProcessableImage(buffer.Receive());
        var baseImages = filters.AsParallel().Select((filter, i) => (filter, image: firstData.Clone(), i));

        for (int i = 0; await buffer.OutputAvailableAsync() && !buffer.Completion.IsCompleted; i++)
        {
            var data = buffer.Receive();
            baseImages.ForAll(item =>
            {
                item.filter.Process(item.image, data);
                var path = Path.Combine("outdata", item.filter.Name + i.ToString("d6") + ".jpg");
                item.image.Save(path);
                Console.WriteLine(path);
            });
        }

        // Start the consumer. The Consume method runs asynchronously. 
        await buffer.Completion;
    }
}

class ProcessingData
{
    public string Filename { get; set; }
    public string OutputDirectory { get; set; }
    public IList<IFilter> Filters { get; set; }
    public IList<string> Files { get; set; }
}
class ProceduralConsumer
{
    public async static Task Stack(ProcessingData metaInformation)
    {

        var image = Image.FromFile(metaInformation.Files.First());
        var firstData = MutableImage.FromImage(image);
        var baseImages = metaInformation.Filters.AsParallel().Select(_ => firstData.Clone());

        var filesData = new ConcurrentQueue<byte[]>();

        var readThread = new Thread(() => ReadImages(metaInformation.Files.Skip(1), filesData));
        readThread.Start();

        int i = 1;
        while (true)
        {
            var reading = readThread.IsAlive;

            if (!filesData.TryDequeue(out byte[] data))
            {
                if (reading)
                {
                    Thread.Sleep(100);
                    continue;
                }
                break;
            }

            metaInformation.Filters.AsParallel().ForAll(filter => ApplyFilter(filter, data, image, i, metaInformation.OutputDirectory));
            i++;
        }
    }
    private static void ReadImages(IEnumerable<string> files, ConcurrentQueue<byte[]> output)
    {
        foreach (var file in files)
        {
            while (output.Count > 8)
            {
                Thread.Sleep(100);
            }
            output.Enqueue(FileReaderHelpers.Read(file));
        }
    }

    private static void ApplyFilter(IFilter filter, byte[] data, Image metadataImage, int i, string foldername)
    {

    }

}
