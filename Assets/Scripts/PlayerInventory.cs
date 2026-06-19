using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class InventoryItem
{
    public string id;
    public string displayName;
    public bool canEquip;
    public bool equipped;
    [NonSerialized] public Action onUse;
}

public class PlayerInventory : MonoBehaviour
{
    public static PlayerInventory Instance;

    public List<InventoryItem> items = new List<InventoryItem>();
    public bool IsOpen { get; private set; }
    public event Action OnChanged;

    public GameObject glassesModel;
    public Camera xrCamera;
    public int hiddenUVLayer = 8;

    bool _syncingPhysicalGlasses;

    void Awake()
    {
        Instance = this;
        HideLegacyGlassesModels();
        items.Clear();
        var glasses = new InventoryItem { id = "glasses", displayName = "UV Glasses", canEquip = true };
        glasses.onUse = ToggleGlasses;
        items.Add(glasses);
        ApplyGlassesState();
    }

    void Start() => OnChanged?.Invoke();

    public void ToggleOpen()
    {
        IsOpen = !IsOpen;
        OnChanged?.Invoke();
    }

    public void UseItem(string id)
    {
        var item = items.Find(i => i.id == id);
        item?.onUse?.Invoke();
    }

    public void ToggleGlasses()
    {
        var item = items.Find(i => i.id == "glasses");
        if (item == null) return;
        SetGlassesEquipped(!item.equipped);
    }

    public void SetGlassesEquipped(bool equipped)
    {
        var item = items.Find(i => i.id == "glasses");
        if (item == null) return;

        bool changed = item.equipped != equipped;
        item.equipped = equipped;
        ApplyGlassesState();
        SyncPhysicalGlasses(equipped);
        if (changed) OnChanged?.Invoke();
    }

    void ApplyGlassesState()
    {
        var item = items.Find(i => i.id == "glasses");
        bool equipped = item != null && item.equipped;
        if (xrCamera != null)
        {
            int bit = 1 << hiddenUVLayer;
            xrCamera.cullingMask = equipped
                ? xrCamera.cullingMask | bit
                : xrCamera.cullingMask & ~bit;
        }
    }

    void SyncPhysicalGlasses(bool equipped)
    {
        if (_syncingPhysicalGlasses || glassesModel == null) return;
        var glasses = glassesModel.GetComponent<Glasses>();
        if (glasses == null) return;

        _syncingPhysicalGlasses = true;
        try
        {
            glasses.SetEquipped(equipped);
        }
        finally
        {
            _syncingPhysicalGlasses = false;
        }
    }

    void HideLegacyGlassesModels()
    {
        foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
        {
            if (go.name == "Glasses_Model" && go.scene.IsValid())
                go.SetActive(false);
        }
    }
}
