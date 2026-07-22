using System.Collections.Generic;
using UnityEngine;

namespace UwU.PathFinding.AStar
{
    public partial class AStarGridBehaviour : MonoBehaviour
    {
        [Space]
        [Header("Config")]
        [SerializeField] private int width = 64;
        [SerializeField] private int height = 64;
        [SerializeField] private int startIndex = 64;
        [SerializeField] private int targetIndex = 64;

        private Node[] grid;
        private List<Node> path;

        private void Awake()
        {
            Refresh();
        }

        private void Refresh()
        {
            if (ValidateInput())
            {
                SetupGrid();
                SetupObstacles();
                Solve();
            }
            else
            {
                Debug.Log("Invalid input !");
            }
        }

        private bool ValidateInput()
        {
            var size = this.width * this.height;
            return this.startIndex < size && this.targetIndex < size && this.startIndex > -1 && this.targetIndex > -1;
        }

        private void Solve()
        {
            this.path = AStarGrid.FindPath(this.grid, this.width, this.height, this.startIndex, this.targetIndex);

            if (this.path != null)
            {
                Debug.Log("Found Solution:");
                //foreach (var node in this.path)
                //{
                //    Debug.Log($"-> ({node.X}, {node.Y})");
                //}
            }
            else
            {
                Debug.Log("Not found path");
            }
        }

        private void SetupObstacles()
        {
            this.grid[1 * this.width + 2].IsWalkable = false;
            this.grid[2 * this.width + 2].IsWalkable = false;
            this.grid[3 * this.width + 2].IsWalkable = false;
        }

        private void SetupGrid()
        {
            this.grid = new Node[this.width * this.height];
            for (var y = 0; y < this.height; y++)
            {
                for (var x = 0; x < this.width; x++)
                {
                    var index = y * this.width + x;
                    this.grid[index] = new Node(x, y, true);
                }
            }
        }
    }
}