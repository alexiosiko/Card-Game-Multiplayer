using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;


public class GameMenuUI : MonoBehaviour
{
	[SerializeField] GameObject gameOverUI;
	[SerializeField] Button restartButton;
    [SerializeField] Button quitButton;
    void Awake()
    {
		if (NetworkManager.Singleton.IsHost)
			restartButton.interactable = true;


		restartButton.onClick.AddListener(() => {
			GameManager.singleton.RestartGame();
			gameOverUI.SetActive(false);
		});

		Singleton = this;
    }
	public void GameOverScreen()
	{
		gameOverUI.SetActive(true);
	}
	public static GameMenuUI Singleton;
}
