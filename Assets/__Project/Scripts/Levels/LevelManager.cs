using System;
using System.Linq;
using UnityEngine;

// Responds for levels lifecycle

public class LevelManager : MonoBehaviour {
    #region Editable settings -------------------------------------------------

    [SerializeField]
    private LevelSettings[] _levels;

    [SerializeField]
    private GameManager _gameManager;

    #endregion Editable settings -------------------------------------------------

    #region Fields, properties, constants -------------------------------------------------

    public static LevelManager instance { get; private set; }
    private int _currentLevelIndex = 0;

    #endregion Fields, properties, constants -------------------------------------------------

    #region MonoBehaviour Hooks -------------------------------------------------

    private void OnValidate() {
        Debug.Assert(_levels.Length != 0, $"Add at least 1 level to {GetType().Name} component!", this);
        Debug.Assert(_gameManager != null, $"Specify {nameof(GameManager)} component to {GetType().Name} component!", this);
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

        // RestartLevel();
        // Add delay before start to let everything warm up - without it ingredients have non-zero angular velocity when
        // fall down at first time
        Invoke(nameof(RestartLevel), .5f);
    }

#if (UNITY_EDITOR)

    private void Update() {
        HandleDebugActions();
    }

#endif

    #endregion MonoBehaviour Hooks -------------------------------------------------

    #region Debug interactions handling -------------------------------------------------

#if (UNITY_EDITOR)

    private void HandleDebugActions() {
        if (Input.GetKeyDown(KeyCode.R)) {
            RestartLevel();
        }
        else if (Input.GetKeyDown(KeyCode.N)) {
            StartNextLevel();
        }
    }

#endif

    #endregion Debug interactions handling -------------------------------------------------

    #region Main functionality -------------------------------------------------

    private void StartLevel(LevelSettings level) {
        if (level == null) {
            throw new Exception($"Unexpected attempt to start absent level ({_currentLevelIndex})");
        }

        Color targetColor = ColorCalculations.MixColors(level.countedIngredients);
        IngredientManager[] ingredients = level.countedIngredients.Select(ingredientCounted => ingredientCounted.ingredient).ToArray();

        // Just ignore method's async nature
        _ = _gameManager.StartLevelAsync(targetColor, ingredients);
    }

    public void RestartLevel() {
        StartLevel(_levels[_currentLevelIndex]);
    }

    public void StartNextLevel() {
        _currentLevelIndex = (_currentLevelIndex + 1) % _levels.Length;
        StartLevel(_levels[_currentLevelIndex]);
    }

    #endregion Main functionality -------------------------------------------------
}
