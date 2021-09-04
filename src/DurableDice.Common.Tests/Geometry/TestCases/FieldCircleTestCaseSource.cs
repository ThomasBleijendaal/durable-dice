using System.Collections;
using DurableDice.Common.Models.State;
using NUnit.Framework;

namespace DurableDice.Common.Tests.Geometry.TestCases;

public class FieldCircleTestCaseSource : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        yield return new TestCaseData(
            new Coordinate(2, 2),
            0,
            new Coordinate[]
            {
                new (2, 2)
            }).SetName("FieldCircleTest(2,2 radius 0)");

        yield return new TestCaseData(
            new Coordinate(2, 2),
            1,
            new Coordinate[] 
            {
                new (2, 2),

                new (2, 1),
                new (1, 1),
                new (1, 2),
                new (2, 3),
                new (3, 2),
                new (3, 1)
            }).SetName("FieldCircleTest(2,2 radius 1)");

        yield return new TestCaseData(
            new Coordinate(2, 2),
            2,
            new Coordinate[]
            {
                new (2, 2),

                new (2, 1),
                new (1, 1),
                new (1, 2),
                new (2, 3),
                new (3, 2),
                new (3, 1),

                new (2, 0),
                new (1, 0),
                new (0, 1),
                new (0, 2),
                new (0, 3),
                new (1, 3),
                new (2, 4),
                new (3, 3),
                new (4, 3),
                new (4, 2),
                new (4, 1),
                new (3, 0),

            }).SetName("FieldCircleTest(2,2 radius 2)");

        yield return new TestCaseData(
            new Coordinate(2, 2),
            3,
            new Coordinate[]
            {
                new (2, 2),

                new (2, 1),
                new (1, 1),
                new (1, 2),
                new (2, 3),
                new (3, 2),
                new (3, 1),

                new (2, 0),
                new (1, 0),
                new (0, 1),
                new (0, 2),
                new (0, 3),
                new (1, 3),
                new (2, 4),
                new (3, 3),
                new (4, 3),
                new (4, 2),
                new (4, 1),
                new (3, 0),

                new (2, -1),
                new (1, -1),
                new (0, 0),
                new (-1, 0),
                new (-1, 1),
                new (-1, 2),
                new (-1, 3),
                new (0, 4),
                new (1, 4),
                new (2, 5),
                new (3, 4),
                new (4, 4),
                new (5, 3),
                new (5, 2),
                new (5, 1),
                new (5, 0),
                new (4, 0),
                new (3, -1),

            }).SetName("FieldCircleTest(2,2 radius 3)");
    }
}
