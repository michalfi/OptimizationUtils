using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalSearch
{
    public interface IProblemModel<SolutionType> where SolutionType : IProblemSolution
    {
        SolutionType RandomSolution();

        SolutionType FindRandomNeighbor(SolutionType origin);

        SolutionType FindBestNeighbor(SolutionType origin);

        IEnumerable<SolutionType> FindAllNeighbors(SolutionType origin);
    }
}
