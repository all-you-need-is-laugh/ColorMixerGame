using UnityEngine;

public class GizmoMarker : MonoBehaviour {
    [SerializeField]
    private Color _color;

    [SerializeField]
    private float _radius = 0.01f;

    [SerializeField]
    private bool _showDirectionOnSelection = false;

    [SerializeField]
    private bool _disabled = false;

    void OnDrawGizmos() {
        if (!_disabled) {
            DrawMarker();
        }
    }

    void OnDrawGizmosSelected() {
        DrawMarker(_showDirectionOnSelection);
    }

    void DrawMarker(bool showDirection = false) {
        var originalColor = Gizmos.color;
        Gizmos.color = _color == Color.clear ? Color.red : _color;

        Gizmos.DrawSphere(transform.position, _radius);

        if (showDirection) {
            Gizmos.DrawRay(transform.position, transform.forward);
        }

        Gizmos.color = originalColor;
    }
}
