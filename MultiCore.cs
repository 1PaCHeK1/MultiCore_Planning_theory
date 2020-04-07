using System;
using System.Collections.Generic;
using System.Linq;

namespace MultiCoreAlgorithm
{
    public class MultiCore
    {
        public class Work
        {
            public int Index;
            public double Lenght;

            public Work()
            {
            }

            public Work(int index, double lenght)
            {
                Index = index;
                Lenght = lenght;
            }
        }

        public class Process
        {
            public List<Work> Work;

            public double Start;
            public double End;

            public double lenght() => End - Start;

            public Process(double start, double end)
            {
                Work = new List<Work>();
                Start = start;
                End = end;
            }

            public Process(double start, double end, IEnumerable<Work> _work)
            {
                Work = new List<Work>(_work);
                Start = start;
                End = end;
            }

            public Process(double start, double end, Work _work)
            {
                Work = new List<Work>() { _work };
                Start = start;
                End = end;
            }
        }

        public class Core
        {
            public List<Process> Processes { get; set; }
            public double Performance { get; set; }

            public Core(){}

            public Core(double performance)
            {
                Processes = new List<Process>();
                Performance = performance;
            }
        }

        public double W { get; private set; }
        private List<Core> P { get; set; }
        private List<Work> T { get; set; }

        public MultiCore(IEnumerable<double> _T, IEnumerable<double> _P)
        {
            if (!_T.Any() || !_P.Any())
                throw new Exception("Один из массивов пуст");

            T = new List<Work>();
            int index = 1;
            foreach (var i in _T)
                T.Add(new Work(index++, i));

            T = T.OrderBy(e => e.Lenght).Reverse().ToList();

            P = new List<Core>();
            foreach (var i in _P)
                P.Add(new Core(i));

            P = P.OrderBy(e => e.Performance).Reverse().ToList();

            double max = 0;
            {
                double sumT = 0;
                double sumP = 0;

                for (int i = 0; i < P.Count() && i < T.Count(); i++)
                {
                    sumT += T[i].Lenght;
                    sumP += P[i].Performance;
                    if (max < sumT / sumP)
                        max = sumT / sumP;
                }
            }
            W = Math.Max(max, T.Sum(e => e.Lenght) / P.Sum(e => e.Performance));
        }

        public IEnumerable<Core> GetPlaning()
        {
            foreach (var time in Times())
            {
                List<int> InWork = new List<int>();
                for (int core = 0; core < P.Count(); core++)
                {
                    List<Work> max = new List<Work>() { new Work(0, -1) };
                    foreach (var i in T)
                    {
                        if (i.Lenght == 0 || (core > 0 && IsEnd(core, i)))
                            continue;

                        if (max[0].Lenght < i.Lenght)
                        {
                            if (max.Count() == 1)
                                max[0] = i;
                            else
                                max = new List<Work>() { i };
                        }
                        else if (max[0].Lenght == i.Lenght)
                            max.Add(i);
                    }
                    if (max.Count() >= 1 && max[0].Lenght != -1)
                        P[core].Processes.Add(new Process(P[core].Processes.Count() != 0 ? P[core].Processes.Last().End : 0, time, max));
                }

                foreach (var core in P)
                    if (core.Processes.Count() > 0)
                        foreach (var work in core.Processes.Last().Work)
                            T.Find(e => e.Index == work.Index).Lenght -= core.Performance * core.Processes.Last().lenght() / core.Processes.Last().Work.Count();
                T = T.OrderBy(e => e.Lenght).Reverse().ToList();
            }

            return P;
        }

        private IEnumerable<double> Times()
        {
            var InList = new List<double>() { 0 };
            while (true)
            {
                var times = new List<double>();
                for (int i = 0; i < T.Count() - 1; i++)
                {
                    if (T[i].Lenght == T[i + 1].Lenght)
                        continue;

                    double[] workCount = { T.Where(e => e.Lenght == T[i].Lenght).Count(), T.Where(e => e.Lenght == T[i + 1].Lenght).Count() };
                    double[] PerformanceSum = { PerformanceSumCalc(i), PerformanceSumCalc(i + 1) };

                    var t = (T[i].Lenght - T[i + 1].Lenght) /
                        (PerformanceSum[0] / workCount[0] - PerformanceSum[1] / workCount[1])
                        + InList.Max();

                    if (t <= W && t > InList.Last())
                        times.Add(t);
                }

                if (times.Where(e => e != 0).Count() == 0)
                    break;

                InList.Add(times.Min());
                yield return times.Min();
            }
            yield return W;
        }

        private double PerformanceSumCalc(int i)
        {
            if (i < P.Count() && P[i].Processes.Count() == 0)
            {
                return P[i].Performance;
            }
            else
            {
                double sum = 0;
                foreach (var core in P)
                    if (core.Processes.Count() > 0 && core.Processes.Last().Work.Select(e => e.Lenght).ToList().IndexOf(T[i].Lenght) != -1)
                        sum += core.Performance;

                return sum;
            }
        }

        private bool IsEnd(int core, Work SearchWork)
        {
            int depth = 1;
            int count = -1;
            for (int i = core - 1; i >= 0; i--)
                if (P[i].Processes.Count() > 0 && P[i].Processes.Last().Work.Find(e => e.Index == SearchWork.Index) != null)
                {
                    count = P[i].Processes.Last().Work.Count();
                    depth++;
                }
            if (count != -1 && depth > count)
                return true;
            else
                return false;
        }
    }
}