using FFMpegCore.Exceptions;
using FFMpegCore.Pipes;
using imageStacker.Core;
using imageStacker.Core.Abstraction;
using imageStacker.Core.ByteImage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace imageStacker.ffmpeg
{
    public class RawVideoPipeSourceAsync : IPipeSource
    {
        public string StreamFormat { get; private set; } = null!;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int FrameRate { get; set; } = 25;
        private bool _formatInitialized;
        private readonly IAsyncEnumerator<IVideoFrame> _framesEnumerator;

        public RawVideoPipeSourceAsync(IAsyncEnumerator<IVideoFrame> framesEnumerator)
        {
            _framesEnumerator = framesEnumerator;
        }

        public RawVideoPipeSourceAsync(IAsyncEnumerable<IVideoFrame> framesEnumerator) : this(framesEnumerator.GetAsyncEnumerator()) { }

        public string GetFormat()
        {
            if (!_formatInitialized)
            {
                //see input format references https://lists.ffmpeg.org/pipermail/ffmpeg-user/2012-July/007742.html
                if (_framesEnumerator.Current == null)
                {
                    var task = _framesEnumerator.MoveNextAsync();
                    task.AsTask().Wait();
                    if (!task.Result)
                        throw new InvalidOperationException("Enumerator is empty, unable to get frame");
                }
                StreamFormat = _framesEnumerator.Current!.Format;
                Width = _framesEnumerator.Current!.Width;
                Height = _framesEnumerator.Current!.Height;

                _formatInitialized = true;
            }

            return $"-f rawvideo -r {FrameRate} -pix_fmt {StreamFormat} -s {Width}x{Height}";
        }

        public async Task CopyAsync(System.IO.Stream outputStream, CancellationToken cancellationToken)
        {
            if (_framesEnumerator.Current != null)
            {
                CheckFrameAndThrow(_framesEnumerator.Current);
                await _framesEnumerator.Current.SerializeAsync(outputStream, cancellationToken).ConfigureAwait(false);
            }

            while (await _framesEnumerator.MoveNextAsync())
            {
                CheckFrameAndThrow(_framesEnumerator.Current!);
                await _framesEnumerator.Current!.SerializeAsync(outputStream, cancellationToken).ConfigureAwait(false);
            }
        }

        private void CheckFrameAndThrow(IVideoFrame frame)
        {
            if (frame.Width != Width || frame.Height != Height || frame.Format != StreamFormat)
                throw new FFMpegException(FFMpegExceptionType.Operation, "Video frame is not the same format as created raw video stream\r\n" +
                    $"Frame format: {frame.Width}x{frame.Height} pix_fmt: {frame.Format}\r\n" +
                    $"Stream format: {Width}x{Height} pix_fmt: {StreamFormat}");
        }
    }

    public class RawMutableByteFramePipeSourceAsync : IPipeSource
    {
        public string StreamFormat { get; private set; } = null!;
        public int Width { get; private set; }
        public int Height { get; private set; }
        public int FrameRate { get; set; } = 25;
        public readonly IAsyncEnumerator<IVideoFrame> FramesEnumerator;
        private bool _formatInitialized;
        private ILogger logger;

        public RawMutableByteFramePipeSourceAsync(IAsyncEnumerator<IVideoFrame> framesEnumerator)
        {
            FramesEnumerator = framesEnumerator;
        }

        public RawMutableByteFramePipeSourceAsync(IBoundedQueue<MutableByteImage> framesEnumerator, ILogger logger) :
            this(new BoundedQueueAsyncEnumerator(framesEnumerator, logger))
        {
            this.logger = logger;
        }

        public class BoundedQueueAsyncEnumerator : IAsyncEnumerator<IVideoFrame>
        {
            private readonly IBoundedQueue<MutableByteImage> _queue;

            public BoundedQueueAsyncEnumerator(IBoundedQueue<MutableByteImage> queue, ILogger logger)
            {
                _queue = queue;
                this.logger = logger;
            }

            private IVideoFrame _currentItem;
            private ILogger logger;
            public IVideoFrame Current => _currentItem;

            public ValueTask DisposeAsync()
            {
                // TODO: Dispose CurrentItem for clearing reference
                return new ValueTask();
            }

            public ValueTask<bool> MoveNextAsync()
            {
                return new ValueTask<bool>(GetNextEntry());
            }

            private async Task<bool> GetNextEntry()
            {
                var result = await _queue.DequeueOrDefault();
                _currentItem = new FfmpegVideoFrame(result);
                return result != default;
            }
        }

        public string GetFormat()
        {
            if (!_formatInitialized)
            {
                //see input format references https://lists.ffmpeg.org/pipermail/ffmpeg-user/2012-July/007742.html
                if (FramesEnumerator.Current == null)
                {
                    var task = FramesEnumerator.MoveNextAsync();
                    task.AsTask().Wait();
                    if (!task.Result)
                        throw new InvalidOperationException("Enumerator is empty, unable to get frame");
                }
                StreamFormat = FramesEnumerator.Current!.Format;
                Width = FramesEnumerator.Current!.Width;
                Height = FramesEnumerator.Current!.Height;

                _formatInitialized = true;
            }

            return $"-f rawvideo -r {FrameRate} -pix_fmt {StreamFormat} -s {Width}x{Height}";
        }

        public async Task CopyAsync(Stream outputStream, CancellationToken cancellationToken)
        {
            int i = 0;
            if (FramesEnumerator.Current != null)
            {
                logger.NotifyFillstate(i++, "FramesEnumerator");
                CheckFrameAndThrow(FramesEnumerator.Current);
                await FramesEnumerator.Current.SerializeAsync(outputStream, cancellationToken).ConfigureAwait(false);
            }

            while (await FramesEnumerator.MoveNextAsync())
            {
                logger.NotifyFillstate(i++, "FramesEnumerator");
                CheckFrameAndThrow(FramesEnumerator.Current!);
                await FramesEnumerator.Current!.SerializeAsync(outputStream, cancellationToken).ConfigureAwait(false);
            }
            logger.WriteLine("Enumerator ended", Verbosity.Info);
        }

        private void CheckFrameAndThrow(IVideoFrame frame)
        {
            if (frame.Width != Width || frame.Height != Height || frame.Format != StreamFormat)
                throw new FFMpegException(FFMpegExceptionType.Operation, "Video frame is not the same format as created raw video stream\r\n" +
                    $"Frame format: {frame.Width}x{frame.Height} pix_fmt: {frame.Format}\r\n" +
                    $"Stream format: {Width}x{Height} pix_fmt: {StreamFormat}");
        }
    }

}
