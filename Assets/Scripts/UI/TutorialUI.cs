using System;
using TMPro;
using UnityEngine;

public class TutorialUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI _keyMoveUpText;
    [SerializeField] private TextMeshProUGUI _keyMoveDownText;
    [SerializeField] private TextMeshProUGUI _keyMoveLeftText;
    [SerializeField] private TextMeshProUGUI _keyMoveRightText;
    [SerializeField] private TextMeshProUGUI _keyInteractText;
    [SerializeField] private TextMeshProUGUI _keyInteractAlternateText;
    [SerializeField] private TextMeshProUGUI _keyPauseText;
    [SerializeField] private TextMeshProUGUI _keyGamepadInteractText;
    [SerializeField] private TextMeshProUGUI _keyGamepadInteractAlternateText;
    [SerializeField] private TextMeshProUGUI _keyGamepadPauseText;

    private void Start()
    {
        GameInput.Instance.OnBindingRebind += GameInput_OnBindingRebind;
        KitchenGameManager.Instance.OnLocalPlayerReadyChanged += KitchenGameManger_OnLocalPlayerReadyChanged;
        
        UpdateVisual();
        Show();
    }

    private void KitchenGameManger_OnLocalPlayerReadyChanged(object sender, EventArgs e)
    {
        if (KitchenGameManager.Instance.IsLocalPlayerReady())
        {
            Hide();
        }
    }
    
    private void GameInput_OnBindingRebind(object sender, EventArgs e)
    {
        UpdateVisual();
    }

    private void UpdateVisual()
    {
        _keyMoveUpText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Up);
        _keyMoveDownText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Down);
        _keyMoveLeftText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Left);
        _keyMoveRightText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Move_Right);
        _keyInteractText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Interact);
        _keyInteractAlternateText.text = GameInput.Instance.GetBindingText(GameInput.Binding.InteractAlternate);
        _keyPauseText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Pause);
        _keyGamepadInteractText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Gamepad_Interact);
        _keyGamepadInteractAlternateText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Gamepad_InteractAlternate);
        _keyGamepadPauseText.text = GameInput.Instance.GetBindingText(GameInput.Binding.Gamepad_Pause);
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