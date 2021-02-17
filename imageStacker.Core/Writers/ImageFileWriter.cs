using imageStacker.Core.Abstraction;
using imageStacker.Core.Extensions;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;

namespace imageStacker.Core.Writers

{
    public class ImageFileWriter<T> : IImageWriter<T> where T : IProcessableImage
    {
        private readonly string OutputFilePrefix, OutputFolder;
        private readonly IMutableImageFactory<T> Factory;
        private readonly TransformBlock<(T image, ISaveInfo info), (MemoryStream image, ISaveInfo info)> encodeBlock;
        private readonly ActionBlock<(MemoryStream image, ISaveInfo info)> saveBlock;

        public ImageFileWriter(string outputFilePrefix, string outputFolder, IMutableImageFactory<T> factory)
        {
            OutputFilePrefix = outputFilePrefix;
            OutputFolder = outputFolder;
            Factory = factory;


            var linkOpts = new DataflowLinkOptions
            {
                PropagateCompletion = true
            };
            var opts = new ExecutionDataflowBlockOptions
            {
                BoundedCapacity = 8,
                MaxDegreeOfParallelism = 8,
                EnsureOrdered = false
            };

            encodeBlock = new TransformBlock<(T image, ISaveInfo info), (MemoryStream image, ISaveInfo info)>(Encode, opts).WithLogging("Encode");
            saveBlock = new ActionBlock<(MemoryStream image, ISaveInfo info)>(Save, opts).WithLogging("WriteFile");
            encodeBlock.LinkTo(saveBlock, linkOpts);
        }

        public async Task Work()
        {
            await saveBlock.Completion;
        }

        private (MemoryStream image, ISaveInfo info) Encode((T image, ISaveInfo info) imageInfo)
        {
            var ms = new MemoryStream();
            var (image, info) = imageInfo;

            using System.Drawing.Image image1 = Factory.ToImage(image);
            image1.Save(ms, ImageFormat.Png);
            ms.Position = 0;
            return (ms, info);
        }

        private void Save((MemoryStream image, ISaveInfo info) imageInfo)
        {
            var (image, info) = imageInfo;

            string path = Path.Combine(OutputFolder,
                string.Join('-',
                    OutputFilePrefix,
                    info.Filtername,
                    info.Index.HasValue ? info.Index.Value.ToString("d6") : string.Empty) + ".png");
            using (image)
            {
                using (var file = File.Open(path, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    image.CopyTo(file);
                }
            }
        }

        public ITargetBlock<(T image, ISaveInfo saveInfo)> GetTarget()
        {
            return this.encodeBlock;
        }
    }
}
