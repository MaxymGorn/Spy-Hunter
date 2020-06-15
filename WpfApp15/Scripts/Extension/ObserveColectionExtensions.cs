using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManager;

namespace WpfApp15.Scripts.Other
{
    public static class  ObserveColectionExtensions
    {
        public static async Task AddAsync<TSource>(this ObservableCollection<TSource> source, params TSource[] items)
        {
            await items.ForEachAsync(items.ToList().Count, async i => { source.Add(i); });
        }
        public static ObservableCollection<T> Load<T>(this ObservableCollection<T> Collection, ICollection<T> Source)
        {
            Collection.Clear();
            Source.ForEachAsync(Source.Count, async i => { Collection.Add(i); });
            return Collection;
        }
    }
}
