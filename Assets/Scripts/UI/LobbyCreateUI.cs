using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyCreateUI : MonoBehaviour
{
	[SerializeField] private Button _closeButton;
	[SerializeField] private Button _createPublicButton;
	[SerializeField] private Button _createPrivateButton;
	[SerializeField] private TMP_InputField _lobbyNameInputField;

	private void Awake()
	{
		_closeButton.onClick.AddListener(Hide);
		
		_createPublicButton.onClick.AddListener(() =>
		{
			KitchenGameLobby.Instance.CreateLobby(_lobbyNameInputField.text, false);
		});
		
		_createPrivateButton.onClick.AddListener(() =>
		{
			KitchenGameLobby.Instance.CreateLobby(_lobbyNameInputField.text, true);
		});
	}

	private void Start()
	{
		Hide();
	}

	public void Show()
	{
		gameObject.SetActive(true);
		
		_createPublicButton.Select();
	}

	private void Hide()
	{
		gameObject.SetActive(false);
	}
}