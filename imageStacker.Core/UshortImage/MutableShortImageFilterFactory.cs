using imageStacker.Core.Abstraction;
using imageStacker.Core.UshortImage.Filters;

namespace imageStacker.Core.UshortImage
{
    public class MutableUshortImageFilterFactory : IFilterFactory<MutableUshortImage>
    {
        public bool UseVectorizedFilters = true;

        public MutableUshortImageFilterFactory(bool useVectorizedFilters = true)
        {
            UseVectorizedFilters = useVectorizedFilters;
        }

        public IFilter<MutableUshortImage> CreateAttackDecayFilter(IAttackDecayOptions options)
        {
            var vectorizedFilter = new AttackDecayVecFilter(options);
            if (vectorizedFilter.IsSupported) { return vectorizedFilter; }
            return new AttackDecayFilter(options);
        }

        public IFilter<MutableUshortImage> CreateCopyFilter(ICopyFilterOptions options) =>
            throw new System.NotImplementedException();

        public IFilter<MutableUshortImage> CreateMaxFilter(IMaxFilterOptions options)
        {
            var vectorizedFilter = new MaxVecFilter(options);
            if (vectorizedFilter.IsSupported) { return vectorizedFilter; }
            return new MaxFilter(options);
        }

        public IFilter<MutableUshortImage> CreateMinFilter(IMinFilterOptions options)
        {
            var vectorizedFilter = new MinVecFilter(options);
            if (vectorizedFilter.IsSupported) { return vectorizedFilter; }
            return new MinFilter(options);
        }
    }
}
