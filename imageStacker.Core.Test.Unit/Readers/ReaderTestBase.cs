using FluentAssertions;
using imageStacker.Core.Abstraction;
using imageStacker.Core.Test.Unit.ByteImage;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace imageStacker.Core.Test.Unit.Readers
{
    public abstract class ReaderTestBase<T> : IDisposable where T : IProcessableImage
    {
        public ReaderTestBase(
            IImageProvider<T> imageProvider,
            IMutableImageFactory<T> factory,
            int imagesCount)
        {
            this.imageProvider = imageProvider;
            this.ImagesCount = imagesCount;
            this.factory = factory;
        }

        protected int ImagesCount;

        protected IMutableImageFactory<T> factory;

        protected IImageProvider<T> imageProvider;

        protected IBoundedQueue<T> queue = BoundedQueueFactory.Get<T>(16);

        protected abstract IImageReader<T> Reader { get; }

        protected string tempPath;

        [Fact]
        public async Task ReadsAllImages()
        {
            Prepare();
            var reader = Reader;
            var t = Task.Run(() => reader.Produce(queue));
            int i = 0;

            while ((await queue.DequeueOrNull()) != null)
            {
                i++;
            }

            i.Should().Be(ImagesCount);
        }

        protected void Prepare()
        {
            tempPath = Path.Combine(
                Path.GetTempPath(),
                "imageStackerTest",
                Path.GetFileNameWithoutExtension(Path.GetRandomFileName()));

            Directory.CreateDirectory(Path.Combine(tempPath));
            for (int i = 0; i < ImagesCount; i++)
            {
                var img = this.imageProvider.PreparePrefilledImage(i);
                this.factory.ToImage(img).Save(Path.Combine(tempPath, $"{i:d5}.png"));
            }

        }

        public void Dispose()
        {
            try
            {
                Directory.Delete(tempPath);
            }
            catch { }
        }
    }
}
