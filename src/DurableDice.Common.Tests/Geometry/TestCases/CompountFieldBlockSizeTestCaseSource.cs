using System.Collections;
using DurableDice.Common.Models.State;
using NUnit.Framework;

namespace DurableDice.Common.Tests.Geometry.TestCases
{
    public class CompoundFieldBlockSizeTestCaseSource : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new TestCaseData(
                new[] {
                    new Field() { Coordinates = { new (0,0), new (0,1), new (1,0) } }
                },
                1).SetName("LargestContinuousFieldBlockTest([0,0 0,1 1,1] -> 1)");

            yield return new TestCaseData(
                new[] {
                    new Field() { Coordinates = { new (0,0), new (0,1), new (1,0) } },
                    new Field() { Coordinates = { new (2,2), new (3,2), new (2,3) } }
                },
                1).SetName("LargestContinuousFieldBlockTest([0,0 0,1 1,1] + [2,2 3,2 2,3] -> 1)");

            yield return new TestCaseData(
                new[] {
                    new Field() { Coordinates = { new (0,0), new (0,1), new (1,0) } },
                    new Field() { Coordinates = { new (2,2), new (3,2), new (2,3) } },
                    new Field() { Coordinates = { new (2,0), new (1,1), new (2,1), new (3,1), new (4,0) } }
                },
                3).SetName("LargestContinuousFieldBlockTest([0,0 0,1 1,1] + [2,2 3,2 2,3] + [0,2 1,1 2,1 3,0 4,0] -> 3)");
        }
    }
}
