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
    [SerializeField] Button hostButton;
    [SerializeField] Button connectButton;
    [SerializeField] Button quitButton;
    [SerializeField] Button startButton;
    void Awake()
    {
        connectButton.onClick.AddListener(() => {
			NetworkManager.Singleton.StartClient();
			// SteamNetworkManager.Singleton.StartClient(0);
        }); 
        hostButton.onClick.AddListener(() => {
            // SteamNetworkManager.Singleton.StartHost();
			NetworkManager.Singleton.StartHost();
			startButton.interactable = true;
        });
        quitButton.onClick.AddListener(() => {
            Application.Quit();
        });
        startButton.onClick.AddListener(() => {
            NetworkManager.Singleton.SceneManager.LoadScene("Game", UnityEngine.SceneManagement.LoadSceneMode.Single); 
        });

		NetworkManager.Singleton.OnServerStarted += OnServerStarted;
		NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnectedCallback;

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
	public static MainMenuUI Singleton;
}