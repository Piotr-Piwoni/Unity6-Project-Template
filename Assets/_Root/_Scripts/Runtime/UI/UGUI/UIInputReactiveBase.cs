using PROJECTNAME.Managers;
using UnityEngine;
using DeviceType = PROJECTNAME.Managers.DeviceType;

namespace PROJECTNAME.UI.UGUI
{
public abstract class UIInputReactiveBase : MonoBehaviour
{
	protected virtual void Start()
	{
		if (UIManager.Instance)
			UIManager.Instance.RegisterReactiveUI(this);
	}

	public abstract void HandleDeviceChange(DeviceType deviceType);
}
}
