using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(XRGrabInteractable))]
public class Glasses : MonoBehaviour
{
    public Transform headEquipPoint;
    public Transform homeAnchor;
    public Collider homeZone;
    public PlayerInventory inventory;
    public float equipRadius = 0.3f;

    // Backward-compatible names for scenes built by the old vest/pocket setup.
    public Transform pocketHome
    {
        get => homeAnchor;
        set => homeAnchor = value;
    }

    public Collider pocketZone
    {
        get => homeZone;
        set => homeZone = value;
    }

    XRGrabInteractable _grab;
    Rigidbody _rb;
    Transform _anchor;
    bool _equipped;

    void Awake()
    {
        _grab = GetComponent<XRGrabInteractable>();
        _rb = GetComponent<Rigidbody>();
        _grab.selectEntered.AddListener(OnGrabbed);
        _grab.selectExited.AddListener(OnReleased);
        if (inventory == null) inventory = PlayerInventory.Instance;
        if (inventory != null && inventory.glassesModel == null) inventory.glassesModel = gameObject;
        _anchor = homeAnchor;
        SnapTo(homeAnchor);
    }

    void Start()
    {
        if (inventory == null) inventory = PlayerInventory.Instance;
        if (inventory != null && inventory.glassesModel == null) inventory.glassesModel = gameObject;
    }

    void Update()
    {
        if (Mouse.current == null || !Mouse.current.leftButton.wasPressedThisFrame) return;

        var cam = inventory != null && inventory.xrCamera != null ? inventory.xrCamera : Camera.main;
        if (cam == null) return;

        Vector2 mousePosition = Mouse.current.position.ReadValue();
        if (!Physics.Raycast(cam.ScreenPointToRay(mousePosition), out var hit, 20f)) return;
        if (hit.collider.GetComponentInParent<Glasses>() != this) return;

        ToggleEquippedFromDesktop();
    }

    void LateUpdate()
    {
        if (_grab != null && _grab.isSelected) return;
        if (_anchor == null) return;
        transform.position = _anchor.position;
        transform.rotation = _anchor.rotation;
    }

    void OnDestroy()
    {
        if (_grab == null) return;
        _grab.selectEntered.RemoveListener(OnGrabbed);
        _grab.selectExited.RemoveListener(OnReleased);
    }

    void OnGrabbed(SelectEnterEventArgs _)
    {
        _anchor = null;
        transform.SetParent(null, true);
    }

    void OnReleased(SelectExitEventArgs _)
    {
        bool nearHead = headEquipPoint != null &&
                        Vector3.Distance(transform.position, headEquipPoint.position) <= equipRadius;
        if (nearHead)
        {
            _anchor = headEquipPoint;
            SnapTo(headEquipPoint);
            if (inventory != null) inventory.SetGlassesEquipped(true);
        }
        else
        {
            _anchor = homeAnchor;
            SnapTo(homeAnchor);
            if (inventory != null) inventory.SetGlassesEquipped(false);
        }
    }

    public void ToggleEquippedFromDesktop()
    {
        bool nextEquipped = !_equipped;
        if (inventory != null)
            inventory.SetGlassesEquipped(nextEquipped);
        else
            SetEquipped(nextEquipped);
    }

    public void SetEquipped(bool equipped)
    {
        _equipped = equipped;
        _anchor = equipped ? headEquipPoint : homeAnchor;
        SnapTo(_anchor);
    }

    void SnapTo(Transform anchor)
    {
        if (anchor == null) return;
        if (_rb != null && !_rb.isKinematic)
        {
            _rb.linearVelocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
        }
        transform.SetParent(anchor, false);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
    }
}
