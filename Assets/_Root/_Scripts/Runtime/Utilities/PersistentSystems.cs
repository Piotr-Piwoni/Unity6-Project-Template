using System.Collections.Generic;
using PROJECTNAME.Interfaces;
using EditorAttributes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PROJECTNAME.Utilities
{
public class PersistentSystems : PersistantSingleton<PersistentSystems>
{
	[SerializeField,
	 HelpBox("Don't modify the \"_Systems\" list!", MessageMode.Warning)]
	private Void _SystemsWarningBox;
	[SerializeField]
	private List<MonoBehaviour> _Systems = new();

	protected override void Awake()
	{
		base.Awake();
		// Remove itself from the list.
		if (_Systems.Contains(this))
			_Systems.Remove(this);
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	public override void OnSceneChange(Scene scene, LoadSceneMode mode)
	{
		// Handle scene change logic if needed.
	}

	// Register a singleton instance.
	public void RegisterSingleton(MonoBehaviour singleton)
	{
		if (!_Systems.Contains(singleton)) _Systems.Add(singleton);
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
	{
		// Call OnSceneChange for all registered Singleton systems.
		foreach (MonoBehaviour system in _Systems)
			if (system is ISceneChangeHandler handler)
				handler.OnSceneChange(scene, mode);
	}
}
}