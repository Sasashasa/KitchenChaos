using System;
using Unity.Netcode;
using UnityEngine;

public class PlatesCounter : BaseCounter
{
    public event EventHandler OnPlateSpawned;
    public event EventHandler OnPlateRemoved;
    
    [SerializeField] private KitchenObjectSO _plateKitchenObjectSO;
    [SerializeField] private float _spawnPlateCooldown = 4f;
    [SerializeField] private int _platesSpawnedAmountMax = 4;
    
    private float _spawnPlateTimer;
    private int _platesSpawnedAmount;

    private void Update()
    {
        if (!IsServer)
            return;
        
        _spawnPlateTimer += Time.deltaTime;

        if (_spawnPlateTimer > _spawnPlateCooldown)
        {
            _spawnPlateTimer = 0f;

            if (KitchenGameManager.Instance.IsGamePlaying() && _platesSpawnedAmount < _platesSpawnedAmountMax)
            {
                SpawnPlateServerRpc();
            }
        }
    }

    public override void Interact(Player player)
    {
        if (!player.HasKitchenObject())
        {
            if (_platesSpawnedAmount > 0)
            {
                KitchenObject.SpawnKitchenObject(_plateKitchenObjectSO, player);
                
                InteractLogicServerRpc();
            }
        }
    }
    
    [ServerRpc]
    private void SpawnPlateServerRpc()
    {
        SpawnPlateClientRpc();
    }
    
    [ClientRpc]
    private void SpawnPlateClientRpc()
    {
        _platesSpawnedAmount++;
                
        OnPlateSpawned?.Invoke(this, EventArgs.Empty);
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void InteractLogicServerRpc()
    {
        InteractLogicClientRpc();
    }
    
    [ClientRpc]
    private void InteractLogicClientRpc()
    {
        _platesSpawnedAmount--;
        
        OnPlateRemoved?.Invoke(this, EventArgs.Empty);
    }
}