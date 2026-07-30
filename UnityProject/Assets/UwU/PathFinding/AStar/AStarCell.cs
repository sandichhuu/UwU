using UwU.Grid;

namespace UwU.PathFinding.AStar
{
    [System.Serializable]
    public class AStarCell : GridCell
    {
        public int parentIndex;
        public double gCost;
        public double hCost;
        public double FCost => this.gCost + this.hCost;

        public AStarCell()
        {
            this.gCost = 0;
            this.hCost = 0;
            this.parentIndex = -1;
        }
    }
}