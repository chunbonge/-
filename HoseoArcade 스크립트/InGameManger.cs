using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Linq;
using Random = UnityEngine.Random;
using UnityEditor;

public class InGameManger : MonoBehaviourPun
{
	// ���� ���� ���� ������ ����
	private Player winner;
	private int count;

	[Header("�÷��̾� ���� ��ġ �Ҵ�")]
	[Tooltip("�÷��̾� ���� ��ġ ����Ʈ")] public  GameObject[] playerSpawnLocation = new GameObject[4];

	[Header("�÷��̾� ���� ����")]
	[Tooltip("�÷��̾� ������")] public GameObject[] PlayerPrefabs = new GameObject[4];

	//���� ��ȯ�� ���� �ð� ���� ����
	[Header("�¸� �÷��̾� ������ ����")]
	[Tooltip("���� Ŭ��� ��� winner UIĵ����")] public GameObject winnerCanvas;
    [Tooltip("���� Ŭ���� �� winner UI �ε������� �ð�")] public float showWinnerUIInvokeTime;
	[Tooltip("�¸��� �̸� �ؽ�Ʈ")] public Text winnerName;
	[Tooltip("�¸� �̹����� ��� ��ġ")] public RectTransform winnerImgPos;
	[Tooltip("�¸��� �̹��� ����")] public RawImage[] winnerImgs = new RawImage[4];
	[Tooltip("�������� ȭ�� �Ѿ�� ��� �ð�")] public float WaitTime;
	[Tooltip("n �� �ڿ� �������� �̵��մϴ� �ؽ�Ʈ")] public Text BackToWatingSecond; //�ð� ����

	[SerializeField]
	private AudioClip finishSound;
	[SerializeField]
	private AudioClip LevelSound;

	private void Start()
    {
		winner = null;
		count = 0;		
	}

	public void IsGameClear()
    {
		winnerCanvas.SetActive(true);
		SoundManager.Instance.StopBGM();
		SoundManager.Instance.PlayEffectOneShot(finishSound);
		RawImage image = Instantiate(winnerImgs[(int)winner.CustomProperties["characterIndex"]], winnerImgPos);
		image.rectTransform.sizeDelta = new Vector2(165f, 165f);
		winnerName.text = winner.NickName;
		StartCoroutine(BackToWaitingRoom(WaitTime));
    }

	IEnumerator BackToWaitingRoom(float time)
    {
		float waitTime = time;
		
		while(waitTime >= 0f)
        {
			SetWatingSecondText((int)waitTime);
			waitTime -= 1f;
			yield return new WaitForSeconds(1f);
		}

		SceneManager.LoadScene("WaitingLevel");
    }

    public void SetWatingSecondText(int time)
    {
		BackToWatingSecond.text = time + " �� �ڿ� �������� �̵��մϴ�...";
	}

	public IEnumerator CreatePlayer(Player myInfo)
	{
		Vector3 spawnPosition = playerSpawnLocation[(int)myInfo.CustomProperties["waitingIndex"]].transform.position;
		PhotonNetwork.Instantiate(PlayerPrefabs[(int)myInfo.CustomProperties["characterIndex"]].name, 
			spawnPosition, Quaternion.identity, 0);

		yield return null;
	}

    public void GameStart(Player myInfo)
	{
		SoundManager.Instance.PlayBGM(LevelSound);

		StartCoroutine(CheckWinner());

        if (PhotonNetwork.IsMasterClient)
		{
            CreateRandomItem[] creatRand;
            creatRand = GameObject.FindObjectsOfType<CreateRandomItem>();
            for (int i = 0; i < creatRand.Length; i++)
            {
				creatRand[i].InitRandItem();
                creatRand[i].RandIteam();
            }
        }
            
    }

	IEnumerator CheckWinner()
    {
		while (true)
		{
			count = CheckPlayerAlive();
			if (count <= 1) break;
			yield return null;
		}

		if(count == 0)
        {
			Debug.Log("�÷��̾ �� �������ϴ� �ٽ� �����ض�");
        }
		else
        {
			foreach(Player player in PhotonNetwork.PlayerList)
            {
				if((bool)player.CustomProperties["isDie"] == false)
                {
					winner = player;
				}
            }
			Invoke("IsGameClear", showWinnerUIInvokeTime);
		}
	}

	public int CheckPlayerAlive()
	{
		int aliveCount = PhotonNetwork.PlayerList.Length;
		foreach (Player player in PhotonNetwork.PlayerList)
		{
			if ((bool)player.CustomProperties["isDie"])
			{
				aliveCount -= 1;
			}
		}
		return aliveCount;
	}
} 
