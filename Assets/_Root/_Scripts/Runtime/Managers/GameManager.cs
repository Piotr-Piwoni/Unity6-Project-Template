using EditorAttributes;
using PROJECTNAME.Utilities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PROJECTNAME.Managers
{
public class GameManager : PersistentSingleton<GameManager>
{
	public GameState CurrentState => _CurrentState;

	[SerializeField, ReadOnly]
	private GameState _CurrentState = GameState.Playing;

	private GameState _PreviousState;


	private void Start()
	{
		_PreviousState = _CurrentState;
	}

	private void Update()
	{
		// Handle game functionality differently based on current state.
		switch (_CurrentState)
		{
			case GameState.MainMenu:
				//* Logic for when the game is in the Main Menu.
				break;
			case GameState.Playing:
				//* Logic for when the game is actually playing.
				break;
			case GameState.Talking:
				//* Logic for when talking occurs in the game.
				break;
			case GameState.Pause:
				//* Logic for when the game is Paused.
				break;
			case GameState.Menu:
				//* Logic for when the game is in a UI menu.
				break;
		}
	}

	public void ChangeState(GameState newState)
	{
		_PreviousState = _CurrentState;
		_CurrentState = newState;
	}


	public override void OnSceneChange(Scene scene, LoadSceneMode mode)
	{
		Debug.Log("Scene Change.");
	}

	public enum GameState
	{
		MainMenu = 0,
		Playing = 1,
		Talking = 2,
		Pause = 3,
		Menu = 4
	}
}
}