using System;
using UnityEngine;

// Responds for interactions with specific ingredient

[RequireComponent(typeof(Rigidbody))]
public class IngredientController : MonoBehaviour {
    #region Editable settings -------------------------------------------------

    [SerializeField]
    private float _movementExtraHeight = 0.2f;

    #endregion Editable settings -------------------------------------------------

    #region Fields, properties, constants -------------------------------------------------

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

    [HideInInspector]
    public Transform blender;

    private bool _renovationStarted = false;

    #endregion Fields, properties, constants -------------------------------------------------

    #region MonoBehaviour Hooks -------------------------------------------------

    private void Start() {
        _rigidbody = GetComponent<Rigidbody>();
    }

    #endregion MonoBehaviour Hooks -------------------------------------------------

    #region Main functionality -------------------------------------------------

    public void Interact() {
        Debug.Log($"### > IngredientController > Interact with {name}!");

        // For now ingredient still can be interacted, but copy of it must be created only after the first interaction
        if (!_renovationStarted) {
            // Just ignore async nature of the call
            _ = ingredientManager.RenewAt(transform.position, transform.rotation, transform.parent);
            _renovationStarted = true;
        }

        MoveToBlender();
    }

    private void MoveToBlender() {
        _rigidbody.velocity = Utils.CalculateLaunchVelocity(transform.position, blender.position, _movementExtraHeight);
    }

    #endregion Main functionality -------------------------------------------------
}
