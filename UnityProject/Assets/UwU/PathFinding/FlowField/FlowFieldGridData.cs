using UwU.Data;
using UwU.Grid;

namespace UwU.PathFinding.FlowField
{
    public class FlowFieldGridData : GridData
    {
        public int[] starts;
        public int[] targets;

        public override void OnSerialize(BytePackage bytePackage)
        {
            bytePackage.WriteArray(this.starts);
            bytePackage.WriteArray(this.targets);
        }

        public override void OnDeserialize(BytePackage bytePackage)
        {
            this.starts = bytePackage.ReadArray<int>();
            this.targets = bytePackage.ReadArray<int>();
        }
    }
}