using PROJECTNAME.Managers;
using Sirenix.OdinInspector;
using UnityEngine;
using DeviceType = PROJECTNAME.Utilities.Types.DeviceType;

namespace PROJECTNAME.UI
{
[HideMonoScript]
public abstract class UIAdaptor : MonoBehaviour
{
	protected virtual void Start()
	{
		UIManager.Instance.RegisterAdaptor(this);
	}

	protected virtual void OnDestroy()
	{
		UIManager.Instance?.UnRegisterAdaptor(this);
	}


	public virtual void OnDeviceChange(DeviceType deviceType) { }
}
}
