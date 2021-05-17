using imageStacker.Core.Abstraction;
using System.Threading.Tasks.Dataflow;

namespace imageStacker.Core.Extensions
{
    public static class TplExtensions
    {
        public static TransformBlock<TIn, TOut> WithLogging<TIn, TOut>(this TransformBlock<TIn, TOut> block, string name)
        {
            StaticLogger.Instance?.AddQueue(() => $"i{block.InputCount:D2}/o{block.OutputCount:D2}", name);
            return block;
        }
        public static BufferBlock<T> WithLogging<T>(this BufferBlock<T> block, string name)
        {
            StaticLogger.Instance?.AddQueue(() => $"i{block.Count:D2}", name);
            return block;
        }
        public static ActionBlock<T> WithLogging<T>(this ActionBlock<T> block, string name)
        {
            StaticLogger.Instance?.AddQueue(() => $"i{block.InputCount:D2}", name);
            return block;
        }
    }
}
