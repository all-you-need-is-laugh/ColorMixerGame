using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Pool;

// Responds for ingredient instances

[CreateAssetMenu(fileName = "New_IngredientManager", menuName = "ColorMixer/Ingredient Manager")]
public class IngredientManager : ScriptableObject {
    #region Editable settings -------------------------------------------------

    public Color ingredientColor;
    public GameObject ingredientPrefab;
    public float renewDelay = 1;
    public int minPoolSize = 3;
    public int maxPoolSize = 10;

    #endregion Editable settings -------------------------------------------------

    #region Fields, properties, constants -------------------------------------------------

    private IObjectPool<IngredientController> _pool;
    private IObjectPool<IngredientController> pool {
        get {
            if (_pool == null) {
                _pool = new ObjectPool<IngredientController>(
                    CreatePooledItem,
                    OnTakeFromPool,
                    OnReturnedToPool,
                    OnDestroyPoolObject,
                    true,
                    minPoolSize,
                    maxPoolSize
                );
            }

            return _pool;
        }
    }

    #endregion Fields, properties, constants -------------------------------------------------

    #region ScriptableObject Hooks -------------------------------------------------

    private void OnValidate() {
        if (!ingredientPrefab.TryGetComponent<IngredientController>(out IngredientController _)) {
            Debug.LogError($"Specified to {GetType().Name} component {nameof(ingredientPrefab)} object must have attached {nameof(IngredientController)} component", this);
        }
    }

    #endregion ScriptableObject Hooks -------------------------------------------------

    #region ObjectPool Hooks -------------------------------------------------

    private IngredientController CreatePooledItem() {
        GameObject instance = Instantiate(ingredientPrefab);

        instance.name += " " + instance.GetInstanceID();

        var ingredientController = instance.GetComponent<IngredientController>();
        ingredientController.ingredientManager = this;

        return ingredientController;
    }

    private void OnDestroyPoolObject(IngredientController ingredient) {
        Destroy(ingredient.gameObject);
    }

    private void OnReturnedToPool(IngredientController ingredient) {
        ingredient.gameObject.SetActive(false);
        ingredient.ResetPhysics();
    }

    private void OnTakeFromPool(IngredientController ingredient) {
        ingredient.gameObject.SetActive(true);
        ingredient.interactable = true;
    }

    #endregion ObjectPool Hooks -------------------------------------------------

    #region Main functionality -------------------------------------------------

    public async Task RenewAtAsync(Vector3 position, Quaternion rotation, Transform parent) {
        await Task.Delay(Mathf.FloorToInt(renewDelay * 1000));

        Acquire(position, rotation, parent);
    }

    public IngredientController Acquire(Vector3 position, Quaternion rotation, Transform parent) {
        IngredientController ingredient = pool.Get();

        if (ingredient != null) {
            ingredient.transform.parent = parent;
            ingredient.transform.position = position;
            ingredient.transform.rotation = rotation;
        }

        return ingredient;
    }

    public void Release(IngredientController ingredient) {
        if (ingredient.gameObject.activeInHierarchy) {
            pool.Release(ingredient);
        }
    }
    #endregion Main functionality -------------------------------------------------
}
