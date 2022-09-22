using UnityEngine;

[CreateAssetMenu(fileName = "New Level", menuName = "ColorMixer/Level Settings")]
public class LevelSettings : ScriptableObject {
    public Color targetColor;
    public GameObject[] ingredients;
}
