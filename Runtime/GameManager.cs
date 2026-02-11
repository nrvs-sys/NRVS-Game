using FishNet.Connection;
using FishNet.Managing.Scened;
using FishNet.Object;
using FishNet.Transporting;
using NaughtyAttributes;
using Network;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

/// <summary>
/// Manages the Game Instance's Lifecyle. This also acts as the main entry point for the Game, kicking it off when the Server starts.
/// </summary>
public class GameManager : NetworkBehaviour
{
    #region Serialized Fields

    [Header("Settings")]

	[SerializeField, FormerlySerializedAs("gameMode"), Tooltip("The game mode to run for the game. This gets instantiated when a new game is created."), Expandable]
	private GameMode startingGameMode;

	[Header("Events")]

	public UnityEvent onInitialized;

	public UnityEvent<Game> onGameCreated;
	public UnityEvent<Game> onGameDestroyed;

	[Space(10)]

    public UnityEvent<Game> onGameReady;
    public UnityEvent<Game> onGameBegin;
    public UnityEvent<Game> onGameEnd;
    public UnityEvent<Game> onGameComplete;
    public UnityEvent<Game> onGameReset;

	[Header("Server Events")]

	[Tooltip("Invokes once all Clients have confirmed the Ready Game State. Only called on the Server.")]
	public UnityEvent<Game> OnClientsConfirmedReady;

    #endregion

    public Game game { get; private set; }

	bool initialized = false;

	Dictionary<NetworkConnection, Game.GameState> clientGameStates = new();

    #region Unity Methods

    void Awake()
	{
		Ref.Register<GameManager>(this);
	}

    void OnDestroy()
	{
		Ref.Unregister<GameManager>(this);
	}

    #endregion

    #region NetworkBehavior Methods

    public override void OnStartNetwork()
    {
        base.OnStartNetwork();

        TimeManager.OnTick += TimeManager_OnTick;
    }

    public override void OnStopNetwork()
	{
		base.OnStopNetwork();

		TimeManager.OnTick -= TimeManager_OnTick;

		game?.Exit(this);
	}

	public override void OnStartServer()
	{
        base.OnStartServer();

		foreach (var conn in ServerManager.Clients)
			clientGameStates.Add(conn.Value, Game.GameState.None);

        ServerManager.OnRemoteConnectionState += ServerManager_OnRemoteConnectionState;

        if (Ref.TryGet<NetworkSceneManager>(out var networkSceneManager))
            networkSceneManager.OnSceneVisibleForAllClients.AddListener(NetworkSceneManager_OnAllClientsSceneVisible);
    }

	public override void OnStopServer()
	{
        base.OnStopServer();

        ServerManager.OnRemoteConnectionState -= ServerManager_OnRemoteConnectionState;

        if (Ref.TryGet<NetworkSceneManager>(out var networkSceneManager))
            networkSceneManager.OnSceneVisibleForAllClients.RemoveListener(NetworkSceneManager_OnAllClientsSceneVisible);
    }

    #endregion

	public void Reboot(GameMode gameMode)
	{
		if (!initialized)
		{
			Debug.LogError("Game: Cannot Reboot the Game because the GameManager is not Initialized");
			return;
		}

        Debug.Log("Game: Rebooting Game");

        if (game != null)
        {
            game.EndGame();
            //game.ResetGame();

			onGameDestroyed?.Invoke(game);
        }

        game = CreateGame(gameMode);

		onGameCreated?.Invoke(game);

        game.Enter(this);
    }

    /// <summary>
    /// Ends any active Game, then creates a new Game and starts it.
    /// 
    /// Note that this is different from calling `ResetGame()` on a Game, which simply resets it.
    /// </summary>
    public void Reboot()
	{
		var gameMode = startingGameMode;

        // if there is a game active, we'll use it's current game mode
        if (game != null)
			gameMode = game.gameMode;

        Reboot(gameMode);
    }

	Game CreateGame(GameMode gameMode)
	{
		void OnGameEnd(Game game)
		{
			onGameEnd?.Invoke(game);
			if (game.completed)
				onGameComplete?.Invoke(game);
        }

		return new Game(
			Instantiate(gameMode),
			(Game game) => onGameReady?.Invoke(game),
			(Game game) => onGameBegin?.Invoke(game),
			(Game game) => OnGameEnd(game),
			(Game game) => onGameReset?.Invoke(game),
			(Game.GameState state) => Game_OnGameStateChanged(state)
			);
	}

