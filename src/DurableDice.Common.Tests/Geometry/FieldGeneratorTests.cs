using DurableDice.Common.Geometry;
using DurableDice.Common.Models.State;
using NUnit.Framework;

namespace DurableDice.Common.Tests.Geometry;

public class FieldGeneratorTests
{
    [Test]
    public void GenerateFieldTwoPlayerTest()
    {
        var fields = FieldGenerator.GenerateFields(new List<Player>
        {
            new Player { Id = "1" }, 
            new Player { Id = "2" } 
        });

        Assert.AreEqual(2, fields.GroupBy(x => x.OwnerId).Count());
        Assert.AreEqual(0, fields.Count(x => x.OwnerId == null));
        Assert.AreEqual(16, fields.GroupBy(x => x.OwnerId).First().Count());
        Assert.AreEqual(16, fields.GroupBy(x => x.OwnerId).Last().Count());

        Assert.AreEqual(32, fields.SelectMany(x => x.Neighbors).Distinct().Count());

        Assert.IsTrue(fields.All(field => field.Neighbors.All(neighbor => fields.First(x => x.Index == neighbor).IsNeighbor(field))));
    }

    [Test]
    public void GenerateFieldEightPlayerTest()
    {
        var fields = FieldGenerator.GenerateFields(new List<Player> 
        { 
            new Player { Id = "1" },
            new Player { Id = "2" },
            new Player { Id = "3" },
            new Player { Id = "4" },
            new Player { Id = "5" },
            new Player { Id = "6" },
            new Player { Id = "7" },
            new Player { Id = "8" }
        });

        Assert.AreEqual(8, fields.GroupBy(x => x.OwnerId).Count());
        Assert.AreEqual(0, fields.Count(x => x.OwnerId == null));
        Assert.AreEqual(4, fields.GroupBy(x => x.OwnerId).ElementAt(0).Count());
        Assert.AreEqual(4, fields.GroupBy(x => x.OwnerId).ElementAt(1).Count());
        Assert.AreEqual(4, fields.GroupBy(x => x.OwnerId).ElementAt(2).Count());
        Assert.AreEqual(4, fields.GroupBy(x => x.OwnerId).ElementAt(3).Count());
        Assert.AreEqual(4, fields.GroupBy(x => x.OwnerId).ElementAt(4).Count());
        Assert.AreEqual(4, fields.GroupBy(x => x.OwnerId).ElementAt(5).Count());
        Assert.AreEqual(4, fields.GroupBy(x => x.OwnerId).ElementAt(6).Count());
        Assert.AreEqual(4, fields.GroupBy(x => x.OwnerId).ElementAt(7).Count());
    }
}
