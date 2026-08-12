using UnityEngine;

/// <summary>
/// Joystick locomotion for a Meta (OVRCameraRig) rig.
/// Left stick = continuous move relative to head facing. Right stick = snap (or smooth) turn.
/// Attach to the camera rig root; assign <see cref="head"/> to CenterEyeAnchor.
/// </summary>
public class PlayerLocomotion : MonoBehaviour
{
    [Header("References")]
    [Tooltip("CenterEyeAnchor (the tracked head). Movement is relative to where the player looks.")]
    public Transform head;

    [Header("Move")]
    public float moveSpeed = 2.5f;
    [Range(0f, 0.9f)] public float deadZone = 0.15f;

    [Header("Laser Blocking")]
    [Tooltip("Radius of the player's body used to test against active laser beams.")]
    public float bodyRadius = 0.3f;

    [Header("Turn")]
    public bool snapTurn = true;
    public float snapAngle = 45f;
    public float smoothTurnSpeed = 90f;
    [Range(0.5f, 0.95f)] public float snapThreshold = 0.7f;

    bool _snapReady = true;

    void Update()
    {
        Vector2 move = OVRInput.Get(OVRInput.Axis2D.PrimaryThumbstick);   // left stick
        Vector2 turn = OVRInput.Get(OVRInput.Axis2D.SecondaryThumbstick); // right stick

        // ---- Move (relative to head yaw) ----
        if (move.magnitude > deadZone)
        {
            Transform dirRef = head != null ? head : transform;
            Vector3 fwd = dirRef.forward; fwd.y = 0f; fwd.Normalize();
            Vector3 right = dirRef.right; right.y = 0f; right.Normalize();
            Vector3 dir = right * move.x + fwd * move.y;
            Vector3 candidate = transform.position + dir * (moveSpeed * Time.deltaTime);
            if (!IsBlockedByLaser(candidate))
                transform.position = candidate;
        }

        // ---- Turn (rotate around the head so the view pivots in place) ----
        if (snapTurn)
        {
            if (Mathf.Abs(turn.x) > snapThreshold)
            {
                if (_snapReady)
                {
                    RotateAroundHead(Mathf.Sign(turn.x) * snapAngle);
                    _snapReady = false;
                }
            }
            else if (Mathf.Abs(turn.x) < deadZone)
            {
                _snapReady = true;
            }
        }
        else if (Mathf.Abs(turn.x) > deadZone)
        {
            RotateAroundHead(turn.x * smoothTurnSpeed * Time.deltaTime);
        }
    }

    void RotateAroundHead(float degrees)
    {
        Vector3 pivot = head != null ? head.position : transform.position;
        transform.RotateAround(pivot, Vector3.up, degrees);
    }

    // Active laser beams stay solid to physics queries; disarmed/solved beams are
    // deactivated (see LaserPuzzleBoard/LaserControlBoard) and drop out of the query.
    bool IsBlockedByLaser(Vector3 candidatePosition)
    {
        float height = head != null ? Mathf.Max(0.1f, head.position.y - transform.position.y) : 1.6f;
        Vector3 bottom = candidatePosition + Vector3.up * bodyRadius;
        Vector3 top = candidatePosition + Vector3.up * Mathf.Max(bodyRadius, height - bodyRadius);

        foreach (var hit in Physics.OverlapCapsule(bottom, top, bodyRadius, ~0, QueryTriggerInteraction.Collide))
        {
            if (hit.GetComponentInParent<LaserBeam>() != null) return true;
        }
        return false;
    }
}
