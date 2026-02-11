using System.Collections;
using UnityEngine;

public class GameModeOnly : MonoBehaviour
{
	[Tooltip("This object gets disabled unless the active game mode is set to the mode assigned here")]
	public GameMode gameMode;

	private void OnEnable()
	{
		StartCoroutine(DoGameModeCheck());
	}

	private IEnumerator DoGameModeCheck()
	{
		// Wait a frame to give a chance for the game manager to register
		yield return null;

		// If the game manager was found, wait for the game to get started
		if (Ref.TryGet(out GameManager gameManager))
		{
			while (gameManager.game == null)
				yield return null;

			// Once the game has started, check for a matching game mode
			if (gameManager.game.gameMode != null && gameManager.game.gameMode.gameModeName == gameMode.gameModeName)
				yield break;
		}
		
		// If a matching game mode was not found, disable this object
		gameObject.SetActive(false);
	}
}