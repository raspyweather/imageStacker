using System;
using System.Collections.Generic;
using System.Text;

namespace imageStacker.Preparation
{
    public interface IStackPreparer
    {
        IList<IList<T>> Prepare<T>(IList<T> items);
    }
}
