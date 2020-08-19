using imageStacker.Core.Abstraction;
using System;
using System.Collections.Generic;
using System.Text;

namespace imageStacker.Cli
{
    public class ReaderOptions : IImageReaderOptions
    {
        public IEnumerable<string> Files { get; set; }

        public string FolderName { get; set; }

        public string Filter { get; set; }
    }
}
