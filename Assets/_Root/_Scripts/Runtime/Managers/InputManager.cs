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
	public event Action<string> OnDeviceChanged;

	public string CurrentControlScheme { get; private set; }
	public Vector2 MoveInput { get; private set; } = Vector2.zero;

	[Header("Input References"), SerializeField, ReadOnly, HideProperty]
	private PlayerInput _PlayerInput;
	[SerializeField, HideProperty]
	private InputActionReference _MoveAction;
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
		 nameof(_MoveAction), nameof(_JumpAction), nameof(_AttackAction),
		 nameof(_InteractionAction), nameof(_GameplayActionMap),
		 nameof(_UIActionMap)), PropertyOrder(-1)]
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

	public override void OnEnable()
	{
		base.OnEnable();
		BindInput();
		_PlayerInput.onControlsChanged += OnControlsChanged;
		CurrentControlScheme = _PlayerInput.currentControlScheme;
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
			_PlayerInput.SwitchCurrentControlScheme(actionMapName);
		else
			Debug.LogError($"No action map found for {actionMap}");
	}

	private void BindInput()
	{
		_MoveAction.action.performed += OnMovePerformed;
		_MoveAction.action.canceled += OnMoveCanceled;

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
		if (input.currentControlScheme == CurrentControlScheme) return;

		// Update the currently used control scheme.
		CurrentControlScheme = input.currentControlScheme;
		Debug.Log($"Control Scheme Changed: {CurrentControlScheme}");

		// Based on name, get the correct scheme name.
		var deviceName = CurrentControlScheme switch
		{
			"Keyboard&Mouse" => "KeyboardMouse",
			"Gamepad" => "Gamepad",
			_ => "Unknown"
		};

		OnDeviceChanged?.Invoke(deviceName);
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

		_MoveAction.action.performed -= ctx => OnMovePressed?.Invoke();
		_JumpAction.action.performed -= ctx => OnJumpPressed?.Invoke();
		_AttackAction.action.performed -= ctx => OnAttackPressed?.Invoke();
		_InteractionAction.action.performed -=
			ctx => OnInteractionPressed?.Invoke();

		DisableAllActions();
	}
}

public enum ActionMap
{
	Gameplay = 0,
	UI = 1
}
}