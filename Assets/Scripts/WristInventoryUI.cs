using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class WristInventoryUI : MonoBehaviour
{
    public PlayerInventory inventory;
    public Button toggleButton;
    public Text toggleButtonLabel;
    public RectTransform itemsContainer;
    public RectTransform itemRowTemplate;

    readonly List<GameObject> _spawned = new List<GameObject>();

    void Start()
    {
        if (inventory == null) inventory = PlayerInventory.Instance;
        if (inventory == null) { Debug.LogWarning("[WristInventoryUI] No PlayerInventory found"); return; }

        if (itemRowTemplate != null) itemRowTemplate.gameObject.SetActive(false);

        inventory.OnChanged += Refresh;
        Refresh();
    }

    void OnDestroy()
    {
        if (inventory != null) inventory.OnChanged -= Refresh;
    }

    void Refresh()
    {
        if (inventory == null) return;

        if (itemsContainer != null) itemsContainer.gameObject.SetActive(inventory.IsOpen);
        if (toggleButtonLabel != null) toggleButtonLabel.text = inventory.IsOpen ? "Close" : "Open";

        foreach (var g in _spawned) Destroy(g);
        _spawned.Clear();
        if (!inventory.IsOpen || itemRowTemplate == null || itemsContainer == null) return;

        foreach (var item in inventory.items)
        {
            var row = Instantiate(itemRowTemplate.gameObject, itemsContainer);
            row.SetActive(true);
            row.name = "Row_" + item.id;

            var txt = row.GetComponentInChildren<Text>(true);
            if (txt != null)
            {
                string state = item.canEquip ? (item.equipped ? "  [ON]" : "  [OFF]") : "";
                txt.text = item.displayName + state;
            }

            var btn = row.GetComponent<Button>();
            if (btn != null)
            {
                string id = item.id;
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => inventory.UseItem(id));
            }
            _spawned.Add(row);
        }
    }
}
