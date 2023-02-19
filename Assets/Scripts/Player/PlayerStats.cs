﻿using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    public static PlayerStats ins;

    [Header("Health Point")]
    [SerializeField] private float maxHP;
    [SerializeField] private float hpLerpDuration;

    [Header("Stamina")]
    [SerializeField] private float maxStamina;
    [SerializeField] private float staminaRegenRate, dashStaminaReduce, sprintStaminaReduce;
    [SerializeField] private float staminaLerpDuration;
    [Header("Hunger Point")]
    [SerializeField] private float maxHungerPoint;
    [SerializeField] private float hungerReduceRate, hungerLerpDuration;
    [Header("Movement")]
    public float speed;
    public float sprintSpeed, dashSpeed, jumpSpeed, dashJumpSpeed, dashDelay, dashTime, maxFallingSpeed = 55;
    public float swimSpeed, swimFastSpeed;
    public Transform pivot;
    [Header("Attack")]
    [Range(0, 100)] public float treeCritRate = 5;
    [Range(0, 100)] public float oreCritRate = 5, enemyCritRate = 5;
    public float treeCritDmg = 150;
    public float oreCritDmg = 150, enemyCritDmg = 150;
    public float knockbackRate = 100;
    public bool isDead;
    [SerializeField] private float baseAtkSpeed;
    [SerializeField] private float regenDelay;
    [Header("UI")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider staminaBar, hungerBar;
    [SerializeField] private TextMeshProUGUI coinsText;

    [Header("current stats")]
    [SerializeField] private float _hp;
    [SerializeField] private float _stamina, _hungerPoint;
    [SerializeField] private bool isRegeneratingStamina, isRegeneratingHunger;
    [SerializeField] private RectTransform test;
    private StateMachine fsm => GetComponent<StateMachine>();
    private InputReader inputReader => InputReader.ins;
    private PlayerAnimation animSystem => GetComponent<PlayerAnimation>();
    private NetworkPlayer netPlayer;
    private Coroutine regenStatmina;
    private Coroutine regenHunger;

    private Coroutine lerpHPBar, lerpStaminaBar, lerpHungerBar;
    private int currentCoins;
    public int coins {
        get => currentCoins;
        set{
            currentCoins = value;
            if(coinsText != null)
                coinsText.text = value.ToString();
        }   
    }
    

    public float hp => _hp;
    public float stamina => _stamina;
    public float hungerPoint => _hungerPoint;

    private void Start()
    {
        netPlayer = GetComponent<NetworkPlayer>();
        _hp = maxHP / 2;
        if (!netPlayer.isLocalPlayer) return;
        if (ins == null) ins = this;
        hpBar.value = Mathf.InverseLerp(0, maxHP, _hp);
        _stamina = maxStamina;
        _hungerPoint = maxHungerPoint;
        
        coins = 8;
    }
    void Update()
    {
        if (!GetComponent<NetworkPlayer>().isLocalPlayer) return;
        if (fsm.currentState.name != "Dash" &&
            (!inputReader.sprint || stamina <= 0) &&
            !isRegeneratingStamina &&
            _hungerPoint > 0 &&
            _stamina < maxStamina)
        {
            regenStatmina = StartCoroutine(GraduallyRegenStamina());
        }
        else if ((fsm.currentState.name == "Dash" || (inputReader.sprint && stamina > 0)) && regenStatmina != null)
        {
            StopCoroutine(regenStatmina);
            isRegeneratingStamina = false;
        }
    }
    IEnumerator GraduallyRegenStamina()
    {
        isRegeneratingStamina = true;
        yield return new WaitForSeconds(regenDelay);
        while (_stamina < maxStamina)
        {
            if (!RegenerateStamina(staminaRegenRate * Time.deltaTime))
                break;
            DecreaseHungerPoint(hungerReduceRate * Time.deltaTime);
            if (_hungerPoint <= 0) break;
            yield return null;
        }
        isRegeneratingStamina = false;
    }
    private void Perish()
    {
        Debug.Log("die");
        TriggerRagdoll();
        this.isDead = true;
        var allPlayers = NetworkManager.ins.GetAllPlayers();
        if(netPlayer.isLocalPlayer){
            foreach(var i in allPlayers){
                var isDead = i.Value.GetComponent<PlayerStats>().isDead;
                if(!isDead){
                    UIManager.ins.ShowDiePanel();
                    return;
                }
            }
            UIManager.ins.ShowGameOverPanel();
        }
        else if(NetworkPlayer.localPlayer.GetComponent<PlayerStats>().isDead){
            foreach(var i in allPlayers){
                var isDead = i.Value.GetComponent<PlayerStats>().isDead;
                if(!isDead){
                    return;
                }
            }
            
            UIManager.ins.ShowGameOverPanel();
        }
    }
    public void TakeDamage(float dmg)
    {
        Debug.Log("player take dmg");
        _hp -= dmg;
        if(netPlayer.isLocalPlayer) animSystem.HurtEffect(0.15f, 0.3f);
        if (_hp <= 0)
        {
            _hp = 0;
            if(netPlayer.isLocalPlayer) UpdateBar(hpBar, lerpHPBar, maxHP, _hp, hpLerpDuration);
            this.Perish();
        }
    }
    public bool RegenerateHP(float additionalHP)
    {
        if (_hp >= maxHP) return false;
        _hp += additionalHP;
        if (_hp > maxHP) _hp = maxHP;
        UpdateBar(hpBar, lerpHPBar, maxHP, _hp, hpLerpDuration);
        return true;
    }
    private bool DecreaseStatmina(float amount)
    {
        if (_stamina <= 0) return false;
        _stamina -= amount;
        if (_stamina < 0) _stamina = 0;
        UpdateBar(staminaBar, lerpStaminaBar, maxStamina, _stamina, staminaLerpDuration);

        return true;
    }
    public bool RegenerateStamina(float amount)
    {
        if (_stamina >= maxStamina) return false;
        _stamina += amount;
        if (_stamina > maxStamina) _stamina = maxStamina;
        UpdateBar(staminaBar, lerpStaminaBar, maxStamina, _stamina, staminaLerpDuration);

        return true;
    }
    private bool DecreaseHungerPoint(float amount)
    {
        if (_hungerPoint <= 0) return false;
        _hungerPoint -= amount;
        if (_hungerPoint < 0) _hungerPoint = 0;
        UpdateBar(hungerBar, lerpHungerBar, maxHungerPoint, _hungerPoint, hungerLerpDuration);

        return true;
    }
    public bool RegenerateHungerPoint(float amount)
    {
        if (_hungerPoint >= maxHungerPoint) return false;
        _hungerPoint += amount;
        if (_hungerPoint > maxHungerPoint) _hungerPoint = maxHungerPoint;
        UpdateBar(hungerBar, lerpHungerBar, maxHungerPoint, _hungerPoint, hungerLerpDuration);
        return true;
    }
    public void DashDecrease()
    {
        DecreaseStatmina(dashStaminaReduce);
    }
    public void SprintDecrease()
    {
        DecreaseStatmina(sprintStaminaReduce * Time.deltaTime);
    }
    private void UpdateBar(Slider bar, Coroutine processor, float ceil, float value, float duration)
    {
        IEnumerator UpdateBarEnum()
        {

            float startVal = bar.value;
            float endVal = Mathf.InverseLerp(0, ceil, value);
            float t = 0;
            while (t < 1)
            {
                float val = Mathf.Lerp(startVal, endVal, t);
                bar.value = val;
                t += Time.deltaTime / duration;
                yield return null;
            }
        }
        if (processor != null) StopCoroutine(processor);
        processor = StartCoroutine(UpdateBarEnum());
    }
    private void TriggerRagdoll(){
        GetComponent<Animator>().enabled = false;
        var colliders = GetComponentsInChildren<Collider>();
        foreach(var i in colliders){
            if(i.gameObject == this.gameObject) continue;
            i.enabled = true;
            if(i.TryGetComponent<Rigidbody>(out var rb)){
                rb.isKinematic = false;
                rb.useGravity = true;
            }
            
        }
    }
}
