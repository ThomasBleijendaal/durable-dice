using System.Collections;
using DurableDice.Common.Models.State;
using NUnit.Framework;

namespace DurableDice.Common.Tests.Geometry.TestCases;

public class NeighboringFieldsTestCaseSource : IEnumerable
{
    public IEnumerator GetEnumerator()
    {
        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(0, 0) } },
            new Field() { Coordinates = { new Coordinate(0, 1) } },
            true).SetName("NeighboringFieldsTest(0,0 -> 0,1)");

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(0, 0) } },
            new Field() { Coordinates = { new Coordinate(1, 0) } },
            true).SetName("NeighboringFieldsTest(0,0 -> 1,0)");

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(0, 0) } },
            new Field() { Coordinates = { new Coordinate(1, 1) } },
            false).SetName("NeighboringFieldsTest(0,0 -> 1,1)");

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(1, 1) } },
            new Field() { Coordinates = { new Coordinate(3, 2) } },
            false).SetName("NeighboringFieldsTest(1,1 -> 3,2)");

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(0, 0) } },
            new Field() { Coordinates = { new Coordinate(0, 0) } },
            false).SetName("NeighboringFieldsTest(0,0 -> 0,0)");

        // --

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(2, 1) } },
            new Field() { Coordinates = { new Coordinate(2, 0) } },
            true).SetName("NeighboringFieldsTest(2,1 -> 2,0)");

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(2, 1) } },
            new Field() { Coordinates = { new Coordinate(1, 0) } },
            true).SetName("NeighboringFieldsTest(2,1 -> 1,0)");

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(2, 1) } },
            new Field() { Coordinates = { new Coordinate(1, 1) } },
            true).SetName("NeighboringFieldsTest(2,1 -> 1,1)");

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(2, 1) } },
            new Field() { Coordinates = { new Coordinate(2, 2) } },
            true).SetName("NeighboringFieldsTest(2,1 -> 2,2)");

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(2, 1) } },
            new Field() { Coordinates = { new Coordinate(3, 1) } },
            true).SetName("NeighboringFieldsTest(2,1 -> 3,1)");

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(2, 1) } },
            new Field() { Coordinates = { new Coordinate(3, 0) } },
            true).SetName("NeighboringFieldsTest(2,1 -> 3,0)");

        // --

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(1, 1) } },
            new Field() { Coordinates = { new Coordinate(1, 0) } },
            true).SetName("NeighboringFieldsTest(1,1 -> 1,0)");

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(1, 1) } },
            new Field() { Coordinates = { new Coordinate(0, 1) } },
            true).SetName("NeighboringFieldsTest(1,1 -> 0,1)");

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(1, 1) } },
            new Field() { Coordinates = { new Coordinate(0, 2) } },
            true).SetName("NeighboringFieldsTest(1,1 -> 0,2)");

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(1, 1) } },
            new Field() { Coordinates = { new Coordinate(1, 2) } },
            true).SetName("NeighboringFieldsTest(1,1 -> 1,2)");

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(1, 1) } },
            new Field() { Coordinates = { new Coordinate(2, 2) } },
            true).SetName("NeighboringFieldsTest(1,1 -> 2,2)");

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(1, 1) } },
            new Field() { Coordinates = { new Coordinate(2, 1) } },
            true).SetName("NeighboringFieldsTest(1,1 -> 2,1)");

        // --

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(1, 1) } },
            new Field() { Coordinates = { new Coordinate(2, 0) } },
            false).SetName("NeighboringFieldsTest(1,1 -> 2,0)");

        yield return new TestCaseData(
            new Field() { Coordinates = { new Coordinate(2, 2) } },
            new Field() { Coordinates = { new Coordinate(0, 1) } },
            false).SetName("NeighboringFieldsTest(2,2 -> 0,1)");
    }
}
