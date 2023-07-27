using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;


public class GameOverUI : MonoBehaviour
{
	[SerializeField] GameObject gameOverUI;
	[SerializeField] Button restartButton;
    [SerializeField] Button quitButton;
    void Awake()
    {
		if (NetworkManager.Singleton.IsHost)
			restartButton.interactable = true;


		restartButton.onClick.AddListener(() => {
			GameManager.Singleton.RestartGame();
			gameOverUI.SetActive(false);
		});

		Singleton = this;
    }
	public void GameOverScreen()
	{
		gameOverUI.SetActive(true);
	}
	public static GameOverUI Singleton;
}
