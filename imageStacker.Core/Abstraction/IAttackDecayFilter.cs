namespace imageStacker.Core.ByteImage.Filters
{
    public interface IAttackDecayFilter<T> : IFilter<T> where T : IProcessableImage
    {
        float Attack { get; }
        float Decay { get; }
    }
}