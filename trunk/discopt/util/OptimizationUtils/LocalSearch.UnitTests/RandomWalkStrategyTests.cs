using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Extensions;

namespace LocalSearch.UnitTests
{
    public class RandomWalkStrategyTests
    {
        [Fact]
        public void Search_NoNeighborsFound_CurrentSolutionIsReturned()
        {
            IProblemModel<IProblemSolution> problem = A.Fake<IProblemModel<IProblemSolution>>();
            IProblemSolution initial = A.Fake<IProblemSolution>();
            A.CallTo(() => problem.FindRandomNeighbor(A<IProblemSolution>._)).Returns(null);
            
            RandomWalkStrategy strategy = new RandomWalkStrategy();
            IProblemSolution solution = strategy.Search(problem, initial).BestSolution;

            Assert.Equal(initial, solution);
        }

        [Fact]
        public void Search_NoBetterNeighbors_CurrentSolutionIsReturned()
        {
            IProblemModel<IProblemSolution> problem = A.Fake<IProblemModel<IProblemSolution>>();
            IProblemSolution initial = A.Fake<IProblemSolution>();
            A.CallTo(() => initial.ObjectiveValue()).Returns(0);
            A.CallTo(() => problem.FindRandomNeighbor(A<IProblemSolution>._)).Returns(FakeSolution.CreateFakeSolution(1));

            RandomWalkStrategy strategy = new RandomWalkStrategy();
            IProblemSolution solution = strategy.Search(problem, initial).BestSolution;

            Assert.Equal(initial, solution);
        }

        [Fact]
        public void Search_StraightLineToMinimum_MinimumIsReturned()
        {
            IProblemModel<IProblemSolution> problem = A.Fake<IProblemModel<IProblemSolution>>();
            IProblemSolution initial = A.Fake<IProblemSolution>();
            A.CallTo(() => initial.ObjectiveValue()).Returns(10);
            A.CallTo(() => problem.FindRandomNeighbor(A<IProblemSolution>._)).Returns(initial);
            A.CallTo(() => problem.FindRandomNeighbor(A<IProblemSolution>._))
                .ReturnsNextFromSequence(Enumerable.Range(0, 10).Reverse().Select(i => FakeSolution.CreateFakeSolution(i)).ToArray());

            RandomWalkStrategy strategy = new RandomWalkStrategy();
            IProblemSolution solution = strategy.Search(problem, initial).BestSolution;

            Assert.Equal(.0, solution.ObjectiveValue());
        }

        [Theory]
        [InlineData(10, new int[] { 11, 8, 7, 9, 9, 13, 3, 0, 2 }, 0)]
        [InlineData(15, new int[] {14, 13, 13, 14, 9, 11, 9, 12, 8, 4, 4, 6, 3, 5, 3 }, 3)]
        public void Search_RandomNeighborSequence_MinimumIsReturned(int initialValue, int[] sequence, int expectedValue)
        {
            IProblemModel<IProblemSolution> problem = A.Fake<IProblemModel<IProblemSolution>>();
            IProblemSolution initial = A.Fake<IProblemSolution>();
            A.CallTo(() => initial.ObjectiveValue()).Returns(initialValue);
            A.CallTo(() => problem.FindRandomNeighbor(A<IProblemSolution>._)).Returns(initial);
            A.CallTo(() => problem.FindRandomNeighbor(A<IProblemSolution>._))
                .ReturnsNextFromSequence(sequence.Select(i => FakeSolution.CreateFakeSolution(i)).ToArray());

            RandomWalkStrategy strategy = new RandomWalkStrategy();
            IProblemSolution solution = strategy.Search(problem, initial).BestSolution;

            Assert.Equal((double)expectedValue, solution.ObjectiveValue());
        }
    }
}
