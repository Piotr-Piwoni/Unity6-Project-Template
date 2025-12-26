using PROJECTNAME.Interfaces;
using PROJECTNAME.UI;
using PROJECTNAME.UI.UGUI;
using PROJECTNAME.Utilities;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PROJECTNAME.Managers
{
public class UIManager : Singleton<UIManager>
{
	public IUIBackend Backend { get; private set; }

	[TabGroup("", "Info", SdfIconType.QuestionSquareFill, TextColor = "lightblue"),
	 ShowInInspector, ReadOnly, HideLabel,]
	private UGuiUIBackendState BackendState => (Backend as UGuiUIBackend)?.State;

	[SerializeField, TabGroup("", "Settings", SdfIconType.GearFill, TextColor = "yellow"),
	 FoldoutGroup("/Settings/Prefabs"),]
	private GameObject _CrosshairCanvasPrefab;


	protected override void Awake()
	{
		base.Awake();
		Backend = new UGuiUIBackend(transform, _CrosshairCanvasPrefab);
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
		Backend?.Shutdown();
	}

	public void RegisterReactiveUI(UIInputReactiveBase reactiveUI)
	{
		if (Backend is IUIReactiveRegistry registry)
			registry.RegisterReactiveUI(reactiveUI);
	}

	public void UnregisterReactiveUI(UIInputReactiveBase reactiveUI)
	{
		if (Backend is IUIReactiveRegistry registry)
			registry.UnregisterReactiveUI(reactiveUI);
	}

	private void OnDeviceChanged(DeviceType deviceType)
	{
		Backend.HandleDeviceChange(deviceType);
	}
}
}
