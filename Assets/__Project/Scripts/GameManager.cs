using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

// Responds for game objects placement and interactions with them

public class GameManager : MonoBehaviour {
    #region Editable settings -------------------------------------------------

    [SerializeField]
    private CameraController _cameraController;

    [Header("UI elements settings")]
    [SerializeField]
    private Image _orderColorImage;

    [SerializeField]
    private ResultsUIController _resultsUiController;


    [Header("Ingredients settings")]
    [SerializeField]
    private Transform _ingredientsHolder;

    [SerializeField]
    private float _ingredientsPlacementWidth = .5f;


    [Header("Blender settings")]
    [SerializeField]
    private BlenderController _blenderController;

    [SerializeField]
    private float _waitBeforeCloseLid = 2;

    [Space()]
    [SerializeField]
    private LayerMask _interactionsLayerMask;

    #endregion Editable settings -------------------------------------------------

    #region Fields, properties, constants -------------------------------------------------

    private const float INGREDIENTS_ROTATION_PERSPECTIVE_K = 1f;
    public static GameManager instance { get; private set; }
    private CancellationTokenSource _lidOpenedWaitCts;
    private Task _ingredientMovementTask;
    private bool _mixRequested = false;
    private Color _currentTargetColor;
    private bool _sequentialStart = false;

    #endregion Fields, properties, constants -------------------------------------------------

    #region MonoBehaviour Hooks -------------------------------------------------

    private void OnDisable() {
        if (_lidOpenedWaitCts != null) {
            _lidOpenedWaitCts.Cancel();
            _lidOpenedWaitCts = null;
        }
    }

    private void OnValidate() {
        Debug.Assert(_orderColorImage != null, $"Specify order UI Image object to {GetType().Name} component!", this);
        Debug.Assert(_ingredientsHolder != null, $"Specify ingredients holder object to {GetType().Name} component!", this);
        Debug.Assert(_blenderController != null, $"Specify {nameof(BlenderController)} component to {GetType().Name} component!", this);
        Debug.Assert(_cameraController != null, $"Specify {nameof(CameraController)} component to {GetType().Name} component!", this);
        Debug.Assert(_resultsUiController != null, $"Specify {nameof(ResultsUIController)} component to {GetType().Name} component!", this);
    }

    private void Awake() {
        if (instance != null) {
            Debug.LogError($"Attempt to instantiate more than one {GetType().Name} component!", this);
            Debug.LogError($"Click on this message to find existing instance of {GetType().Name} component", instance);
            return;
        }
        else {
            instance = this;
        }
    }

    private void Update() {
        HandleTouch();
    }

    #endregion MonoBehaviour Hooks -------------------------------------------------

    #region Interactions handling -------------------------------------------------

    private void HandleTouch() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = _cameraController.camera.ScreenPointToRay(Input.mousePosition);

