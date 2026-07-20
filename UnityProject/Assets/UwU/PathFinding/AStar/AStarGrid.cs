namespace UwU.PathFinding.AStar
{
    using System.Collections.Generic;
    using UnityEngine;
    using UwU.Collections;

    public class AStarGrid
    {
        private static double GetHeuristic(int x1, int y1, int x2, int y2)
        {
            // Manhattan Distance is enough, Real distance compute cost is higher.
            return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
        }

        public static List<Node> FindPath(Node[] grid, int width, int height, int startIndex, int targetIndex)
        {
            var openSet = new PriorityQueue<int, double>();
            var closedSet = new bool[grid.Length];
            var gCosts = new double[grid.Length];

            for (int i = 0; i < gCosts.Length; i++)
                gCosts[i] = double.MaxValue;

            gCosts[startIndex] = 0;
            var startH = GetHeuristic(grid[startIndex].X, grid[startIndex].Y, grid[targetIndex].X, grid[targetIndex].Y);

            openSet.Enqueue(startIndex, startH);

            while (openSet.Count > 0)
            {
                int currentIndex = openSet.Dequeue();

                if (closedSet[currentIndex])
                    continue;

                if (currentIndex == targetIndex)
                {
                    return RetracePath(grid, startIndex, targetIndex);
                }

                closedSet[currentIndex] = true;
                var currentNode = grid[currentIndex];

                foreach (var neighborIndex in GetValidNeighbors(currentNode.X, currentNode.Y, width, height))
                {
                    if (!grid[neighborIndex].IsWalkable || closedSet[neighborIndex])
                    {
                        continue;
                    }

                    var tentativeGCost = gCosts[currentIndex] + 1;

                    if (tentativeGCost < gCosts[neighborIndex])
                    {
                        gCosts[neighborIndex] = tentativeGCost;

                        grid[neighborIndex].GCost = tentativeGCost;
                        grid[neighborIndex].HCost = GetHeuristic(grid[neighborIndex].X, grid[neighborIndex].Y, grid[targetIndex].X, grid[targetIndex].Y);
                        grid[neighborIndex].ParentIndex = currentIndex;

                        var fCost = tentativeGCost + grid[neighborIndex].HCost;
                        openSet.Enqueue(neighborIndex, fCost);
                    }
                }
            }

            return null; // Not found path
        }

        private static int GetIndex(int x, int y, int width)
        {
            return y * width + x;
        }

        private static List<int> GetValidNeighbors(int x, int y, int width, int height)
        {
            var neighbors = new List<int>(4);
            var dx = new int[] { 0, 0, -1, 1 };
            var dy = new int[] { -1, 1, 0, 0 };

            for (int i = 0; i < 4; i++)
            {
                int checkX = x + dx[i];
                int checkY = y + dy[i];

                if (checkX >= 0 && checkX < width && checkY >= 0 && checkY < height)
                {
                    neighbors.Add(GetIndex(checkX, checkY, width));
                }
            }

            return neighbors;
        }

        private static List<Node> RetracePath(Node[] grid, int startIndex, int targetIndex)
        {
            var path = new List<Node>();
            var currentIndex = targetIndex;

            while (currentIndex != startIndex)
            {
                path.Add(grid[currentIndex]);
                currentIndex = grid[currentIndex].ParentIndex;
            }
            path.Reverse();
            return path;
        }
    }
}