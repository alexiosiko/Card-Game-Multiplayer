using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameMenuUI : NetworkBehaviour
{
	[SerializeField] GameObject GameMenuObject;
	[SerializeField] Button gameMenuButton;
	[SerializeField] Button resumeButton;
	[SerializeField] Button disconnectButton;
	void Awake()
	{
		gameMenuButton.onClick.AddListener(() => GameMenuObject.SetActive(true));
		resumeButton.onClick.AddListener(() => GameMenuObject.SetActive(false));

		disconnectButton.onClick.AddListener(() => {
			SteamNetworkManager.Singleton.Disconnect();
			UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
		});
	}
}
