using System.Collections;
using DurableDice.Common.Models.State;
using NUnit.Framework;

namespace DurableDice.Common.Tests.Geometry.TestCases
{
    public class FieldBlockSizeTestCaseSource : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            yield return new TestCaseData(
                new[] {
                    new Field() { Coordinates = { new (0,0) } }
                },
                1).SetName("LargestContinuousFieldBlockTest([0,0] -> 1)");

            yield return new TestCaseData(
                new[] {
                    new Field() { Coordinates = { new (0,0) } },
                    new Field() { Coordinates = { new (0,1) } }
                },
                2).SetName("LargestContinuousFieldBlockTest([0,0] [0,1] -> 2)");

            yield return new TestCaseData(
                new[] {
                    new Field() { Coordinates = { new (0,0) } },
                    new Field() { Coordinates = { new (0,1) } },
                    new Field() { Coordinates = { new (0,2) } }
                },
                3).SetName("LargestContinuousFieldBlockTest([0,0] [0,1] [0,2] -> 3)");

            yield return new TestCaseData(
                new[] {
                    new Field() { Coordinates = { new (0,0) } },
                    new Field() { Coordinates = { new (0,1) } },
                    new Field() { Coordinates = { new (2,0) } }
                },
                2).SetName("LargestContinuousFieldBlockTest([0,0] [0,1] [2,0] -> 2)");

            yield return new TestCaseData(
                new[] {
                    new Field() { Coordinates = { new (0,0) } },
                    new Field() { Coordinates = { new (1,1) } },
                    new Field() { Coordinates = { new (3,2) } }
                },
                1).SetName("LargestContinuousFieldBlockTest([0,0] [1,1] [2,2] -> 1)");
        }
    }
}
