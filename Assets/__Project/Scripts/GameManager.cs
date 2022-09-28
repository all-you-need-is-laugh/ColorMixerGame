using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

// Responds for game rules, interactions handling and level switching
// TODO: Refactor Game manager to comply Single responsibility principle

public class GameManager : MonoBehaviour {
    #region Editable settings -------------------------------------------------

    [Tooltip("Main camera will be picked by default")]
    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private LevelSettings[] _levels;

    [SerializeField]
    private Image _orderColorImage;

    [SerializeField]
    private Transform _ingredientsHolder;

    [SerializeField]
    private float _ingredientsPlacementWidth = .5f;

    [SerializeField]
    private float _waitBeforeCloseLid = 2;

    [SerializeField]
    private BlenderController _blenderController;

    [SerializeField]
    private LayerMask _interactionsLayerMask;

    #endregion Editable settings -------------------------------------------------

    #region Fields, properties, constants -------------------------------------------------

    private const float INGREDIENTS_ROTATION_PERSPECTIVE_K = 1f;
    private static GameManager _instance;
    private int _currentLevelIndex = 0;
    private CancellationTokenSource _lidOpenedWaitCts;
    private Task _ingredientMovementTask;
    private bool _mixRequested = false;

    #endregion Fields, properties, constants -------------------------------------------------

    #region MonoBehaviour Hooks -------------------------------------------------

    private void OnDisable() {
        if (_lidOpenedWaitCts != null) {
            _lidOpenedWaitCts.Cancel();
            _lidOpenedWaitCts = null;
        }
    }

    private void Start() {
        if (_instance != null) {
            Debug.LogError($"Attempt to instantiate more than one {GetType().Name} component!", this);
            Debug.LogError($"Click on this message to find existing instance of {GetType().Name} component", _instance);
            return;
        }
        else {
            _instance = this;
        }

        if (_levels.Length == 0) {
            Debug.LogError($"Add at least 1 level to {GetType().Name} component!", this);
            return;
        }

        if (_orderColorImage == null) {
            Debug.LogError($"Specify order UI Image object to {GetType().Name} component!", this);
            return;
        }

        if (_ingredientsHolder == null) {
            Debug.LogError($"Specify ingredients holder to {GetType().Name} component!", this);
            return;
        }

        if (_blenderController == null) {
            Debug.LogError($"Specify {nameof(BlenderController)} component to {GetType().Name} component!", this);
            return;
        }

        if (_camera == null) {
            if (Camera.main == null) {
                Debug.LogError($"Specify camera to {GetType().Name} component or add main camera to the scene!", this);
                return;
            }
            _camera = Camera.main;
        }

        _ingredientMovementTask = Task.FromResult<object>(null);

        // RestartLevel();
        // Add delay before start to let everything warm up - without it ingredients have non-zero angular velocity when
        // fall down at first time
        Invoke(nameof(RestartLevel), .5f);
    }

    private void Update() {
        HandleTouch();

        HandleDebugActions();
    }

    #endregion MonoBehaviour Hooks -------------------------------------------------

    #region Interactions handling -------------------------------------------------

    private void HandleDebugActions() {
        if (Input.GetKeyDown(KeyCode.R)) {
            RestartLevel();
        }
        else if (Input.GetKeyDown(KeyCode.N)) {
            StartNextLevel();
        }
        else if (Input.GetKeyDown(KeyCode.Backspace)) {
            debugPoints.Clear();
        }
    }

    private void HandleTouch() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (!_mixRequested && Physics.Raycast(ray, out RaycastHit hitInfo, 10, _interactionsLayerMask)) {
                if (hitInfo.collider.CompareTag("Ingredient")) {
                    hitInfo.collider.tag = "Ingredient_Non_Interactive";
                    _ingredientMovementTask = InteractWithIngredientAsync(hitInfo.collider.gameObject);
                }
                else if (hitInfo.collider.CompareTag("MixButton")) {
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

        if (ingredientController != null) {
            if (_lidOpenedWaitCts != null) {
                _lidOpenedWaitCts.Cancel();
            }
            _lidOpenedWaitCts = new CancellationTokenSource();

            await _blenderController.OpenLid();

            _ = ingredientController.ingredientManager.RenewAtAsync(ingredient.transform.position, ingredient.transform.rotation, ingredient.transform.parent);
            await ingredientController.MoveToAsync(_blenderController.jugEntryPointPosition);

            await Task.Delay(Mathf.FloorToInt(_waitBeforeCloseLid * 1000), _lidOpenedWaitCts.Token);

            await _blenderController.CloseLid();
        }
    }

    private async Task InteractWithMixButtonAsync() {
        await _blenderController.Mix();
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

    private void StartLevel(LevelSettings level) {
        if (level == null) {
            throw new Exception($"Unexpected attempt to start absent level ({_currentLevelIndex})");
        }
        Debug.Log("### > GameManager > StartLevel > level " + level);

        SetOrderColor(level.targetColor);

        CleanIngredientsHolder();
        InstantiateIngredients(level.ingredients);
    }

    private void RestartLevel() {
        StartLevel(_levels[_currentLevelIndex]);
    }

    private void StartNextLevel() {
        _currentLevelIndex = (_currentLevelIndex + 1) % _levels.Length;
        StartLevel(_levels[_currentLevelIndex]);
    }

    #endregion Main functionality -------------------------------------------------

    #region Debug functionality -------------------------------------------------

    struct DPoint {
        public Vector3 position;
        public Color color;
        public float radius;
        public bool wire;
    }

    private List<DPoint> debugPoints = new List<DPoint>();

    public void DebugPoint(Vector3 position, Color color, float radius = 0.01f, bool wire = false) {
        debugPoints.Add(new DPoint { position = position, color = color, radius = radius, wire = wire });
    }

    private void OnDrawGizmos() {
        var originalColor = Gizmos.color;

        foreach (DPoint point in debugPoints) {
            Gizmos.color = point.color;
            if (point.wire) {
                Gizmos.DrawWireSphere(point.position, point.radius);
        }
            else {
                Gizmos.DrawSphere(point.position, point.radius);
            }
        }

        Gizmos.color = originalColor;
    }

    #endregion Debug functionality -------------------------------------------------
}
