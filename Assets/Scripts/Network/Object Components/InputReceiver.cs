using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputReceiver : MonoBehaviour
{
    // Start is called before the first frame update
    // [SerializeField] private float dashTime, dashDelay;
    public Vector2 movementInputVector;
    public bool sprint;
    public bool jumpPress;
    public bool attack;
    public bool isConsumingItem;
    public bool startDash;
    public Vector2 camDir;
    public int tick;
    private Queue<InputPacket> pendingInputPackets;
    private bool dashcheck, isDashDelaying;
    
    private PlayerStats stats;
    
    private Coroutine setDashCoroutine, dashDelayCoroutine;
    private float dashTime => stats.dashTime;
    private float dashDelay => stats.dashDelay;
    private int lastTick;

    private void Start()
    {
        pendingInputPackets = new Queue<InputPacket>();
        stats = GetComponent<PlayerStats>();
    }
    private void FixedUpdate()
    {
        if (Client.ins.isHost)
        {
            if(pendingInputPackets.Count > 0) HandleInput(pendingInputPackets.Dequeue());
            // else {
            //     this.movementInputVector = Vector2.zero;
            // }
        }
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
        this.attack = _packet.atk;
        this.isConsumingItem = _packet.isConsumingItem;
        this.tick = _packet.tick;
        if (this.jumpPress) Debug.Log("jump");
    }
    public void AddPacket(InputPacket packet)
    {
        pendingInputPackets?.Enqueue(packet);
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
