using System.Threading.Tasks;
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

    [SerializeField]
    public Transform ingredientMovementEndPoint;

    #endregion Editable settings -------------------------------------------------

    #region Fields, properties, constants -------------------------------------------------

    private Vector3 _lidStartPosition;
    private bool _isOpening = false;
    private bool _isClosing = true;
    private Task _emptyTask = Task.FromResult<object>(null);

    #endregion Fields, properties, constants -------------------------------------------------

    #region MonoBehaviour Hooks -------------------------------------------------

    private void Start() {
        _lidStartPosition = _lid.position;
    }

    private void Update() {
        HandleLidInteractions();
    }

    private void OnValidate() {
        Debug.Assert(_lid != null, $"Specify lid object to {GetType().Name} component!", this);
        Debug.Assert(_animationEndPoint != null, $"Specify animation end point object to {GetType().Name} component!", this);
        Debug.Assert(ingredientMovementEndPoint != null, $"Specify ingredient movement end point object to {GetType().Name} component!", this);
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

    public Task OpenLid() {
        if (_isOpening) {
            return _emptyTask;
        }

        _isOpening = true;
        _isClosing = false;

        return _lid
            .DOJump(_animationEndPoint.position, _animationJumpPower, 1, _animationDuration)
            .AsyncWaitForCompletion();
    }

    public Task CloseLid() {
        if (_isClosing) {
            return _emptyTask;
        }

        _isOpening = false;
        _isClosing = true;

        return _lid
            .DOJump(_lidStartPosition, _animationJumpPower, 1, _animationDuration)
            .AsyncWaitForCompletion();
    }

    public async Task Mix() {
        Debug.Log("Mix called!");
        await CloseLid();
        Debug.Log("Do mix!");
    }

    #endregion Main functionality -------------------------------------------------
}
