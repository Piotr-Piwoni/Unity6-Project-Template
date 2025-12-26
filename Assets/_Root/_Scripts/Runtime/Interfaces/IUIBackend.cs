using DeviceType = PROJECTNAME.Managers.DeviceType;

namespace PROJECTNAME.Interfaces
{
public interface IUIBackend
{
	void Shutdown();
	void HandleDeviceChange(DeviceType deviceType);
}
}