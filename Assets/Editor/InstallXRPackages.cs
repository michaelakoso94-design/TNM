using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class InstallXRPackages
{
    static AddAndRemoveRequest _req;

    [MenuItem("Tools/Bank Vault/Install XR (OpenXR + Interaction Toolkit)")]
    public static void Install()
    {
        if (_req != null && !_req.IsCompleted)
        {
            Debug.LogWarning("[BankVault] Package install already in progress.");
            return;
        }

        _req = Client.AddAndRemove(packagesToAdd: new[]
        {
            "com.unity.xr.interaction.toolkit",
            "com.unity.xr.openxr"
        });

        EditorApplication.update -= Tick;
        EditorApplication.update += Tick;
        Debug.Log("[BankVault] Installing XR packages... Watch the Package Manager window for progress.");
    }

    static void Tick()
    {
        if (_req == null || !_req.IsCompleted) return;
        EditorApplication.update -= Tick;

        if (_req.Status == StatusCode.Success)
            Debug.Log("[BankVault] XR packages installed. Unity will recompile, then run Tools → Bank Vault → Add XR Origin.");
        else
            Debug.LogError("[BankVault] Package install failed: " + (_req.Error != null ? _req.Error.message : "unknown"));

        _req = null;
    }
}
