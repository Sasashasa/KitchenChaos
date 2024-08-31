using System;
using UnityEngine;
using UnityEngine.UI;

public class CharacterColorSelectSingleUI : MonoBehaviour
{
	[SerializeField] private int _colorId;
	[SerializeField] private Image _image;
	[SerializeField] private GameObject _selectedGameObject;

	private void Awake()
	{
		GetComponent<Button>().onClick.AddListener(() =>
		{
			KitchenGameMultiplayer.Instance.ChangePlayerColor(_colorId);
		});
	}

	private void Start()
	{
		KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged += KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
		
		_image.color = KitchenGameMultiplayer.Instance.GetPlayerColor(_colorId);
		
		UpdateIsSelected();
	}

	private void OnDestroy()
	{
		KitchenGameMultiplayer.Instance.OnPlayerDataNetworkListChanged -= KitchenGameMultiplayer_OnPlayerDataNetworkListChanged;
	}

	private void KitchenGameMultiplayer_OnPlayerDataNetworkListChanged(object sender, EventArgs e)
	{
		UpdateIsSelected();
	}

	private void UpdateIsSelected()
	{
		_selectedGameObject.SetActive(KitchenGameMultiplayer.Instance.GetPlayerData().ColorId == _colorId);
	}
}