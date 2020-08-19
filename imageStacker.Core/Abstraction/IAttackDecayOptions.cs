namespace imageStacker.Core.Abstraction
{
    public interface IAttackDecayOptions : IFilterOptions
    {
        public float Attack { get; set; }
        public float Decay { get; set; }
    }
}
