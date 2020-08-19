using System;
using System.Collections.Generic;
using System.Text;

namespace imageStacker.Core.Abstraction
{
    public interface IImageReaderOptions
    {
        public IEnumerable<string> Files { get; }
        public string FolderName { get; }
        public string Filter { get; }
    }
}
