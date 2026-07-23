#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace UwU.PathFinding.AStar
{
    public partial class AStarGridBehaviour : MonoBehaviour
    {
        [Space]
        [Header("Gizmos")]
        [SerializeField] private bool debug = true;

        private void DrawGrid3D()
        {
            var offsetX = 0.5f * ((this.width - 1.0f) * this.space);
            var offsetY = 0.5f * ((this.height - 1.0f) * this.space);

            Gizmos.color = Color.white;
            for (var i = 0; i < this.grid.Length; i++)
            {
                var node = this.grid[i];
                var center = this.transform.position - new Vector3(offsetX, 0, offsetY) + new Vector3(node.X, 0, node.Y) * this.space;

                if (node.IsWalkable)
                {
                    Gizmos.color = Color.white;
                }
                else
                {
                    Gizmos.color = Color.red;
                }

                if (this.path != null && this.path.Contains(node))
                {
                    Gizmos.color = Color.green;
                }

                if (this.startIndex == i)
                    Gizmos.color = Color.teal;

                if (this.targetIndex == i)
                    Gizmos.color = Color.magenta;

                Gizmos.DrawCube(center, Vector3.one * this.cellSize);
            }
        }

        private void DrawGrid2D()
        {
            var gap = 0.5f * (this.space - this.cellSize);
            var offsetX = 0.5f * (this.width * this.space) - gap;
            var offsetY = 0.5f * (this.height * this.space) - gap;

            Gizmos.color = Color.white;
            for (var i = 0; i < this.grid.Length; i++)
            {
                var node = this.grid[i];
                var center = this.transform.position - new Vector3(offsetX, offsetY, 0) + new Vector3(node.X, node.Y, 0) * this.space;

                if (node.IsWalkable)
                {
                    Gizmos.color = Color.white;
                }
                else
                {
                    Gizmos.color = Color.red;
                }

                if (this.path != null && this.path.Contains(node))
                {
                    Gizmos.color = Color.green;
                }

                if (this.startIndex == i)
                    Gizmos.color = Color.teal;

                if (this.targetIndex == i)
                    Gizmos.color = Color.magenta;

                Handles.DrawSolidRectangleWithOutline(new Rect(center.x, center.y, this.cellSize, this.cellSize), Gizmos.color, Color.black);
            }
        }

        private void OnDrawGizmos()
        {
            if (this.debug == false)
            {
                return;
            }

            if (Camera.current == Camera.main || Camera.current == SceneView.lastActiveSceneView.camera)
            {
                Refresh();

                var originalGizmosColor = Gizmos.color;
                var originalHandlesColor = Handles.color;

                if (this.dimension == Common.Dimension.Two)
                {
                    DrawGrid2D();
                }
                else
                {
                    DrawGrid3D();
                }

                Gizmos.color = originalGizmosColor;
                Handles.color = originalHandlesColor;
            }
        }
    }
}
#endif