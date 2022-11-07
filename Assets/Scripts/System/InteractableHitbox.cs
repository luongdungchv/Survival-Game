using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.Events;

public class InteractableHitbox : HitBox
{
    private InteractableObject owner;

    protected override void OnHitDetect(RaycastHit hit)
    {
        // if (!interactable) return;
        // if (hit.collider.tag == "Player" && !isPlayerTouch)
        // {
        //     btnInstance = Instantiate(interactBtnPrefab);
        //     btnInstance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = displayName;
        //     var btn = btnInstance.GetComponent<Button>();
        //     btn.onClick.AddListener(onInteractAction);
        //     UIManager.ins.AddCollectBtn(btnInstance);
        //     isPlayerTouch = true;

        // }
        owner.TouchDetect(hit);
    }
    protected override void OnNoHitDetect()
    {
        owner.ExitDetect();

    }
    public void SetOwner(InteractableObject owner)
    {
        this.owner = owner;
    }
}