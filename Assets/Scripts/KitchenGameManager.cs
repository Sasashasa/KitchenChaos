using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class KitchenGameManager : NetworkBehaviour
{
    public static KitchenGameManager Instance { get; private set; }

    public event EventHandler OnStateChanged;
    public event EventHandler OnLocalGamePaused;
    public event EventHandler OnLocalGameUnpaused;
    public event EventHandler OnMultiplayerGamePaused;
    public event EventHandler OnMultiplayerGameUnpaused;
    public event EventHandler OnLocalPlayerReadyChanged;
    
    private enum State
    {
        WaitingToStart,
        CountdownToStart,
        GamePlaying,
        GameOver,
    }

    [SerializeField] private Transform _playerPrefab;
    [SerializeField] private float _gamePlayingTimerMax = 90f;

    private NetworkVariable<State> _state = new NetworkVariable<State>(State.WaitingToStart);
    private bool _isLocalPlayerReady;
    private NetworkVariable<float> _countdownToStartTimer = new NetworkVariable<float>(3f);
    private NetworkVariable<float> _gamePlayingTimer = new NetworkVariable<float>(0f);
    private bool _isLocalGamePaused = false;
    private NetworkVariable<bool> _isGamePaused = new NetworkVariable<bool>(false);
    private Dictionary<ulong, bool> _playerReadyDictionary;
    private Dictionary<ulong, bool> _playerPausedDictionary;
    private bool _autoTestGamePausedState;

    private void Awake()
    {
        Instance = this;
        
        _playerReadyDictionary = new Dictionary<ulong, bool>();
        _playerPausedDictionary = new Dictionary<ulong, bool>();
    }

    private void Start()
    {
        GameInput.Instance.OnPauseAction += GameInput_OnPauseAction;
        GameInput.Instance.OnInteractAction += GameInput_OnInteractAction;
    }

    public override void OnNetworkSpawn()
    {
        _state.OnValueChanged += State_OnValueChanged;
        _isGamePaused.OnValueChanged += IsGamePaused_OnValueChanged;

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += SceneManager_OnLoadEventCompleted;
        }
    }
    
    private void Update()
    {
        if (!IsServer)
            return;
        
        switch (_state.Value)
        {
            case State.WaitingToStart:
                break;
            case State.CountdownToStart:
                _countdownToStartTimer.Value -= Time.deltaTime;
                
                if (_countdownToStartTimer.Value < 0)
                {
                    _state.Value = State.GamePlaying;
                    _gamePlayingTimer.Value = _gamePlayingTimerMax;
                }
                
                break;
            case State.GamePlaying:
                _gamePlayingTimer.Value -= Time.deltaTime;
                
                if (_gamePlayingTimer.Value < 0)
                {
                    _state.Value = State.GameOver;
                }
                
                break;
            case State.GameOver:
                break;
        }
    }

    private void LateUpdate()
    {
        if (_autoTestGamePausedState)
        {
            _autoTestGamePausedState = false;
            TestGamePausedState();
        }
    }
    
    public bool IsGamePlaying()
    {
        return _state.Value == State.GamePlaying;
    }

    public bool IsCountdownToStartActive()
    {
        return _state.Value == State.CountdownToStart;
    }

    public float GetCountdownToStartTimer()
    {
        return _countdownToStartTimer.Value;
    }

    public bool IsGameOver()
    {
        return _state.Value == State.GameOver;
    }

    public bool IsLocalPlayerReady()
    {
        return _isLocalPlayerReady;
    }

    public float GetGamePlayingTimerNormalized()
    {
        return 1 - _gamePlayingTimer.Value / _gamePlayingTimerMax;
    }
    
    public void TogglePauseGame()
    {
        _isLocalGamePaused = !_isLocalGamePaused;

        if (_isLocalGamePaused)
        {
            PauseGameServerRpc();
            
            OnLocalGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            UnpauseGameServerRpc();
            
            OnLocalGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }

    private void SceneManager_OnLoadEventCompleted(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            Transform playerTransform = Instantiate(_playerPrefab);
            playerTransform.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId, true);
        }
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong clientId)
    {
        _autoTestGamePausedState = true;
    }

    private void State_OnValueChanged(State previousValue, State newValue)
    {
        OnStateChanged?.Invoke(this, EventArgs.Empty);
    }

    private void IsGamePaused_OnValueChanged(bool previousValue, bool newValue)
    {
        if (_isGamePaused.Value)
        {
            Time.timeScale = 0f;
            OnMultiplayerGamePaused?.Invoke(this, EventArgs.Empty);
        }
        else
        {
            Time.timeScale = 1f;
            OnMultiplayerGameUnpaused?.Invoke(this, EventArgs.Empty);
        }
    }
    
    private void GameInput_OnPauseAction(object sender, EventArgs e)
    {
        TogglePauseGame();
    }
    
    private void GameInput_OnInteractAction(object sender, EventArgs e)
    {
        if (_state.Value == State.WaitingToStart)
        {
            _isLocalPlayerReady = true;
            
            OnLocalPlayerReadyChanged?.Invoke(this, EventArgs.Empty);
            
            SetPlayerReadyServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void SetPlayerReadyServerRpc(ServerRpcParams serverRpcParams = default)
    {
        _playerReadyDictionary[serverRpcParams.Receive.SenderClientId] = true;

        bool allClientsReady = true;
        
        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (!_playerReadyDictionary.ContainsKey(clientId) || !_playerReadyDictionary[clientId])
            {
                allClientsReady = false;
                break;
            }
        }

        if (allClientsReady)
        {
            _state.Value = State.CountdownToStart;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void PauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        _playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = true;
        
        TestGamePausedState();
    }
    
    [ServerRpc(RequireOwnership = false)]
    private void UnpauseGameServerRpc(ServerRpcParams serverRpcParams = default)
    {
        _playerPausedDictionary[serverRpcParams.Receive.SenderClientId] = false;
        
        TestGamePausedState();
    }

    private void TestGamePausedState()
    {
        foreach (ulong clientID in NetworkManager.Singleton.ConnectedClientsIds)
        {
            if (_playerPausedDictionary.ContainsKey(clientID) && _playerPausedDictionary[clientID])
            {
                _isGamePaused.Value = true;
                return;
            }
        }
        
        _isGamePaused.Value = false;
    }
}