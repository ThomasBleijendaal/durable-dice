using DurableDice.Common.Geometry;
using DurableDice.Common.Models.State;
using NUnit.Framework;

namespace DurableDice.Common.Tests.Geometry
{
    public class FieldGeneratorTests
    {
        [Test]
        public void GenerateFieldTest()
        {
            var fields = FieldGenerator.GenerateFields(new List<Player> { new Player { Id = "1" }, new Player { Id = "2" } });

            Assert.AreEqual(2, fields.GroupBy(x => x.OwnerId).Count());
            Assert.AreEqual(16, fields.GroupBy(x => x.OwnerId).First().Count());
            Assert.AreEqual(16, fields.GroupBy(x => x.OwnerId).Last().Count());
        }
    }
}
