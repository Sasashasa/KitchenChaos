using System;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class DeliveryManager : NetworkBehaviour
{
    public event EventHandler OnRecipeSpawned;
    public event EventHandler OnRecipeCompleted;
    public event EventHandler OnRecipeSuccess;
    public event EventHandler OnRecipeFailed;
    
    public static DeliveryManager Instance { get; private set; }
    
    [SerializeField] private RecipeListSO _recipeListSO;
    [SerializeField] private float _spawnRecipeTimerCooldown = 4f;
    [SerializeField] private int _waitingRecipesMax = 4;
    
    private List<RecipeSO> _waitingRecipeSOList;
    private float _spawnRecipeTimer = 4f;
    private int _successfulRecipesAmount;

    private void Awake()
    {
        Instance = this;
        _waitingRecipeSOList = new List<RecipeSO>();
    }

    private void Update()
    {
        if (!IsServer)
            return;
        
        _spawnRecipeTimer -= Time.deltaTime;

        if (_spawnRecipeTimer <= 0f)
        {
            _spawnRecipeTimer = _spawnRecipeTimerCooldown;

            if (KitchenGameManager.Instance.IsGamePlaying() && _waitingRecipeSOList.Count < _waitingRecipesMax)
            {
                int waitingRecipeSOIndex = UnityEngine.Random.Range(0, _recipeListSO.RecipeSOList.Count);
                
                SpawnNewWaitingRecipeClientRpc(waitingRecipeSOIndex);
            }
        }
    }
    
    public void DeliverRecipe(PlateKitchenObject plateKitchenObject)
    {
        for (int i = 0; i < _waitingRecipeSOList.Count; i++)
        {
            RecipeSO waitingRecipeSO = _waitingRecipeSOList[i];

            if (waitingRecipeSO.KitchenObjectSOList.Count == plateKitchenObject.GetKitchenObjectSOList().Count)
            {
                bool plateContentsMatchesRecipe = true;
                
                foreach (KitchenObjectSO recipeKitchenObjectSO in waitingRecipeSO.KitchenObjectSOList)
                {
                    bool ingredientFound = false;
                    
                    foreach (KitchenObjectSO plateKitchenObjectSO in plateKitchenObject.GetKitchenObjectSOList())
                    {
                        if (plateKitchenObjectSO == recipeKitchenObjectSO)
                        {
                            ingredientFound = true;
                            break;
                        }
                    }

                    if (!ingredientFound)
                    {
                        plateContentsMatchesRecipe = false;
                    }
                }

                if (plateContentsMatchesRecipe)
                {
                    DeliverCorrectRecipeServerRpc(i);
                    return;
                }
            }
        }
        
        DeliverIncorrectRecipeServerRpc();
    }
    
    public List<RecipeSO> GetWaitingRecipeSOList()
    {
        return _waitingRecipeSOList;
    }

    public int GetSuccessfulRecipesAmount()
    {
        return _successfulRecipesAmount;
    }

    [ClientRpc]
    private void SpawnNewWaitingRecipeClientRpc(int waitingRecipeSOIndex)
    {
        RecipeSO waitingRecipeSO = _recipeListSO.RecipeSOList[waitingRecipeSOIndex];
        
        _waitingRecipeSOList.Add(waitingRecipeSO);
        
        OnRecipeSpawned?.Invoke(this, EventArgs.Empty);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void DeliverIncorrectRecipeServerRpc()
    {
        DeliverIncorrectRecipeClientRpc();
    }

    [ClientRpc]
    private void DeliverIncorrectRecipeClientRpc()
    {
        OnRecipeFailed?.Invoke(this, EventArgs.Empty);
    }

    [ServerRpc(RequireOwnership = false)]
    private void DeliverCorrectRecipeServerRpc(int waitingRecipeSOListIndex)
    {
        DeliverCorrectRecipeClientRpc(waitingRecipeSOListIndex);
    }

    [ClientRpc]
    private void DeliverCorrectRecipeClientRpc(int waitingRecipeSOListIndex)
    {
        _successfulRecipesAmount++;
                    
        _waitingRecipeSOList.RemoveAt(waitingRecipeSOListIndex);
                    
        OnRecipeCompleted?.Invoke(this, EventArgs.Empty);
        OnRecipeSuccess?.Invoke(this, EventArgs.Empty);
    }
}