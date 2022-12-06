using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
    [SerializeField] private float _speed;
    [SerializeField] private float _sprintSpeed, _dashSpeed, _jumpSpeed;
    [SerializeField] private float baseAtkSpeed;
    [SerializeField] private float regenDelay;
    [Header("UI")]
    [SerializeField] private Slider hpBar;
    [SerializeField] private Slider staminaBar, hungerBar;

    [Header("current stats")]
    [SerializeField] private float _hp;
    [SerializeField] private float _stamina, _hungerPoint;
    [SerializeField] private bool isRegeneratingStamina, isRegeneratingHunger;
    [SerializeField] private RectTransform test;
    private StateMachine fsm => GetComponent<StateMachine>();
    private InputReader inputReader => InputReader.ins;
    private Coroutine regenStatmina;
    private Coroutine regenHunger;

    private Coroutine lerpHPBar, lerpStaminaBar, lerpHungerBar;

    public float hp => _hp;
    public float stamina => _stamina;
    public float hungerPoint => _hungerPoint;

    private void Start()
    {
        if (ins == null) ins = this;
        _hp = maxHP / 2;
        hpBar.value = Mathf.InverseLerp(0, maxHP, _hp);
        _stamina = maxStamina;
        _hungerPoint = maxHungerPoint;
        Debug.Log(Mathf.Lerp(1, 0, 0.7f));
    }
    void Update()
    {
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
    }
    private void TakeDamage(float dmg)
    {
        _hp -= dmg;
        if (_hp <= 0)
        {
            _hp = 0;
            UpdateBar(hpBar, lerpHPBar, maxHP, _hp, hpLerpDuration);
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
}
