using Amplifier.OpenCL;
using System;
using Xunit;

namespace imageStacker.Cli.Test.Integration
{
    public class UnitTest1
    {
        [Fact]
        public void Test_Get_Info()
        {



        }

     /*   public void Execute()
        {
            //Create instance of OpenCL compiler and use device
            var compiler1 = new OpenCLCompiler();
            compiler1.UseDevice(0);

            var compiler2 = new OpenCLCompiler();
            compiler2.UseDevice(1);

            compiler1.CompileKernel(typeof(NNActivationKernels));
            compiler2.CompileKernel(typeof(NNActivationKernels));

            float[] x = new float[] { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

            x.AmplifyFor(compiler1, "Sigmoid");
            PrintArray(x);

            Console.WriteLine();

            x.AmplifyFor(compiler2, "Threshold", 0.85f);
            PrintArray(x);
        }

        private void PrintArray(float[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                Console.Write(data[i] + " ");
            }
        }*/
    }
}
