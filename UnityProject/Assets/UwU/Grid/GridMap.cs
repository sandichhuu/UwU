using UnityEngine;

namespace UwU.Grid
{
    public class GridMap<T> where T : GridCell
    {
        public readonly int width;
        public readonly int height;
        public readonly T[] cells;

        public GridMap(ref T[] cells, int width, int height)
        {
            this.width = width;
            this.height = height;
            this.cells = cells;

            var length = width * height;
            if (cells == null || cells.Length != length)
                cells = new T[length];
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
            return !this.cells[index].IsObstacle;
        }

        public int Length()
        {
            return this.cells == null ? 0 : this.cells.Length;
        }

        public T this[int index]
        {
            get
            {
                return this.cells[index];
            }
        }

        public T this[int x, int y]
        {
            get
            {
                return this.cells == null ? default : this.cells[GetIndex(x, y)];
            }
        }

        public void Toggle(int index)
        {
            this.cells[index].IsObstacle = !this.cells[index].IsObstacle;
        }

        public void Toggle(int x, int y)
        {
            var index = GetIndex(x, y);
            this.cells[index].IsObstacle = !this.cells[index].IsObstacle;
        }
    }
}