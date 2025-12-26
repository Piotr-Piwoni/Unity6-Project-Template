using System;
using System.Collections.Generic;
using PROJECTNAME.Interfaces;
using UnityEngine;
using UnityEngine.UIElements;
using DeviceType = PROJECTNAME.Managers.DeviceType;

namespace PROJECTNAME.UI.UIToolkit
{
public class UIToolkitBackend : IUIBackend, IUIReactiveRegistry
{
	public UIToolkitBackendState State { get; private set; }

	private readonly List<UIInputReactiveBase> _ReactiveUIs = new();
	private readonly UIDocument _uiDocument;


	public UIToolkitBackend(UIDocument uiDocument)
	{
		_uiDocument = uiDocument ?? throw new ArgumentNullException(nameof(uiDocument));
		UpdateState();
	}

	public void Shutdown()
	{
		if (_uiDocument)
			_uiDocument.rootVisualElement.Clear();

		_ReactiveUIs.Clear();
		UpdateState();
	}

	public void HandleDeviceChange(DeviceType deviceType)
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

		foreach (UIInputReactiveBase reactiveUI in _ReactiveUIs)
			reactiveUI.HandleDeviceChange(deviceType);
	}

	public void RegisterReactiveUI(UIInputReactiveBase reactiveUI)
	{
		if (_ReactiveUIs.Contains(reactiveUI))
			return;
		_ReactiveUIs.Add(reactiveUI);
		UpdateState();
	}

	public void UnregisterReactiveUI(UIInputReactiveBase reactiveUI)
	{
		if (_ReactiveUIs.Remove(reactiveUI))
			UpdateState();
	}

	private void UpdateState()
	{
		State = new UIToolkitBackendState(_uiDocument, _ReactiveUIs.Count);
	}
}
}