            if (!_mixRequested && Physics.Raycast(ray, out RaycastHit hitInfo, 10, _interactionsLayerMask)) {
                if (hitInfo.collider.CompareTag("Ingredient")) {
                    _ingredientMovementTask = InteractWithIngredientAsync(hitInfo.collider.gameObject);
                    return;
                }

                bool atLeastOneIngredientSentToBlender = _ingredientMovementTask != null;
                if (atLeastOneIngredientSentToBlender && hitInfo.collider.CompareTag("MixButton")) {
                    if (_lidOpenedWaitCts != null) {
                        _lidOpenedWaitCts.Cancel();
                    }

                    _mixRequested = true;

                    _ingredientMovementTask.ContinueWith<Task>(
                        (_) => InteractWithMixButtonAsync(),
                        TaskScheduler.FromCurrentSynchronizationContext()
                    );
                }
            }
        }
    }

    private async Task InteractWithIngredientAsync(GameObject ingredient) {
        IngredientController ingredientController = ingredient.GetComponent<IngredientController>();

        if (ingredientController?.interactable == true) {
            ingredientController.interactable = false;

            if (_lidOpenedWaitCts != null) {
                _lidOpenedWaitCts.Cancel();
            }
            _lidOpenedWaitCts = new CancellationTokenSource();

            await _blenderController.OpenLidAsync();

            _ = ingredientController.ingredientManager.RenewAtAsync(ingredient.transform.position, ingredient.transform.rotation, ingredient.transform.parent);

            await ingredientController.MoveToAsync(_blenderController.jugEntryPointPosition);

            await Task.WhenAll(
                _blenderController.ResetJugTransformAsync(),
                Task.Delay(Mathf.FloorToInt(_waitBeforeCloseLid * 1000), _lidOpenedWaitCts.Token)
            );

            await _blenderController.CloseLidAsync();
        }
    }

    private async Task InteractWithMixButtonAsync() {
        Color resultColor = await _blenderController.MixAsync();

        _resultsUiController.SetTargetColor(_currentTargetColor);
        _resultsUiController.SetResultColor(resultColor);
        _resultsUiController.SetColorSimilarity(ColorCalculations.ColorsSimilarity(resultColor, _currentTargetColor));

        await Task.WhenAll(
            _blenderController.OpenLidAsync(),
            _cameraController.MoveToResultsViewAsync(),
            _resultsUiController.ShowAsync()
        );
    }

    #endregion Interactions handling -------------------------------------------------

    #region Main functionality -------------------------------------------------

    private void CleanIngredientsHolder() {
        foreach (Transform child in _ingredientsHolder) {
            IngredientController ingredientController = child.GetComponent<IngredientController>();
            if (ingredientController != null) {
                if (ingredientController.ingredientManager != null) {
                    ingredientController.ingredientManager.Release(ingredientController);
                    continue;
                }

                Debug.LogError($"Found object inside ingredients holder without {nameof(IngredientManager)} specified in {nameof(IngredientController)} component: {child.name}. It will be destroyed directly!", this);
            }
            else {
                Debug.LogError($"Found object inside ingredients holder without {nameof(IngredientController)} component: {child.name}. It will be destroyed directly!", this);
            }

            Destroy(child.gameObject);
        }
    }

    private void InstantiateIngredient(IngredientManager ingredientManager, Vector3 position, Transform parent) {
        var rotation = Quaternion.LookRotation(parent.position + parent.forward * INGREDIENTS_ROTATION_PERSPECTIVE_K - position, Vector3.up);
        ingredientManager.Acquire(position, rotation, parent);
    }

    private void InstantiateIngredients(IngredientManager[] ingredients) {
        if (ingredients.Length <= 1) {
            InstantiateIngredient(ingredients[0], _ingredientsHolder.position, _ingredientsHolder);
            return;
        }

        Vector3 leftPosition = _ingredientsHolder.position - _ingredientsHolder.right * _ingredientsPlacementWidth / 2;
        Vector3 rightPosition = _ingredientsHolder.position + _ingredientsHolder.right * _ingredientsPlacementWidth / 2;
        Vector3 positionPadding = (rightPosition - leftPosition) / ingredients.Length;
        Vector3 position = leftPosition + positionPadding / 2;
        foreach (IngredientManager ingredient in ingredients) {
            InstantiateIngredient(ingredient, position, _ingredientsHolder);
            position += positionPadding;
        }
    }

    private void SetOrderColor(Color color) {
        _orderColorImage.color = color;
    }

    private async Task ResetSceneAsync() {
        CleanIngredientsHolder();

        await Task.WhenAll(
            _cameraController.MoveToStartPointAsync(),
            _resultsUiController.HideAsync(),
            _blenderController.ResetJugContentAsync()
        );

        await _blenderController.CloseLidAsync();

        _mixRequested = false;
    }

    public async Task StartLevelAsync(Color targetColor, IngredientManager[] ingredients) {
        if (_sequentialStart) {
            await ResetSceneAsync();
        }

        _sequentialStart = true;

        _currentTargetColor = targetColor;

        SetOrderColor(targetColor);
        InstantiateIngredients(ingredients);
    }

    #endregion Main functionality -------------------------------------------------
}
