using System;
using System.Collections.Generic;
using PROJECTNAME.Utilities;
using PROJECTNAME.Utilities.Types;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.InputSystem;
using DeviceType = PROJECTNAME.Utilities.Types.DeviceType;

namespace PROJECTNAME.Managers
{
/// <summary>
///     Centralized manager responsible for handling all player input.
/// </summary>
/// <remarks>
///     This class is intentionally decoupled from gameplay logic and only exposes
///     input state and events. Consumers decide how to interpret the input.
/// </remarks>
[HideMonoScript]
public class InputManager : PersistentSingleton<InputManager>
{
	public event Action OnAttackPressed;
	public event Action OnInteractionPressed;
	public event Action OnJumpPressed;
	public event Action OnMovePressed;
	public event Action<DeviceType> OnDeviceChanged;
	public bool IsSprinting { get; private set; }

	[ShowInInspector, ReadOnly,]
	public DeviceType CurrentDeviceType { get; private set; } = DeviceType.Unknown;
	public Vector2 LookInput { get; private set; } = Vector2.zero;
	public Vector2 MoveInput { get; private set; } = Vector2.zero;

	[SerializeField, ReadOnly,]
	private PlayerInput _PlayerInput;
	[SerializeField]
	private InputActionReference _MoveAction;
	[SerializeField]
	private InputActionReference _LookAction;
	[SerializeField]
	private InputActionReference _JumpAction;
	[SerializeField]
	private InputActionReference _AttackAction;
	[SerializeField]
	private InputActionReference _InteractionAction;
	[SerializeField]
	private InputActionReference _SprintAction;
	[SerializeField]
	private string _GameplayActionMap = "Gameplay";
	[SerializeField]
	private string _UIActionMap = "UI";

	private Action<InputAction.CallbackContext> _attackCallback;
	private Action<InputAction.CallbackContext> _interactionCallback;
	private Action<InputAction.CallbackContext> _jumpCallback;
	private Dictionary<ActionMap, string> _ActionMapDictionary;


	protected override void Awake()
	{
		base.Awake();
		InitializeActionMaps();
	}


	public override void OnEnable()
	{
		base.OnEnable();
		BindInput();

		if (!_PlayerInput) return;
		_PlayerInput.onControlsChanged += OnControlsChanged;
		UpdateCurrentDeviceType(_PlayerInput.currentControlScheme);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		UnbindInput();

		if (_PlayerInput)
			_PlayerInput.onControlsChanged -= OnControlsChanged;
	}

	/// <summary>
	///     Assigns the PlayerInput instance used by the InputManager.
	/// </summary>
	/// <param name="input">The PlayerInput instance to bind.</param>
	public void SetPlayerInput(PlayerInput input)
	{
		if (!input)
		{
			Debug.LogWarning("The provied PlayerInput was NULL!");
			return;
		}

		// Unbind events if there was an existing PlayerInput.
		if (_PlayerInput)
			_PlayerInput.onControlsChanged -= OnControlsChanged;

		_PlayerInput = input;
		_PlayerInput.onControlsChanged += OnControlsChanged;
		UpdateCurrentDeviceType(_PlayerInput.currentControlScheme);
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

		if (_ActionMapDictionary.TryGetValue(actionMap, out string actionMapName))
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
		_SprintAction.action.performed += OnSprintPerformed;
		_SprintAction.action.canceled += OnSprintCanceled;


		_jumpCallback = _ => OnJumpPressed?.Invoke();
		_JumpAction.action.performed += _jumpCallback;
		_attackCallback = _ => OnAttackPressed?.Invoke();
		_AttackAction.action.performed += _attackCallback;
		_interactionCallback = _ => OnInteractionPressed?.Invoke();
		_InteractionAction.action.performed += _interactionCallback;

		EnableAllActions();
	}

	private void DisableAllActions()
	{
		_MoveAction.action.Disable();
		_JumpAction.action.Disable();
		_AttackAction.action.Disable();
		_InteractionAction.action.Disable();
		_SprintAction.action.Disable();
	}

	private void EnableAllActions()
	{
		_MoveAction.action.Enable();
		_JumpAction.action.Enable();
		_AttackAction.action.Enable();
		_InteractionAction.action.Enable();
		_SprintAction.action.Enable();
	}

	/// <summary>
	///     Initializes the action map lookup dictionary.
	/// </summary>
	private void InitializeActionMaps()
	{
		_ActionMapDictionary = new Dictionary<ActionMap, string>
		{
				{ ActionMap.Gameplay, _GameplayActionMap },
				{ ActionMap.UI, _UIActionMap },
		};
	}

	/// <summary>
	///     Handles control scheme changes from the PlayerInput component.
	/// </summary>
	/// <param name="input">The PlayerInput instance reporting the change.</param>
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

	private void OnSprintCanceled(InputAction.CallbackContext context)
	{
		IsSprinting = false;
	}

	private void OnSprintPerformed(InputAction.CallbackContext context)
	{
		IsSprinting = true;
	}

	private void UnbindInput()
	{
		_MoveAction.action.performed -= OnMovePerformed;
		_MoveAction.action.canceled -= OnMoveCanceled;
		_LookAction.action.performed -= OnLookPerformed;
		_LookAction.action.canceled -= OnLookCanceled;
		_SprintAction.action.performed -= OnSprintPerformed;
		_SprintAction.action.canceled -= OnSprintCanceled;

		_JumpAction.action.performed -= _jumpCallback;
		_AttackAction.action.performed -= _attackCallback;
		_InteractionAction.action.performed -= _interactionCallback;

		DisableAllActions();
	}

	/// <summary>
	///     Converts a control scheme name into a DeviceType and applies it if changed.
	/// </summary>
	/// <param name="controlScheme">The control scheme string from PlayerInput.</param>
	private void UpdateCurrentDeviceType(string controlScheme)
	{
		DeviceType newDevice = controlScheme switch
		{
				"Keyboard&Mouse" => DeviceType.KeyboardMouse,
				"Gamepad" => DeviceType.Gamepad,
				_ => DeviceType.Unknown,
		};

		if (newDevice == CurrentDeviceType) return;
		CurrentDeviceType = newDevice;
		Debug.Log($"Device Changed: <color=red>{CurrentDeviceType}</color>");
		OnDeviceChanged?.Invoke(CurrentDeviceType);
	}
}
}
