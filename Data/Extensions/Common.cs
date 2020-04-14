using Data.ViewModel.Task;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Data.Extensions
{
   public static class Common
    {
            public static IEnumerable<T> FlattenGenaric<T>(
       this IEnumerable<T> e
    , Func<T, IEnumerable<T>> f
    ) => e.SelectMany(c => f(c).FlattenGenaric(f)).Concat(e);

        public static IEnumerable<TreeViewTask> Flatten(this IEnumerable<TreeViewTask> e) =>
        e.SelectMany(c => c.children.Flatten()).Concat(e);
    }
}
