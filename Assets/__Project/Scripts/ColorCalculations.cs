using UnityEngine;

public static class ColorCalculations {
    private static readonly float fullColorEuclideanDistance = ColorsEuclideanDistance(Color.white, Color.black);

    public static float ColorsSimilarity(Color colorA, Color colorB) {
        return 1 - ColorsEuclideanDistance(colorA, colorB) / fullColorEuclideanDistance;
    }

    private static float ColorsEuclideanDistance(Color colorA, Color colorB) {
        Vector3 a = new Vector3(colorA.r, colorA.g, colorA.b);
        Vector3 b = new Vector3(colorB.r, colorB.g, colorB.b);

        return (a - b).magnitude;
    }
}
