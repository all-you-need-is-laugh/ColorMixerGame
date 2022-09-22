using UnityEngine;

// Responds for specific level settings

[CreateAssetMenu(fileName = "New Level", menuName = "ColorMixer/Level Settings")]
public class LevelSettings : ScriptableObject {
    public Color targetColor;
    public IngredientManager[] ingredients;
}
