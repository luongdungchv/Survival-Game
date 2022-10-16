using System.Collections;
using System.Security;
using UnityEngine;
using UnityEngine.PlayerLoop;

public class PlayerEquipment : MonoBehaviour
{
    public static PlayerEquipment ins;
    public Item rightHandItem;
    public Item leftHandItem;
    public AttackPattern swordAtkPattern => currentSwordAtk;
    [SerializeField] private AttackPattern currentSwordAtk;

    private Coroutine consumeCoroutine;
    private void Awake()
    {
        if (ins == null) ins = this;
    }
    private void Update()
    {
        // if (InputReader.ins.SlashPress())
        // {
        //     if (rightHandItem.TryGetComponent<IUsable>(out var usableItem))
        //     {
        //         usableItem.OnUse();
        //     }
        //     if (rightHandItem.TryGetComponent<IConsumable>(out var consumableItem))
        //     {
        //         StartCoroutine(ConsumeEnumerator(consumableItem));
        //     }
        // }
        // else if (InputReader.ins.SlashRelease())
        // {
        //     if (consumeCoroutine != null) StopCoroutine(consumeCoroutine);
        // }

    }
    private IEnumerator ConsumeEnumerator(IConsumable consumableItem)
    {
        float t = 0;
        while (t < 1)
        
        {
            t += Time.deltaTime / consumableItem.duration;
            yield return null;
        }
        consumableItem.OnConsume();
    }
    public void OnUsePress()
    {
        if (rightHandItem.TryGetComponent<IUsable>(out var usableItem))
        {
            usableItem.OnUse();
        }
        if (rightHandItem.TryGetComponent<IConsumable>(out var consumableItem))
        {
            StartCoroutine(ConsumeEnumerator(consumableItem));
        }
    }
    public void OnUseReleases()
    {
        if (consumeCoroutine != null) StopCoroutine(consumeCoroutine);

    }

}