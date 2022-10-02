using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

// Responds for RectTransform movement

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

#if (UNITY_EDITOR)

    private void Update() {
        HandleDebugInteractions();
    }

#endif

    #endregion MonoBehaviour Hooks -------------------------------------------------

    #region Debug interactions handling -------------------------------------------------

#if (UNITY_EDITOR)

    private void HandleDebugInteractions() {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            _ = ShowAsync();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            _ = HideAsync();
        }
    }

#endif

    #endregion Debug interactions handling -------------------------------------------------

    #region Main functionality -------------------------------------------------

    public Task ShowAsync(float duration = -1) {
        if (duration < 0) {
            duration = _defaultAnimationDuration;
        }

        return _rect
            .DOAnchorPos(_shownStatePosition, duration)
            .AsyncWaitForCompletion();
    }

    public Task HideAsync(float duration = -1) {
        if (duration < 0) {
            duration = _defaultAnimationDuration;
        }

        return _rect
            .DOAnchorPos(_hiddenStatePosition, duration)
            .AsyncWaitForCompletion();
    }

    #endregion Main functionality -------------------------------------------------
}
