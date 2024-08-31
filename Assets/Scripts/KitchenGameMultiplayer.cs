using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using Unity.Services.Authentication;
using UnityEngine.SceneManagement;

public class KitchenGameMultiplayer : NetworkBehaviour
{
	public const int MAX_PLAYER_AMOUNT = 4;
	private const string PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER = "PlayerNameMultiplayer";
	
	public static KitchenGameMultiplayer Instance { get; private set; }

	public static bool PlayMultiplayer;

	public event EventHandler OnTryingToJoinGame;
	public event EventHandler OnFailedToJoinGame;
	public event EventHandler OnPlayerDataNetworkListChanged;

	[SerializeField] private KitchenObjectListSO _kitchenObjectListSO;
	[SerializeField] private List<Color> _playerColorList;

	private NetworkList<PlayerData> _playerDataNetworkList;
	private string _playerName;

	private void Awake()
	{
		Instance = this;
		
		DontDestroyOnLoad(gameObject);

		_playerName = PlayerPrefs.GetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, "PlayerName" + UnityEngine.Random.Range(100, 1000));
		
		_playerDataNetworkList = new NetworkList<PlayerData>();
		_playerDataNetworkList.OnListChanged += PlayerDataNetworkList_OnListChanged;
	}

	private void Start()
	{
		if (!PlayMultiplayer)
		{
			StartHost();
			Loader.LoadNetwork(Loader.Scene.GameScene);
		}
	}

	public string GetPlayerName()
	{
		return _playerName;
	}

	public void SetPlayerName(string playerName)
	{
		_playerName = playerName;
		
		PlayerPrefs.SetString(PLAYER_PREFS_PLAYER_NAME_MULTIPLAYER, playerName);
	}
	
	public void StartHost()
	{
		NetworkManager.Singleton.ConnectionApprovalCallback += NetworkManager_ConnectionApprovalCallback;
		NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
		NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Server_OnClientDisconnectCallback;
		
		NetworkManager.Singleton.StartHost();
	}
	
	public void StartClient()
	{
		OnTryingToJoinGame?.Invoke(this, EventArgs.Empty);
		
		NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_Client_OnClientDisconnectCallback;
		NetworkManager.Singleton.OnClientConnectedCallback += NetworkManager_Client_OnClientConnectedCallback;
		NetworkManager.Singleton.StartClient();
	}
	
	public void SpawnKitchenObject(KitchenObjectSO kitchenObjectSO, IKitchenObjectParent kitchenObjectParent) 
	{
		SpawnKitchenObjectServerRpc(GetKitchenObjectSOIndex(kitchenObjectSO), kitchenObjectParent.GetNetworkObject());
	}
	
	public int GetKitchenObjectSOIndex(KitchenObjectSO kitchenObjectSO)
	{
		return _kitchenObjectListSO.KitchenObjectSOList.IndexOf(kitchenObjectSO);
	}
	
	public KitchenObjectSO GetKitchenObjectSOFromIndex(int kitchenObjectSOIndex)
	{
		return _kitchenObjectListSO.KitchenObjectSOList[kitchenObjectSOIndex];
	}

	public void DestroyKitchenObject(KitchenObject kitchenObject)
	{
		DestroyKitchenObjectServerRpc(kitchenObject.NetworkObject);
	}
	
	public bool IsPlayerIndexConnected(int playerIndex)
	{
		return playerIndex < _playerDataNetworkList.Count;
	}
	
	public int GetPlayerDataIndexFromClientId(ulong clientId)
	{
		for (int i = 0; i < _playerDataNetworkList.Count; i++)
		{
			if (_playerDataNetworkList[i].ClientId == clientId)
			{
				return i;
			}
		}

		return -1;
	}

	public PlayerData GetPlayerDataFromClientId(ulong clientId)
	{
		foreach (PlayerData playerData in _playerDataNetworkList)
		{
			if (playerData.ClientId == clientId)
			{
				return playerData;
			}
		}

		return default;
	}

	public PlayerData GetPlayerData()
	{
		return GetPlayerDataFromClientId(NetworkManager.Singleton.LocalClientId);
	}

	public PlayerData GetPlayerDataFromPlayerIndex(int playerIndex)
	{
		return _playerDataNetworkList[playerIndex];
	}

	public Color GetPlayerColor(int colorId)
	{
		return _playerColorList[colorId];
	}

	public void ChangePlayerColor(int colorId)
	{
		ChangePlayerColorServerRpc(colorId);
	}
	
	public void KickPlayer(ulong clientId)
	{
		NetworkManager.Singleton.DisconnectClient(clientId);
		NetworkManager_Server_OnClientDisconnectCallback(clientId);
	}

	private void PlayerDataNetworkList_OnListChanged(NetworkListEvent<PlayerData> changeEvent)
	{
		OnPlayerDataNetworkListChanged?.Invoke(this, EventArgs.Empty);
	}

	private void NetworkManager_Server_OnClientDisconnectCallback(ulong clientId)
	{
		for (int i = 0; i < _playerDataNetworkList.Count; i++)
		{
			PlayerData playerData = _playerDataNetworkList[i];

			if (playerData.ClientId == clientId)
			{
				_playerDataNetworkList.RemoveAt(i);
			}
		}
	}

	private void NetworkManager_OnClientConnectedCallback(ulong clientId)
	{
		_playerDataNetworkList.Add(new PlayerData
		{
			ClientId = clientId,
			ColorId = GetFirstUnusedColorId(),
		});
		
		SetPlayerNameServerRpc(GetPlayerName());
		SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
	}

	private void NetworkManager_ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest connectionApprovalRequest, NetworkManager.ConnectionApprovalResponse connectionApprovalResponse)
	{
		if (SceneManager.GetActiveScene().name != Loader.Scene.CharacterSelectScene.ToString())
		{
			connectionApprovalResponse.Approved = false;
			connectionApprovalResponse.Reason = "Game has already started";
			return;
		}

		if (NetworkManager.Singleton.ConnectedClientsIds.Count >= MAX_PLAYER_AMOUNT)
		{
			connectionApprovalResponse.Approved = false;
			connectionApprovalResponse.Reason = "Game is full";
			return;
		}
		
		connectionApprovalResponse.Approved = true;
	}

	private void NetworkManager_Client_OnClientConnectedCallback(ulong clientId)
	{
		SetPlayerNameServerRpc(GetPlayerName());
		SetPlayerIdServerRpc(AuthenticationService.Instance.PlayerId);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SetPlayerNameServerRpc(string playerName, ServerRpcParams serverRpcParams = default)
	{
		int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

		PlayerData playerData = _playerDataNetworkList[playerDataIndex];

		playerData.PlayerName = playerName;

		_playerDataNetworkList[playerDataIndex] = playerData;
	}
	
	[ServerRpc(RequireOwnership = false)]
	private void SetPlayerIdServerRpc(string playerId, ServerRpcParams serverRpcParams = default)
	{
		int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

		PlayerData playerData = _playerDataNetworkList[playerDataIndex];

		playerData.PlayerId = playerId;

		_playerDataNetworkList[playerDataIndex] = playerData;
	}

	private void NetworkManager_Client_OnClientDisconnectCallback(ulong clientId)
	{
		OnFailedToJoinGame?.Invoke(this, EventArgs.Empty);
	}

	[ServerRpc(RequireOwnership = false)]
	private void SpawnKitchenObjectServerRpc(int kitchenObjectSOIndex, NetworkObjectReference kitchenObjectParentNetworkObjectReference)
	{
		KitchenObjectSO kitchenObjectSO = GetKitchenObjectSOFromIndex(kitchenObjectSOIndex);
		
		kitchenObjectParentNetworkObjectReference.TryGet(out NetworkObject kitchenObjectParentNetworkObject);
		IKitchenObjectParent kitchenObjectParent = kitchenObjectParentNetworkObject.GetComponent<IKitchenObjectParent>();
		
		if (kitchenObjectParent.HasKitchenObject())
			return;
		
		Transform kitchenObjectTransform = Instantiate(kitchenObjectSO.Prefab);

		NetworkObject kitchenObjectNetworkObject = kitchenObjectTransform.GetComponent<NetworkObject>();
		kitchenObjectNetworkObject.Spawn(true);
        
		KitchenObject kitchenObject = kitchenObjectTransform.GetComponent<KitchenObject>();
		
		kitchenObject.SetKitchenObjectParent(kitchenObjectParent);
	}

	[ServerRpc(RequireOwnership = false)]
	private void DestroyKitchenObjectServerRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
	{
		kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);

		if (kitchenObjectNetworkObject == null)
			return;
            
		KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();
		
		ClearKitchenObjectOnParentClientRpc(kitchenObjectNetworkObjectReference);
		
		kitchenObject.DestroySelf();
	}

	[ClientRpc]
	private void ClearKitchenObjectOnParentClientRpc(NetworkObjectReference kitchenObjectNetworkObjectReference)
	{
		kitchenObjectNetworkObjectReference.TryGet(out NetworkObject kitchenObjectNetworkObject);
		KitchenObject kitchenObject = kitchenObjectNetworkObject.GetComponent<KitchenObject>();
		
		kitchenObject.ClearKitchenObjectOnParent();
	}

	[ServerRpc(RequireOwnership = false)]
	private void ChangePlayerColorServerRpc(int colorId, ServerRpcParams serverRpcParams = default)
	{
		if (!IsColorAvailable(colorId))
			return;

		int playerDataIndex = GetPlayerDataIndexFromClientId(serverRpcParams.Receive.SenderClientId);

		PlayerData playerData = _playerDataNetworkList[playerDataIndex];

		playerData.ColorId = colorId;

		_playerDataNetworkList[playerDataIndex] = playerData;
	}

	private bool IsColorAvailable(int colorId)
	{
		foreach (PlayerData playerData in _playerDataNetworkList)
		{
			if (playerData.ColorId == colorId)
				return false;
		}

		return true;
	}

	private int GetFirstUnusedColorId()
	{
		for (int i = 0; i < _playerColorList.Count; i++)
		{
			if (IsColorAvailable(i))
			{
				return i;
			}
		}

		return -1;
	}
}