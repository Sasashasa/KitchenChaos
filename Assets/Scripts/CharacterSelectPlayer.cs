using System;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectPlayer : MonoBehaviour
{
	[SerializeField] private int _playerIndex;
	[SerializeField] private GameObject _readyGameObject;
	[SerializeField] private PlayerVisual _playerVisual;
	[SerializeField] private Button _kickButton;
	[SerializeField] private TextMeshPro _playerNameText;

	private void Awake()
	{
		_kickButton.onClick.AddListener(() =>
		{
			PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(_playerIndex);
			KitchenGameLobby.Instance.KickPlayer(playerData.PlayerId.ToString());
			KitchenGameMultiplayer.Instance.KickPlayer(playerData.ClientId);
		});
	}

	private void Start()
	{
		KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged += KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
		CharacterSelectReady.Instance.OnReadyChanged += CharacterSelectReady_OnReadyChanged;
		
		_kickButton.gameObject.SetActive(NetworkManager.Singleton.IsServer);
		
		UpdatePlayer();
	}

	private void OnDestroy()
	{
		KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
	}

	private void CharacterSelectReady_OnReadyChanged(object sender, EventArgs e)
	{
		UpdatePlayer();
	}

	private void KitchenGameMultiplayer_OnPlayerDataNetworkListChanged(object sender, EventArgs e)
	{
		UpdatePlayer();
	}

	private void UpdatePlayer()
	{
		if (KitchenGameMultiplayer.Instance.IsPlayerIndexConnected(_playerIndex))
		{
			Show();

			PlayerData playerData = KitchenGameMultiplayer.Instance.GetPlayerDataFromPlayerIndex(_playerIndex);
			
			_readyGameObject.SetActive(CharacterSelectReady.Instance.IsPlayerReady(playerData.ClientId));

			_playerNameText.text = playerData.PlayerName.ToString();
			
			_playerVisual.SetPlayerColor(KitchenGameMultiplayer.Instance.GetPlayerColor(playerData.ColorId));
		}
		else
		{
			Hide();
		}
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