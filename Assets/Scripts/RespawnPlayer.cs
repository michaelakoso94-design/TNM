using UnityEngine;
using UnityEngine.UI;

/// Attach to the restart button. Drag the XR Rig and the spawn point into the fields.
public class RespawnPlayer : MonoBehaviour
{
    [Tooltip("The root XR Rig / OVRCameraRig object that moves the player")]
    public Transform xrRig;

    [Tooltip("Empty GameObject placed at the starting position and rotation")]
    public Transform spawnPoint;

    void Start()
    {
        var btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(Respawn);
    }

    public void Respawn()
    {
        if (xrRig == null || spawnPoint == null) { Debug.LogWarning("[RespawnPlayer] Missing references"); return; }
        xrRig.position = spawnPoint.position;
        xrRig.rotation = spawnPoint.rotation;
    }
}
