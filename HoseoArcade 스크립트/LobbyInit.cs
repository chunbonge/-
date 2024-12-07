using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Unity.VisualScripting;
//using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class LobbyInit : MonoBehaviour
{
    [SerializeField] private Text playerName;
    [SerializeField] private InputField roomName; //�� �̸� �Է�
    [SerializeField] private InputField passwordNumber; // ��й�ȣ �Է�
    [SerializeField] private GameObject makeRoomPopupUI; //�� ����� �˾�
    [SerializeField] private GameObject password;
    [SerializeField] private Toggle myToggle; // ��й�ȣ ���� ���
    [SerializeField] private GameObject passwordPanel;
    [SerializeField] private InputField enterRoomPW;
    [SerializeField] private Button[] roomNumberButton;

    [SerializeField]
    private AudioClip buttonSound;
    
    private int passwordRoomNum;

    void Start()
    {
        // Toggle ��ü�� onValueChanged �̺�Ʈ�� �ݹ� �Լ��� ����մϴ�.
        myToggle.onValueChanged.AddListener(delegate { ToggleValueChanged(myToggle); });

        if(PhotonNetwork.IsConnectedAndReady == false)
        {
            StartCoroutine(TryReconnect());
        }
    }

	private void Update()
	{
		if (Input.GetMouseButtonDown(0))
		{
			SoundManager.Instance.PlayEffectOneShot(buttonSound);
		}
	}

	IEnumerator TryReconnect()
    {
        if (!PhotonNetwork.IsConnectedAndReady)
        {
            PhotonNetwork.ConnectUsingSettings();

            PhotonInit.Instance.toolTipText.StartTextEffect("�κ� ���� ��", Effect.WAIT);
            while (PhotonNetwork.IsConnectedAndReady == false)
            {
                yield return null;
            }
        }

        PhotonNetwork.JoinLobby();

        while(PhotonNetwork.InLobby == false)
        {
            yield return null;
        }
        PhotonInit.Instance.toolTipText.SetInvisible();
    }

    public void InitMainLobby(string name)
    {
        playerName.text = name;

        foreach(Button button in roomNumberButton)
        {
            SetBtnsInteractive(button, false);
        }
    }

    void SetBtnsInteractive(Button btn, bool b)
    {
        btn.interactable = false;
    }

    /// <summary>
    /// ����ȭ������ UI
    /// </summary>
    public void MoveToMainUI()
    {
        PhotonNetwork.LeaveLobby();
    }

    public void ClearInputText()
    {
        roomName.text = "";
        passwordNumber.text = "";
        enterRoomPW.text = "";
    }

    // Toggle�� ���°� ������ �� ȣ��Ǵ� �Լ��Դϴ�.
    public void ToggleValueChanged(Toggle change)
    {
        if (myToggle.isOn)
        {
            password.SetActive(true);
        }
        else
        {
            password.SetActive(false);
            passwordNumber.text = "";
        }
    }

    public void CreateNewRoom()
    {
        if (PhotonInit.Instance.CreateRoom(roomName.text, passwordNumber.text, myToggle.isOn))
        {
            ClearInputText();
            makeRoomPopupUI.SetActive(false);
        }
    }

    public void RoomListRenewal(List<RoomInfo> roomList)
    {
        for(int i = 0; i < roomNumberButton.Length; i++)
        {
            roomNumberButton[i].interactable = i < roomList.Count ? true : false;
            roomNumberButton[i].transform.GetChild(0).GetComponent<Text>().text =
                i < roomList.Count 
                ? roomList[i].Name + "\n(" + roomList[i].PlayerCount + "/" + roomList[i].MaxPlayers + ")" 
                : "";
        }
    }

    public void EnterRoom(int roomNum)
    {
        passwordRoomNum = roomNum;
        PhotonInit.Instance.EnterRoom(roomNum, false);
    }

    public void EnterRoomWithPw()
    {
        PhotonInit.Instance.EnterRoom(passwordRoomNum, true, enterRoomPW.text);
    }

    public void ShowPasswordPanel(bool b_show)
    {
        if (!b_show)
            ClearInputText();

        passwordPanel.SetActive(b_show);
    }

    public string UpdateRoomPlayerCount(Text roomText, int playerCount)
    {
        char[] tmp = null;
        int lastCharIndex = roomText.text.Length;
        for(int i = 0; i < lastCharIndex; i++)
        {
            tmp[i] = roomText.text[i];
            if (i == lastCharIndex - 4)
                tmp[i] = (char)(playerCount + '0');
        }

        return new string(tmp);
    }
}
