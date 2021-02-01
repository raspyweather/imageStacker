using imageStacker.Core.Abstraction;
using System.Collections.Generic;

namespace imageStacker.Piping.Cli
{
    public class AttackOptions : IAttackDecayOptions
    {
        public string Name { get; set; }
        public float Attack { get; set; }
        public float Decay { get; set; }
    }
    public class MaxOptions : IMaxFilterOptions
    {
        public string Name { get; set; }
    }
    public class MinOptions : IMinFilterOptions
    {
        public string Name { get; set; }
    }

    public class ReaderOptions : IImageReaderOptions
    {
        public IEnumerable<string> Files { get; set; }

        public string FolderName { get; set; }

        public string Filter { get; set; }
    }
}
