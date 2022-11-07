using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour
{
    [SerializeField] protected string displayName;
    [SerializeField] protected GameObject interactBtnPrefab;
    [SerializeField] protected bool interactable = true;
    [SerializeField] private InteractableHitbox hitbox;

    private bool isPlayerTouch;

    private GameObject btnInstance;
    protected virtual void Awake()
    {
        hitbox = GetComponent<InteractableHitbox>();
        hitbox.SetOwner(this);
    }
    private void Update()
    {
        hitbox.DetectHit();
    }
    // Start is called before the first frame update

    public void TouchDetect(RaycastHit target)
    {
        if (!interactable) return;
        if (target.collider.tag == "Player" && !isPlayerTouch)
        {
            btnInstance = Instantiate(interactBtnPrefab);
            btnInstance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = displayName;
            var btn = btnInstance.GetComponent<Button>();
            btn.onClick.AddListener(() => OnInteractBtnClick(btn));
            UIManager.ins.AddCollectBtn(btnInstance);
            isPlayerTouch = true;

        }
    }
    public void ExitDetect()
    {
        if (!interactable) return;
        if (isPlayerTouch)
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

