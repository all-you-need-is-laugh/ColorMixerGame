using UnityEngine;

// Responds for interactions with specific ingredient

public class IngredientController : MonoBehaviour, IInteractable {
    #region Editable settings -------------------------------------------------

    [SerializeField]
    private Color _color;

    #endregion Editable settings -------------------------------------------------

    #region Fields, properties, constants -------------------------------------------------

    #endregion Fields, properties, constants -------------------------------------------------

    #region IInteractable Hooks -------------------------------------------------

    public void Interact() {
        Debug.Log($"### > IngredientController > Interact with {name}!");
    }

    #endregion IInteractable Hooks -------------------------------------------------

    #region Main functionality -------------------------------------------------

    #endregion Main functionality -------------------------------------------------
}
