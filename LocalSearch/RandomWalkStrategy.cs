using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalSearch
{
    public class RandomWalkStrategy : ISearchStrategy
    {
        public int ImprovementRetryNumber { get; set; }
        private const int ImprovementRetryNumberDefault = 100;

        public event SearchValueEventHandler SearchUpdate;

        public RandomWalkStrategy()
        {
            ImprovementRetryNumber = ImprovementRetryNumberDefault;
        }
        public SearchResult<SolutionType> Search<SolutionType>(IProblemModel<SolutionType> problem, SolutionType initialSolution) where SolutionType: IProblemSolution
        {
            SolutionType bestSolution = initialSolution;
            bool improved = true;
            while (improved)
            {
                if (SearchUpdate != null)
                    SearchUpdate(this, new SearchValueEventArgs(bestSolution.ObjectiveValue()));
                improved = false;
                for (int i = 0; i < ImprovementRetryNumber; i++)
                {
                    SolutionType s = problem.FindRandomNeighbor(bestSolution);
                    if (s == null)
                        break;
                    if (s.ObjectiveValue() < bestSolution.ObjectiveValue())
                    {
                        bestSolution = s;
                        improved = true;
                        break;
                    }
                }
            }
            return new SearchResult<SolutionType>() { BestSolution = bestSolution };
        }
    }

    public class RandomWalkStrategyFactory : ISearchStrategyFactory
    {
        public ISearchStrategy Create()
        {
            return new RandomWalkStrategy();
        }
    }
}
