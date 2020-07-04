using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace imageStacker.Core
{
    public interface IFilter
    {
        public string Name { get; }
        public void Process(MutableImage currentIamge, IProcessableImage nextPicture);
    }

    public abstract class BasicFilter : IFilter
    {
        public abstract string Name { get; }

        public abstract void Process(MutableImage currentImage, IProcessableImage nextPicture);
    }
    public class MinFilter : BasicFilter
    {
        public override string Name => nameof(MinFilter);
        public override void Process(MutableImage currentPicture, IProcessableImage nextPicture)
        {
            int length = nextPicture.Data.Length;
            for (int i = 0; i < length; i++)
            {
                if (currentPicture.Data[i] > nextPicture.Data[i])
                {
                    currentPicture.Data[i] = nextPicture.Data[i];
                }
            }
        }
    }


    public class MaxFilter : BasicFilter
    {
        public override string Name => nameof(MaxFilter);

        public override void Process(MutableImage currentImage, IProcessableImage nextPicture)
        {
            int length = nextPicture.Data.Length;
            for (int i = 0; i < length; i++)
            {
                if (currentImage.Data[i] < nextPicture.Data[i])
                {
                    currentImage.Data[i] = nextPicture.Data[i];
                }
            }
        }
    }

    public class ExtremaFilter : BasicFilter
    {
        public override string Name => nameof(ExtremaFilter);

        private readonly int Sigma = 20;

        public override void Process(MutableImage currentImage, IProcessableImage nextPicture)
        {
            int length = nextPicture.Data.Length;
            for (int i = 0; i < length; i++)
            {
                if (currentImage.Data[i] - nextPicture.Data[i] > Sigma)
                {
                    // max
                    currentImage.Data[i] = nextPicture.Data[i];
                    continue;
                }
                if (nextPicture.Data[i] - currentImage.Data[i] > Sigma)
                {
                    // min
                    currentImage.Data[i] = nextPicture.Data[i];
                }

            }
        }

    }

}
