using UnityEngine;

namespace UwU.Grid
{
    public class GridMap
    {
        public readonly int width;
        public readonly int height;
        public readonly GridCell[] cells;

        public GridMap(int width, int height)
        {
            this.width = width;
            this.height = height;
            this.cells = new GridCell[width * height];
        }

        public int GetIndex(int x, int y)
        {
            return y * this.width + x;
        }

        public Vector2Int GetLocation(int index)
        {
            return new Vector2Int(index % this.width, index / this.width);
        }

        public bool IsWalkable(int index)
        {
            return this.cells[index].Walkable;
        }
    }
}