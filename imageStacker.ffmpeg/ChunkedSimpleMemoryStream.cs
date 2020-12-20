using imageStacker.Core.Abstraction;
using System;
using System.IO;

namespace imageStacker.ffmpeg
{
    public class ChunkedSimpleMemoryStream : Stream
    {
        public readonly int BytesperChunk;
        public byte[] CurrentChunk;
        private readonly IBoundedQueue<byte[]> _chunks;

        public ChunkedSimpleMemoryStream(int bytesPerChunk, IBoundedQueue<byte[]> chunks)
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
            int currentChunkCapacity = (int)(Length - Position);

            int fillCurrentImage(int sourceBytesToCopy, int sourceOffset)
            {
                for (var to = sourceBytesToCopy + sourceOffset; sourceOffset < to; sourceOffset++)
                {
                    CurrentChunk[Position++] = buffer[sourceOffset];
                }

                CheckForNextImage();
                return sourceOffset;
            }

            int copyCompleteImage(int offset)
            {
                byte[] nextChunk = new byte[Length];
                Array.Copy(buffer, offset, nextChunk, 0, Length);
                _chunks.Enqueue(nextChunk);
                return (int)(offset + Length);
            }

            if (currentChunkCapacity > count)
            {
                fillCurrentImage(count, offset);
                return;
            }

            // current image is too small
            if (count > currentChunkCapacity)
            {
                offset = fillCurrentImage(currentChunkCapacity, offset);
            }

            int remainingCounted = count - offset;

            // copy image-block-wise
            for (; remainingCounted >= Length; remainingCounted = count - offset)
            {
                offset = copyCompleteImage(offset);
            }

            // fill until source is depleted
            if (remainingCounted > 0)
            {
                fillCurrentImage(remainingCounted, offset);
                return;
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
