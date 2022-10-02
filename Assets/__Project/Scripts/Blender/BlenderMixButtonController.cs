using UnityEngine;

// Responds for interactions with Blender Mix Button

public class BlenderMixButtonController : MonoBehaviour {
    #region Editable settings -------------------------------------------------

    [SerializeField]
    private Transform _pushedPosition;

    #endregion Editable settings -------------------------------------------------

    #region Fields, properties, constants -------------------------------------------------

    private Vector3 _startPosition;

    #endregion Fields, properties, constants -------------------------------------------------

    #region MonoBehaviour Hooks -------------------------------------------------

    private void Start() {
        _startPosition = transform.position;
    }

#if (UNITY_EDITOR)

    private void Update() {
        HandleDebugInteractions();
    }

#endif

    private void OnValidate() {
        Debug.Assert(_pushedPosition != null, $"Specify animation end point object to {GetType().Name} component!", this);
    }

    #endregion MonoBehaviour Hooks -------------------------------------------------

    #region Debug interactions handling -------------------------------------------------

#if (UNITY_EDITOR)

    private void HandleDebugInteractions() {
        if (Input.GetKeyDown(KeyCode.Alpha9)) {
            SwitchOn();
        }
        else if (Input.GetKeyDown(KeyCode.Alpha0)) {
            SwitchOff();
        }
    }

#endif

    #endregion Debug interactions handling -------------------------------------------------

    #region Main functionality -------------------------------------------------

    public void SwitchOn() {
        transform.position = _pushedPosition.position;
    }

    public void SwitchOff() {
        transform.position = _startPosition;
    }

    #endregion Main functionality -------------------------------------------------
}
