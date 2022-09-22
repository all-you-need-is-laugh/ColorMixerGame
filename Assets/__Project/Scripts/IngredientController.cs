using System;
using UnityEngine;

// Responds for interactions with specific ingredient

[RequireComponent(typeof(Rigidbody))]
public class IngredientController : MonoBehaviour, IInteractable {
    #region Editable settings -------------------------------------------------

    [SerializeField]
    private Color _color;

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
    public Transform blender;

    #endregion Fields, properties, constants -------------------------------------------------

    #region MonoBehaviour Hooks -------------------------------------------------

    private void Start() {
        _rigidbody = GetComponent<Rigidbody>();
    }

    #endregion MonoBehaviour Hooks -------------------------------------------------

    #region IInteractable Hooks -------------------------------------------------

    public void Interact() {
        Debug.Log($"### > IngredientController > Interact with {name}!");

        // ingredientManager.RenewAt(transform.position);

        MoveToBlender();
    }

    #endregion IInteractable Hooks -------------------------------------------------

    #region Main functionality -------------------------------------------------

    void MoveToBlender() {
        _rigidbody.velocity = Utils.CalculateLaunchVelocity(transform.position, blender.position, 0.3f);
    }

    #endregion Main functionality -------------------------------------------------
}
