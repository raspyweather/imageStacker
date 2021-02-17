using FFMpegCore.Pipes;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace imageStacker.ffmpeg
{
    class RawImagePipeSink : IPipeSink
    {
        private readonly int _frameSize;
        private readonly Func<byte[], Task> _onFrame;

        public RawImagePipeSink(int frameSizeInBytes, Func<byte[], Task> onFrame)
        {
            _frameSize = frameSizeInBytes;
            _onFrame = onFrame;
        }

        public string GetFormat()
        {
            return string.Empty;
        }

        public async Task ReadAsync(Stream inputStream, CancellationToken cancellationToken)
        {
            var buffer = new byte[_frameSize];
            int bufferPos = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                while (bufferPos < _frameSize)
                {
                    var readBytes = await inputStream.ReadAsync(buffer, bufferPos, _frameSize - bufferPos);
                    if (readBytes == 0) // stream end
                        return;
                    bufferPos += readBytes;
                }
                if (bufferPos != _frameSize)
                    throw new InvalidOperationException("invalid BufferPos");
                await _onFrame(buffer);
                bufferPos = 0;
            }
        }
    }
}
