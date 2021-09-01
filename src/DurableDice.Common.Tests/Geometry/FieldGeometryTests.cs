using DurableDice.Common.Geometry;
using DurableDice.Common.Models.State;
using DurableDice.Common.Tests.Geometry.TestCases;
using NUnit.Framework;

namespace DurableDice.Common.Tests.Geometry;

public class FieldGeometryTests
{
    [SetUp]
    public void Setup()
    {

    }

    [TestCaseSource(typeof(NeighboringFieldsTestCaseSource))]
    [TestCaseSource(typeof(CompoundNeighboringFieldsTestCaseSource))]
    public void NeighboringFieldsTest(Field field1, Field field2, bool areNeighbors)
    {
        field1.Id = "1";
        field2.Id = "2";

        var subject = new FieldGeometry(new[] { field1, field2 });

        Assert.AreEqual(areNeighbors, subject.AreNeighboringFields("1", "2"));
    }

    [TestCaseSource(typeof(FieldBlockSizeTestCaseSource))]
    [TestCaseSource(typeof(CompoundFieldBlockSizeTestCaseSource))]
    public void LargestContinuousFieldBlockTest(IEnumerable<Field> fields, int blocksize)
    {
        var id = 0;

        var ownedFields = fields.ToList();
        ownedFields.ForEach(x =>
        {
            x.Id = $"{++id}";
            x.OwnerId = "A";
        });

        var subject = new FieldGeometry(ownedFields);

        Assert.AreEqual(blocksize, subject.GetLargestContinuousFieldBlock("A"));
    }
}

