namespace UwU.PathFinding.FlowField
{
    using System.Collections.Generic;
    using UnityEngine;

    public partial class FlowFieldPathfinder : MonoBehaviour
    {
        [Space]
        [Header("Grid Settings")]
        public int width = 20;
        public int height = 10;

        [SerializeField] public int[] distanceMap;
        [SerializeField] public bool[] obstacles;
        [SerializeField] public List<Vector2Int> startCells = new();
        [SerializeField] public List<Vector2Int> targetCells = new();

        private static readonly Vector2Int[] Directions = {
            new(0, 1),
            new(0, -1),
            new(-1, 0),
            new(1, 0)
        };

        void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            var totalSize = this.width * this.height;
            this.distanceMap = new int[totalSize];
            this.obstacles = new bool[totalSize];
        }

        public void ClearStartAndTargets()
        {
            this.startCells.Clear();
            this.targetCells.Clear();
        }

        public void AddStartCell(Vector2Int pos)
        {
            if (IsValidPosition(pos)) this.startCells.Add(pos);
        }

        public void AddTargetCell(Vector2Int pos)
        {
            if (IsValidPosition(pos)) this.targetCells.Add(pos);
        }

        public void RecomputeFlowField()
        {
            var totalSize = this.width * this.height;
            for (var i = 0; i < totalSize; i++)
            {
                this.distanceMap[i] = -1;
            }

            var queue = new Queue<Vector2Int>();

            foreach (var target in this.targetCells)
            {
                int idx = GetIndex(target);
                this.distanceMap[idx] = 0;
                queue.Enqueue(target);
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                var currentDist = this.distanceMap[GetIndex(current)];

                foreach (var dir in Directions)
                {
                    var neighbor = current + dir;

                    if (IsValidPosition(neighbor))
                    {
                        var neighborIdx = GetIndex(neighbor);
                        if (!this.obstacles[neighborIdx] && this.distanceMap[neighborIdx] == -1)
                        {
                            this.distanceMap[neighborIdx] = currentDist + 1;
                            queue.Enqueue(neighbor);
                        }
                    }
                }
            }
        }

        public bool TestAndSetObstacle(Vector2Int pos, bool isObstacle)
        {
            if (!IsValidPosition(pos)) return false;
            if (IsStartOrTarget(pos)) return false;

            var idx = GetIndex(pos);
            var previousState = this.obstacles[idx];

            this.obstacles[idx] = isObstacle;

            RecomputeFlowField();

            var allStartsValid = true;
            foreach (var start in this.startCells)
            {
                if (this.distanceMap[GetIndex(start)] == -1)
                {
                    allStartsValid = false;
                    break;
                }
            }

            if (!allStartsValid)
            {
                this.obstacles[idx] = previousState;
                RecomputeFlowField();
                return false;
            }

            return true;
        }

        public Vector2Int GetNextStep(Vector2Int currentPos)
        {
            if (!IsValidPosition(currentPos)) return currentPos;

            var currentIndex = GetIndex(currentPos);
            var currentDist = this.distanceMap[currentIndex];

            if (currentDist <= 0) return currentPos;

            var bestNextCell = currentPos;
            var minDistance = currentDist;

            foreach (var dir in Directions)
            {
                var neighbor = currentPos + dir;

                if (IsValidPosition(neighbor))
                {
                    var neighborIndex = GetIndex(neighbor);
                    var neighborDist = this.distanceMap[neighborIndex];

                    if (neighborDist != -1 && neighborDist < minDistance)
                    {
                        minDistance = neighborDist;
                        bestNextCell = neighbor;
                    }
                }
            }

            return bestNextCell;
        }

        public int GetIndex(Vector2Int pos) => pos.y * this.width + pos.x;

        public bool IsValidPosition(Vector2Int pos)
        {
            return pos.x >= 0 && pos.x < this.width && pos.y >= 0 && pos.y < this.height;
        }

        public bool IsStartOrTarget(Vector2Int pos)
        {
            return this.startCells.Contains(pos) || this.targetCells.Contains(pos);
        }
    }
}