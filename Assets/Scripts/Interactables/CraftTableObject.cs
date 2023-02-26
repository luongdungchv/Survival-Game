using UnityEngine;
using UnityEngine.UI;

public class CraftTableObject : InteractableObject, IDamagable
{
    [SerializeField] private float hp;

    protected override void OnInteractBtnClick(Button clicker)
    {
        UIManager.ins.ToggleCraftUI();
    }

    public void OnDamage(IHitData hitData)
    {
        throw new System.NotImplementedException();
    }
}