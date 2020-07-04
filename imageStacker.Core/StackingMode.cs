using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace imageStacker.Core
{
    public interface IStackingMode
    {
        IList<IList<T>> Prepare<T>(IList<T> items);
    }

    public class VariableStackingMode : IStackingMode
    {
        private readonly Func<int, int> startIndexSelector, countSelector;

        public VariableStackingMode(Func<int, int> startIndexSelector, Func<int, int> endIndexSelector)
        {
            this.startIndexSelector = startIndexSelector;
            this.countSelector = endIndexSelector;
        }

        public IList<IList<T>> Prepare<T>(IList<T> items)
        {
            var itemsList = items.ToList();
            return itemsList.Select((x, i) =>
            {
                return itemsList.GetRange(startIndexSelector(i), countSelector(i)) as IList<T>;
            }).ToList();

        }
    }

    public class LinearStackingMode : IStackingMode
    {
        private readonly int startCount, endCount;

        public LinearStackingMode(int startCount, int endCount)
        {
            this.startCount = startCount;
            this.endCount = endCount;
        }

        public IList<IList<T>> Prepare<T>(IList<T> items)
        {
            var itemsList = items.ToList();
            return itemsList.SkipLast(endCount - 1).Select((x, i) =>
            {
                return itemsList.GetRange(i, LinearFunction(startCount, endCount, i, itemsList.Count - endCount)) as IList<T>;
            }).ToList();

        }
        internal static int LinearFunction(int start, int end, int val, int endX)
        {
            return Convert.ToInt32(Math.Min(Math.Floor(start + end * (val) / (endX * 1f)), end));
        }
    }

    public class SingleImageMode : IStackingMode
    {
        public IList<IList<T>> Prepare<T>(IList<T> items)
        {
            return new List<IList<T>> { items };
        }
    }

    public class StaticLengthMode : IStackingMode
    {
        private readonly int length;

        public StaticLengthMode(int length)
        {
            this.length = length;
        }

        public IList<IList<T>> Prepare<T>(IList<T> items)
        {
            var files = items.ToList();
            return files.SkipLast(length - 1).Select((x, i) => (IList<T>)files.GetRange(i, length)).ToList();
        }
    }
}
