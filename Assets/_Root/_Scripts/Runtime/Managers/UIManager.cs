using PROJECTNAME.Interfaces;
using PROJECTNAME.UI.UGUI;
using PROJECTNAME.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PROJECTNAME.Managers
{
public class UIManager : Singleton<UIManager>
{
	[TabGroup("", "Info", SdfIconType.QuestionSquareFill, TextColor = "lightblue"),
	 ShowInInspector, ReadOnly, HideLabel,]
	private UGuiUIBackendState BackendState => (_Backend as UGuiUIBackend)?.State;

	[SerializeField, TabGroup("", "Settings", SdfIconType.GearFill, TextColor = "yellow"),
	 FoldoutGroup("/Settings/Prefabs"),]
	private GameObject _CrosshairCanvasPrefab;

	private IUIBackend _Backend;


	protected override void Awake()
	{
		base.Awake();
		_Backend = new UGuiUIBackend(transform, _CrosshairCanvasPrefab);
	}

	private void OnEnable()
	{
		InputManager.Instance.OnDeviceChanged += OnDeviceChanged;
	}

	private void OnDisable()
	{
		if (!InputManager.Instance)
			return;

		InputManager.Instance.OnDeviceChanged -= OnDeviceChanged;
		_Backend?.Shutdown();
	}

	public void RegisterReactiveUI(UIInputReactiveBase reactiveUI)
	{
		if (_Backend is IUIReactiveRegistry registry)
			registry.RegisterReactiveUI(reactiveUI);
	}

	public void UnregisterReactiveUI(UIInputReactiveBase reactiveUI)
	{
		if (_Backend is IUIReactiveRegistry registry)
			registry.UnregisterReactiveUI(reactiveUI);
	}

	private void OnDeviceChanged(DeviceType deviceType)
	{
		_Backend.HandleDeviceChange(deviceType);
	}
}
}
