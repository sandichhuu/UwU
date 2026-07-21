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

        private void DrawGrid()
        {
            Gizmos.color = Color.white;
            for (var i = 0; i < this.grid.Length; i++)
            {
                var node = this.grid[i];
                var center = this.transform.position + new Vector3(node.X, node.Y, 0);

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

                Gizmos.DrawCube(center, Vector3.one * 0.5f);
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