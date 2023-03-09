using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class ButtonEvents : MonoBehaviour
{
    public void InitializeGame()
    {
        GameManager.singleton.InitializeGame();
    }
    public void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }
    public void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
