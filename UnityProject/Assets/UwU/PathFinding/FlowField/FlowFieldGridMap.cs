using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UwU.Common;
using UwU.Grid;

namespace UwU.PathFinding.FlowField
{
    public class FlowFieldGridMap : GridMap<FlowFieldCell, FlowFieldGridData>
    {
        private static readonly Vector2Int[] Directions = {
            new(0, 1),
            new(0, -1),
            new(-1, 0),
            new(1, 0)
        };

        public readonly List<Vector2Int> startCells;
        public readonly List<Vector2Int> targetCells;

        public FlowFieldGridMap(FlowFieldGridData gridData, Dimension dimension) :base(gridData, dimension)
        {
            this.startCells = gridData.starts.Select(a => new Vector2Int(a % this.width, a / this.width)).ToList();
            this.targetCells = gridData.targets.Select(a => new Vector2Int(a % this.width, a / this.width)).ToList();
        }

        public FlowFieldGridMap(ref FlowFieldCell[] cells, int width, int height) : base(ref cells, width, height)
        {
            this.startCells = new();
            this.targetCells = new();
        }

        public void Compute()
        {
            for (var i = 0; i < Length(); i++)
            {
                this[i].distance = -1;
            }

            var queue = new Queue<Vector2Int>();

            foreach (var target in this.targetCells)
            {
                this[target].distance = 0;
                queue.Enqueue(target);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var currentDist = this[current].distance;

                foreach (var dir in Directions)
                {
                    var neighbor = current + dir;

                    if (IsValidPosition(neighbor))
                    {
                        if (!this[neighbor].IsObstacle && this[neighbor].distance == -1)
                        {
                            this[neighbor].distance = currentDist + 1;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }
        }

        public bool IsValidPosition(int index)
        {
            var x = index % this.width;
            var y = index / this.width;
            return IsValidPosition(x, y);
        }

        public bool IsValidPosition(int x, int y)
        {
            return IsValidPosition(new Vector2Int(x, y));
        }

        public bool IsValidPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < this.width && pos.y >= 0 && pos.y < this.height;
        }

        public Vector2Int GetNextStep(Vector2Int currentLocation)
        {
            if (!IsValidPosition(currentLocation))
                return currentLocation;

            var currentDist = this[currentLocation].distance;

            if (currentDist <= 0)
                return currentLocation;

            var bestNextCell = currentLocation;
            var minDistance = currentDist;

            foreach (var dir in Directions)
            {
                var neighbor = currentLocation + dir;
                if (IsValidPosition(neighbor))
                {
                    var neighborDist = this[neighbor].distance;
                    if (neighborDist != -1 && neighborDist < minDistance)
                    {
                        minDistance = neighborDist;
                        bestNextCell = neighbor;
                    }
                }
            }

            return bestNextCell;
        }

        public bool IsStart(int index)
        {
            var x = index % this.width;
            var y = index / this.width;
            return IsStart(x, y);
        }

        public bool IsStart(int x, int y)
        {
            return IsStart(new Vector2Int(x, y));
        }

        public bool IsStart(Vector2Int pos)
        {
            return this.startCells.Contains(pos);
        }

        public bool IsTarget(int index)
        {
            var x = index % this.width;
            var y = index / this.width;
            return IsTarget(x, y);
        }

        public bool IsTarget(int x, int y)
        {
            return IsTarget(new Vector2Int(x, y));
        }

        public bool IsTarget(Vector2Int pos)
        {
            return this.targetCells.Contains(pos);
        }

        public bool TestAndSetObstacle(Vector2Int pos, bool isObstacle)
        {
            if (!IsValidPosition(pos))
                return false;

            if (IsStart(pos) || IsTarget(pos))
                return false;

            var previousState = this[pos].IsObstacle;
            this[pos].IsObstacle = isObstacle;

            Compute();

            var allStartsValid = true;
            foreach (var start in this.startCells)
            {
                if (this[start].distance == -1)
                {
                    allStartsValid = false;
                    break;
                }
            }

            if (!allStartsValid)
            {
                this[pos].IsObstacle = previousState;
                Compute();
                return false;
            }

            return true;
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
    }
}