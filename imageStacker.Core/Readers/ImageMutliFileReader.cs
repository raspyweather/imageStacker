using imageStacker.Core.Abstraction;
using imageStacker.Core.Extensions;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace imageStacker.Core.Readers
{
    public class ImageMutliFileReader<T> : ImageReaderBase<T> where T : IProcessableImage
    {
        private const int readParallelism = 8;
        private const int decodeParallelism = 6;
        private const int decodeQueueLength = 16;

        private readonly IList<string> filenames;
        private readonly TransformBlock<string, MemoryStream> readingBlock;
        private readonly TransformBlock<MemoryStream, T> decodeBlock;

        public ImageMutliFileReader(ILogger logger, IMutableImageFactory<T> factory, IImageReaderOptions options, bool ordered)
            : base(logger, factory)
        {
            if (options?.Files != null && options.Files.Any())
            {
                this.filenames = options.Files.ToList();
            }
            else
            {
                this.filenames = Directory.GetFiles(options.FolderName, options.Filter, new EnumerationOptions
                {
                    AttributesToSkip = FileAttributes.System,
                    IgnoreInaccessible = true,
                    MatchCasing = MatchCasing.CaseInsensitive
                });
            }

            this.logger?.WriteLine($"Items found: {filenames.Count}", Verbosity.Info);

            var readOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = readParallelism,
                EnsureOrdered = ordered,
                BoundedCapacity = decodeQueueLength
            };
            readingBlock = new TransformBlock<string, MemoryStream>(ReadFromDisk, readOptions).WithLogging("Read");

            var decodeOptions = new ExecutionDataflowBlockOptions
            {
                MaxDegreeOfParallelism = decodeParallelism,
                EnsureOrdered = ordered,
                BoundedCapacity = decodeQueueLength
            };

            decodeBlock = new TransformBlock<MemoryStream, T>(DecodeImage, decodeOptions).WithLogging("Decode");

            var linkOptions = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };
            readingBlock.LinkTo(decodeBlock, linkOptions);
        }

        private MemoryStream ReadFromDisk(string filename)
        {
            return new MemoryStream(File.ReadAllBytes(filename), false);
        }

        private T DecodeImage(MemoryStream data)
        {
            using (data)
            {
                using var bmp = new Bitmap(data);
                var width = bmp.Width;
                var height = bmp.Height;
                var pixelFormat = bmp.PixelFormat;

                var bmp1Data = bmp.LockBits(new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, pixelFormat);
                var length = bmp1Data.Stride * bmp1Data.Height;
                byte[] bitmapBytes = new byte[length];
                Marshal.Copy(bmp1Data.Scan0, bitmapBytes, 0, length);
                bmp.UnlockBits(bmp1Data);

                return factory.FromBytes(width, height, bitmapBytes);
            }
        }

        public override async Task Work()
        {
            foreach (var filename in this.filenames)
            {
                await readingBlock.SendAsync(filename);
            }
            readingBlock.Complete();

            await decodeBlock.Completion;
        }

        public override ISourceBlock<T> GetSource()
        {
            return decodeBlock;
        }
    }

}
