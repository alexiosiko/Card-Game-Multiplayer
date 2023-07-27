using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class GameMenuUI : NetworkBehaviour
{
	public static event System.EventHandler MenuOpen;
	public static event System.EventHandler MenuClose;
	[SerializeField] GameObject GameMenuObject;
	[SerializeField] Button gameMenuButton;
	[SerializeField] Button resumeButton;
	[SerializeField] Button disconnectButton;
	void Awake()
	{
		gameMenuButton.onClick.AddListener(() =>
		{
			GameMenuObject.SetActive(true);
			MenuOpen?.Invoke(this, System.EventArgs.Empty);
		});
		resumeButton.onClick.AddListener(() =>
		{
			GameMenuObject.SetActive(false);
			MenuClose?.Invoke(this, System.EventArgs.Empty);
		});
		disconnectButton.onClick.AddListener(() => {
			SteamNetworkManager.Singleton.Disconnect();
			UnityEngine.SceneManagement.SceneManager.LoadScene("Menu");
		});
	}
}
