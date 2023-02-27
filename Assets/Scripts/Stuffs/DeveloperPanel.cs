using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeveloperPanel : MonoBehaviour
{
    public static DeveloperPanel instance;
    private void Awake() {
        instance = this;
    }
    [SerializeField] private TMP_InputField inputField;
    private void Start() {
        inputField.onSubmit.AddListener((s) => {
            var command = inputField.text;
            inputField.text = "";
            if(command.Length == 0) return;
            var split = command.Split(' ');
            if(split[0] == "add"){
                Inventory.ins.Add(split[1], int.Parse(split[2]));
            }
            else if(split[0] == "regen_hp"){
                NetworkPlayer.localPlayer.GetComponent<PlayerStats>().RegenerateHP(float.Parse(split[1]));
            }
            else if(split[0] == "regen_hunger"){
                NetworkPlayer.localPlayer.GetComponent<PlayerStats>().RegenerateHungerPoint(float.Parse(split[1]));
            }
            else if(split[0] == "regen_stamina"){
                NetworkPlayer.localPlayer.GetComponent<PlayerStats>().RegenerateStamina(float.Parse(split[1]));
            }
            else if(split[0] == "invincible"){
                if(split[1] == "on") NetworkPlayer.localPlayer.GetComponent<PlayerStats>().SetInvincible(true);
                else if(split[1] == "off") NetworkPlayer.localPlayer.GetComponent<PlayerStats>().SetInvincible(false);
            }
            
            inputField.ActivateInputField();
        });
        inputField.ActivateInputField();
    }
    public void FocusInputField(){
        inputField.ActivateInputField();
    }
    private void OnEnable() {
        inputField.ActivateInputField();
    }
    private void Update() {
        if(inputField.isFocused && Input.GetKeyDown(KeyCode.Return)){
            
        }
    }
}
