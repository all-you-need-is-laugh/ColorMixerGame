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

        // RestartLevel();
        // Add delay before start to let everything warm up - without it ingredients have non-zero angular velocity when
        // fall down
        Invoke(nameof(RestartLevel), .5f);
    }

    private void Update() {
        // Debug functionality
        if (Input.GetKeyDown(KeyCode.R)) {
            RestartLevel();
        }
        else if (Input.GetKeyDown(KeyCode.N)) {
            StartNextLevel();
        }
    }

    #endregion MonoBehaviour Hooks -------------------------------------------------

    #region Main functionality -------------------------------------------------

    private void InstantiateIngredient(GameObject ingredient, Vector3 position, Transform parent) {
        DebugPoint(parent.position + parent.forward * INGREDIENTS_ROTATION_PERSPECTIVE_K, Color.red);
        // var rotation = Quaternion.identity;
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
