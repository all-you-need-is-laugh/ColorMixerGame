using System;
using System.Threading.Tasks;
using DG.Tweening;
using UnityEngine;

// Responds for interactions with specific ingredient

[RequireComponent(typeof(Rigidbody))]
public class IngredientController : MonoBehaviour {
    #region Editable settings -------------------------------------------------

    [SerializeField]
    private float _animationJumpPower = 0.5f;

    [SerializeField]
    private float _animationDuration = 1;

    #endregion Editable settings -------------------------------------------------

    #region Fields, properties, constants -------------------------------------------------

    public bool interactable = true;

    private IngredientManager _ingredientManager;
    public IngredientManager ingredientManager {
        get => _ingredientManager;
        set {
            if (_ingredientManager != null) {
                throw new Exception($"Reassignment of {nameof(ingredientManager)} is forbidden!");
            }

            _ingredientManager = value;
        }
    }

    private Rigidbody _rigidbody;

    #endregion Fields, properties, constants -------------------------------------------------

    #region MonoBehaviour Hooks -------------------------------------------------

    private void Start() {
        _rigidbody = GetComponent<Rigidbody>();
    }

    #endregion MonoBehaviour Hooks -------------------------------------------------

    #region Main functionality -------------------------------------------------

    public Task MoveToAsync(Vector3 destination) {
        return _rigidbody
            .DOJump(destination, _animationJumpPower, 1, _animationDuration)
            .AsyncWaitForCompletion();

    }

    #endregion Main functionality -------------------------------------------------
}
