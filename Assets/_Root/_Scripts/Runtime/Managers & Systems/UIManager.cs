using System;
using System.Collections.Generic;
using PROJECTNAME.UI;
using PROJECTNAME.Utilities;
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
public class UIManager : Singleton<UIManager>
{
	[SerializeField]
	private GameObject _CrosshairCanvasPrefab;
	[SerializeField, ReadOnly,]
	private Canvas _CrosshairCanvas;

	private readonly List<UIInputReactiveBase> _ReactiveUIs = new();


	private void Start()
	{
		// If the player exists and the prefab to the crosshair canvas was provide, spawn it.
		if (GameManager.Instance.Player && _CrosshairCanvasPrefab)
			_CrosshairCanvas = Instantiate(_CrosshairCanvasPrefab, transform)
					.GetComponent<Canvas>();
	}

	private void OnEnable()
	{
		InputManager.Instance.OnDeviceChanged += OnDeviceChanged;
	}

	private void OnDisable()
	{
		if (!InputManager.Instance) return;
		InputManager.Instance.OnDeviceChanged -= OnDeviceChanged;
	}


	public void AddReactiveUI(UIInputReactiveBase uiInputReactiveBase)
	{
		if (!_ReactiveUIs.Contains(uiInputReactiveBase))
			_ReactiveUIs.Add(uiInputReactiveBase);
	}

	private void OnDeviceChanged(DeviceType deviceType)
	{
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

		foreach (UIInputReactiveBase uiInputReactiveBase in _ReactiveUIs)
			uiInputReactiveBase.HandleDeviceChange(deviceType);
	}
}
}
