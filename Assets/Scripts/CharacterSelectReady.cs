using System;
using System.Collections.Generic;
using Unity.Netcode;

public class CharacterSelectReady : NetworkBehaviour
{
	public static CharacterSelectReady Instance { get; private set; }

	public event EventHandler OnReadyChanged;
	
	private Dictionary<ulong, bool> _playerReadyDictionary;

	private void Awake()
	{
		Instance = this;
		_playerReadyDictionary = new Dictionary<ulong, bool>();
	}

	public void SetPlayerReady()
	{
		SetPlayerReadyServerRpc();
	}
	
	public bool IsPlayerReady(ulong clientId)
	{
		return _playerReadyDictionary.ContainsKey(clientId) && _playerReadyDictionary[clientId];
	}
	
	[ServerRpc(RequireOwnership = false)]
	private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
	{
		SetPlayerReadyClientRpc(serverRpcParams.Receive.SenderClientId);
		
		_playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

		bool allClientsReady = true;
        
		foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
		{
			if (!_playerReadyDictionary.ContainsKey(clientId) || !_playerReadyDictionary[clientId])
			{
				allClientsReady = false;
				break;
			}
		}

		if (allClientsReady)
		{
			KitchenGameLobby.Instance.DeleteLobby();
			Loader.LoadNetwork(Loader.Scene.GameScene);
		}
	}

	[ClientRpc]
	private void SetPlayerReadyClientRpc(ulong clientId)
	{
		_playerReadyDictionary[clientId] = true;
		
		OnReadyChanged?.Invoke(this, EventArgs.Empty);
	}
}