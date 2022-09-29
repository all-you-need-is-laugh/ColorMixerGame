using System;
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
    private HashSet<IngredientController> _ingredients = new HashSet<IngredientController>();

    public Vector3 jugEntryPointPosition { get => _jugEntryPoint.position; }

    public int ingredientsNumber { get => _ingredients.Count; }

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
            _ingredients.Add(ingredientController);
        }
    }

    private void OnTriggerExit(Collider collider) {
        if (collider.TryGetComponent<IngredientController>(out IngredientController ingredientController)) {
            _ingredients.Remove(ingredientController);
        }
    }

    #endregion MonoBehaviour Hooks -------------------------------------------------

    #region Interactions handling -------------------------------------------------

    private void HandleLidInteractions() {
        if (Input.GetKeyDown(KeyCode.LeftBracket)) {
            OpenLidAsync();
        }
        else if (Input.GetKeyDown(KeyCode.RightBracket)) {
            CloseLidAsync();
        }
    }

    #endregion Interactions handling -------------------------------------------------

    #region Main functionality -------------------------------------------------

    public Task OpenLidAsync() {
        if (_isOpening) {
            return _emptyTask;
        }

        _isOpening = true;
        _isClosing = false;

        return _lid
            .DOJump(_openLidPosition.position, _lidAnimationJumpPower, 1, _lidAnimationDuration)
            .AsyncWaitForCompletion();
    }

    public Task CloseLidAsync() {
        if (_isClosing) {
            return _emptyTask;
        }

        _isOpening = false;
        _isClosing = true;

        return _lid
            .DOJump(_lidStartPosition, _lidAnimationJumpPower, 1, _lidAnimationDuration)
            .AsyncWaitForCompletion();
    }

    public async Task<Color> Mix() {
        if (ingredientsNumber < 1) {
            throw new Exception($"Forbidden to call '{nameof(Mix)}' method if there are no ingredients in blender!");
        }

        await CloseLidAsync();

        _lid.SetParent(_jug);

        Color[] colorSteps = GenerateMixColorSteps(_ingredients);

        Sequence animation = DOTween.Sequence()
            .Join(_jug.DOShakeRotation(_mixDuration, _mixStrength, _mixVibrato, _mixRandomness, false))
            .Join(AnimateColorMixing(_jugContentMaterial, colorSteps, _mixDuration));

        await Task.WhenAll(animation.AsyncWaitForCompletion(), DisposeIngredientsAsync(_ingredients, _mixDuration));

        Color finalColor = colorSteps[colorSteps.Length - 1];

        return finalColor;
    }

    public Task ResetJugTransformAsync(float animationDuration = 0.5f) {
        return DOTween.Sequence()
            .Append(_jug.DORotate(_jugStartRotation, animationDuration))
            .Join(_jug.DOMove(_jugStartPosition, animationDuration))
            .AsyncWaitForCompletion();
    }

    public void Reset() {
        _jugContentMaterial.SetFloat("_Fill", 0);
    }

    private Sequence AnimateColorMixing(Material material, Color[] colorSteps, float duration) {
        material.SetColor("_Color", colorSteps[0]);

        int stepsNumber = colorSteps.Length;
        Sequence sequence = DOTween.Sequence()
            .Join(material.DOFloat(1, "_Fill", duration));

        if (stepsNumber > 1) {
            float stepDuration = duration / (stepsNumber - 1);

            for (int i = 1; i < stepsNumber; i++) {
                sequence.Append(material.DOColor(colorSteps[i], stepDuration));
            }
        }

        return sequence;
    }

    private Color[] GenerateMixColorSteps(ICollection<IngredientController> ingredients) {
        int ingredientsNumber = ingredients.Count;
        Color[] colorSteps = new Color[ingredientsNumber];

        Color mixColor = Color.black;
        int i = 0;
        foreach (var ingredient in ingredients) {
            mixColor += ingredient.ingredientManager.ingredientColor;
            colorSteps[i] = mixColor / (i + 1);
            colorSteps[i].a = 1;
            i++;
        }

        return colorSteps;
    }

    private async Task DisposeIngredientsAsync(ICollection<IngredientController> ingredients, float duration) {
        int stepDuration = Mathf.FloorToInt(duration / ingredients.Count * 1000);
        foreach (var ingredient in ingredients) {
            await Task.Delay(stepDuration);
            ingredient.ingredientManager.Release(ingredient);
        }
        ingredients.Clear();
    }

    #endregion Main functionality -------------------------------------------------
}
