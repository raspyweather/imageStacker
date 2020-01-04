using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace imageStacker
{
    class Program
    {
        async static Task Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            const string str2 = @"./sample1";
            var files = System.IO.Directory.GetFiles(str2, "*.jpg", System.IO.SearchOption.TopDirectoryOnly).ToList();
            // Create a BufferBlock<byte[]> object. This object serves as the 
            // target block for the producer and the source block for the consumer.
            int workers = 5;
            int minImagesPerWorker = 5;
            List<string[]> fileArrays = new List<string[]>();
            if (files.Count <= workers * minImagesPerWorker)
            {
                workers = files.Count / minImagesPerWorker;
                for (int i = 0; i < workers - 1; i++)
                {
                    fileArrays.Add(files.GetRange(i * minImagesPerWorker, minImagesPerWorker).ToArray());
                }
                fileArrays.Add(files.GetRange(minImagesPerWorker * (workers - 1), minImagesPerWorker + (files.Count % workers)).ToArray());
            }
            else
            {
                int imagesPerWorker = files.Count / workers;
                for (int i = 0; i < workers - 1; i++)
                {
                    fileArrays.Add(files.GetRange(i * imagesPerWorker, imagesPerWorker).ToArray());
                }
                fileArrays.Add(files.GetRange(imagesPerWorker * (workers - 1), imagesPerWorker + (files.Count % workers)).ToArray());
            }

            var intermediateImages = fileArrays.AsParallel().WithDegreeOfParallelism(workers).Select(x => BulkProcessing(x)).ToArray();

            var buffer = new BufferBlock<byte[]>(new DataflowBlockOptions()
            {
                BoundedCapacity = 3,
                EnsureOrdered = true,
            });
            DataflowImplementation.Produce(buffer, intermediateImages);
            var consumer = DataflowImplementation.ConsumeAsync(buffer, files.First());
            consumer.Wait();
            consumer.Result.Save(DateTime.Now.ToString().Replace(".", "").Replace(":", "") + ".png");
        }
        static Bitmap BulkProcessing(string[] files)
        {
            Stopwatch st = new Stopwatch();
            st.Start();
            var buffer = new BufferBlock<byte[]>(new DataflowBlockOptions()
            {
                BoundedCapacity = 3,
                EnsureOrdered = true,
            });
            DataflowImplementation.Produce(buffer, files.Skip(1).ToArray());
            // Start the consumer. The Consume method runs asynchronously. 
            var consumer = DataflowImplementation.ConsumeAsync(buffer, files.First());
            consumer.Wait();
            st.Stop();
            Console.WriteLine("Processed {0} files in {1}ms", files.Length, st.ElapsedMilliseconds);
            return consumer.Result;
        }

    }
}
