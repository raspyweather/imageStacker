using imageStacker.Core.Abstraction;
using System.Collections.Generic;

namespace imageStacker.Core.Test.Unit.Readers
{
    public class ReaderOptions : IImageReaderOptions
    {
        public IEnumerable<string> Files { get; set; }

        public string FolderName { get; set; }

        public string Filter { get; set; }
    }
}
