using UnityEngine;

public class LaserSweep : MonoBehaviour
{
    public float minAngle = -10f;
    public float maxAngle = 10f;
    public float period = 4f;
    public float phase = 0f;

    void Update()
    {
        float t = Mathf.PingPong(Time.time / (period * 0.5f) + phase, 1f);
        float a = Mathf.Lerp(minAngle, maxAngle, t);
        transform.localRotation = Quaternion.Euler(0f, 0f, a);
    }
}
