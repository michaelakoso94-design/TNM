using UnityEngine;
using UnityEngine.UI;

/// Attaches a Canvas to the VR camera so the restart button always appears
/// in the top-left corner of the player's view, regardless of where they look.
public class RestartButtonUI : MonoBehaviour
{
    [Header("References")]
    public Camera xrCamera;
    public Button restartButton;

    [Header("Position")]
    public float distanceFromCamera = 1.5f;
    public float offsetX = -0.55f; // negative = left
    public float offsetY = 0.35f;  // positive = up

    Canvas _canvas;

    void Awake()
    {
        if (xrCamera == null)
            xrCamera = Camera.main;

        _canvas = GetComponentInChildren<Canvas>();
        if (_canvas != null)
        {
            _canvas.renderMode = RenderMode.WorldSpace;
            _canvas.worldCamera = xrCamera;
        }

        if (restartButton != null)
            restartButton.onClick.AddListener(GameManager.RestartGame);
    }

    void LateUpdate()
    {
        if (xrCamera == null) return;

        Transform cam = xrCamera.transform;
        Vector3 forward = cam.forward;
        Vector3 right = cam.right;
        Vector3 up = cam.up;

        transform.position = cam.position
            + forward * distanceFromCamera
            + right * offsetX
            + up * offsetY;

        transform.rotation = cam.rotation;
    }
}
