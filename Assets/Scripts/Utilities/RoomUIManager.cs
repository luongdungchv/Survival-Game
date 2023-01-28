using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomUIManager : MonoBehaviour
{
    [SerializeField] private Button startOrReadyBtn, leaveBtn;
    [SerializeField] private List<GameObject> playerTextList;
    [SerializeField] private TextMeshProUGUI roomIdText;
    [SerializeField] private TMP_InputField seedInputField;
    private Client client => Client.ins;
    private NetworkRoom roomInstance => NetworkRoom.ins;
    public int playerCount = 0;

    private void Awake()
    {
        if (roomInstance.localPlayerId == 0)
        {
            startOrReadyBtn.onClick.AddListener(client.StartGame);
            seedInputField.onValueChanged.AddListener((val) =>
            {
                client.mapSeed = int.Parse(val);
            });
        }
        else
        {
            startOrReadyBtn.onClick.AddListener(client.Ready);
            startOrReadyBtn.GetComponentInChildren<TextMeshProUGUI>().text = "Ready";
            seedInputField.interactable = false;
        }
        leaveBtn.onClick.AddListener(client.LeaveRoom);
        playerCount = roomInstance.playerNames.Count;
        for (int i = 0; i < roomInstance.playerNames.Count; i++)
        {
            var textObjContainer = playerTextList[i];
            textObjContainer.SetActive(true);
            textObjContainer.GetComponentInChildren<TextMeshProUGUI>().text = roomInstance.playerNames[i];
        }
        roomIdText.text = roomInstance.id;
        roomInstance.RegisterJoinEvent(HandlePlayerJoin);
        roomInstance.RegisterLeaveEvent(HandlePlayerLeave);
        roomInstance.RegisterReadyEvent(HandleReadyStateChange);


    }
    private void HandlePlayerJoin(string playerName)
    {
        if (playerCount >= 4) return;
        var index = playerCount;
        playerTextList[index].SetActive(true);
        playerTextList[index].GetComponentInChildren<TextMeshProUGUI>().text = playerName;
        playerCount++;
    }
    private void HandlePlayerLeave(int playerId)
    {
        playerTextList[playerId].transform.parent.gameObject.SetActive(false);
        playerCount--;
    }
    private void HandleReadyStateChange(int playerId, bool state)
    {
        playerTextList[playerId].transform.GetChild(1).gameObject.SetActive(state);
    }
    private void OnDestroy()
    {
        roomInstance.UnRegisterJoinEvent(HandlePlayerJoin);
        roomInstance.UnRegisterLeaveEvent(HandlePlayerLeave);
    }

}
