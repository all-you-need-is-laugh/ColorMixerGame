using UnityEngine;

// Responds for ingredient instances

[CreateAssetMenu(fileName = "New_IngredientManager", menuName = "ColorMixer/Ingredient Manager")]
public class IngredientManager : ScriptableObject {
    #region Editable settings -------------------------------------------------

    public Color ingredientColor;
    public GameObject ingredientPrefab;

    #endregion Editable settings -------------------------------------------------

    #region Fields, properties, constants -------------------------------------------------

    #endregion Fields, properties, constants -------------------------------------------------

    #region ScriptableObject Hooks -------------------------------------------------

    private void OnValidate() {
        if (!ingredientPrefab.TryGetComponent<IngredientController>(out IngredientController _)) {
            Debug.LogError($"{nameof(ingredientPrefab)} must have attached {nameof(IngredientController)} component", this);
        }
    }

    #endregion ScriptableObject Hooks -------------------------------------------------

    #region Main functionality -------------------------------------------------
    public void Acquire(Vector3 position, Quaternion rotation, Transform parent) {
        GameObject instance = Instantiate(ingredientPrefab, position, rotation, parent);
        instance.GetComponent<IngredientController>().ingredientManager = this;
    }

    public void Release(IngredientController ingredient) {
        Debug.Log("### > IngredientManager > Release > ingredient " + ingredient);
        Destroy(ingredient.gameObject);
    }
    #endregion Main functionality -------------------------------------------------
}
