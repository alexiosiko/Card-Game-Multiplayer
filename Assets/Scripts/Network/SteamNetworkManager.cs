using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System;
using Unity.Netcode;

public class SteamNetworkManager : MonoBehaviour
{
	public static SteamNetworkManager Singleton = null;
	public Lobby? currentLobby ;
	FacepunchTransport transport;
	void Start()
	{	
		transport = GetComponent<FacepunchTransport>();

		SteamMatchmaking.OnLobbyCreated 		+= OnLobbyCreated;
		SteamMatchmaking.OnLobbyEntered 		+= OnLobbyEntered;
		SteamMatchmaking.OnLobbyMemberJoined 	+= OnLobbyMemberJoined;
		SteamMatchmaking.OnLobbyMemberLeave 	+= OnLobbyMemberLeave;
		SteamMatchmaking.OnLobbyInvite 			+= OnLobbyInvite;
		SteamMatchmaking.OnLobbyGameCreated 	+= OnLobbyGameCreated;
		SteamFriends.OnGameLobbyJoinRequested 	+= OnGameLobbyJoinRequested;
	}
	#region Server Callbacks
	public async void StartHost(int maxMembers = 8)
	{
		NetworkManager.Singleton.OnServerStarted += OnServerStarted;
		NetworkManager.Singleton.StartHost();

		currentLobby = await SteamMatchmaking.CreateLobbyAsync(maxMembers);
	}
	void StartClient(SteamId id)
	{
		NetworkManager.Singleton.OnClientConnectedCallback 	+= OnClientConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback;

		transport.targetSteamId = id;

		if (NetworkManager.Singleton.StartClient())
			Debug.Log("Client has connect", this);
	}
	void OnApplicationQuit() => Disconnect();
	public void Disconnect()
	{
		currentLobby?.Leave();

		if (NetworkManager.Singleton == null)
			return;
		
		NetworkManager.Singleton.Shutdown();
	}
	#endregion
	
	#region Steam Callbacks
	private void OnClientDisconnectCallback(ulong clientId)
	{
		Debug.Log($"Client disconnect, clientId={clientId}");
	
		NetworkManager.Singleton.OnClientConnectedCallback 	-= OnClientConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
	}
	private void OnClientConnectedCallback(ulong clientId) => Debug.Log($"Client connected, clientId={clientId}");
	private void OnServerStarted() => Debug.Log("Server has start!", this);
	private void OnGameLobbyJoinRequested(Lobby lobby, SteamId id) => StartClient(id);
	private void OnLobbyGameCreated(Lobby lobby, uint arg2, ushort arg3, SteamId id) {}
	private void OnLobbyInvite(Friend friend, Lobby lobby) => Debug.Log($"You got an invite from {friend.Name}, this");
	private void OnLobbyMemberLeave(Lobby lobby, Friend friend) {}
	private void OnLobbyMemberJoined(Lobby lobby, Friend friend) {}
	private void OnLobbyEntered(Lobby lobby)
	{
		if (NetworkManager.Singleton.IsHost)
			return;

		StartClient(lobby.Id);
	}
	void OnLobbyCreated(Result result, Lobby lobby) 
	{
		if (result != Result.OK)
		{
			Debug.Log($"Lobby cvounlt' be created, {result}", this);
			return;
		}

		lobby.SetFriendsOnly();
		lobby.SetData("Lobby name", "Cool lobby");
		lobby.SetJoinable(true);

		Debug.Log("Lobby has been created!", this);
	}
	#endregion
	void OnDestroy()
	{
		SteamMatchmaking.OnLobbyCreated -= OnLobbyCreated;
		SteamMatchmaking.OnLobbyEntered -= OnLobbyEntered;
		SteamMatchmaking.OnLobbyMemberJoined -= OnLobbyMemberJoined;
		SteamMatchmaking.OnLobbyMemberLeave -= OnLobbyMemberLeave;
		SteamMatchmaking.OnLobbyInvite -= OnLobbyInvite;
		SteamMatchmaking.OnLobbyGameCreated -= OnLobbyGameCreated;
		SteamFriends.OnGameLobbyJoinRequested -= OnGameLobbyJoinRequested;

		if (NetworkManager.Singleton == null)
			return;

		NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
		NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnectCallback;
	}
	void Awake()
	{
		if (Singleton == null)
			Singleton = this;
		else
			Destroy(gameObject);

		DontDestroyOnLoad(gameObject);
	}	
}