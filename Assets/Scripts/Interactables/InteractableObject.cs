using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class InteractableObject : MonoBehaviour
{
    public string displayName;
    [SerializeField] protected GameObject interactBtnPrefab;
    [SerializeField] protected bool interactable = true;
    [SerializeField] private float distanceThreshold = 10;
    private InteractableHitbox hitbox;
    private Transform playerTransform;

    public bool isPlayerTouch;
    private float sqrDist;

    private GameObject btnInstance;
    protected virtual void Awake()
    {
        hitbox = GetComponent<InteractableHitbox>();
        hitbox.SetOwner(this);
        distanceThreshold = 10;
        sqrDist = distanceThreshold * distanceThreshold;
    }
    protected virtual void Start()
    {
        playerTransform = NetworkPlayer.localPlayer.transform;
//        InteractableObserver.instance.SubscribeInteractable(this);
    }
    public virtual void OnUpdate()
    {
        if ((transform.position - playerTransform.position).sqrMagnitude > distanceThreshold) return;
        hitbox.DetectHit();
    }
    public bool TouchDetect(RaycastHit target)
    {
        if (!interactable) return false;
        var netPlayer = target.collider.GetComponent<NetworkPlayer>();
        if (!netPlayer.isLocalPlayer) return false;
        if (isPlayerTouch) return true;
        if (target.collider.tag == "Player" && !isPlayerTouch)
        {
            btnInstance = Instantiate(interactBtnPrefab);
            btnInstance.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = displayName;
            var btn = btnInstance.GetComponent<Button>();
            btn.onClick.AddListener(() => OnInteractBtnClick(btn));
            UIManager.ins.AddCollectBtn(btnInstance);
            isPlayerTouch = true;
            return true;
        }
        return false;
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
