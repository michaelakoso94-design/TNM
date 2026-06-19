using UnityEngine;

public class FollowCameraChest : MonoBehaviour
{
    public Transform head;
    public float dropBelowHead = 0.4f;
    public float minimumWorldY = 0.6f;

    void LateUpdate()
    {
        if (head == null) return;
        float y = Mathf.Max(head.position.y - dropBelowHead, minimumWorldY);
        Vector3 pos = head.position;
        pos.y = y;
        transform.position = pos;
        transform.rotation = Quaternion.Euler(0f, head.eulerAngles.y, 0f);
    }
}
