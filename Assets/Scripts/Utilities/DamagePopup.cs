using System.Collections;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour, IPoolObject
{
    [SerializeField] private float popupDuration, popupDistance;
    private Vector3 worldPos;
    private RectTransform rectTransform => GetComponent<RectTransform>();
    private ObjectPool<DamagePopup> _pool;
    private TextMeshProUGUI tmp;
    public object pool
    {
        get => _pool;
        set
        {
            _pool = (value as ObjectPool<DamagePopup>);
        }
    }
    private void Start()
    {
        tmp = GetComponent<TextMeshProUGUI>();
        _pool = PoolManager.ins.dmgPopupPool;
        _pool.AddToPool(this);
    }
    public void OnPooled()
    {
        this.gameObject.SetActive(false);
    }

    public void OnReleased()
    {
        this.gameObject.SetActive(true);
    }
    public void Popup(Vector3 position, string text, int critLevel)
    {
        tmp.text = text;
        if (critLevel == 1)
        {
            tmp.color = Color.yellow;
            tmp.fontSize = 48;
        }
        else if(critLevel == 0){
            tmp.color = Color.white;
        }
        else if (critLevel == 2)
        {
            tmp.color = new Color(0.7f, 0, 0);
            tmp.fontSize = 60;
        }
        StartCoroutine(PopupCoroutine(position));
    }
    IEnumerator PopupCoroutine(Vector3 position)
    {
        float t = 0;
        Vector3 destination = position + Vector3.up * popupDistance;
        while (t <= 1)
        {
            worldPos = Vector3.Lerp(position, destination, t);
            rectTransform.position = GameFunctions.ins.WorldToCanvasPosition(worldPos, 90);
            yield return null;
            t += Time.deltaTime / popupDuration;
        }
        _pool.AddToPool(this);

    }
}
