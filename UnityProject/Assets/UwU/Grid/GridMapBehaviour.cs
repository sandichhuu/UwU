using UnityEngine;
using UwU.Common;

namespace UwU.Grid
{
    public class GridMapBehaviour<T> : MonoBehaviour where T : GridCell
    {
        [Space]
        [Header("Map")]
        [SerializeField] protected int width = 10;
        [SerializeField] protected int height = 10;
        [Space]
        [Header("Config")]
        [SerializeField] protected float space = 1.0f;
        [SerializeField] protected float cellSize = 1.0f;
        [SerializeField] protected Dimension dimension;

        [SerializeField, HideInInspector] protected T[] cells;

        protected GridMap<T> gridMap;

        public GridMap<T> GetGridMap()
        {
            var length = this.width * this.height;

            if (this.cells == null || this.cells.Length != length)
                this.cells = new T[length];

            if (this.gridMap == null ||
                this.gridMap.Length() != length)
            {
                this.gridMap = new GridMap<T>(ref this.cells, this.width, this.height);
            }

            return this.gridMap;
        }

        public Vector3 GetCellPosition(int index)
        {
            var gridMap = GetGridMap();
            if (this.dimension == Dimension.Two)
            {
                var gap = 0.5f * (this.space - this.cellSize);
                var offsetX = 0.5f * (this.width * this.space) - gap;
                var offsetY = 0.5f * (this.height * this.space) - gap;
                var location = gridMap.GetLocation(index);
                return this.transform.position - new Vector3(offsetX, offsetY, 0) + new Vector3(location.x, location.y, 0) * this.space;
            }
            else
            {
                var offsetX = 0.5f * ((gridMap.width - 1.0f) * this.space);
                var offsetY = 0.5f * ((gridMap.height - 1.0f) * this.space);
                var location = gridMap.GetLocation(index);
                return this.transform.position - new Vector3(offsetX, 0, offsetY) + new Vector3(location.x, 0, location.y) * this.space;
            }
        }

        public Vector3 GetCellPosition(int x, int y)
        {
            var gridMap = GetGridMap();
            if (this.dimension == Dimension.Two)
            {
                var gap = 0.5f * (this.space - this.cellSize);
                var offsetX = 0.5f * (this.width * this.space) - gap;
                var offsetY = 0.5f * (this.height * this.space) - gap;
                return this.transform.position - new Vector3(offsetX, offsetY, 0) + new Vector3(x, y, 0) * this.space;
            }
            else
            {
                var offsetX = 0.5f * ((gridMap.width - 1.0f) * this.space);
                var offsetY = 0.5f * ((gridMap.height - 1.0f) * this.space);
                return this.transform.position - new Vector3(offsetX, 0, offsetY) + new Vector3(x, 0, y) * this.space;
            }
        }
    }
}