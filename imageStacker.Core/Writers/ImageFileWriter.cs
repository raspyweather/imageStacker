using imageStacker.Core.Abstraction;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;

namespace imageStacker.Core.Writers

{
    public class ImageFileWriter<T> : IImageWriter<T> where T : IProcessableImage
    {
        private readonly string OutputFilePrefix, OutputFolder;
        private readonly IMutableImageFactory<T> Factory;
        private IBoundedQueue<(T image, ISaveInfo info)> queue;

        public ImageFileWriter(string outputFilePrefix, string outputFolder, IMutableImageFactory<T> factory)
        {
            OutputFilePrefix = outputFilePrefix;
            OutputFolder = outputFolder;
            Factory = factory;
        }

        public async Task WaitForCompletion()
        {
            while (true)
            {
                var (image, info) = await queue.DequeueOrDefault();
                if (image == null || info == null)
                {
                    break;
                }
                string path = Path.Combine(OutputFolder,
                string.Join('-',
                    OutputFilePrefix,
                    info.Filtername,
                    info.Index.HasValue ? info.Index.Value.ToString("d6") : string.Empty) + ".png");
                File.Delete(path);
                using System.Drawing.Image image1 = Factory.ToImage(image);
                image1.Save(path, ImageFormat.Png);
            }
        }

        public void SetQueue(IBoundedQueue<(T image, ISaveInfo info)> queue)
        {
            this.queue = queue;
        }
    }
}
