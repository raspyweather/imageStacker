namespace imageStacker.Core.Abstraction
{
    public interface IFilterFactory<T> where T : IProcessableImage
    {
        public IFilter<T> CreateMaxFilter(IMaxFilterOptions options);
        public IFilter<T> CreateMinFilter(IMinFilterOptions options);
        public IFilter<T> CreateAttackDecayFilter(IAttackDecayOptions options);
    }
}
