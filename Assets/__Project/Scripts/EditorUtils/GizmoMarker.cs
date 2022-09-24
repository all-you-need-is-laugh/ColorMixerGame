using UnityEngine;

public class GizmoMarker : MonoBehaviour {
    [SerializeField]
    private Color _color;

    [SerializeField]
    private float _radius = 0.01f;

    [SerializeField]
    private bool _disabled = false;

    private static Color emptyColor { get; }

    void OnDrawGizmos() {
        if (!_disabled) {
            DrawMarker();
        }
    }

    void OnDrawGizmosSelected() {
        DrawMarker();
    }

    void DrawMarker() {
        var originalColor = Gizmos.color;
        Gizmos.color = _color == emptyColor ? Color.red : _color;

        Gizmos.DrawSphere(transform.position, _radius);

        Gizmos.color = originalColor;
    }
}
