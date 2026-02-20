using PROJECTNAME.Managers;
using UnityEngine;
using DeviceType = PROJECTNAME.Utilities.Types.DeviceType;

namespace PROJECTNAME.UI
{
public abstract class UIAdaptorBase : MonoBehaviour
{
	protected virtual void Start()
	{
		UIManager.Instance.RegisterAdaptor(this);
	}

	protected virtual void OnDestroy()
	{
		UIManager.Instance.UnRegisterAdaptor(this);
	}


	public virtual void OnDeviceChange(DeviceType deviceType) { }
}
}
