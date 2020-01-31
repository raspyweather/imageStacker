using System;
using System.Collections.Generic;
using System.Text;

namespace imageStacker.Preparation
{
    public class SingleImagePreparer : IStackPreparer
    {
        public IList<IList<T>> Prepare<T>(IList<T> items)
        {
            return new List<IList<T>> { items };
        }
    }
}