	#region RPC Methods

	[ObserversRpc(BufferLast = true, RunLocally = true, ExcludeServer = true)]
	void RpcInitialized(Channel channel = Channel.Reliable)
	{
		initialized = true;

        onInitialized?.Invoke();

        if (startingGameMode != null)
			Reboot();
	}

    /// <summary>
    /// Sends the current Server game state to all Clients. Clients will then update their game state to match the Server's.
    /// </summary>
    /// <param name="state"></param>
    [ObserversRpc(BufferLast = true, ExcludeServer = true)]
	void RpcSendGameState(Game.GameState state, Channel channel = Channel.Reliable)
	{
		Debug.Log($"Game: Received {state} GameState from server");

        switch (state)
        {
            case Game.GameState.Ready:
				switch (game.gameState)
				{
					case Game.GameState.Active:
						game.EndGame();
						game.ResetGame();
                        break;
					case Game.GameState.Stopped:
						game.ResetGame();
                        break;
				}
                break;
            case Game.GameState.Active:
				switch (game.gameState)
				{
					case Game.GameState.Ready:
						game.BeginGame();
                        break;
					case Game.GameState.Stopped:
						game.ResetGame();
                        game.BeginGame();
                        break;
				}
                break;
            case Game.GameState.Stopped:
				switch (game.gameState)
				{
					case Game.GameState.Active:
					case Game.GameState.Ready:
						game.EndGame();
						break;
				}
                break;
        }
    }

	/// <summary>
	/// Confirms that the Client has received the Server's game state.
	/// 
	/// If all Clients have confirmed the same game state, the Server will call `GameStateConfirmed()`.
	/// </summary>
	/// <param name="state"></param>
	/// <param name="conn"></param>
    [ServerRpc(RequireOwnership = false)]
    void RpcConfirmGameState(Game.GameState state, Channel channel = Channel.Reliable, NetworkConnection conn = null)
	{
		ConfirmGameState(conn, state);
    }

    #endregion

    #region Server Methods

	void ConfirmGameState(NetworkConnection conn, Game.GameState state)
    {
        Debug.Log($"Game: Client {conn.ClientId} confirmed {state} GameState");

        if (clientGameStates.ContainsKey(conn))
        {
            clientGameStates[conn] = state;
        }
        else
        {
            clientGameStates.Add(conn, state);
        }

        if (IsGameStateConfirmed(state))
            GameStateConfirmed(state);
    }

	/// <summary>
	/// Returns true if all Clients have confirmed the same game state.
	/// </summary>
	/// <param name="state"></param>
	/// <returns></returns>
    [Server]
	bool IsGameStateConfirmed(Game.GameState state)
	{
		//Debug.Log($"Client Game States Count: {clientGameStates.Count} | Client Count: {ServerManager.Clients.Count}");

        if (clientGameStates.Count != ServerManager.Clients.Count)
			return false;

		foreach (var client in clientGameStates)
		{
			if (client.Value != state)
				return false;
		}

        return true;
    }

	/// <summary>
	/// Called when all Clients have confirmed the same game state.
	/// </summary>
	/// <param name="state"></param>
	[Server]
	void GameStateConfirmed(Game.GameState state)
	{
        Debug.Log($"Game: All clients confirmed {state} GameState");

        switch (state)
		{
			case Game.GameState.Ready:
                OnClientsConfirmedReady?.Invoke(game);
                break;
		}
    }

	[Server]
	public void CloseGameScene()
	{
		var sceneUnloadData = new SceneUnloadData(gameObject.scene.name);
		SceneManager.UnloadGlobalScenes(sceneUnloadData);
    }

    #endregion

    #region Event Handlers

    void TimeManager_OnTick()
    {
        game?.Execute(this);
    }

    void Game_OnGameStateChanged(Game.GameState state)
	{
		if (IsServerInitialized)
		{
            RpcSendGameState(state);
        }
		if (IsClientInitialized)
		{
			Debug.Log($"Game: Local client confirming {state} GameState");
			RpcConfirmGameState(state);
		}
	}

    void ServerManager_OnRemoteConnectionState(NetworkConnection conn, FishNet.Transporting.RemoteConnectionStateArgs args)
    {
        switch (args.ConnectionState)
        {
            case RemoteConnectionState.Started:
				clientGameStates.Add(conn, Game.GameState.None);
				break;
            case RemoteConnectionState.Stopped:
                clientGameStates.Remove(conn);
                break;
        }
    }

