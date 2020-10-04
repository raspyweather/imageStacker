using System;
using System.Collections.Concurrent;
using System.IO.IsolatedStorage;
using System.Linq;
using Xunit;

namespace imageStacker.ffmpeg.Test.Unit
{
    public class ChunkedSimpleMemoryStreamTest
    {
        [Fact]
        public void EmitsImageOnOverflow()
        {
            var (chunkedStream, bytes, concurrentQueue) = Prepare(64, 64);

            chunkedStream.Write(bytes, 0, bytes.Length);

            Assert.Single(concurrentQueue);
        }

        [Fact]
        public void IsInCleanStateAfterWrite()
        {
            var (chunkedStream, bytes, _) = Prepare(64, 64);

            chunkedStream.Write(bytes, 0, bytes.Length);

            Assert.False(chunkedStream.HasUnwrittenData);
        }

        [Fact]
        public void IsDi6rtyAfterIncompleteImageWrite()
        {
            var (chunkedStream, bytes, _) = Prepare(48, 64);

            chunkedStream.Write(bytes, 0, bytes.Length);

            Assert.True(chunkedStream.HasUnwrittenData);
        }

        [Fact]
        public void LargerChunksWrittenIntoSmallerImages()
        {
            var (chunkedStream, bytes, concurrentQueue) = Prepare(640, 160);

            chunkedStream.Write(bytes);

            Assert.Equal(4, concurrentQueue.Count);
            Assert.False(chunkedStream.HasUnwrittenData);
        }

        [Fact]
        public void ChunkOverflowsIntoMultipleImages()
        {
            var (chunkedStream, bytes, concurrentQueue) = Prepare(48, 64);

            for (int i = 0; i < 10; i++)
            {
                chunkedStream.Write(bytes, 0, bytes.Length);
            }

            Assert.Equal(7, concurrentQueue.Count);
            Assert.True(chunkedStream.HasUnwrittenData);
        }

        private (ChunkedSimpleMemoryStream chunkedStream, byte[] bytes, ConcurrentQueue<byte[]> concurrentQueue) Prepare(int bytesData, int bytesPerChunk)
        {
            var concurrentQueue = new ConcurrentQueue<byte[]>();
            var chunkedStream = new ChunkedSimpleMemoryStream(bytesPerChunk, concurrentQueue);

            var bytes = new byte[bytesData];
            var random = new Random();
            random.NextBytes(bytes);
            return (chunkedStream, bytes, concurrentQueue);
        }

    }
}
