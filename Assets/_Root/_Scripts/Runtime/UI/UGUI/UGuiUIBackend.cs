using System;
using System.Collections.Generic;
using PROJECTNAME.Interfaces;
using PROJECTNAME.Managers;
using UnityEngine;
using DeviceType = PROJECTNAME.Managers.DeviceType;
using Object = UnityEngine.Object;

namespace PROJECTNAME.UI.UGUI
{
public class UGuiUIBackend : IUIBackend, IUIReactiveRegistry
{
	public UGuiUIBackendState State { get; private set; }

	private readonly Canvas _CrosshairCanvas;
	private readonly List<UIInputReactiveBase> _ReactiveUIs = new();
	private readonly Transform _Root;


	public UGuiUIBackend(Transform root, GameObject crosshairCanvasPrefab)
	{
		_Root = root;

		// If the player exists and the prefab to the crosshair canvas was provide, spawn it.
		if (GameManager.Instance.Player && crosshairCanvasPrefab)
			_CrosshairCanvas = Object.Instantiate(crosshairCanvasPrefab, _Root).GetComponent<Canvas>();

		UpdateState();
	}

	public void Shutdown()
	{
		if (_CrosshairCanvas)
			Object.Destroy(_CrosshairCanvas.gameObject);

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

	/// Cache the backend state.
	private void UpdateState()
	{
		State = new UGuiUIBackendState(_CrosshairCanvas, _ReactiveUIs.Count);
	}
}
}
