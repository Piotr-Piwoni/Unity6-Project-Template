using System;
using System.Collections.Generic;
using PROJECTNAME.UI;
using PROJECTNAME.Utilities;
using PROJECTNAME.Utilities.Types;
using Sirenix.OdinInspector;
using UnityEngine;
using DeviceType = PROJECTNAME.Utilities.Types.DeviceType;

namespace PROJECTNAME.Managers
{
/// <summary>
///     Manages global UI elements and input-reactive UI behaviour.
/// </summary>
/// <remarks>
///     This manager listens for input device changes and propagates them to all
///     registered UI elements that react to input method changes.
/// </remarks>
[HideMonoScript]
public class UIManager : PersistentSingleton<UIManager>
{
	public event Action<UIMode> OnUIModeChanged;

	private readonly List<UIAdaptor> _UIAdaptors = new();


	protected override void Awake()
	{
		base.Awake();
		_UIAdaptors.Capacity = 20;
	}

	public override void OnEnable()
	{
		base.OnEnable();
		InputManager.Instance.OnDeviceChanged += OnDeviceChanged;
	}

	public override void OnDisable()
	{
		base.OnDisable();
		if (!InputManager.Instance) return;
		InputManager.Instance.OnDeviceChanged -= OnDeviceChanged;
	}


	public void RegisterAdaptor(UIAdaptor adaptor)
	{
		if (!_UIAdaptors.Contains(adaptor))
			_UIAdaptors.Add(adaptor);
	}

	public void SetUIMode(UIMode mode)
	{
		OnUIModeChanged?.Invoke(mode);
	}

	public void UnRegisterAdaptor(UIAdaptor adaptor)
	{
		if (_UIAdaptors.Contains(adaptor))
			_UIAdaptors.Remove(adaptor);
	}

	private void OnDeviceChanged(DeviceType deviceType)
	{
		// Example showing what control scheme is currently active.
		switch (deviceType)
		{
		case DeviceType.KeyboardMouse:
			Debug.Log("Showing Keyboard & Mouse UI.");
			break;
		case DeviceType.Gamepad:
			Debug.Log("Showing Gamepad UI.");
			break;
		case DeviceType.Unknown:
			throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null);
		}

		foreach (UIAdaptor adaptor in _UIAdaptors)
			adaptor.OnDeviceChange(deviceType);
	}
}
}
