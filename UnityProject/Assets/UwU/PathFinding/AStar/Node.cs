namespace UwU.PathFinding.AStar
{
    public struct Node
    {
        public int X;
        public int Y;
        public int ParentIndex;
        public bool IsWalkable;
        public double GCost;
        public double HCost;
        public double FCost => this.GCost + this.HCost;

        public Node(int x, int y, bool isWalkable)
        {
            this.X = x;
            this.Y = y;
            this.IsWalkable = isWalkable;
            this.GCost = 0;
            this.HCost = 0;
            this.ParentIndex = -1;
        }
    }
}