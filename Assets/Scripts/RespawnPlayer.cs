using UnityEngine;
using UnityEngine.UI;

/// Attach to the restart button.
public class RespawnPlayer : MonoBehaviour
{
    void Start()
    {
        var btn = GetComponent<Button>();
        if (btn != null) btn.onClick.AddListener(Respawn);
    }

    public void Respawn()
    {
        GameManager.RestartGame();
    }
}
