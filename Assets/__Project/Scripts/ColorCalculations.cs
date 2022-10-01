using System.Collections.Generic;
using UnityEngine;

public static class ColorCalculations {
    private static readonly float fullColorEuclideanDistance = ColorsEuclideanDistance(Color.white, Color.black);

    public static float ColorsSimilarity(Color colorA, Color colorB) {
        return 1 - ColorsEuclideanDistance(colorA, colorB) / fullColorEuclideanDistance;
    }

    public static Color MixColors(IEnumerable<CountedIngredient> countedIngredients) {
        Color totalColor = Color.clear;
        int weights = 0;

        foreach (var cIngredient in countedIngredients) {
            totalColor += cIngredient.count * cIngredient.ingredient.color;
            weights += cIngredient.count;
        }

        if (weights == 0) {
            return Color.white;
        }

        totalColor /= weights;
        totalColor.a = 1;

        return totalColor;
    }

    private static float ColorsEuclideanDistance(Color colorA, Color colorB) {
        Vector3 a = new Vector3(colorA.r, colorA.g, colorA.b);
        Vector3 b = new Vector3(colorB.r, colorB.g, colorB.b);

        return (a - b).magnitude;
    }
}
