using System.Drawing.Imaging;
using System.IO;

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

    public interface IImageWriter
    {
        public void WriteFile(IProcessableImage image, ISaveInfo info);
    }

    public class ImageFileWriter : IImageWriter
    {
        private readonly string Filename, OutputFolder;

        public ImageFileWriter(string filename, string outputFolder)
        {
            Filename = filename;
            OutputFolder = outputFolder;
        }

        public void WriteFile(IProcessableImage image, ISaveInfo info)
        {
            string path = Path.Combine(OutputFolder,
                string.Join('-',
                    Filename,
                    info.Filtername,
                    info.Index.HasValue ? info.Index.Value.ToString("d6") : string.Empty) + ".png");
            File.Delete(path);
            MutableImage.ToImage(image as MutableImage).Save(path, ImageFormat.Png);
        }
    }

    /// <summary>
    /// Outputs raw RGB Byte Stream
    /// </summary>
    public class ImageStreamWriter : IImageWriter
    {
        private Stream outputStream;
        public ImageStreamWriter(Stream outputStream)
        {
            this.outputStream = outputStream;
        }

        public void WriteFile(IProcessableImage image, ISaveInfo info)
        {
            MutableImage.ToImage(MutableImage.FromProcessableImage(image)).Save(outputStream, ImageFormat.MemoryBmp);
        }
    }
}
