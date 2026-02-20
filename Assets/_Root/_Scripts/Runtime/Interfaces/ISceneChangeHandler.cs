using UnityEngine.SceneManagement;

namespace PROJECTNAME.Interfaces
{
/// <summary>
///     Defines a contract for objects that need to respond to Unity scene changes.
/// </summary>
/// <remarks>
///     Implementers are typically managers, systems, or persistant singletons
///     that must react when a new scene is loaded or unloaded.
/// </remarks>
public interface ISceneChangeHandler
{
	/// <summary>
	///     Called when the active scene changes.
	/// </summary>
	/// <param name="scene">The scene to load or activate.</param>
	/// <param name="mode">The mode used to load the scene.</param>
	void OnSceneChange(Scene scene, LoadSceneMode mode);
}
}
