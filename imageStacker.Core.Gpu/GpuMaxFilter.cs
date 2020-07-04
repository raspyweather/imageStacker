using Amplifier;
using Amplifier.OpenCL;
using Humanizer;
using System;

namespace imageStacker.Core.Gpu
{
    public class GpuMaxFilter : IFilter
    {
        public string Name => typeof(GpuMaxFilter).Name;

        public void Process(MutableImage currentIamge, IProcessableImage nextPicture)
        {
            

        }


        [OpenCLKernel]
        public void Fill([Global] float[] x, float value)
        {
            var compiler = new OpenCLCompiler();
            compiler.Devices.ForEach(x => Console.WriteLine($"{x.Arch.Humanize()} {x.ID.ToString()} {x.Name} {x.Type.Humanize()} {x.Vendor}"));
           /* SingleDoubleSerie emaSerie = new SingleDoubleSerie();
            int i = get_global_id(0);
            x[i] = value;*/
        }
    }
}
