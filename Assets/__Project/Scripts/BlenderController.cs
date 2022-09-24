using DG.Tweening;
using UnityEngine;

public class BlenderController : MonoBehaviour {
    #region Editable settings -------------------------------------------------

    [SerializeField]
    private Transform _lid;

    [SerializeField]
    private float _animationDuration = 1f;

    [SerializeField]
    private float _animationJumpPower = 0.5f;

    [SerializeField]
    private Transform _animationEndPoint;

    #endregion Editable settings -------------------------------------------------

    #region Fields, properties, constants -------------------------------------------------

    private Tween _lidAnimation;

    #endregion Fields, properties, constants -------------------------------------------------

    #region MonoBehaviour Hooks -------------------------------------------------

    private void Start() {
        _lidAnimation = _lid.DOJump(_animationEndPoint.position, _animationJumpPower, 1, _animationDuration)
            .SetAutoKill(false)
            .Pause();
    }

    private void Update() {
        HandleLidInteractions();
    }

    private void OnValidate() {
        Debug.Assert(_lid != null, $"Attach lid object to {GetType().Name} component!", this);
        Debug.Assert(_animationEndPoint != null, $"Attach animation end point object to {GetType().Name} component!", this);
    }

    #endregion MonoBehaviour Hooks -------------------------------------------------

    #region Interactions handling -------------------------------------------------

    private void HandleLidInteractions() {
        if (Input.GetKeyDown(KeyCode.LeftBracket)) {
            OpenLid();
        }
        else if (Input.GetKeyDown(KeyCode.RightBracket)) {
            CloseLid();
        }
    }

    #endregion Interactions handling -------------------------------------------------

    #region Main functionality -------------------------------------------------

    private void OpenLid() {
        _lidAnimation.PlayForward();
    }

    private void CloseLid() {
        _lidAnimation.PlayBackwards();
    }

    #endregion Main functionality -------------------------------------------------
}
