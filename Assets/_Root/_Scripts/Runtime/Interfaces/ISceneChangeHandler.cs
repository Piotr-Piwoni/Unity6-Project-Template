using UnityEngine.SceneManagement;

namespace ProjectName.Interfaces
{
public interface ISceneChangeHandler
{
	void OnSceneChange(Scene scene, LoadSceneMode mode);
}
}