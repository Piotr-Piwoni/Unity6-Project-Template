using EditorAttributes;
using PROJECTNAME.Systems;
using PROJECTNAME.Utilities;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PROJECTNAME.Managers
{
public class GameManager : PersistentSingleton<GameManager>
{
	public Camera Camera => _Camera;
	public CinemachineCamera CinemachineCam => _CinemachineCam;
	public GameObject Player => _Player;
	public GameState CurrentState => _CurrentState;

	[SerializeField, ReadOnly]
	private Camera _Camera;
	[SerializeField, ReadOnly]
	private CinemachineCamera _CinemachineCam;
	[Header("Game Values"), SerializeField, ReadOnly, PropertyOrder(-1)]
	private GameState _CurrentState = GameState.Playing;

	[Header("Settings"), SerializeField, PropertyOrder(-12)]
	private GameObject _PlayerPrefab;
	[SerializeField, ReadOnly]
	private GameObject _Player;
	[SerializeField, PropertyOrder(-12)]
	private AudioClip _MusicClip;

	private GameState _PreviousState;


	protected override void Awake()
	{
		base.Awake();
		_PreviousState = _CurrentState;

		// Get the Player if it already exists, otherwise create one if possible.
		_Player = GameObject.FindGameObjectWithTag("Player");
		if (!_Player && _PlayerPrefab)
			_Player = Instantiate(_PlayerPrefab);

		GetCamera();
	}

	private void Start()
	{
		// If a music clip is provided, play the music.
		if (_MusicClip)
			AudioSystem.Instance.PlayMusic(_MusicClip);
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
	}

	// Try to get the camera in the scene.
	private void GetCamera()
	{
		// Locate the main camera.
		var cameraObjs = FindObjectsByType<Camera>(FindObjectsInactive.Include,
			FindObjectsSortMode.None);
		foreach (Camera camObj in cameraObjs)
			if (camObj.CompareTag("MainCamera"))
				_Camera = camObj;

		if (!_Camera)
		{
			Debug.LogError("No Camera found on the scene!");
			return;
		}

		// Try to get the CinemachineCamera from the Camera's parent.
		_CinemachineCam = _Camera.GetComponentInParent<CinemachineCamera>(true);
		if (!_CinemachineCam)
		{
			Debug.LogWarning("A Cinemachine Camera was not found in the " +
			                 "scene or is not the parent object of the Camera.");
		}

		// If there's a Player, move the cameras to the Player, otherwise move
		// them to the Game Manager.
		if (_CinemachineCam)
		{
			if (_CinemachineCam.transform.parent == _Player?.transform) return;
			_CinemachineCam.transform.SetParent(_Player
				? _Player.transform
				: transform);
			return;
		}

		if (!_Camera) return;
		if (_Camera.transform.parent == _Player?.transform) return;
		_Camera.transform.SetParent(_Player ? _Player.transform : transform);
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