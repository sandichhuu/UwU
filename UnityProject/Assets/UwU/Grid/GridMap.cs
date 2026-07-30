using UnityEngine;
using UwU.Common;

namespace UwU.Grid
{
    public class GridMap<TCell, TData> where TCell : GridCell, new() where TData : GridData, new()
    {
        public readonly int width;
        public readonly int height;
        public float space;
        public float cellSize;
        public Dimension dimension;
        public readonly TCell[] cells;

        public GridMap(TData gridData, Dimension dimension)
        {
            this.width = gridData.width;
            this.height = gridData.height;
            this.space = gridData.space;
            this.cellSize = gridData.cellSize;
            this.dimension = dimension;
            var length = this.width * this.height;

            this.cells = new TCell[length];
            for (var i = 0; i < length; i++)
            {
                this.cells[i] = new TCell();
            }

            for (var i = 0; i < gridData.obstacles.Length; i++)
            {
                var obstacle = gridData.obstacles[i];
                this.cells[obstacle].IsObstacle = true;
            }
        }

        public GridMap(ref TCell[] cells, int width, int height)
        {
            this.width = width;
            this.height = height;
            this.cells = cells;
        }

        public void SetConfig(Dimension dimension, float space, float cellSize)
        {
            this.dimension = dimension;
            this.space = space;
            this.cellSize = cellSize;
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

        public TCell this[int index]
        {
            get
            {
                if (index < this.cells.Length)
                    return this.cells[index];

                return null;
            }
        }

        public TCell this[Vector2Int location]
        {
            get
            {
                var index = GetIndex(location.x, location.y);
                if (index < this.cells.Length)
                    return this.cells[index];

                return null;
            }
        }

        public TCell this[int x, int y]
        {
            get
            {
                var index = GetIndex(x, y);
                if (index < this.cells.Length)
                    return this.cells[index];

                return null;
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

        public Vector3 GetCellPosition(Vector3 rootPosition, int index)
        {
            if (this.dimension == Dimension.Two)
            {
                var gap = 0.5f * (this.space - this.cellSize);
                var offsetX = 0.5f * (this.width * this.space) - gap;
                var offsetY = 0.5f * (this.height * this.space) - gap;
                var location = GetLocation(index);
                return rootPosition - new Vector3(offsetX, offsetY, 0) + new Vector3(location.x, location.y, 0) * this.space;
            }
            else
            {
                var offsetX = 0.5f * ((this.width - 1.0f) * this.space);
                var offsetY = 0.5f * ((this.height - 1.0f) * this.space);
                var location = GetLocation(index);
                return rootPosition - new Vector3(offsetX, 0, offsetY) + new Vector3(location.x, 0, location.y) * this.space;
            }
        }

        public Vector3 GetCellPosition(Vector3 rootPosition, int x, int y)
        {
            if (this.dimension == Dimension.Two)
            {
                var gap = 0.5f * (this.space - this.cellSize);
                var offsetX = 0.5f * (this.width * this.space) - gap;
                var offsetY = 0.5f * (this.height * this.space) - gap;
                return rootPosition - new Vector3(offsetX, offsetY, 0) + new Vector3(x, y, 0) * this.space;
            }
            else
            {
                var offsetX = 0.5f * ((this.width - 1.0f) * this.space);
                var offsetY = 0.5f * ((this.height - 1.0f) * this.space);
                return rootPosition - new Vector3(offsetX, 0, offsetY) + new Vector3(x, 0, y) * this.space;
            }
        }

        public Vector3 GetCellPosition(Vector3 rootPosition, Vector2Int location)
        {
            if (this.dimension == Dimension.Two)
            {
                var gap = 0.5f * (this.space - this.cellSize);
                var offsetX = 0.5f * (this.width * this.space) - gap;
                var offsetY = 0.5f * (this.height * this.space) - gap;
                return rootPosition - new Vector3(offsetX, offsetY, 0) + new Vector3(location.x, location.y, 0) * this.space;
            }
            else
            {
                var offsetX = 0.5f * ((this.width - 1.0f) * this.space);
                var offsetY = 0.5f * ((this.height - 1.0f) * this.space);
                return rootPosition - new Vector3(offsetX, 0, offsetY) + new Vector3(location.x, 0, location.y) * this.space;
            }
        }
    }
}