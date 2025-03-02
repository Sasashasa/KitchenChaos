using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class LobbyMessageUI : MonoBehaviour
{
	[SerializeField] private TextMeshProUGUI _messageText;
	[SerializeField] private Button _closeButton;

	private void Awake()
	{
		_closeButton.onClick.AddListener(Hide);
	}

	private void Start()
	{
		KitchenGameMultiplayer.Instance.OnFailedToJoinGame += KitchenGameMultiplayer_OnFailedToJoinGame;
		KitchenGameLobby.Instance.OnCreateLobbyStarted += KitchenGameLobby_OnCreateLobbyStarted;
		KitchenGameLobby.Instance.OnCreateLobbyFailed += KitchenGameLobby_OnCreateLobbyFailed;
		KitchenGameLobby.Instance.OnJoinStarted += KitchenGameLobby_OnJoinStarted;
		KitchenGameLobby.Instance.OnJoinFailed += KitchenGameLobby_OnJoinFailed;
		KitchenGameLobby.Instance.OnQuickJoinFailed += KitchenGameLobby_OnQuickJoinFailed;
		
		Hide();
	}
	
	private void OnDestroy()
	{
		KitchenGameMultiplayer.Instance.OnFailedToJoinGame -= KitchenGameMultiplayer_OnFailedToJoinGame;
		KitchenGameLobby.Instance.OnCreateLobbyStarted -= KitchenGameLobby_OnCreateLobbyStarted;
		KitchenGameLobby.Instance.OnCreateLobbyFailed -= KitchenGameLobby_OnCreateLobbyFailed;
		KitchenGameLobby.Instance.OnJoinStarted -= KitchenGameLobby_OnJoinStarted;
		KitchenGameLobby.Instance.OnJoinFailed -= KitchenGameLobby_OnJoinFailed;
		KitchenGameLobby.Instance.OnQuickJoinFailed -= KitchenGameLobby_OnQuickJoinFailed;
	}

	private void KitchenGameLobby_OnQuickJoinFailed(object sender, EventArgs e)
	{
		ShowMessage("Could not find a Lobby to Quick Join!");
	}

	private void KitchenGameLobby_OnJoinFailed(object sender, EventArgs e)
	{
		ShowMessage("Failed to join Lobby!");
	}

	private void KitchenGameLobby_OnJoinStarted(object sender, EventArgs e)
	{
		ShowMessage("Joining Lobby...");
	}

	private void KitchenGameLobby_OnCreateLobbyFailed(object sender, EventArgs e)
	{
		ShowMessage("Failed to create Lobby!");
	}

	private void KitchenGameLobby_OnCreateLobbyStarted(object sender, EventArgs e)
	{
		ShowMessage("Creating Lobby...");
	}

	private void KitchenGameMultiplayer_OnFailedToJoinGame(object sender, EventArgs e)
	{
		if (NetworkManager.Singleton.DisconnectReason == "")
		{
			ShowMessage("Failed to connect");
		}
		else
		{
			ShowMessage(NetworkManager.Singleton.DisconnectReason);
		}
	}

	private void ShowMessage(string message)
	{
		Show();
		_messageText.text = message;
	}

	private void Show()
	{
		gameObject.SetActive(true);
	}

	private void Hide()
	{
		gameObject.SetActive(false);
	}
}