using System.Collections;
using DurableDice.Common.Models.State;
using NUnit.Framework;

namespace DurableDice.Common.Tests.Geometry.TestCases
{
    public class CompoundNeighboringFieldsTestCaseSource : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new TestCaseData(
                new Field() { Coordinates = { new (0,0), new (0,1), new (0,2) } },
                new Field() { Coordinates = { new (1,0) } },
                true).SetName("NeighboringFieldsTest([0,0 0,1 0,2] -> 1,0)");

            yield return new TestCaseData(
                new Field() { Coordinates = { new(0, 0), new(0, 1), new(0, 2) } },
                new Field() { Coordinates = { new(1, 0), new(2, 0) } },
                true).SetName("NeighboringFieldsTest([0,0 0,1 0,2] -> [1,0 2,0])");

            yield return new TestCaseData(
                new Field() { Coordinates = { new(0, 0), new(0, 1), new(0, 2) } },
                new Field() { Coordinates = { new(2, 0), new(3, 0) } },
                false).SetName("NeighboringFieldsTest([0,0 0,1 0,2] -> [2,0, 3,0])");

            yield return new TestCaseData(
                new Field() { Coordinates = { new(0, 0), new(0, 1), new(1, 0) } },
                new Field() { Coordinates = { new(1, 1) } },
                true).SetName("NeighboringFieldsTest([0,0 0,1 1,0] -> 1,1)");

            yield return new TestCaseData(
                new Field() { Coordinates = { new(0, 0), new(0, 1), new(1, 0) } },
                new Field() { Coordinates = { new(3, 0) } },
                false).SetName("NeighboringFieldsTest([0,0 0,1 1,0] -> 3,0)");
        }
    }
}
