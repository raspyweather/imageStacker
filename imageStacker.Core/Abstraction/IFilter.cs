namespace imageStacker.Core
{
    public interface IFilter<T> where T : IProcessableImage
    {
        public string Name { get; }

        public bool IsSupported { get; }

        public void Process(T currentIamge, T nextPicture);
    }
}