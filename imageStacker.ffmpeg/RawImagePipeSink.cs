using FFMpegCore.Pipes;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace imageStacker.ffmpeg
{
    internal class RawImagePipeSink : IPipeSink
    {
        private readonly int _frameSize;
        private readonly Func<byte[], Task> _onFrame;

        public TaskCompletionSource Completion;

        public RawImagePipeSink(int frameSizeInBytes, Func<byte[], Task> onFrame)
        {
            _frameSize = frameSizeInBytes;
            _onFrame = onFrame;
            this.Completion = new TaskCompletionSource();
        }

        public string GetFormat()
            => string.Empty;


        public async Task ReadAsync(Stream inputStream, CancellationToken cancellationToken)
        {
            var bufferAr = new byte[_frameSize];

            for (int bufferPos = 0; !cancellationToken.IsCancellationRequested; bufferPos = 0)
            {
                while (bufferPos < _frameSize)
                {
                    var readBytes = await inputStream.ReadAsync(bufferAr.AsMemory(bufferPos, _frameSize - bufferPos), cancellationToken);
                    if (readBytes == 0) // stream end
                    {
                        this.Completion.SetResult();
                        return;
                    }

                    bufferPos += readBytes;
                }

                if (bufferPos != _frameSize)
                {
                    var ex = new InvalidOperationException("invalid BufferPos");
                    this.Completion.SetException(ex);
                    throw ex;
                }

                await _onFrame(bufferAr);
            }
        }
    }
}
