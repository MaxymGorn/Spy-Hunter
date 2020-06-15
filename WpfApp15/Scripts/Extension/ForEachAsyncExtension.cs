using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WpfApp15.ViewModel;


namespace WpfApp15.Scripts.Other
{
    public static class ForEachAsyncExtension
    {
        public static IEnumerable<BedProgram> queryable(this TaskManager.ViewModel viewModel, ObservableCollection<BedProgram> bedPrograms, string name)
        {
            foreach (var dr in bedPrograms)
            {
                if (dr.Name.Contains(name))
                {
                    yield return dr;
                }
            }
        }


        public static Task ForEachAsync<T>(this IEnumerable<T> source, int dop, Func<T, Task> body)
        {
            return Task.WhenAll(from partition in Partitioner.Create(source).GetPartitions(dop)
                                select Task.Run(async delegate
                                {
                                    using (partition)
                                    {
                                        while (partition.MoveNext())
                                        {
                                            await body(partition.Current).ConfigureAwait(false);
                                        }
                                    }
                                }));
        }
        public static Task ForEachAsync<T>(this ICollection<T> source, int dop, Func<T, Task> body)
        {
            return Task.WhenAll(from partition in Partitioner.Create(source).GetPartitions(dop)
                                select Task.Run(async delegate
                                {
                                    using (partition)
                                    {
                                        while (partition.MoveNext())
                                        {
                                            await body(partition.Current).ConfigureAwait(false);
                                        }
                                    }
                                }));
        }

    }
}
