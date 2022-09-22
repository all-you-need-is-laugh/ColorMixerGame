using System.Linq;
using UnityEngine;

public class Utils {
    public static Vector3 CalculateLaunchVelocity(Vector3 start, Vector3 end, float extraHeight) {
        float gravity = Physics.gravity.y;
        float verticalDisplacement = end.y - start.y;
        float hTop = Mathf.Max(end.y, start.y) + extraHeight;
        float y = Mathf.Sqrt(-2 * gravity * hTop);

        float[] solutions = SolveQuadraticEquation(gravity, 2 * y, -2 * verticalDisplacement);
        float t = solutions.Max();

        float x = (end.x - start.x) / t;
        float z = (end.z - start.z) / t;

        return new Vector3(x, y, z);
    }

    public static float[] SolveQuadraticEquation(float a, float b, float c) {
        float discriminant = b * b - 4 * a * c;
        float squareRootOfDiscriminant = Mathf.Sqrt(discriminant);

        return new float[]{
            (-b - squareRootOfDiscriminant) / (2 * a),
            (-b + squareRootOfDiscriminant) / (2 * a)
        };
    }
}
