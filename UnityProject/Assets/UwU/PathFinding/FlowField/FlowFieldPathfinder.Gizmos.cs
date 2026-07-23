#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
namespace UwU.PathFinding.FlowField
{
    public partial class FlowFieldPathfinder : MonoBehaviour
    {
        [Space]
        [Header("Gizmos")]
        [SerializeField] private bool debug = true;

        private void DrawGrid3D()
        {
            var length = this.width * this.height;
            var offsetX = 0.5f * ((this.width - 1.0f) * this.space);
            var offsetY = 0.5f * ((this.height - 1.0f) * this.space);

            Gizmos.color = Color.white;
            for (var i = 0; i < length; i++)
            {
                var nodeX = i % this.width;
                var nodeY = i / this.width;
                var center = this.transform.position - new Vector3(offsetX, 0, offsetY) + new Vector3(nodeX, 0, nodeY) * this.space;

                if (this.obstacles[i])
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.white;
                }

                var nodeLocation = new Vector2Int(nodeX, nodeY);
                if (this.startCells.Contains(nodeLocation))
                {
                    Gizmos.color = Color.teal;
                }

                if (this.targetCells.Contains(nodeLocation))
                {
                    Gizmos.color = Color.magenta;
                }

                Gizmos.DrawCube(center, Vector3.one * this.cellSize);
            }
        }

        private void DrawGrid2D()
        {
            var length = this.width * this.height;
            var gap = 0.5f * (this.space - this.cellSize);
            var offsetX = 0.5f * (this.width * this.space) - gap;
            var offsetY = 0.5f * (this.height * this.space) - gap;

            Gizmos.color = Color.white;
            for (var i = 0; i < length; i++)
            {
                var nodeX = i % this.width;
                var nodeY = i / this.width;
                var center = this.transform.position - new Vector3(offsetX, offsetY, 0) + new Vector3(nodeX, nodeY, 0) * this.space;

                if (this.obstacles[i])
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.white;
                }

                var nodeLocation = new Vector2Int(nodeX, nodeY);
                if (this.startCells.Contains(nodeLocation))
                {
                    Gizmos.color = Color.teal;
                }

                if (this.targetCells.Contains(nodeLocation))
                {
                    Gizmos.color = Color.magenta;
                }

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