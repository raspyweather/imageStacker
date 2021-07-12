using imageStacker.Core.Abstraction;
using imageStacker.Core.ShortImage.Filters;

namespace imageStacker.Core.ShortImage
{
    public class MutableShortImageFilterFactory : IFilterFactory<MutableShortImage>
    {
        public bool UseVectorizedFilters = true;

        public MutableShortImageFilterFactory(bool useVectorizedFilters = true)
        {
            UseVectorizedFilters = useVectorizedFilters;
        }

        public IFilter<MutableShortImage> CreateAttackDecayFilter(IAttackDecayOptions options)
        {
            var vectorizedFilter = new AttackDecayVecFilter(options);
            if (vectorizedFilter.IsSupported) { return vectorizedFilter; }
            return new AttackDecayFilter(options);
        }

        public IFilter<MutableShortImage> CreateCopyFilter(ICopyFilterOptions options) =>
            throw new System.NotImplementedException();

        public IFilter<MutableShortImage> CreateMaxFilter(IMaxFilterOptions options)
        {
            var vectorizedFilter = new MaxVecFilter(options);
            if (vectorizedFilter.IsSupported) { return vectorizedFilter; }
            return new MaxFilter(options);
        }

        public IFilter<MutableShortImage> CreateMinFilter(IMinFilterOptions options)
        {
            var vectorizedFilter = new MinVecFilter(options);
            if (vectorizedFilter.IsSupported) { return vectorizedFilter; }
            return new MinFilter(options);
        }
    }
}
