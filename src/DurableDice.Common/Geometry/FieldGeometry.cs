using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DurableDice.Common.Models.State;

namespace DurableDice.Common.Geometry
{
    public class FieldGeometry
    {
        public FieldGeometry()
        {
        }

        public bool AreNeighboringFields(string fieldId1, string fieldId2)
        {
            return true;
        }

        public int GetLargestContinuousFieldBlock(string ownerId)
        {
            return 32;
        }
    }
}
