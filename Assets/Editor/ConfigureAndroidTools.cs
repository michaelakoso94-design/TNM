using UnityEditor;
using UnityEditor.Android;
using UnityEngine;

public static class ConfigureAndroidTools
{
    const string Root = "/Users/michaelakosovsky/UnityAndroidTools6000_3_12f1";

    [MenuItem("Tools/Bank Vault/Fix Android Tool Paths")]
    public static void Apply()
    {
        AndroidExternalToolsSettings.sdkRootPath = Root + "/SDK";
        AndroidExternalToolsSettings.ndkRootPath = Root + "/NDK";
        AndroidExternalToolsSettings.jdkRootPath = Root + "/OpenJDK";
        AndroidExternalToolsSettings.Gradle.path = Root + "/gradle";

        Debug.Log("[BankVault] Android tool paths set to no-spaces copies under " + Root);
        Debug.Log("[BankVault] Retry File > Build And Run. If Preferences is open, close and reopen it to refresh the displayed paths.");
    }
}
