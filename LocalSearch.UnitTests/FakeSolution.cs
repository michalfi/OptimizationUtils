using FakeItEasy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LocalSearch.UnitTests
{
    public static class FakeSolution
    {
        public static IProblemSolution CreateFakeSolution(int value)
        {
            IProblemSolution s = A.Fake<IProblemSolution>();
            A.CallTo(() => s.ObjectiveValue()).Returns(value);
            return s;
        }
    }
}
