using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

// Responds for interactions with Blender

public class BlenderController : MonoBehaviour {
    #region Editable settings -------------------------------------------------

    [SerializeField]
    private BlenderLidController _blenderLidController;

    [SerializeField]
    private BlenderMixButtonController _blenderMixButtonController;

    [Header("Jug settings")]
    [SerializeField]
    private Transform _jug;

    [SerializeField]
    private Transform _jugEntryPoint;

    [SerializeField]
    private Transform _jugHiddenResetPoint;

    [SerializeField]
    private GameObject _jugContent;

    [SerializeField]
    private float _resetJugContentAnimationDuration = 1;

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

    private Vector3 _jugStartPosition;
    private Vector3 _jugStartRotation;
    private Material _jugContentMaterial;
    private HashSet<IngredientController> _ingredients = new HashSet<IngredientController>();
    private Rigidbody _jugRigidbody;

    public Vector3 jugEntryPointPosition { get => _jugEntryPoint.position; }

    public int ingredientsNumber { get => _ingredients.Count; }

    #endregion Fields, properties, constants -------------------------------------------------

    #region MonoBehaviour Hooks -------------------------------------------------

    private void Start() {
        _jugStartPosition = _jug.position;
        _jugStartRotation = _jug.rotation.eulerAngles;
        _jugRigidbody = _jug.GetComponent<Rigidbody>();

        _jugContentMaterial = _jugContent.GetComponent<Renderer>().material;

        ResetJugContentFillLevel();
    }

    private void OnValidate() {
        Debug.Assert(_blenderLidController != null, $"Specify {nameof(BlenderLidController)} component to {GetType().Name} component!", this);
        Debug.Assert(_blenderMixButtonController != null, $"Specify {nameof(BlenderMixButtonController)} component to {GetType().Name} component!", this);
        Debug.Assert(_jugEntryPoint != null, $"Specify jug entry point object to {GetType().Name} component!", this);
        Debug.Assert(_jugHiddenResetPoint != null, $"Specify jug hidden reset point object to {GetType().Name} component!", this);
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

    #region Main functionality -------------------------------------------------

    public Task OpenLidAsync() {
        return _blenderLidController.OpenAsync();
    }

    public Task CloseLidAsync() {
        return _blenderLidController.CloseAsync();
    }

    public async Task<Color> MixAsync() {
        if (ingredientsNumber < 1) {
            throw new Exception($"Forbidden to call '{nameof(MixAsync)}' method if there are no ingredients in blender!");
        }

        _blenderMixButtonController.SwitchOn();

        await CloseLidAsync();

        Transform lid = _blenderLidController.transform;
        Transform originalLidParent = lid.parent;
        lid.SetParent(_jug);

        Color[] colorSteps = GenerateMixColorSteps(_ingredients);

        Sequence animation = DOTween.Sequence()
            .Join(_jug.DOShakeRotation(_mixDuration, _mixStrength, _mixVibrato, _mixRandomness, false))
            .Join(AnimateColorMixing(_jugContentMaterial, colorSteps, _mixDuration));

        await Task.WhenAll(animation.AsyncWaitForCompletion(), DisposeIngredientsAsync(_ingredients, _mixDuration));

        _blenderMixButtonController.SwitchOff();

        lid.SetParent(originalLidParent);

        Color finalColor = colorSteps[colorSteps.Length - 1];

        return finalColor;
    }

    public Task ResetJugContentAsync(float animationDuration = -1) {
        if (animationDuration < 0) {
            animationDuration = _resetJugContentAnimationDuration;
        }

        _jugRigidbody.constraints = RigidbodyConstraints.FreezeAll;

        Tween moveForward = _jug
            .DOMove(_jugHiddenResetPoint.position, animationDuration / 2)
            .OnComplete(ResetJugContentFillLevel);

        Tween moveBackWards = _jug
            .DOJump(_jugStartPosition, 1, 1, animationDuration / 2)
            .OnComplete(() => _jugRigidbody.constraints = RigidbodyConstraints.None);

        return DOTween.Sequence()
            .Append(moveForward)
            .Append(moveBackWards)
            .AsyncWaitForCompletion();
    }

    public Task ResetJugTransformAsync(float animationDuration = 0.5f) {
        return DOTween.Sequence()
            .Join(_jug.DORotate(_jugStartRotation, animationDuration))
            .Join(_jug.DOMove(_jugStartPosition, animationDuration))
            .OnComplete(ResetJugPhysics)
            .AsyncWaitForCompletion();
    }

    public void ResetJugContentFillLevel() {
        _jugContentMaterial.SetFloat("_Fill", 0);
    }

    public void ResetJugPhysics() {
        _jugRigidbody.velocity = Vector3.zero;
        _jugRigidbody.angularVelocity = Vector3.zero;
    }

    private Sequence AnimateColorMixing(Material material, Color[] colorSteps, float duration) {
        material.SetColor("_Color", colorSteps[0]);

        int stepsNumber = colorSteps.Length;
        Sequence sequence = DOTween.Sequence();

        if (stepsNumber > 1) {
            float stepDuration = duration / (stepsNumber - 1);

            for (int i = 1; i < stepsNumber; i++) {
                sequence.Append(material.DOColor(colorSteps[i], stepDuration));
            }
        }

        sequence.Insert(0, material.DOFloat(1, "_Fill", duration));

        return sequence;
    }

    private Color[] GenerateMixColorSteps(ICollection<IngredientController> ingredients) {
        int ingredientsNumber = ingredients.Count;
        Color[] colorSteps = new Color[ingredientsNumber];

        Color mixColor = Color.black;
        int i = 0;
        foreach (var ingredient in ingredients) {
            mixColor += ingredient.ingredientManager.color;
            colorSteps[i] = mixColor / (i + 1);
            colorSteps[i].a = 1;
            i++;
        }

        return colorSteps;
    }

    private async Task DisposeIngredientsAsync(ICollection<IngredientController> ingredients, float duration) {
        int halfStepDuration = Mathf.FloorToInt(duration / ingredients.Count * 1000 / 2);
        foreach (var ingredient in ingredients) {
            await Task.Delay(halfStepDuration);
            ingredient.ingredientManager.Release(ingredient);
            await Task.Delay(halfStepDuration);
        }
        ingredients.Clear();
    }

    #endregion Main functionality -------------------------------------------------
}
