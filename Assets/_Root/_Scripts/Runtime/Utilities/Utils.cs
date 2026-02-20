using PROJECTNAME.Utilities.Types;
using UnityEngine;

namespace PROJECTNAME.Utilities
{
public static class Utils
{
	public static UIMode StateToUIMode(GameState state)
	{
		switch (state)
		{
		case GameState.MainMenu: return UIMode.Menu;
		case GameState.Gameplay: return UIMode.Gameplay;
		case GameState.Dialogue: return UIMode.Dialogue;
		case GameState.Paused: return UIMode.Menu;
		case GameState.Menu: return UIMode.Menu;
		default:
			Debug.LogWarning($"No UI mode mapping defined for {state}, " +
							 $"defaulting to {nameof(UIMode.Gameplay)}.");
			return UIMode.Gameplay;
		}
	}
}
}
