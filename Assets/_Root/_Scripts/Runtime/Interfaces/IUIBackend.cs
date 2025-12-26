using DeviceType = PROJECTNAME.Managers.DeviceType;

namespace PROJECTNAME.Interfaces
{
public interface IUIBackend
{
	void Init();
	void Shutdown();
	void OnDeviceChanged(DeviceType deviceType);
}
}