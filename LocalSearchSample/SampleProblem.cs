using LocalSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocalSearchSample
{
    class SampleProblem : IProblemModel<SampleSolution>
    {
        private Random Randomizer = new Random();

        public SampleSolution RandomSolution()
        {
            return new SampleSolution(Randomizer.Next());
        }

        public SampleSolution FindRandomNeighbor(SampleSolution origin)
        {
            Thread.Sleep(1);
            return new SampleSolution(origin.BaseValue, Randomizer.NextDouble());
        }

        public SampleSolution FindBestNeighbor(SampleSolution origin)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<SampleSolution> FindAllNeighbors(SampleSolution origin)
        {
            throw new NotImplementedException();
        }
    }

    class SampleSolution : IProblemSolution
    {
        public SampleSolution(int baseVal)
        {
            BaseValue = baseVal;
            Improvement = 1;
        }

        public SampleSolution(int baseVal, double improvement)
        {
            BaseValue = baseVal;
            Improvement = improvement;
        }

        public readonly int BaseValue;
        public double Improvement { get; set; }
        public double ObjectiveValue()
        {
            return BaseValue * Improvement;
        }
    }
}
