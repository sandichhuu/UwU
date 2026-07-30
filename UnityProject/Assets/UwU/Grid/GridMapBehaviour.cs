using System.Collections.Generic;
using UnityEngine;
using UwU.Common;

namespace UwU.Grid
{
    public class GridMapBehaviour<TCell, TData> : MonoBehaviour where TCell : GridCell, new() where TData : GridData, new()
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

        [SerializeField, HideInInspector] protected TCell[] cells;

        protected GridMap<TCell, TData> gridMap;

        public virtual TData GetGridData()
        {
            var gridData = new TData
            {
                width = this.width,
                height = this.height,
                space = this.space,
                cellSize = this.cellSize,
            };

            var obstacles = new List<int>();
            for (var i = 0; i < this.cells.Length; i++)
            {
                if (this.cells[i].IsObstacle)
                {
                    obstacles.Add(i);
                }
            }

            gridData.obstacles = obstacles.ToArray();
            return gridData;
        }

        public virtual void ApplyGridData(TData gridData)
        {
            this.width = gridData.width;
            this.height = gridData.height;
            this.space = gridData.space;
            this.cellSize = gridData.cellSize;
            var length = this.width * this.height;

            this.cells = new TCell[length];
            for (var i = 0; i < length; i++)
            {
                this.cells[i] = new TCell();
            }

            this.gridMap = new GridMap<TCell, TData>(ref this.cells, this.width, this.height);
            this.gridMap.SetConfig(this.dimension, this.space, this.cellSize);

            for (var i = 0; i < gridData.obstacles.Length; i++)
            {
                var obstacle = gridData.obstacles[i];
                this.cells[obstacle].IsObstacle = true;
            }
        }

        public GridMap<TCell, TData> GetGridMap()
        {
            var length = this.width * this.height;

            if (this.cells == null || this.cells.Length != length)
            {
                this.cells = new TCell[length];
                for (var i = 0; i < length; i++)
                {
                    this.cells[i] = new TCell();
                }
            }

            if (this.gridMap == null)
            {
                this.gridMap = new GridMap<TCell, TData>(ref this.cells, this.width, this.height);
                this.gridMap.SetConfig(this.dimension, this.space, this.cellSize);
            }
            else
            {
                if (this.gridMap.Length() != length)
                {
                    this.gridMap = new GridMap<TCell, TData>(ref this.cells, this.width, this.height);
                    this.gridMap.SetConfig(this.dimension, this.space, this.cellSize);
                }
                else
                {
                    if (this.gridMap.cellSize != this.cellSize ||
                        this.gridMap.space != this.space ||
                        this.gridMap.dimension != this.dimension)
                    {
                        this.gridMap.SetConfig(this.dimension, this.space, this.cellSize);
                    }
                }
            }

            return this.gridMap;
        }
    }
}