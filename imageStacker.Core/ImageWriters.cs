using imageStacker.Core.Abstraction;
using System;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace imageStacker.Core
{
    public interface ISaveInfo
    {
        int? Index { get; }

        string Filtername { get; }
    }

    public class SaveInfo : ISaveInfo
    {
        public SaveInfo(int? index, string filtername)
        {
            Index = index;
            Filtername = filtername;
        }

        public int? Index { get; }

        public string Filtername { get; }

    }

    public abstract class ImageWriter<T> : IImageWriter<T> where T : IProcessableImage
    {
        protected readonly ILogger logger;
        protected readonly IMutableImageFactory<T> factory;
        public ImageWriter(ILogger logger, IMutableImageFactory<T> factory)
        {
            this.logger = logger;
            this.factory = factory;
        }

        public Task WaitForCompletion() => Task.CompletedTask;

        public abstract Task WriteFile(T image, ISaveInfo info);
    }

    public class ImageFileWriter<T> : IImageWriter<T> where T : IProcessableImage
    {
        private readonly string OutputFilePrefix, OutputFolder;
        private readonly IMutableImageFactory<T> Factory;

        public ImageFileWriter(string outputFilePrefix, string outputFolder, IMutableImageFactory<T> factory)
        {
            OutputFilePrefix = outputFilePrefix;
            OutputFolder = outputFolder;
            Factory = factory;
        }

        public Task WriteFile(T image, ISaveInfo info)
        {
            string path = Path.Combine(OutputFolder,
                string.Join('-',
                    OutputFilePrefix,
                    info.Filtername,
                    info.Index.HasValue ? info.Index.Value.ToString("d6") : string.Empty) + ".png");
            File.Delete(path);
            using (System.Drawing.Image image1 = Factory.ToImage(image))
            {
                image1.Save(path, ImageFormat.Png);
            }
            return Task.CompletedTask;
        }
        
        public Task WaitForCompletion() => Task.CompletedTask;
    }

    /// <summary>
    /// Outputs raw RGB Byte Stream
    /// </summary>
    public class ImageStreamWriter<T> : ImageWriter<T>, IDisposable where T : IProcessableImage
    {
        private readonly Stream outputStream;
        public ImageStreamWriter(ILogger logger, IMutableImageFactory<T> factory, Stream outputStream)
            : base(logger, factory)
        {
            this.outputStream = outputStream;
        }

        public void Dispose()
        {
            outputStream?.Close();
        }

        public override Task WriteFile(T image, ISaveInfo info)
        {
            var imageAsBytes = factory.ToBytes(image);
            outputStream.Write(imageAsBytes, 0, imageAsBytes.Length);
            image = default(T);
            return Task.CompletedTask;
        }

        ~ImageStreamWriter()
        {
            outputStream?.Close();
        }
    }

    public class TestImageWriter<T> : ImageWriter<T> where T : IProcessableImage
    {
        public TestImageWriter(ILogger logger, IMutableImageFactory<T> factory)
         : base(logger, factory)
        {
        }
        public override Task WriteFile(T image, ISaveInfo info)
        {
            return Task.CompletedTask;
        }
    }
}
