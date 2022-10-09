using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InteractableItem : MonoBehaviour
{
    [SerializeField] protected string displayName;
    [SerializeField] protected GameObject interactBtnPrefab;
    [SerializeField] protected bool interactable = true;

    private bool isPlayerTouch;

    private GameObject btnInstance;
    // Start is called before the first frame update

    private void OnTriggerEnter(Collider target)
    {
        if (!interactable) return;
        if (target.tag == "Player" && !isPlayerTouch)
        {
            btnInstance = Instantiate(interactBtnPrefab);
            btnInstance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = displayName;
            var btn = btnInstance.GetComponent<Button>();
            btn.onClick.AddListener(() => OnInteractBtnClick(btn));
            UIManager.ins.AddCollectBtn(btnInstance);
            isPlayerTouch = true;

        }
    }
    private void OnTriggerExit(Collider target)
    {
        if (!interactable) return;
        if (target.tag == "Player" && isPlayerTouch)
        {
            Destroy(btnInstance);
            isPlayerTouch = false;

        }
    }
    protected virtual void OnInteractBtnClick(Button clicker)
    {
        Destroy(clicker.gameObject);
    }
}

