using UnityEngine;
using UnityEngine.UI;

public class LaserControlBoard : MonoBehaviour
{
    public GameObject lasersRoot;
    public Button toggleButton;
    public Text toggleButtonLabel;
    public Image statusLED;
    public Text statusText;

    public Color armedColor = new Color(1f, 0.15f, 0.1f);
    public Color disarmedColor = new Color(0.15f, 0.9f, 0.25f);

    public bool LasersEnabled { get; private set; } = true;

    void Awake()
    {
        Apply();
    }

    public void Toggle()
    {
        LasersEnabled = !LasersEnabled;
        Apply();
    }

    void Apply()
    {
        if (lasersRoot != null) lasersRoot.SetActive(LasersEnabled);
        if (statusLED != null) statusLED.color = LasersEnabled ? armedColor : disarmedColor;
        if (statusText != null) statusText.text = LasersEnabled ? "STATUS: ARMED" : "STATUS: DISARMED";
        if (toggleButtonLabel != null) toggleButtonLabel.text = LasersEnabled ? "DISARM" : "ARM";
    }
}
