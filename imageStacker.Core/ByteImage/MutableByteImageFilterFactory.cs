using imageStacker.Core.Abstraction;
using imageStacker.Core.ByteImage.Filters;

namespace imageStacker.Core.ByteImage
{
    public class MutableByteImageFilterFactory : IFilterFactory<MutableByteImage>
    {
        public bool UseVectorizedFilters = true;

        public MutableByteImageFilterFactory(bool useVectorizedFilters = true)
        {
            UseVectorizedFilters = useVectorizedFilters;
        }

        public IFilter<MutableByteImage> CreateAttackDecayFilter(IAttackDecayOptions options)
        {
            var vectorizedFilter = new AttackDecayVecFilter(options);
            if (vectorizedFilter.IsSupported) { return vectorizedFilter; }
            return new AttackDecayFilter(options);
        }

        public IFilter<MutableByteImage> CreateCopyFilter(ICopyFilterOptions options)
            => new CopyFilter(options);

        public IFilter<MutableByteImage> CreateMaxFilter(IMaxFilterOptions options)
        {
            var vectorizedFilter = new MaxVecFilter(options);
            if (vectorizedFilter.IsSupported) { return vectorizedFilter; }
            return new MaxFilter(options);
        }

        public IFilter<MutableByteImage> CreateMinFilter(IMinFilterOptions options)
        {
            var vectorizedFilter = new MinVecFilter(options);
            if (vectorizedFilter.IsSupported) { return vectorizedFilter; }
            return new MinFilter(options);
        }
    }
}
