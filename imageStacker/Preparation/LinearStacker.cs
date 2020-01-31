using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace imageStacker.Preparation
{
    public class LinearStacker : IStackPreparer
    {
        private readonly int startCount, endCount, endIdx;

        public LinearStacker(int startCount, int endCount, int endIdx)
        {
            this.startCount = startCount;
            this.endCount = endCount;
            this.endIdx = endIdx;
        }

        public IList<IList<T>> Prepare<T>(IList<T> items)
        {
            var itemsList = items.ToList();
            return itemsList.SkipLast(endCount).Select((x, i) =>
            itemsList.GetRange(i, LinearFunction(startCount, endCount, itemsList.Count, i)) as IList<T>).ToList();
        }
        internal static int LinearFunction(int start, int end, int endX, int val)
        {
            int b = start;
            float m = (end - start) * 1f / val;
            return Convert.ToInt32(Math.Min(Math.Floor(b + m * val), 1));
        }
    }
}
