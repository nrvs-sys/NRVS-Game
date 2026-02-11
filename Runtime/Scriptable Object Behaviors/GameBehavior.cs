using System.Collections;
using System.Collections.Generic;
using UnityAtoms.BaseAtoms;
using UnityEngine;

[CreateAssetMenu(fileName = "Game_ New", menuName = "Behaviors/Game/Game")]
public class GameBehavior : ScriptableObject
{
	public void BeginGame() => Ref.Get<GameManager>()?.game?.BeginGame();
	public void EndGame(bool playEndGameSequence) => Ref.Get<GameManager>()?.game.EndGame(playEndGameSequence);
	public void CompleteGame() => Ref.Get<GameManager>()?.game?.CompleteGame();
	public void ResetGame() => Ref.Get<GameManager>()?.game?.ResetGame();
	public void RebootGame() => Ref.Get<GameManager>()?.Reboot();
	public void RebootGame(GameMode gameMode) => Ref.Get<GameManager>()?.Reboot(gameMode);
	public void RebootGame(GameModeVariable gameModeVariable) => Ref.Get<GameManager>()?.Reboot(gameModeVariable?.Value);
	public void CloseGameScene() => Ref.Get<GameManager>()?.CloseGameScene();
}