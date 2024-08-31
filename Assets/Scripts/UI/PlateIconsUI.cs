using UnityEngine;

public class PlateIconsUI : MonoBehaviour
{
    [SerializeField] private PlateKitchenObject _plateKitchenObject;
    [SerializeField] private Transform _iconTemplate;

    private void Awake()
    {
        _iconTemplate.gameObject.SetActive(false);
    }
    
    private void Start()
    {
        _plateKitchenObject.OnIngredientAdded += UpdateVisual;
    }

    private void UpdateVisual(object sender, PlateKitchenObject.OnIngredientAddedEventArgs e)
    {
        foreach (Transform child in transform)
        {
            if (child == _iconTemplate) continue;
            Destroy(child.gameObject);
        }
        
        foreach (KitchenObjectSO kitchenObjectSO in _plateKitchenObject.GetKitchenObjectSOList())
        {
            Transform iconTransform = Instantiate(_iconTemplate, transform);
            iconTransform.gameObject.SetActive(true);
            iconTransform.GetComponent<PlateIconsSingleUI>().SetKitchenObjectSO(kitchenObjectSO);
        }
    }
}