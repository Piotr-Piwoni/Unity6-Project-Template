using UnityEngine.SceneManagement;

namespace PROJECT.Interfaces
{
public interface ISceneChangeHandler
{
	void OnSceneChange(Scene scene, LoadSceneMode mode);
}
}