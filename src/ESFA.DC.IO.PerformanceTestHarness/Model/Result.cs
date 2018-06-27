using System;
using System.Collections.Generic;
using System.Linq;

namespace ESFA.DC.IO.PerformanceTestHarness.Model
{
    public sealed class Result : IComparable<Result>
    {
        public string Name { get; }

        public IEnumerable<GetSetRemove> Results { get; }

        public bool Failed { get; }

        public double Average { get; }

        public double AverageGet { get; }

        public double AverageSet { get; }

        public double AverageRemove { get; }

        public long Sum { get; }

        public long SumGet { get; }

        public long SumSet { get; }

        public long SumRemove { get; }

        public float AverageCpu { get; set; }

        public Result(string name, List<GetSetRemove> results, bool failed)
        {
            Name = name;
            Results = results;
            Failed = failed;
            SumGet = results.Sum(x => x.Get);
            SumSet = results.Sum(x => x.Set);
            SumRemove = results.Sum(x => x.Remove);
            Sum = SumSet + SumGet + SumRemove;
            AverageGet = results.Count == 0 ? 0 : results.Average(x => x.Get);
            AverageSet = results.Count == 0 ? 0 : results.Average(x => x.Set);
            AverageRemove = results.Count == 0 ? 0 : results.Average(x => x.Remove);
            Average = AverageSet + AverageGet + AverageRemove;
            AverageCpu = results.Count == 0 ? 0 : results.Average(x => x.CpuCount);
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
            return $"{Name} - Failed: {Failed}, Cpu: {AverageCpu}%, Sum: {Sum}ms [Set:{SumSet}ms,Get:{SumGet}ms,Remove:{SumRemove}ms]; Average: {Average}ms [Set:{AverageSet}ms,Get:{AverageGet}ms,Remove:{AverageRemove}ms]";
        }
    }
}
