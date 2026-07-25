using System;
using UnityEngine;
using UwU.Common;

namespace UwU.Grid
{
    public partial class GridMapBehaviour : MonoBehaviour
    {
        [Space]
        [Header("Map")]
        [SerializeField] private int width = 10;
        [SerializeField] private int height = 10;
        [Space]
        [Header("Config")]
        [SerializeField] private float space = 1.0f;
        [SerializeField] private float cellSize = 1.0f;
        [SerializeField] private Dimension dimension;

        [SerializeField, HideInInspector] private GridCell[] cells;

        private GridMap gridMap;

        public GridMap GetGridMap()
        {
            if (this.gridMap == null ||
                this.gridMap.Length() != this.width * this.height)
            {
                this.gridMap = new GridMap(ref this.cells, this.width, this.height);
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