using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class MainMenuUI : NetworkBehaviour
{
    [SerializeField] Button hostButton;
    [SerializeField] Button connectButton;
    [SerializeField] Button quitButton;
    [SerializeField] Button startButton;
    void Awake()
    {
        connectButton.onClick.AddListener(() => {
			NetworkManager.Singleton.StartClient();
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
    }
}