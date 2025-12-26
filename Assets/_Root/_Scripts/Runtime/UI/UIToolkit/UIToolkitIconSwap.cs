using System;
using PROJECTNAME.Managers;
using UnityEngine;
using UnityEngine.UIElements;
using DeviceType = PROJECTNAME.Managers.DeviceType;

namespace PROJECTNAME.UI.UIToolkit
{
public class UIToolkitIconSwap : UIInputReactiveBase
{
	[SerializeField]
	private string _KeyboardMouseElementName = "KeyboardMouseIcon";
	[SerializeField]
	private string _GamepadElementName = "GamepadIcon";

	private VisualElement _IconElement;
	private VisualElement _Root;


	protected override void Start()
	{
		base.Start();

		// Get the root VisualElement from the backend.
		if (UIManager.Instance?.Backend is not UIToolkitBackend backend)
			return;

		_Root = backend.State.UIDocument.rootVisualElement;
		_IconElement = _Root.Q<VisualElement>(_KeyboardMouseElementName);
	}

	public override void HandleDeviceChange(DeviceType deviceType)
	{
		if (_Root == null || _IconElement == null)
			return;

		switch (deviceType)
		{
		case DeviceType.KeyboardMouse:
			_IconElement.style.display = DisplayStyle.Flex;
			ToggleOtherIcon(_GamepadElementName, false);
			break;
		case DeviceType.Gamepad:
			_IconElement = _Root.Q<VisualElement>(_GamepadElementName);
			_IconElement.style.display = DisplayStyle.Flex;
			ToggleOtherIcon(_KeyboardMouseElementName, false);
			break;
		case DeviceType.Unknown:
			throw new ArgumentOutOfRangeException(nameof(deviceType), deviceType, null);
		}
	}

	private void ToggleOtherIcon(string elementName, bool show)
	{
		var other = _Root.Q<VisualElement>(elementName);
		if (other != null)
			other.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
	}
}
}
