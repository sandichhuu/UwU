using System.Collections.Generic;
using UnityEngine;
using UwU.Grid;

namespace UwU.PathFinding.FlowField
{
    public partial class FlowFieldGridMapBehaviour : GridMapBehaviour<FlowFieldCell>
    {
        [SerializeField, HideInInspector] public List<Vector2Int> startCells = new();
        [SerializeField, HideInInspector] public List<Vector2Int> targetCells = new();

        public void Compute()
        {
            var gridMap = GetGridMap();
            for (var i = 0; i < gridMap.Length(); i++)
            {
                gridMap[i].distance = -1;
            }

            var queue = new Queue<Vector2Int>();

            foreach (var target in this.targetCells)
            {
                gridMap[target].distance = 0;
                queue.Enqueue(target);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var currentDist = gridMap[current].distance;

                foreach (var dir in Directions)
                {
                    var neighbor = current + dir;

                    if (IsValidPosition(neighbor))
                    {
                        if (!gridMap[neighbor].IsObstacle && gridMap[neighbor].distance == -1)
                        {
                            gridMap[neighbor].distance = currentDist + 1;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }
        }

        public void ToggleStartCell(Vector2Int location)
        {
            var id = this.startCells.IndexOf(location);
            if (id < 0)
            {
                this.startCells.Add(location);
            }
            else
            {
                this.startCells.RemoveAt(id);
            }
        }

        public void ToggleTargetCell(Vector2Int location)
        {
            var id = this.targetCells.IndexOf(location);
            if (id < 0)
            {
                this.targetCells.Add(location);
            }
            else
            {
                this.targetCells.RemoveAt(id);
            }
        }

        private static readonly Vector2Int[] Directions = {
            new(0, 1),
            new(0, -1),
            new(-1, 0),
            new(1, 0)
        };

        public bool IsValidPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < this.width && pos.y >= 0 && pos.y < this.height;
        }

        /// <summary>
        /// Try to set location is obstacle but not blocking path
        /// </summary>
        /// <param name="pos"></param>
        /// <param name="isObstacle"></param>
        /// <returns></returns>
        public bool TestAndSetObstacle(Vector2Int pos, bool isObstacle)
        {
            if (!IsValidPosition(pos)) 
                return false;

            if (IsStartOrTarget(pos)) 
                return false;

            var gridMap = GetGridMap();
            var previousState = gridMap[pos].IsObstacle;
            gridMap[pos].IsObstacle = isObstacle;

            Compute();

            var allStartsValid = true;
            foreach (var start in this.startCells)
            {
                if (gridMap[start].distance == -1)
                {
                    allStartsValid = false;
                    break;
                }
            }

            if (!allStartsValid)
            {
                gridMap[pos].IsObstacle = previousState;
                Compute();
                return false;
            }

            return true;
        }

        public Vector2Int GetNextStep(Vector2Int currentLocation)
        {
            if (!IsValidPosition(currentLocation)) 
                return currentLocation;

            var gridMap = GetGridMap();
            var currentDist = gridMap[currentLocation].distance;

            if (currentDist <= 0) 
                return currentLocation;

            var bestNextCell = currentLocation;
            var minDistance = currentDist;

            foreach (var dir in Directions)
            {
                var neighbor = currentLocation + dir;
                if (IsValidPosition(neighbor))
                {
                    var neighborDist = gridMap[neighbor].distance;
                    if (neighborDist != -1 && neighborDist < minDistance)
                    {
                        minDistance = neighborDist;
                        bestNextCell = neighbor;
                    }
                }
            }

            return bestNextCell;
        }

        public bool IsStartOrTarget(Vector2Int pos)
        {
            return this.startCells.Contains(pos) || this.targetCells.Contains(pos);
        }
    }
}