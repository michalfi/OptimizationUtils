using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalSearch
{
    public interface ISearchStrategy
    {
        SearchResult<SolutionType> Search<SolutionType>(IProblemModel<SolutionType> problem, SolutionType initialSolution) where SolutionType : IProblemSolution;
        event SearchValueEventHandler SearchUpdate;
    }

    public interface ISearchStrategyFactory
    {
        ISearchStrategy Create();
    }
}
