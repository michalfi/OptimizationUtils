using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace LocalSearch
{
    public class SearchEngine<SolutionType> where SolutionType : IProblemSolution
    {
        protected IProblemModel<SolutionType> Problem { get; set; }
        protected ISearchStrategyFactory StrategyFactory { get; set; }

        protected SolutionType BestSolution { get; set; }

        public SearchEngine(IProblemModel<SolutionType> problem, ISearchStrategyFactory strategyFactory)
        {
            Problem = problem;
            WorkerProcessCount = WorkerProcessCountDefault;
            StrategyFactory = strategyFactory;
        }

        public int WorkerProcessCount { get; set; }
        public const int WorkerProcessCountDefault = 4;

        public event SearchValueEventHandler BestSolutionImproved;
        public event SearchValueEventHandler WorkerSolutionUpdate;

        public async Task<SolutionType> SearchAsync(CancellationTokenSource cts)
        {
            BestSolution = Problem.RandomSolution();
            Task<SearchResult<SolutionType>>[] workers = new Task<SearchResult<SolutionType>>[WorkerProcessCount];

            while (!cts.IsCancellationRequested)
            {
                for (int i = 0; i < WorkerProcessCount; i++)
                {
                    if (workers[i] == null)
                    {
                        int workerSlot = i;
                        workers[i] = Task.Run(() =>
                        {
                            ISearchStrategy strategy = StrategyFactory.Create();
                            strategy.SearchUpdate += (s, e) =>
                            {
                                if (WorkerSolutionUpdate != null)
                                    WorkerSolutionUpdate(this, new SearchValueEventArgs(e.Value, workerSlot));
                            };
                            return strategy.Search(Problem, Problem.RandomSolution());
                        });
                    }
                }
                Task<SearchResult<SolutionType>> completedWorker = await Task.WhenAny(workers);
                RecordSearchResult(completedWorker.Result);
                for (int i = 0; i < WorkerProcessCount; i++)
                    if (workers[i] == completedWorker)
                        workers[i] = null;
            }

            await Task.WhenAll(workers.Where(w => w!= null));
            foreach (var worker in workers.Where(w => w != null))
                RecordSearchResult(worker.Result);

            return BestSolution;
        }

        protected virtual void RecordSearchResult(SearchResult<SolutionType> result)
        {
            SolutionType s = result.BestSolution;
            if (s.ObjectiveValue() < BestSolution.ObjectiveValue())
            {
                BestSolution = s;
                if (BestSolutionImproved != null)
                    BestSolutionImproved(this, new SearchValueEventArgs(s.ObjectiveValue()));
            }
            // TODO handle rest of the result where appropriate
        }

        protected virtual SolutionType PickSeed()
        {
            throw new NotImplementedException();
        }
    }
}