    void NetworkSceneManager_OnAllClientsSceneVisible(Scene scene)
    {
		if (game == null && gameObject.scene == scene)
			StartCoroutine(WaitForHostClientInitialized());
    }

	IEnumerator WaitForHostClientInitialized()
    {
        while (IsHostStarted && !IsHostInitialized)
            yield return null;

		// We want to wait for both the Server and the Client to be initialized on the host before starting the game
		// This is so all the correct RPCs will be sent and received
        RpcInitialized();
    }

    #endregion
}

public class Game : IState<GameManager>
{
	public GameMode gameMode { get; private set; }
	public GameState gameState { get; private set; } = GameState.Ready;

	public bool completed { get; private set; }

	private Action<Game> OnGameReady;
	private Action<Game> OnGameBegin;
	private Action<Game> OnGameEnd;
	private Action<Game> OnGameReset;

    private Action<GameState> OnGameStateChanged;

    private Coroutine gameEndCoroutine;

	// TODO - add networked confirmations of game state changes. The Server will send out requests to change the game state, and the clients will confirm the change.
	public enum GameState
	{
		/// <summary>
		/// No game state set. Only used before the game is first created.
		/// </summary>
		None,
        /// <summary>
        /// Pre-game, inactive
        /// </summary>
        Ready,
        /// <summary>
        /// Game is running
        /// </summary>
        Active,
        /// <summary>
        /// Post-game, inactive
        /// </summary>
        Stopped,
	}

	private GameManager gameManager;

	public Game(GameMode gameMode, Action<Game> OnGameReady = null, Action<Game> OnGameBegin = null, Action<Game> OnGameEnd = null, Action<Game> OnGameReset = null, Action<GameState> OnGameStateChanged = null)
	{
		this.gameMode = gameMode;
		this.OnGameReady = OnGameReady;
		this.OnGameBegin = OnGameBegin;
		this.OnGameEnd = OnGameEnd;
		this.OnGameReset = OnGameReset;
		this.OnGameStateChanged = OnGameStateChanged;

		gameMode.Create(this);

		this.OnGameReady?.Invoke(this);
		this.OnGameStateChanged?.Invoke(gameState);
	}

	public void Enter(GameManager owner)
	{
		gameManager = owner;
    }

    public void Execute(GameManager owner)
	{
		if (gameState == GameState.Active)
			gameMode.Execute(this);
	}

	public void Exit(GameManager owner)
	{
		EndGame();

		if (gameEndCoroutine != null && gameManager != null)
		{
			gameManager.StopCoroutine(gameEndCoroutine);

			gameEndCoroutine = null;
		}
	}


	public void BeginGame()
	{
		if (gameState != GameState.Ready)
		{
			Debug.LogError($"Game: Could not begin the game because the game state was incorrect ({gameState})");

			return;
		}

        gameState = GameState.Active;

        OnGameStateChanged?.Invoke(gameState);

        OnGameBegin?.Invoke(this);

        gameMode.Enter(this);
    }

    /// <summary>
    /// Ends the game, marking it as an unsuccessful completion.
    /// </summary>
    /// <param name="playGameEndSequence"></param>
    public void EndGame(bool playGameEndSequence = true)
	{
		// If the game is in the ready state, we don't need to do anything
		if (gameState == GameState.Ready)
		{
            gameState = GameState.Stopped;
            OnGameStateChanged?.Invoke(gameState);
        }

		if (gameState != GameState.Active)
			return;

        gameMode.Exit(this);

		if (playGameEndSequence)
		{
			//if (completed)
			//	PlayGameComplete();
			//else
			//	PlayGameEnd();
		}

		gameState = GameState.Stopped;

        OnGameEnd?.Invoke(this);

        OnGameStateChanged?.Invoke(gameState);
    }

	/// <summary>
	/// Ends the game, marking it as a successful completion.
	/// </summary>
	public void CompleteGame()
	{
		completed = true;

		EndGame();
	}

	/// <summary>
	/// Resets the game, and sets it back to the Ready state.
	/// </summary>
	public void ResetGame()
	{
		EndGame();

		gameMode.ResetGame(this);

		completed = false;

		gameState = GameState.Ready;

        OnGameReset?.Invoke(this);

        OnGameStateChanged?.Invoke(gameState);
    }
}