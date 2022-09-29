using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class RectTransformController : MonoBehaviour {

    #region Editable settings -------------------------------------------------

    [SerializeField]
    private Vector2 _shownStatePosition;

    [SerializeField]
    private float _defaultAnimationDuration = 1;

    #endregion Editable settings -------------------------------------------------

    #region Fields, properties, constants -------------------------------------------------

    private RectTransform _rect;
    private Vector2 _hiddenStatePosition;

    #endregion Fields, properties, constants -------------------------------------------------

    #region MonoBehaviour Hooks -------------------------------------------------

    void Start() {
        _rect = GetComponent<RectTransform>();
        _hiddenStatePosition = _rect.anchoredPosition;
    }

    #endregion MonoBehaviour Hooks -------------------------------------------------

    #region Main functionality -------------------------------------------------

    public Task ShowAsync(float duration = -1) {
        if (duration < 0) {
            duration = _defaultAnimationDuration;
        }

        return _rect
            .DOAnchorPos(_shownStatePosition, 3)
            .AsyncWaitForCompletion();
    }

    public Task HideAsync(float duration = -1) {
        if (duration < 0) {
            duration = _defaultAnimationDuration;
        }

        return _rect
            .DOAnchorPos(_hiddenStatePosition, 3)
            .AsyncWaitForCompletion();
    }

    #endregion Main functionality -------------------------------------------------
}
