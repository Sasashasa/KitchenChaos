using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class HostDisconnectUI : MonoBehaviour
{
	[SerializeField] private Button _playAgainButton;

	private void Awake()
	{
		_playAgainButton.onClick.AddListener(() =>
		{
			Loader.Load(Loader.Scene.MainMenuScene);
		});
	}

	private void Start()
	{
		NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
		
		Hide();
	}
	
	private void OnDestroy()
	{
		NetworkManager.Singleton.OnClientDisconnectCallback -= NetworkManager_OnClientDisconnectCallback;
	}

	private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
	{
		if (clientId == NetworkManager.ServerClientId)
		{
			Show();
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