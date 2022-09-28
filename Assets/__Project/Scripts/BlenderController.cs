using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

public class BlenderController : MonoBehaviour {
    #region Editable settings -------------------------------------------------

    [Header("Lid settings")]
    [SerializeField]
    private Transform _lid;

    [SerializeField]
    private float _lidAnimationDuration = 1f;

    [SerializeField]
    private float _lidAnimationJumpPower = 0.5f;

    [SerializeField]
    private Transform _openLidPosition;

    [Header("Jug settings")]
    [SerializeField]
    private Transform _jug;

    [SerializeField]
    private Transform _jugEntryPoint;

    [SerializeField]
    private GameObject _jugContent;

    [SerializeField]
    private float _mixDuration = 3;

    [SerializeField]
    private float _mixStrength = 90;

    [SerializeField]
    private int _mixVibrato = 10;

    [SerializeField]
    private float _mixRandomness = 90;

    #endregion Editable settings -------------------------------------------------

    #region Fields, properties, constants -------------------------------------------------

    private Vector3 _lidStartPosition;
    private bool _isOpening = false;
    private bool _isClosing = true;
    private Task _emptyTask = Task.FromResult<object>(null);
    private Vector3 _jugStartPosition;
    private Vector3 _jugStartRotation;
    private Material _jugContentMaterial;
    private HashSet<IngredientController> _ingredientsSet = new HashSet<IngredientController>();

    public Vector3 jugEntryPointPosition { get => _jugEntryPoint.position; }

    public int ingredientsNumber { get => _ingredientsSet.Count; }

    #endregion Fields, properties, constants -------------------------------------------------

    #region MonoBehaviour Hooks -------------------------------------------------

    private void Start() {
        _lidStartPosition = _lid.position;
        _jugStartPosition = _jug.position;
        _jugStartRotation = _jug.rotation.eulerAngles;

        _jugContentMaterial = _jugContent.GetComponent<Renderer>().material;

        Reset();
    }

    private void Update() {
        HandleLidInteractions();
    }

    private void OnValidate() {
        Debug.Assert(_lid != null, $"Specify lid object to {GetType().Name} component!", this);
        Debug.Assert(_openLidPosition != null, $"Specify animation end point object to {GetType().Name} component!", this);
        Debug.Assert(_jugEntryPoint != null, $"Specify ingredient movement end point object to {GetType().Name} component!", this);
        Debug.Assert(_jug != null, $"Specify jug object to {GetType().Name} component!", this);
        Debug.Assert(_jugContent != null, $"Specify jug content object to {GetType().Name} component!", this);
    }

    private void OnTriggerEnter(Collider collider) {
        if (collider.TryGetComponent<IngredientController>(out IngredientController ingredientController)) {
            _ingredientsSet.Add(ingredientController);
        }
    }

    private void OnTriggerExit(Collider collider) {
        if (collider.TryGetComponent<IngredientController>(out IngredientController ingredientController)) {
            _ingredientsSet.Remove(ingredientController);
        }
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
            .DOJump(_openLidPosition.position, _lidAnimationJumpPower, 1, _lidAnimationDuration)
            .AsyncWaitForCompletion();
    }

    public Task CloseLid() {
        if (_isClosing) {
            return _emptyTask;
        }

        _isOpening = false;
        _isClosing = true;

        return _lid
            .DOJump(_lidStartPosition, _lidAnimationJumpPower, 1, _lidAnimationDuration)
            .AsyncWaitForCompletion();
    }

    public async Task Mix() {
        if (ingredientsNumber < 1) {
            return;
        }

        await CloseLid();

        _lid.SetParent(_jug);

        await DOTween.Sequence()
            .Join(_jug.DOShakeRotation(_mixDuration, _mixStrength, _mixVibrato, _mixRandomness, false))
            .Join(GenerateMixColorAnimation())
            .AsyncWaitForCompletion();

        Debug.Log("Mixed!");
    }

    public async Task ResetJugTransform(float animationDuration = 0.5f) {
        Sequence animationSequence = DOTween.Sequence();

        await animationSequence
            .Append(_jug.DORotate(_jugStartRotation, animationDuration))
            .Join(_jug.DOMove(_jugStartPosition, animationDuration))
            .AsyncWaitForCompletion();
    }

    public void Reset() {
        _jugContentMaterial.SetFloat("_Fill", 0);
    }

    private Sequence GenerateMixColorAnimation() {
        var ingredients = _ingredientsSet.ToArray<IngredientController>();

        _jugContentMaterial.SetColor("_Color", ingredients[0].ingredientManager.ingredientColor);

        int ingredientsNumber = ingredients.Length;
        Sequence sequence = DOTween.Sequence()
            .Join(_jugContentMaterial.DOFloat(1, "_Fill", _mixDuration));

        if (ingredientsNumber > 1) {
            float stepDuration = _mixDuration / (ingredientsNumber - 1);

            Color mixColor = Color.black;
            for (int i = 1; i < ingredientsNumber; i++) {
                mixColor += ingredients[i].ingredientManager.ingredientColor;
                Color stepColor = mixColor / (i + 1);
                sequence.Append(_jugContentMaterial.DOColor(stepColor, stepDuration));
            }
        }

        return sequence;
    }

    #endregion Main functionality -------------------------------------------------
}
