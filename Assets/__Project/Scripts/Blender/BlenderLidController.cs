using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

// Responds for interactions with Blender Lid

public class BlenderLidController : MonoBehaviour {
    #region Editable settings -------------------------------------------------

    [SerializeField]
    private Transform _openPosition;

    [SerializeField]
    private float _animationDuration = 1f;

    [SerializeField]
    private float _animationJumpPower = 0.5f;

    #endregion Editable settings -------------------------------------------------

    #region Fields, properties, constants -------------------------------------------------

    private Vector3 _startPosition;
    private Tween _openingTween;
    private Tween _closingTween;
    private bool _isOpened = false;
    private bool _isClosed = true;
    private Task _emptyTask = Task.FromResult<object>(null);

    #endregion Fields, properties, constants -------------------------------------------------

    #region MonoBehaviour Hooks -------------------------------------------------

    private void Start() {
        _startPosition = transform.position;
    }

    private void Update() {
        HandleInteractions();
    }

    private void OnValidate() {
        Debug.Assert(_openPosition != null, $"Specify animation end point object to {GetType().Name} component!", this);
    }

    #endregion MonoBehaviour Hooks -------------------------------------------------

    #region Interactions handling -------------------------------------------------

    private void HandleInteractions() {
        if (Input.GetKeyDown(KeyCode.LeftBracket)) {
            OpenAsync();
        }
        else if (Input.GetKeyDown(KeyCode.RightBracket)) {
            CloseAsync();
        }
    }

    #endregion Interactions handling -------------------------------------------------

    #region Main functionality -------------------------------------------------

    public Task OpenAsync() {
        if (_isOpened) {
            return _emptyTask;
        }

        _isClosed = false;
        if (_closingTween != null) {
            _closingTween.Kill();
            _closingTween = null;
        }

        if (_openingTween == null) {
            _openingTween = transform
                .DOJump(_openPosition.position, _animationJumpPower, 1, _animationDuration)
                .OnComplete(() => {
                    _openingTween = null;
                    _isOpened = true;
                });
        }

        return _openingTween.AsyncWaitForCompletion();
    }

    public Task CloseAsync() {
        if (_isClosed) {
            return _emptyTask;
        }

        _isOpened = false;
        if (_openingTween != null) {
            _openingTween.Kill();
            _openingTween = null;
        }

        if (_closingTween == null) {
            _closingTween = transform
                .DOJump(_startPosition, _animationJumpPower, 1, _animationDuration)
                .OnComplete(() => {
                    _closingTween = null;
                    _isClosed = true;
                });
        }

        return _closingTween.AsyncWaitForCompletion();
    }

    #endregion Main functionality -------------------------------------------------
}
