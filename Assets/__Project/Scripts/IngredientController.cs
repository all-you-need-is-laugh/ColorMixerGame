using System;
using UnityEngine;

// Responds for interactions with specific ingredient

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

    #endregion Fields, properties, constants -------------------------------------------------

    #region IInteractable Hooks -------------------------------------------------

    public void Interact() {
        Debug.Log($"### > IngredientController > Interact with {name}!");
    }

    #endregion IInteractable Hooks -------------------------------------------------

    #region Main functionality -------------------------------------------------

    #endregion Main functionality -------------------------------------------------
}
