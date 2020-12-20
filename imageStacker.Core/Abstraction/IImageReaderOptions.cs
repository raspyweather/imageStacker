using System.Collections.Generic;

namespace imageStacker.Core.Abstraction
{
    public interface IImageReaderOptions
    {
        public IEnumerable<string> Files { get; }
        public string FolderName { get; }
        public string Filter { get; }
    }
}
