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
        [SerializeField] private float cellSize = 1.0f;
        [SerializeField] private float space = 1.0f;

        private void DrawGrid()
        {
            var offsetX = 0.5f * ((this.width - 1.0f) * this.space);
            var offsetY = 0.5f * ((this.height - 1.0f) * this.space);

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

                Gizmos.DrawCube(center, Vector3.one * this.cellSize);
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

                DrawGrid();

                Gizmos.color = originalGizmosColor;
                Handles.color = originalHandlesColor;
            }
        }
    }
}
#endif