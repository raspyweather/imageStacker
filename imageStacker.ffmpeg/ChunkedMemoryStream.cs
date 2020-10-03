using System;
using System.Collections.Concurrent;
using System.IO;

namespace imageStacker.ffmpeg
{
    public class ChunkedMemoryStream : Stream
    {
        public readonly int BytesperChunk;
        public byte[] CurrentChunk;
        private readonly ConcurrentQueue<byte[]> _chunks;
        private int producedImages = 0;
        public ChunkedMemoryStream(int bytesPerChunk, ConcurrentQueue<byte[]> chunks)
        {
            this.BytesperChunk = bytesPerChunk;
            this.CurrentChunk = new byte[bytesPerChunk];
            _chunks = chunks;
        }
        public override bool CanRead => false;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => BytesperChunk;

        public override long Position { get; set; }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
            {
                Position = offset;
            }
            else if (origin == SeekOrigin.Current)
            {
                Position += offset;
            }
            else if (origin == SeekOrigin.End)
            {
                Position = Length + Position;
            }

            return Position;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {

            for (int i = 0; i < count; i++)
            {
                CurrentChunk[Position] = buffer[offset + i];
                Position++;
                CheckForNextImage();
            }
        }

        public bool HasUnwrittenData => this.Position != 0;

        private void CheckForNextImage()
        {
            if (Position == Length)
            {
                _chunks.Enqueue(CurrentChunk);
                CurrentChunk = new byte[Length];
                Position = 0;
            }
        }
    }
}
