using System.Threading.Tasks;
using UnityEngine;

// Responds for ingredient instances

[CreateAssetMenu(fileName = "New_IngredientManager", menuName = "ColorMixer/Ingredient Manager")]
public class IngredientManager : ScriptableObject {
    #region Editable settings -------------------------------------------------

    public Color ingredientColor;
    public GameObject ingredientPrefab;
    public float renewDelay = 1;

    #endregion Editable settings -------------------------------------------------

    #region ScriptableObject Hooks -------------------------------------------------

    private void OnValidate() {
        if (!ingredientPrefab.TryGetComponent<IngredientController>(out IngredientController _)) {
            Debug.LogError($"Specified to {GetType().Name} component {nameof(ingredientPrefab)} object must have attached {nameof(IngredientController)} component", this);
        }
    }

    #endregion ScriptableObject Hooks -------------------------------------------------

    #region Main functionality -------------------------------------------------

    public async Task RenewAtAsync(Vector3 position, Quaternion rotation, Transform parent) {
        await Task.Delay(Mathf.FloorToInt(renewDelay * 1000));

        Acquire(position, rotation, parent);
    }

    public IngredientController Acquire(Vector3 position, Quaternion rotation, Transform parent) {
        GameObject instance = Instantiate(ingredientPrefab, position, rotation, parent);

        instance.name += " " + instance.GetInstanceID();

        var ingredientController = instance.GetComponent<IngredientController>();
        ingredientController.ingredientManager = this;

        return ingredientController;
    }

    public void Release(IngredientController ingredient) {
        Debug.Log("### > IngredientManager > Release > ingredient " + ingredient);
        Destroy(ingredient.gameObject);
    }
    #endregion Main functionality -------------------------------------------------
}
