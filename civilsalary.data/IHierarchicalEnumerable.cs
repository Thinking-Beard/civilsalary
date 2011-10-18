using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace civilsalary.data
{
    public interface IHierarchicalEnumerable<out T> //TODO: inherit IGrouping<T, T> ?
    {
        IEnumerable<T> Children { get; }
    }
}
