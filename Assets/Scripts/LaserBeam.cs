using UnityEngine;
using UnityEngine.Events;

public class LaserBeam : MonoBehaviour
{
    public string emitterId;
    public UnityEvent laserHit;

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("[Laser] " + emitterId + " broken by " + other.name);
        
        laserHit?.Invoke();
    }
}
