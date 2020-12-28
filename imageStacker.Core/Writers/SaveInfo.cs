using imageStacker.Core.Abstraction;

namespace imageStacker.Core.Writers

{
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
}
