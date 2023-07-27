using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using TMPro;
using Steamworks;

public class MainMenuUI : NetworkBehaviour
{
	[SerializeField] TMP_Text lobbyLog;
    [SerializeField] Button creditsButton;
    [SerializeField] Button closeCreditsButton;
    [SerializeField] Button hostButton;
    [SerializeField] Button connectButton;
    [SerializeField] Button stopHost;
    [SerializeField] Button quitButton;
    [SerializeField] Button startButton;
	[SerializeField] GameObject creditsGameObject;
	[SerializeField] bool usingSteamNetworking = false;
    void Awake()
    {
        connectButton.onClick.AddListener(() => {
			if (usingSteamNetworking)
				SteamNetworkManager.Singleton.StartClient(0);
			else
				NetworkManager.Singleton.StartClient();
        }); 
		stopHost.onClick.AddListener(() => {
			if (usingSteamNetworking)
				SteamNetworkManager.Singleton.Disconnect();
			else
				NetworkManager.Singleton.Shutdown();

			if (usingSteamNetworking)
				stopHost.interactable = false;
			else
				stopHost.gameObject.SetActive(false);
		});
        hostButton.onClick.AddListener(() => {
			if (usingSteamNetworking)
			{
            	SteamNetworkManager.Singleton.StartHost();
				stopHost.interactable = true;
			}
			else
			{
				NetworkManager.Singleton.StartHost();
				stopHost.gameObject.SetActive(true);
			}
			
			startButton.interactable = true;
        });
        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });
        startButton.onClick.AddListener(() => {
            NetworkManager.Singleton.SceneManager.LoadScene("Game", UnityEngine.SceneManagement.LoadSceneMode.Single); 
        });
		creditsButton.onClick.AddListener(() => creditsGameObject.SetActive(true));
		closeCreditsButton.onClick.AddListener(() => creditsGameObject.SetActive(false));
		if (usingSteamNetworking)
		{
			NetworkManager.Singleton.OnServerStarted += OnServerStarted;
			NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;
		}
		
		Singleton = this;
    }
	void OnClientConnectedCallback(ulong id) => LobbyLogClientRpc($"Client connected id {id}");
	void OnServerStarted() => LobbyLogClientRpc("Server started!");
	public void LobbyLog(string text)
	{
		lobbyLog.text += '\n' + text;
	}
	[ClientRpc] public void LobbyLogClientRpc(string text)
	{
		LobbyLog(text);
	}
	public override void OnDestroy()
	{
		if (!usingSteamNetworking)
		{
			NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
			NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;	
		}
	}
	public static MainMenuUI Singleton;
}