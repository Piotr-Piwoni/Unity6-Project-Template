using System;
using System.Collections.Generic;
using EditorAttributes;
using PROJECTNAME.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using Void = EditorAttributes.Void;

namespace PROJECTNAME.Managers
{
public class InputManager : PersistentSingleton<InputManager>
{
	public event Action OnAttackPressed;
	public event Action OnInteractionPressed;
	public event Action OnJumpPressed;
	public event Action OnMovePressed;
	public event Action<DeviceType> OnDeviceChanged;

	public DeviceType CurrentDeviceType => _CurrentDeviceType;
	public Vector2 LookInput { get; private set; } = Vector2.zero;
	public Vector2 MoveInput { get; private set; } = Vector2.zero;

	[Header("Input Manager Values"), SerializeField, ReadOnly]
	private DeviceType _CurrentDeviceType = DeviceType.Unknown;

	[Header("Input References"), SerializeField, ReadOnly, HideProperty]
	private PlayerInput _PlayerInput;
	[SerializeField, HideProperty]
	private InputActionReference _MoveAction;
	[SerializeField, HideProperty]
	private InputActionReference _LookAction;
	[SerializeField, HideProperty]
	private InputActionReference _JumpAction;
	[SerializeField, HideProperty]
	private InputActionReference _AttackAction;
	[SerializeField, HideProperty]
	private InputActionReference _InteractionAction;

	[Header("Action Maps"), SerializeField, HideProperty]
	private string _GameplayActionMap = "Gameplay";
	[SerializeField, HideProperty]
	private string _UIActionMap = "UI";

	// Decorative Holders.
	[SerializeField, FoldoutGroup("Settings", true, nameof(_PlayerInput),
		 nameof(_MoveAction), nameof(_LookAction), nameof(_JumpAction),
		 nameof(_AttackAction), nameof(_InteractionAction),
		 nameof(_GameplayActionMap), nameof(_UIActionMap)), PropertyOrder(-1)]
	private Void _SettingsGroupHolder;


	private Dictionary<ActionMap, string> _ActionMapDictionary;


	protected override void Awake()
	{
		base.Awake();

		// Obtain the PlayerInput from the player object.
		_PlayerInput = GameManager.Instance.Player.GetComponent<PlayerInput>();
		if (!_PlayerInput)
		{
			Debug.LogError("PlayerInput not found on player object!");
			return;
		}

		InitializeActionMaps();
	}

	private void Start()
	{
		// Enable the Player once the Input Manager initialises.
		GameManager.Instance.Player.SetActive(true);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		BindInput();
		_PlayerInput.onControlsChanged += OnControlsChanged;
		UpdateCurrentDeviceType(_PlayerInput.currentControlScheme);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		UnbindInput();
		_PlayerInput.onControlsChanged -= OnControlsChanged;
	}

	public override void OnSceneChange(Scene scene, LoadSceneMode mode)
	{
	}

	/// <summary>
	///     Switch the currently in-use action map to a new one.
	/// </summary>
	/// <param name="actionMap">The map to switch to.</param>
	public void SwitchActionMap(ActionMap actionMap)
	{
		if (!_PlayerInput)
		{
			Debug.LogWarning("The Input Manager has no Player Input!");
			return;
		}

		if (_ActionMapDictionary.TryGetValue(actionMap, out var actionMapName))
			_PlayerInput.SwitchCurrentActionMap(actionMapName);
		else
			Debug.LogError($"No action map found for \"{actionMap}\"");
	}

	private void BindInput()
	{
		_MoveAction.action.performed += OnMovePerformed;
		_MoveAction.action.canceled += OnMoveCanceled;
		_LookAction.action.performed += OnLookPerformed;
		_LookAction.action.canceled += OnLookCanceled;


		_JumpAction.action.performed += ctx => OnJumpPressed?.Invoke();
		_AttackAction.action.performed += ctx => OnAttackPressed?.Invoke();
		_InteractionAction.action.performed +=
			ctx => OnInteractionPressed?.Invoke();

		EnableAllActions();
	}

	private void DisableAllActions()
	{
		_MoveAction.action.Disable();
		_JumpAction.action.Disable();
		_AttackAction.action.Disable();
		_InteractionAction.action.Disable();
	}

	private void EnableAllActions()
	{
		_MoveAction.action.Enable();
		_JumpAction.action.Enable();
		_AttackAction.action.Enable();
		_InteractionAction.action.Enable();
	}

	// Initialise a dictionary that links the ActionMap Enum with their string counterpart.
	private void InitializeActionMaps()
	{
		_ActionMapDictionary = new Dictionary<ActionMap, string>
		{
			{ ActionMap.Gameplay, _GameplayActionMap },
			{ ActionMap.UI, _UIActionMap }
		};
	}

	// Handles changing the control scheme.
	private void OnControlsChanged(PlayerInput input)
	{
		if (input.currentControlScheme == null) return;
		UpdateCurrentDeviceType(input.currentControlScheme);
	}

	private void OnLookCanceled(InputAction.CallbackContext context)
	{
		LookInput = Vector2.zero;
	}

	private void OnLookPerformed(InputAction.CallbackContext context)
	{
		LookInput = context.ReadValue<Vector2>();
	}

	private void OnMoveCanceled(InputAction.CallbackContext context)
	{
		MoveInput = Vector2.zero;
	}

	private void OnMovePerformed(InputAction.CallbackContext context)
	{
		OnMovePressed?.Invoke();
		MoveInput = context.ReadValue<Vector2>();
	}

	private void UnbindInput()
	{
		_MoveAction.action.performed -= OnMovePerformed;
		_MoveAction.action.canceled -= OnMoveCanceled;
		_LookAction.action.performed -= OnLookPerformed;
		_LookAction.action.canceled -= OnLookCanceled;

		_MoveAction.action.performed -= ctx => OnMovePressed?.Invoke();
		_JumpAction.action.performed -= ctx => OnJumpPressed?.Invoke();
		_AttackAction.action.performed -= ctx => OnAttackPressed?.Invoke();
		_InteractionAction.action.performed -=
			ctx => OnInteractionPressed?.Invoke();

		DisableAllActions();
	}

	// And internal function used to convert the
	// "PlayerInput.currentControlScheme" from string to DeviceType.
	private void UpdateCurrentDeviceType(string controlScheme)
	{
		DeviceType newDevice = controlScheme switch
		{
			"Keyboard&Mouse" => DeviceType.KeyboardMouse,
			"Gamepad" => DeviceType.Gamepad,
			_ => DeviceType.Unknown
		};

		if (newDevice == _CurrentDeviceType) return;
		_CurrentDeviceType = newDevice;
		Debug.Log($"Device Changed: <color=red>{_CurrentDeviceType}</color>");
		OnDeviceChanged?.Invoke(_CurrentDeviceType);
	}
}

public enum ActionMap
{
	Gameplay = 0,
	UI = 1
}

public enum DeviceType
{
	Unknown = -1,
	KeyboardMouse = 0,
	Gamepad = 1
}
}