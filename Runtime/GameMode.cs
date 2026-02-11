using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Localization;

public abstract class GameMode : ScriptableObject, IState<Game>
{
	[Header("Game Mode Settings")]
	public string gameModeName = "New Game Mode";

	[Tooltip("Localized string for displaying the game mode name")]
	public LocalizedString gameModeDisplayName;

	[Space(10)]
	public bool playPlayerDeathSequence = true;

	[Header("Game Mode Events")]

	[Tooltip("Event invoked when the Game Mode is first created. This is called only once, before the Game Mode is entered.")]
	public UnityEvent onCreate;
	[Tooltip("Event invoked when the Game Mode has Started. This may be called multiple times, as the Game Mode is Reset.")]
	public UnityEvent onEnter;
	[Tooltip("Event invoked as the Game Mode is ticked forward.")]
	public UnityEvent onExecute;
	[Tooltip("Event invoked when the Game Mode has Ended.")]
	public UnityEvent onExit;
	[Tooltip("Event invoked when the Game Mode is Reset.")]
	public UnityEvent onReset;

	protected Game game;

	public virtual void Create(Game owner)
	{
		game = owner;

		onCreate?.Invoke();
	}

	public virtual void Enter(Game owner)
	{
		onEnter?.Invoke();
	}

	public virtual void Execute(Game owner) => onExecute?.Invoke();
	public virtual void Exit(Game owner)
	{
		onExit?.Invoke();
	}

	public virtual void ResetGame(Game owner)
	{
		onReset?.Invoke();
	}
}
