namespace imageStacker.Core.Abstraction

{
    public interface ISaveInfo
    {
        int? Index { get; }

        string Filtername { get; }
    }
}
