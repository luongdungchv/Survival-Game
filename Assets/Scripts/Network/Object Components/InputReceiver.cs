using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputReceiver : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private float dashTime, dashDelay;
    public Vector2 movementInputVector;
    public bool sprint;
    public bool jumpPress;
    public Vector2 camDir;
    public bool startDash;
    private bool dashcheck, isDashDelaying;
    private NetworkObject netObj;
    private Coroutine setDashCoroutine, dashDelayCoroutine;

    void Start()
    {

    }
    public void HandleInput(InputPacket _packet)
    {
        if (_packet.sprint && !this.sprint && !isDashDelaying)
        {
            this.startDash = true;

            if (setDashCoroutine != null) StopCoroutine(setDashCoroutine);
            setDashCoroutine = StartCoroutine(SetDash(dashTime));

            StartCoroutine(DelayDash(dashDelay));
        }
        else if (!dashcheck) this.startDash = false;
        this.movementInputVector = _packet.inputVector;
        this.sprint = _packet.sprint;
        this.jumpPress = _packet.jump;
        this.camDir = _packet.camDir;
    }
    // Update is called once per frame
    IEnumerator SetDash(float duration)
    {
        this.dashcheck = true;
        yield return new WaitForSeconds(duration);
        this.dashcheck = false;
    }
    IEnumerator DelayDash(float duration)
    {
        this.isDashDelaying = true;
        yield return new WaitForSeconds(duration);
        this.isDashDelaying = false;
    }

}
