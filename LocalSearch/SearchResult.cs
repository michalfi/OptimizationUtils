using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalSearch
{
    public class SearchResult<SolutionType> where SolutionType : IProblemSolution
    {
        public SolutionType BestSolution { get; set; }
        public IEnumerable<SolutionType> PromisingSolutions { get; set; }
    }
}
