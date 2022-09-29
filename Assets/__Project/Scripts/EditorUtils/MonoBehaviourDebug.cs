using System.Collections.Generic;
using UnityEngine;

public class MonoBehaviourDebug : MonoBehaviour {
    struct DPoint {
        public Vector3 position;
        public Color color;
        public float radius;
        public bool wire;
    }

    private List<DPoint> debugPoints = new List<DPoint>();

    // Most probably this hook is not implemented by user
    private void LateUpdate() {
        if (Input.GetKeyDown(KeyCode.Backspace)) {
            debugPoints.Clear();
        }
    }

    public void DebugPoint(Vector3 position, Color color, float radius = 0.01f, bool wire = false) {
        debugPoints.Add(new DPoint { position = position, color = color, radius = radius, wire = wire });
    }

    private void OnDrawGizmos() {
        var originalColor = Gizmos.color;

        foreach (DPoint point in debugPoints) {
            Gizmos.color = point.color;
            if (point.wire) {
                Gizmos.DrawWireSphere(point.position, point.radius);
            }
            else {
                Gizmos.DrawSphere(point.position, point.radius);
            }
        }

        Gizmos.color = originalColor;
    }
}
