using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CraftSlotUI : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] private string itemName;
    [SerializeField] private int quantity;
    [SerializeField] private RawImage icon;
    private Color originalTint;
    private InventoryInteractionHandler iih => InventoryInteractionHandler.currentOpen;
    private Item item => Item.GetItem(itemName);
    private static UnityEvent OnRefresh;
    private void Awake()
    {
        if(OnRefresh == null) OnRefresh = new UnityEvent();
        OnRefresh.AddListener(() => this.Refresh());
        icon.texture = item.icon;
        originalTint = icon.color;
    }
    private void OnEnable()
    {
        Refresh();
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        if (!(item is ICraftable) || iih.isItemMoving) return;

        var craftData = (item as ICraftable).requiredMats;
        if (!Refresh()) return;
        foreach (var matName in craftData.Keys)
        {
            var remove = Inventory.ins.Remove(matName, craftData[matName]);
            if (remove)
            {
                iih.movingItemHolder.InitReplaceAction(itemName, quantity);
            }
        }
        OnRefresh.Invoke();
        Refresh();
    }
    public bool Refresh()
    {
        var craftData = (item as ICraftable).requiredMats;
        foreach (var mat in craftData)
        {
            if (Inventory.ins.GetItemQuantity(mat.Key) < mat.Value)
            {
                icon.color = new Color(originalTint.r, 0.5f, 0.5f, 0.5f);
                return false;
            }
        }
        icon.color = new Color(originalTint.r, originalTint.g, originalTint.b, 1f);
        return true;
    }
}