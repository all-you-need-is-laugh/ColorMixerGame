using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// Responds for game rules, interactions handling and level switching
// TODO: Refactor Game manager to comply Single responsibility principle

public class GameManager : MonoBehaviour {
    #region Editable settings -------------------------------------------------

    [SerializeField]
    private LevelSettings[] _levels;

    [SerializeField]
    private Image _orderColorImage;

    [SerializeField]
    private Transform _ingredientsHolder;

    [SerializeField]
    private float _ingredientsPlacementWidth = .5f;

    [Tooltip("Main camera will be picked by default")]
    [SerializeField]
    private Camera _camera;

    #endregion Editable settings -------------------------------------------------

    #region Fields, properties, constants -------------------------------------------------

    private const float INGREDIENTS_ROTATION_PERSPECTIVE_K = 1f;
    private static GameManager _instance;
    private int _currentLevelIndex = 0;

    #endregion Fields, properties, constants -------------------------------------------------

    #region MonoBehaviour Hooks -------------------------------------------------

    private void Start() {
        if (_instance != null) {
            Debug.LogError($"Attempt to instantiate more than one {GetType().Name} component", this);
            Debug.LogError($"Existing instance of {GetType().Name} component", _instance);
            return;
        }
        else {
            _instance = this;
        }

        if (_levels.Length == 0) {
            Debug.LogError($"Add at least 1 level to {GetType().Name} component", this);
            return;
        }

        if (_orderColorImage == null) {
            Debug.LogError($"Attach order UI Image object to {GetType().Name} component", this);
            return;
        }

        if (_ingredientsHolder == null) {
            Debug.LogError($"Attach ingredients holder to {GetType().Name} component", this);
            return;
        }

        if (_camera == null) {
            if (Camera.main == null) {
                Debug.LogError($"Attach camera to {GetType().Name} component or add main camera to the scene", this);
                return;
            }
            _camera = Camera.main;
        }

        // RestartLevel();
        // Add delay before start to let everything warm up - without it ingredients have non-zero angular velocity when
        // fall down
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
    }

    private void HandleTouch() {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hitInfo, 10)) {
                IInteractable interactable = hitInfo.collider.GetComponent<IInteractable>();
                interactable?.Interact();
            }
        }
    }

    #endregion Interactions handling -------------------------------------------------

    #region Main functionality -------------------------------------------------

    private void InstantiateIngredient(GameObject ingredient, Vector3 position, Transform parent) {
        var rotation = Quaternion.LookRotation(parent.position + parent.forward * INGREDIENTS_ROTATION_PERSPECTIVE_K - position, Vector3.up);
        Instantiate(ingredient, position, rotation, parent);
    }

    private void InstantiateIngredients(GameObject[] ingredients) {
        foreach (Transform child in _ingredientsHolder) {
            Destroy(child.gameObject);
        }

        if (ingredients.Length <= 1) {
            InstantiateIngredient(ingredients[0], _ingredientsHolder.position, _ingredientsHolder);
            return;
        }

        Vector3 leftPosition = _ingredientsHolder.position - _ingredientsHolder.right * _ingredientsPlacementWidth / 2;
        Vector3 rightPosition = _ingredientsHolder.position + _ingredientsHolder.right * _ingredientsPlacementWidth / 2;
        Vector3 positionPadding = (rightPosition - leftPosition) / ingredients.Length;
        Vector3 position = leftPosition + positionPadding / 2;
        foreach (GameObject ingredient in ingredients) {
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

        InstantiateIngredients(level.ingredients);
    }

    private void RestartLevel() {
        StartLevel(_levels[_currentLevelIndex]);
    }

    private void StartNextLevel() {
        _currentLevelIndex = (_currentLevelIndex + 1) % _levels.Length;
        StartLevel(_levels[_currentLevelIndex]);
    }

    struct PointWithColor {
        public Vector3 point;
        public Color color;
    }
    private List<PointWithColor> debugPoints = new List<PointWithColor>();
    private void DebugPoint(Vector3 point, Color color) {
        debugPoints.Add(new PointWithColor { point = point, color = color });
    }
    void OnDrawGizmos() {
        foreach (PointWithColor pair in debugPoints) {
            Gizmos.color = pair.color;
            Gizmos.DrawSphere(pair.point, 0.01f);
        }
    }

    #endregion Main functionality -------------------------------------------------
}
