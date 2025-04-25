using PROJECTNAME.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

// Inspired and taken from a Tarodev video - Unity Architecture for Noobs - Game Structure
// URL: https://www.youtube.com/watch?v=tE1qH8OxO2Y

namespace PROJECTNAME.Utilities
{
/// <summary>
///     A static instance is similar to a singleton, but instead of destroying new
///     instance, it overrides the current instance.
///     Great for resetting object state.
/// </summary>
/// <typeparam name="T">The class to make a static instance.</typeparam>
public abstract class StaticInstance<T> : MonoBehaviour
	where T : MonoBehaviour
{
	public static T m_Instance { get; private set; }


	protected virtual void Awake()
	{
		if (!m_Instance)
			m_Instance = this as T;
		else if (m_Instance != this) Destroy(gameObject);
	}

	protected virtual void OnApplicationQuit()
	{
		m_Instance = null;
		Destroy(gameObject);
	}
}

/// <summary>
///     A basic singleton. It will destroy any new versions created, leaving the
///     original instance intact.
/// </summary>
/// <typeparam name="T">The class to make a singleton.</typeparam>
public abstract class Singleton<T> : StaticInstance<T>, ISceneChangeHandler
	where T : MonoBehaviour
{
	protected override void Awake()
	{
		if (m_Instance)
		{
			Destroy(gameObject);
			return;
		}

		base.Awake();
		PersistentSystems.m_Instance.RegisterSingleton(this);
	}

	public abstract void OnSceneChange(Scene scene, LoadSceneMode mode);
}


/// <summary>
///     A persistent version of the singleton. This will survive through scene
///     loads.
/// </summary>
/// <typeparam name="T">The class to make a persistent singleton.</typeparam>
public abstract class PersistantSingleton<T> : Singleton<T>
	where T : MonoBehaviour
{
	protected override void Awake()
	{
		base.Awake();
		if (m_Instance != this) return;
		transform.SetParent(null); //< Make root object if not.
		DontDestroyOnLoad(gameObject);
	}
}
}