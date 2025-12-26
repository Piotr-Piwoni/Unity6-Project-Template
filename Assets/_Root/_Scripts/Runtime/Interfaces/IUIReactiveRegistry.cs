using PROJECTNAME.UI;

namespace PROJECTNAME.Interfaces
{
public interface IUIReactiveRegistry
{
	void RegisterReactiveUI(UIInputReactiveBase reactiveUI);
	void UnregisterReactiveUI(UIInputReactiveBase reactiveUI);
}
}
