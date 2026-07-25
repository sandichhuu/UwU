#if UNITY_EDITOR

using UnityEditor;
using UnityEngine;

namespace UwU.Grid
{
    public partial class CommonGridMapBehaviour
    {
        [Space]
        [Header("Gizmos")]
        [SerializeField] private bool debug = true;

        private void DrawGrid3D(GridMap<GridCell> gridMap)
        {
            var offsetX = 0.5f * ((gridMap.width - 1.0f) * this.space);
            var offsetY = 0.5f * ((gridMap.height - 1.0f) * this.space);

            Gizmos.color = Color.white;
            for (var i = 0; i < gridMap.Length(); i++)
            {
                var node = gridMap.cells[i];
                var location = gridMap.GetLocation(i);
                var center = this.transform.position - new Vector3(offsetX, 0, offsetY) + new Vector3(location.x, 0, location.y) * this.space;

                if (node.IsObstacle)
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.white;
                }

                Gizmos.DrawCube(center, Vector3.one * this.cellSize);
            }
        }

        private void DrawGrid2D(GridMap<GridCell> gridMap)
        {
            var gap = 0.5f * (this.space - this.cellSize);
            var offsetX = 0.5f * (this.width * this.space) - gap;
            var offsetY = 0.5f * (this.height * this.space) - gap;

            Gizmos.color = Color.white;
            for (var i = 0; i < gridMap.Length(); i++)
            {
                var node = gridMap[i];
                var location = gridMap.GetLocation(i);
                var center = this.transform.position - new Vector3(offsetX, offsetY, 0) + new Vector3(location.x, location.y, 0) * this.space;

                if (node.IsObstacle)
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = Color.white;
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
                var gridMap = GetGridMap();
                if (this.dimension == Common.Dimension.Two)
                {
                    DrawGrid2D(gridMap);
                }
                else
                {
                    DrawGrid3D(gridMap);
                }

                Gizmos.color = originalGizmosColor;
                Handles.color = originalHandlesColor;
            }
        }
    }
}

#endif