using UnityEngine;

public class Utils {
    public static Vector3 CalculateLaunchVelocity(Vector3 start, Vector3 end, float extraHeight, out float fullTime) {
        float gravity = Mathf.Abs(Physics.gravity.y);
        float hTop = Mathf.Max(end.y, start.y) + extraHeight;

        float hUp = hTop - start.y;
        float timeUp = Mathf.Sqrt(2 * hUp / gravity);

        float hDown = hTop - end.y;
        float timeDown = Mathf.Sqrt(2 * hDown / gravity);

        float yVelocity = gravity * timeUp;

        fullTime = timeUp + timeDown;

        float xVelocity = (end.x - start.x) / fullTime;
        float zVelocity = (end.z - start.z) / fullTime;

        return new Vector3(xVelocity, yVelocity, zVelocity);
    }
}
