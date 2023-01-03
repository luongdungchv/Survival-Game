using System.Collections;
using System.Security;
using UnityEngine;
using UnityEngine.PlayerLoop;
using UnityEngine.UI;

public class PlayerEquipment : MonoBehaviour
{
    public static PlayerEquipment ins;
    public Item rightHandItem;
    public Item leftHandItem;
    public int currentEquipIndex;
    [SerializeField] private AttackPattern currentSwordAtk, currentAxeAtk;
    [SerializeField] private Slider consumeBar;
    public bool isConsumingItem => consumeBar.gameObject.activeSelf;


    private Coroutine consumeCoroutine;
    private void Awake()
    {
        if (ins == null && GetComponent<NetworkPlayer>().isLocalPlayer) ins = this;
    }
    private void Update()
    {

    }
    private IEnumerator ConsumeEnumerator(IConsumable consumableItem)
    {
        yield return new WaitForSeconds(0.2f);
        consumeBar.gameObject.SetActive(true);
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime / consumableItem.duration;
            consumeBar.value = 1 - t;
            yield return null;
        }
        consumableItem.OnConsume(currentEquipIndex);
        consumeBar.gameObject.SetActive(false);
    }
    public void OnUsePress()
    {

        if (rightHandItem != null && rightHandItem.TryGetComponent<IUsable>(out var usableItem))
        {

            usableItem.OnUse(currentEquipIndex);
        }
        if (rightHandItem != null && rightHandItem.TryGetComponent<IConsumable>(out var consumableItem))
        {
            // Debug.Log("h");
            consumeCoroutine = StartCoroutine(ConsumeEnumerator(consumableItem));
        }
    }
    public void OnUseReleases()
    {
        if (consumeCoroutine != null)
        {
            StopCoroutine(consumeCoroutine);
            consumeBar.gameObject.SetActive(false);
        }

    }

}