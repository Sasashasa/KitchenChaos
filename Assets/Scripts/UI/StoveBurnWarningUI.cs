using UnityEngine;

public class StoveBurnWarningUI : MonoBehaviour
{
    [SerializeField] private StoveCounter _stoveCounter;

    private void Start()
    {
        _stoveCounter.OnProgressChanged += StoveCounter_OnProgressChanged;
        
        Hide();
    }

    private void StoveCounter_OnProgressChanged(object sender, IHasProgress.OnProgressChangedEventArgs e)
    {
        float burnShowProgressAmount = .5f;
        bool show = _stoveCounter.IsFried() && e.ProgressNormalized >= burnShowProgressAmount;

        if (show)
        {
            Show();
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