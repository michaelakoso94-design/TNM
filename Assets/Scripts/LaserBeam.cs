using UnityEngine;

public class LaserBeam : MonoBehaviour
{
    public string emitterId;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("[Laser] " + emitterId + " broken by " + other.name);
    }
}
