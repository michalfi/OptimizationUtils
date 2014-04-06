using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace LocalSearch.UnitTests
{
    public class SearchEngineTests
    {
        // TODO use http://svengrand.blogspot.cz/2013/12/unit-testing-c-code-which-starts-new.html for task testing
        /*[Fact(Skip="unreliable")]
        public void Search_Threading_RespectsWorkerCountSetting()
        {
            foreach (int workerCount in new int[] { 1, 2, 4 })
            {
                ISearchStrategy strategy = A.Fake<ISearchStrategy>();
                IProblemModel<IProblemSolution> problem = A.Fake<IProblemModel<IProblemSolution>>();
                A.CallTo(() => strategy.Search(A<IProblemModel<IProblemSolution>>._, A<IProblemSolution>._))
                    .Invokes((IProblemModel<IProblemSolution> p, IProblemSolution i) => Thread.Sleep(100))
                    .ReturnsLazily((IProblemModel<IProblemSolution> p, IProblemSolution i) => new SearchResult<IProblemSolution>() { BestSolution = i });

                SearchEngine<IProblemSolution> engine = new SearchEngine<IProblemSolution>(problem, strategy) { WorkerProcessCount = workerCount };
                engine.SearchAsync(new CancellationTokenSource(50)).Wait();

                A.CallTo(() => strategy.Search(A<IProblemModel<IProblemSolution>>._, A<IProblemSolution>._))
                    .MustHaveHappened(Repeated.Exactly.Times(workerCount));
            }
        }*/

        [Fact]
        public void Search_Threading_EndsAfterCancel()
        {
            ISearchStrategyFactory strategyFactory = A.Fake<ISearchStrategyFactory>();
            ISearchStrategy strategy = A.Fake<ISearchStrategy>();
            IProblemModel<IProblemSolution> problem = A.Fake<IProblemModel<IProblemSolution>>();
            A.CallTo(() => strategyFactory.Create()).Returns(strategy);
            A.CallTo(() => strategy.Search(A<IProblemModel<IProblemSolution>>._, A<IProblemSolution>._))
                .ReturnsLazily((IProblemModel<IProblemSolution> p, IProblemSolution i) => new SearchResult<IProblemSolution>() { BestSolution = i });

            SearchEngine<IProblemSolution> engine = new SearchEngine<IProblemSolution>(problem, strategyFactory);
            CancellationTokenSource cts = new CancellationTokenSource();
            var searchTask = engine.SearchAsync(cts);
            cts.Cancel();
            int callsAfterCancel = 0;
            A.CallTo(() => strategy.Search(A<IProblemModel<IProblemSolution>>._, A<IProblemSolution>._))
                .Invokes(() => callsAfterCancel++ )
                .ReturnsLazily((IProblemModel<IProblemSolution> p, IProblemSolution i) => new SearchResult<IProblemSolution>() { BestSolution = i });
            searchTask.Wait();

            Assert.Equal(0, callsAfterCancel);
        }

        [Fact]
        public void Search_Threading_EveryWorkerGetsItsOwnStrategy()
        {
            //TODO
        }

        [Fact]
        public void Search_ReturnsSolutionWithLowestObjective()
        {
            foreach (int step in new int[] { 1, -1 })
            {
                ISearchStrategyFactory strategyFactory = A.Fake<ISearchStrategyFactory>();
                ISearchStrategy strategy = A.Fake<ISearchStrategy>();
                IProblemModel<IProblemSolution> problem = A.Fake<IProblemModel<IProblemSolution>>();
                A.CallTo(() => strategyFactory.Create()).Returns(strategy);
                A.CallTo(() => strategy.Search(A<IProblemModel<IProblemSolution>>._, A<IProblemSolution>._))
                    .ReturnsLazily((IProblemModel<IProblemSolution> p, IProblemSolution i) => new SearchResult<IProblemSolution>() { BestSolution = i });
                int value = 0;
                A.CallTo(() => problem.RandomSolution())
                    .Invokes(() => value += step)
                    .ReturnsLazily(() => FakeSolution.CreateFakeSolution(value));

                SearchEngine<IProblemSolution> engine = new SearchEngine<IProblemSolution>(problem, strategyFactory);
                var searchTask = engine.SearchAsync(new CancellationTokenSource(100));
                searchTask.Wait();

                Assert.Equal(step > 0 ? (double)1 : (double)value, searchTask.Result.ObjectiveValue());
            }
        }
    }
}