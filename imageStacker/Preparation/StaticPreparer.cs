using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace imageStacker.Preparation
{
    public class StaticPreparer : IStackPreparer
    {
        private readonly int length;

        public StaticPreparer(int length)
        {
            this.length = length;
        }

        public IList<IList<T>> Prepare<T>(IList<T> items)
        {
            var files = items.ToList();
            return files.SkipLast(length - 1).Select((x, i) =>(IList<T>) files.GetRange(i, length)).ToList();
        }
    }
}
