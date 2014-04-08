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
        [Fact]
        public void Search_Threading_RespectsWorkerCountSetting()
        {
            foreach (int workerCount in new int[] { 1, 2, 4 })
            {
                ISearchStrategy strategy = A.Fake<ISearchStrategy>();
                IProblemModel<IProblemSolution> problem = A.Fake<IProblemModel<IProblemSolution>>();
                ISearchStrategyFactory strategyFactory = A.Fake<ISearchStrategyFactory>();
                A.CallTo(() => strategyFactory.Create()).Returns(strategy);
                A.CallTo(() => strategy.Search(A<IProblemModel<IProblemSolution>>._, A<IProblemSolution>._))
                    .Invokes((IProblemModel<IProblemSolution> p, IProblemSolution i) => Thread.Sleep(100))
                    .ReturnsLazily((IProblemModel<IProblemSolution> p, IProblemSolution i) => new SearchResult<IProblemSolution>() { BestSolution = i });
                DeterministicTaskScheduler scheduler = new DeterministicTaskScheduler();

                SearchEngine<IProblemSolution> engine = new SearchEngine<IProblemSolution>(problem, strategyFactory, scheduler) { WorkerProcessCount = workerCount };
                var searchTask = engine.SearchAsync(new CancellationTokenSource(50));
                scheduler.RunTasksUntilIdle();
                searchTask.Wait();

                A.CallTo(() => strategy.Search(A<IProblemModel<IProblemSolution>>._, A<IProblemSolution>._))
                    .MustHaveHappened(Repeated.Exactly.Times(workerCount));
            }
        }

        [Fact]
        public void Search_Threading_EndsAfterCancel()
        {
            ISearchStrategyFactory strategyFactory = A.Fake<ISearchStrategyFactory>();
            ISearchStrategy strategy = A.Fake<ISearchStrategy>();
            IProblemModel<IProblemSolution> problem = A.Fake<IProblemModel<IProblemSolution>>();
            A.CallTo(() => strategyFactory.Create()).Returns(strategy);
            A.CallTo(() => strategy.Search(A<IProblemModel<IProblemSolution>>._, A<IProblemSolution>._))
                .ReturnsLazily((IProblemModel<IProblemSolution> p, IProblemSolution i) => new SearchResult<IProblemSolution>() { BestSolution = i });
            DeterministicTaskScheduler scheduler = new DeterministicTaskScheduler();

            SearchEngine<IProblemSolution> engine = new SearchEngine<IProblemSolution>(problem, strategyFactory, scheduler);
            CancellationTokenSource cts = new CancellationTokenSource(50);
            var searchTask = engine.SearchAsync(cts);
            scheduler.RunTasksUntilIdle();
            int callsAfterCancel = 0;
            A.CallTo(() => strategyFactory.Create())
                .Invokes(() => callsAfterCancel++ )
                .Returns(strategy);
            searchTask.Wait();

            Assert.Equal(0, callsAfterCancel);
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
                DeterministicTaskScheduler scheduler = new DeterministicTaskScheduler();

                SearchEngine<IProblemSolution> engine = new SearchEngine<IProblemSolution>(problem, strategyFactory, scheduler);
                var searchTask = engine.SearchAsync(new CancellationTokenSource(100));
                scheduler.RunTasksUntilIdle();
                searchTask.Wait();

                Assert.Equal(step > 0 ? (double)1 : (double)value, searchTask.Result.ObjectiveValue());
            }
        }

        [Fact]
        public void Search_BestSolutionImproved_EventIsRaised()
        {
            ISearchStrategyFactory strategyFactory = A.Fake<ISearchStrategyFactory>();
            ISearchStrategy strategy = A.Fake<ISearchStrategy>();
            IProblemModel<IProblemSolution> problem = A.Fake<IProblemModel<IProblemSolution>>();
            A.CallTo(() => strategyFactory.Create()).Returns(strategy);
            A.CallTo(() => strategy.Search(A<IProblemModel<IProblemSolution>>._, A<IProblemSolution>._))
                .ReturnsLazily((IProblemModel<IProblemSolution> p, IProblemSolution i) => new SearchResult<IProblemSolution>() { BestSolution = i });
            A.CallTo(() => problem.RandomSolution()).Returns(FakeSolution.CreateFakeSolution(1));
            A.CallTo(() => problem.RandomSolution()).Returns(FakeSolution.CreateFakeSolution(2)).NumberOfTimes(4);
            DeterministicTaskScheduler scheduler = new DeterministicTaskScheduler();

            List<double> bestSolutionUpdates = new List<double>();
            SearchEngine<IProblemSolution> engine = new SearchEngine<IProblemSolution>(problem, strategyFactory, scheduler);
            engine.BestSolutionImproved += (s, e) => bestSolutionUpdates.Add(e.Value);
            CancellationTokenSource cts = new CancellationTokenSource(50);
            var searchTask = engine.SearchAsync(cts);
            scheduler.RunTasksUntilIdle();
            searchTask.Wait();

            List<double> expectedUpdates = new List<double>() { 2, 1 };
            Assert.Equal(expectedUpdates, bestSolutionUpdates);
        }

        [Fact]
        public void Search_StrategySendsUpdate_EventIsRaised()
        {
            ISearchStrategyFactory strategyFactory = A.Fake<ISearchStrategyFactory>();
            ISearchStrategy strategy = A.Fake<ISearchStrategy>();
            IProblemModel<IProblemSolution> problem = A.Fake<IProblemModel<IProblemSolution>>();
            A.CallTo(() => strategyFactory.Create()).Returns(strategy);
            A.CallTo(() => strategy.Search(A<IProblemModel<IProblemSolution>>._, A<IProblemSolution>._))
                .ReturnsLazily((IProblemModel<IProblemSolution> p, IProblemSolution i) => new SearchResult<IProblemSolution>() { BestSolution = i });
            A.CallTo(() => strategy.Search(A<IProblemModel<IProblemSolution>>._, A<IProblemSolution>._))
                .Invokes((IProblemModel<IProblemSolution> p, IProblemSolution i) => strategy.SearchUpdate += Raise.With(new SearchValueEventArgs(33)).Now)
                .ReturnsLazily((IProblemModel<IProblemSolution> p, IProblemSolution i) => new SearchResult<IProblemSolution>() { BestSolution = i }).Once();
            A.CallTo(() => problem.RandomSolution()).Returns(FakeSolution.CreateFakeSolution(1));
            A.CallTo(() => problem.RandomSolution()).Returns(FakeSolution.CreateFakeSolution(2)).NumberOfTimes(4);
            DeterministicTaskScheduler scheduler = new DeterministicTaskScheduler();

            List<double> workerUpdates = new List<double>();
            SearchEngine<IProblemSolution> engine = new SearchEngine<IProblemSolution>(problem, strategyFactory, scheduler);
            engine.WorkerSolutionUpdate += (s, e) => workerUpdates.Add(e.Value);
            CancellationTokenSource cts = new CancellationTokenSource(50);
            var searchTask = engine.SearchAsync(cts);
            scheduler.RunTasksUntilIdle();
            searchTask.Wait();

            List<double> expectedUpdates = new List<double>() { 33 };
            Assert.Equal(expectedUpdates, workerUpdates);
        }
    }
}