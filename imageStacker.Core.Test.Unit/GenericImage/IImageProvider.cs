namespace imageStacker.Core.Test.Unit.GenericImage
{
    public interface IImageProvider<T> where T : IProcessableImage
    {
        T PrepareEmptyImage();
        T PrepareNoisyImage();
        T PreparePrefilledImage(int value);
    }
}
