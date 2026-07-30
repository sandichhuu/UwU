using System.Collections.Generic;
using UnityEngine;
using UwU.Collections;
using UwU.Grid;

namespace UwU.PathFinding.AStar
{
    public partial class AStarGridMapBehaviour : GridMapBehaviour<AStarCell, AStarGridData>
    {
        [SerializeField] private int startIndex;
        [SerializeField] private int targetIndex;
        private List<int> path;

        public int GetStartIndex()
        {
            return this.startIndex;
        }

        public int GetTargetIndex()
        {
            return this.targetIndex;
        }

        public void SetStartIndex(int index)
        {
            this.startIndex = index;
        }

        public void SetTargetIndex(int index)
        {
            this.targetIndex = index;
        }

        public ref List<int> GetPath()
        {
            return ref this.path;
        }

        public void Compute()
        {
            var grid = GetGridMap();
            var openSet = new PriorityQueue<int, double>();
            var closedSet = new bool[grid.Length()];
            var gCosts = new double[grid.Length()];

            for (int i = 0; i < gCosts.Length; i++)
                gCosts[i] = double.MaxValue;

            gCosts[this.startIndex] = 0;

            var startLocation = grid.GetLocation(this.startIndex);
            var targetLocation = grid.GetLocation(this.targetIndex);
            var startH = GetHeuristic(startLocation.x, startLocation.y, targetLocation.x, targetLocation.y);

            openSet.Enqueue(this.startIndex, startH);

            while (openSet.Count > 0)
            {
                int currentIndex = openSet.Dequeue();

                if (closedSet[currentIndex])
                    continue;

                if (currentIndex == this.targetIndex)
                {
                    this.path = RetracePath(this.startIndex, this.targetIndex);
                    return;
                }

                closedSet[currentIndex] = true;
                var currentLocation = grid.GetLocation(currentIndex);

                foreach (var neighborIndex in GetValidNeighbors(currentLocation.x, currentLocation.y, this.width, this.height))
                {
                    if (grid[neighborIndex].IsObstacle || closedSet[neighborIndex])
                    {
                        continue;
                    }

                    var tentativeGCost = gCosts[currentIndex] + 1;

                    if (tentativeGCost < gCosts[neighborIndex])
                    {
                        gCosts[neighborIndex] = tentativeGCost;

                        grid[neighborIndex].gCost = tentativeGCost;

                        var neighborLocation = grid.GetLocation(neighborIndex);
                        grid[neighborIndex].hCost = GetHeuristic(neighborLocation.x, neighborLocation.y, targetLocation.x, targetLocation.y);
                        grid[neighborIndex].parentIndex = currentIndex;

                        var fCost = tentativeGCost + grid[neighborIndex].hCost;
                        openSet.Enqueue(neighborIndex, fCost);
                    }
                }
            }

            this.path = null;
            return; // Not found path
        }

        private static double GetHeuristic(int x1, int y1, int x2, int y2)
        {
            // Manhattan Distance is enough, Real distance compute cost is higher.
            return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
        }

        private List<int> GetValidNeighbors(int x, int y, int width, int height)
        {
            var grid = GetGridMap();
            var neighbors = new List<int>(4);
            var dx = new int[] { 0, 0, -1, 1 };
            var dy = new int[] { -1, 1, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                int checkX = x + dx[i];
                int checkY = y + dy[i];

                if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
                {
                    neighbors.Add(grid.GetIndex(checkX, checkY));
                }
            }

            return neighbors;
        }

        private List<int> RetracePath(int startIndex, int targetIndex)
        {
            var grid = GetGridMap();
            var path = new List<int>();
            var currentIndex = targetIndex;

            while (currentIndex != startIndex)
            {
                path.Add(currentIndex);
                currentIndex = grid[currentIndex].parentIndex;
            }
            path.Reverse();
            return path;
        }
    }
}