using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// Responds for interactions with "results" UI

public class ResultsUIController : MonoBehaviour {
    #region Editable settings -------------------------------------------------

    [SerializeField]
    private float _winThreshold = 0.85f;

    [SerializeField]
    private RectTransformController _resultsPanel;

    [SerializeField]
    private Image _targetColorImageUI;

    [SerializeField]
    private Image _resultColorImageUI;

    [SerializeField]
    private TextMeshProUGUI _colorSimilarityTextUI;

    [SerializeField]
    private Button _nextLevelButtonUI;

    [SerializeField]
    private Color _successTextColor;

    [SerializeField]
    private Color _failureTextColor;

    #endregion Editable settings -------------------------------------------------


    #region MonoBehaviour Hooks -------------------------------------------------

    private void OnValidate() {
        Debug.Assert(_resultsPanel != null, $"Specify results panel object to {GetType().Name} component!", this);
        Debug.Assert(_targetColorImageUI != null, $"Specify target color image object to {GetType().Name} component!", this);
        Debug.Assert(_resultColorImageUI != null, $"Specify result color image object to {GetType().Name} component!", this);
        Debug.Assert(_colorSimilarityTextUI != null, $"Specify color similarity text object to {GetType().Name} component!", this);
        Debug.Assert(_nextLevelButtonUI != null, $"Specify next level button object to {GetType().Name} component!", this);
    }

    #endregion MonoBehaviour Hooks -------------------------------------------------


    #region Main functionality -------------------------------------------------

    public void SetTargetColor(Color targetColor) {
        _targetColorImageUI.color = targetColor;
    }

    public void SetResultColor(Color resultColor) {
        _resultColorImageUI.color = resultColor;
    }

    public void SetColorSimilarity(float similarity) {
        _colorSimilarityTextUI.text = Mathf.FloorToInt(similarity * 100) + "%";

        if (similarity >= _winThreshold) {
            _colorSimilarityTextUI.color = _successTextColor;
            _nextLevelButtonUI.interactable = true;
        }
        else {
            _colorSimilarityTextUI.color = _failureTextColor;
            _nextLevelButtonUI.interactable = false;
        }
    }

    public Task ShowAsync() {
        return _resultsPanel.ShowAsync();
    }

    public Task HideAsync() {
        return _resultsPanel.HideAsync();
    }

    #endregion Main functionality -------------------------------------------------
}

