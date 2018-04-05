using System;
using System.Collections.Generic;
using System.Linq;

namespace ESFA.DC.IO.PerformanceTestHarness.Model
{
    public sealed class Result : IComparable<Result>
    {
        public string Name { get; }

        public IEnumerable<GetSetRemove> Results { get; }

        public double Average { get; }

        public double AverageGet { get; }

        public double AverageSet { get; }

        public double AverageRemove { get; }

        public long Sum { get; }

        public long SumGet { get; }

        public long SumSet { get; }

        public long SumRemove { get; }

        public Result(string name, List<GetSetRemove> results)
        {
            Name = name;
            Results = results;
            SumGet = results.Sum(x => x.Get);
            SumSet = results.Sum(x => x.Set);
            SumRemove = results.Sum(x => x.Remove);
            Sum = SumSet + SumGet + SumRemove;
            AverageGet = results.Average(x => x.Get);
            AverageSet = results.Average(x => x.Set);
            AverageRemove = results.Average(x => x.Remove);
            Average = AverageSet + AverageGet + AverageRemove;
        }

        public int CompareTo(Result other)
        {
            if (Sum > other.Sum)
            {
                return -1;
            }

            if (Sum < other.Sum)
            {
                return 1;
            }

            return 0;
        }

        public override string ToString()
        {
            return $"{Name} - Sum: {Sum} [{SumSet},{SumGet},{SumRemove}]; Average: {Average} [{AverageSet},{AverageGet},{AverageRemove}]";
        }
    }
}
