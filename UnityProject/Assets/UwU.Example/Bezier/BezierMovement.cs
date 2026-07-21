namespace UwU.Examples.Bezier
{
    using UnityEngine;
    using UwU.BezierSolver;

    public class BezierMovement : MonoBehaviour
    {
        [SerializeField] private CurveBehaviour curve;
        [SerializeField, Range(0, 1f)] private float movementT = 1;

        private void Update()
        {
            this.transform.position = this.curve.Solve(this.movementT);
        }
    }
}