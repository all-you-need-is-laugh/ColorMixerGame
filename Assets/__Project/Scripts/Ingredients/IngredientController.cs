using System;
using System.Threading.Tasks;
using UnityEngine;

// Responds for interactions with specific ingredient

[RequireComponent(typeof(Rigidbody))]
public class IngredientController : MonoBehaviour {
    #region Editable settings -------------------------------------------------

    [SerializeField]
    private float _animationExtraHeight = 0.5f;

    [SerializeField]
    private Vector3 _animationRotation = Vector3.zero;

    #endregion Editable settings -------------------------------------------------

    #region Fields, properties, constants -------------------------------------------------

    public static readonly string TAG = "Ingredient";
    public static readonly string LAYER_NAME = "Interactable";

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

    private void Awake() {
        _rigidbody = GetComponent<Rigidbody>();
    }

    private void OnValidate() {
        Debug.Assert(CompareTag(TAG), $"{GetType().Name} component must be attached to object with tag \"{TAG}\"!", this);
        Debug.Assert(gameObject.layer == LayerMask.NameToLayer(LAYER_NAME), $"{GetType().Name} component must be attached to object with layer \"{LAYER_NAME}\"!", this);
    }

    #endregion MonoBehaviour Hooks -------------------------------------------------

    #region Main functionality -------------------------------------------------

    public void FreezePhysics() {
        // Leave RigidbodyConstraints.FreezePositionY untouched!
        _rigidbody.constraints =
            RigidbodyConstraints.FreezePositionX
            | RigidbodyConstraints.FreezePositionZ
            | RigidbodyConstraints.FreezeRotation;
    }

    public void UnfreezePhysics() {
        _rigidbody.constraints = RigidbodyConstraints.None;
    }

    public Task MoveToAsync(Vector3 destination) {
        UnfreezePhysics();

        float time;
        _rigidbody.velocity = Utils.CalculateLaunchVelocity(transform.position, destination, _animationExtraHeight, out time);
        _rigidbody.angularVelocity = Mathf.Deg2Rad / time * _animationRotation;

        return Task.Delay(Mathf.FloorToInt(time * 1000));
    }

    public void ResetPhysics() {
        _rigidbody.velocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
    }

    #endregion Main functionality -------------------------------------------------
}
