using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using EditorAttributes;
using PROJECTNAME.Utilities;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Pool;
using Debug = UnityEngine.Debug;

namespace PROJECTNAME.Systems
{
[RequireComponent(typeof(AudioSource))]
public class AudioSystem : Singleton<AudioSystem>
{
	[SerializeField, PropertyOrder(-1)]
	private bool _Log;

	[SerializeField, Clamp(0, int.MaxValue)]
	private int _MaxAudioSources = 50;

	private readonly Dictionary<AudioSource, Coroutine> _AudioSourceCoroutines =
		new();
	private ObjectPool<AudioSource> _AudioSourcePool;
	private AudioMixer _Mixer;
	private AudioSource _MusicSrc;

	protected override void Awake()
	{
		base.Awake();

		// Load the AudioMixer.
		_Mixer = Resources.Load<AudioMixer>("GameAudioMixer");

		// The AudioSource on this object will act as the music source.
		_MusicSrc = GetComponent<AudioSource>();
		_MusicSrc.loop = true;
		_MusicSrc.playOnAwake = false;
		FindMixerGroup(_MusicSrc, "Music");

		// Initialize the pool with a factory methods and pooling settings.
		_AudioSourcePool = new ObjectPool<AudioSource>(CreateAudioSource,
			OnGetAudioSource, OnReleaseAudioSource, OnDestroyAudioSource,
			defaultCapacity: 10, maxSize: _MaxAudioSources);
	}

	private void Update()
	{
		// Only if logging is enabled.
		if (_Log) LogSources();
	}

	/// <summary>
	///     Plays the provided audio clip.
	/// </summary>
	/// <param name="clip">The audio to play.</param>
	/// <param name="position">The position of the clip in 3D.</param>
	/// <param name="type">What type of audio is it.</param>
	/// <param name="volume">The audio's volume.</param>
	/// <param name="loop">If the audio should be looping.</param>
	/// <returns>The audio source used.</returns>
	public AudioSource PlayClip(AudioClip clip, Vector3? position = null,
		AudioType type = AudioType.Sfx, float volume = 1f, bool loop = false)
	{
		// Retrieve an AudioSource from the pool.
		AudioSource audioSrc = _AudioSourcePool.Get();

		// Set up the AudioSource.
		// If position was provided, turn the current audio source into 3D sound.
		if (position.HasValue)
		{
			audioSrc.transform.position = position.Value;
			audioSrc.spatialBlend = 1f; //< 3D sound.
		}

		// Based on the AudioType find the corresponding mixer group.
		switch (type)
		{
			case AudioType.Music:
				FindMixerGroup(audioSrc, "Music");
				break;
			case AudioType.Sfx:
				FindMixerGroup(audioSrc, "SFX");
				break;
			case AudioType.Dialogue:
				FindMixerGroup(audioSrc, "Dialogue");
				break;
		}

		audioSrc.clip = clip;
		audioSrc.volume = volume;
		audioSrc.loop = loop;
		audioSrc.Play();

		if (!_AudioSourceCoroutines.ContainsKey(audioSrc))
		{
			// If the AudioSource is not already in the dictionary, add it.
			// Start the coroutine and add it to the dictionary for quick lookup.
			Coroutine coroutine = WaitToFinishPlaying(audioSrc);
			_AudioSourceCoroutines.Add(audioSrc, coroutine);
		}
		else
		{
			// Start the coroutine and add it to the dictionary for quick lookup.
			Coroutine coroutine = WaitToFinishPlaying(audioSrc);
			_AudioSourceCoroutines[audioSrc] = coroutine;
		}

		return audioSrc;
	}

	/// <summary>
	///     Plays the provided audio clip on loop.
	/// </summary>
	/// <param name="clip">The audio to play.</param>
	public void PlayMusic(AudioClip clip)
	{
		// Stop any existing music before playing a new one.
		if (_MusicSrc.isPlaying) _MusicSrc.Stop();

		_MusicSrc.clip = clip;
		_MusicSrc.Play();
	}

	/// <summary>
	///     Stop the current audio clip from playing.
	/// </summary>
	/// <param name="audioSrc">The audio source that you wish to stop playing.</param>
	public void StopClip(AudioSource audioSrc)
	{
		if (!audioSrc) return;

		// Stop the audio source and update the dictionary.
		audioSrc.Stop();

		// If it's in the dictionary and it had a coroutine, stop it.
		if (!_AudioSourceCoroutines.TryGetValue(audioSrc,
			    out Coroutine coroutine)) return;

		// Clean up.
		if (coroutine != null)
			StopCoroutine(coroutine);

		_AudioSourceCoroutines[audioSrc] = null;

		// Check if the audio source is already released into the pool.
		if (!audioSrc.gameObject.activeSelf)
			return;

		// Release the audio source back to the pool.
		_AudioSourcePool.Release(audioSrc);
	}

	// Stops the current music from playing.
	public void StopMusic()
	{
		_MusicSrc.Stop();
	}

	// Create a new AudioSource.
	private AudioSource CreateAudioSource()
	{
		var tempOb = new GameObject("TempSrc", typeof(AudioSource));
		tempOb.transform.SetParent(transform);
		var source = tempOb.GetComponent<AudioSource>();
		source.playOnAwake = false;
		return source;
	}

	/// <summary>
	///     Find and assign the desired group from the Audio Mixer.
	/// </summary>
	/// <param name="audioSrs">The source that the mixer group will be assigned to.</param>
	/// <param name="groupName">The name of the mixer group to find.</param>
	private void FindMixerGroup(AudioSource audioSrs, string groupName)
	{
		var mixerGroups = _Mixer.FindMatchingGroups(groupName);
		if (mixerGroups.Length > 0)
			audioSrs.outputAudioMixerGroup = mixerGroups[0];
		else
		{
			Debug.LogError(
				$"The group: \"{groupName}\", was not found in Audio Mixer! Defaulting to \"Master\".");
			audioSrs.outputAudioMixerGroup =
				_Mixer.FindMatchingGroups("Master")[0]; //< Fallback to Master.
		}
	}

	// Output to console the AudioSources and their respected Coroutines.
	[Conditional("UNITY_EDITOR")]
	private void LogSources()
	{
		if (_AudioSourceCoroutines.Count <= 0) return;

		Debug.Log(
			$"<color=Red>------------- {name}<AudioSystem> Log Start -------------</color>");
		foreach ((AudioSource key, Coroutine value) in _AudioSourceCoroutines)
		{
			// If there's a active coroutine, get its code, otherwise output NULL.
			var logVal =
				value != null ? value.GetHashCode().ToString() : "Null";

			// If the source is looping, specify that it is.
			if (key.loop) logVal += ", is Loop";

			Debug.Log(
				$"{key?.name}: <color=yellow>{key?.GetInstanceID()}</color>\t" +
				$"Coroutine: <color=green>{logVal}</color>");
		}

		Debug.Log(
			$"<color=Red>------------- {name}<AudioSystem> End -------------</color>");
	}

	// Called when an AudioSource is destroyed.
	private void OnDestroyAudioSource(AudioSource audioSource)
	{
		// Stop its corresponding coroutine and clean up.
		if (_AudioSourceCoroutines.ContainsKey(audioSource))
		{
			StopCoroutine(_AudioSourceCoroutines[audioSource]);
			_AudioSourceCoroutines.Remove(audioSource);
		}

		Destroy(audioSource.gameObject);
	}

	// Called when an AudioSource is retrieved from the pool
	private void OnGetAudioSource(AudioSource audioSource)
	{
		audioSource.gameObject.SetActive(true);
	}

	// Called when an AudioSource is released back to the pool.
	private void OnReleaseAudioSource(AudioSource audioSource)
	{
		audioSource.gameObject.SetActive(false);
	}

	// Release the AudioSource back to the pool after it finishes playing.
	private IEnumerator ReturnAudioSourceToPool(AudioSource audioSource,
		float clipLength)
	{
		yield return new WaitForSeconds(clipLength);
		_AudioSourcePool.Release(audioSource);

		// Set the coroutine to NULL in the dictionary after the audio finishes.
		if (_AudioSourceCoroutines.ContainsKey(audioSource))
			_AudioSourceCoroutines[audioSource] = null;
	}

	private Coroutine WaitToFinishPlaying(AudioSource src)
	{
		// Ensure that the coroutine only happens for non-looping sources.
		Coroutine coroutine = null;
		if (src.loop) return null;

		// Ensure no coroutine is running for the same source.
		if (!_AudioSourceCoroutines.ContainsKey(src) ||
		    _AudioSourceCoroutines[src] == null)
		{
			coroutine =
				StartCoroutine(ReturnAudioSourceToPool(src, src.clip.length));
		}

		return coroutine;
	}
#if UNITY_EDITOR

	[Space, Title("<b>Debug Settings</b>", 15), SerializeField,
	 PropertyOrder(-1), EnableField(nameof(_Log))]
	private AudioClip _TestingClip;
	[SerializeField, PropertyOrder(-1), EnableField(nameof(_Log))]
	private bool _LoopClip;
	[SerializeField, PropertyOrder(-1), ReadOnly]
	private AudioSource _TestingSource;
	[SerializeField, PropertyOrder(-1),
	 ButtonField(nameof(PlayTestingClip), "Play Test Clip")]
	private Void _PlayTestClipButtonHolder;
	[SerializeField, PropertyOrder(-1),
	 ButtonField(nameof(StopTestingClip), "Stop Test Clip")]
	private Void _StopTestClipButtonHolder;


	private void StopTestingClip()
	{
		if (!_Log) return;
		if (!_TestingSource) return;
		StopClip(_TestingSource);
	}

	private void PlayTestingClip()
	{
		if (!_Log) return;
		if (!_TestingClip)
		{
			Debug.Log("\"_TestingClip\" was not assigned!");
			return;
		}

		_TestingSource = PlayClip(_TestingClip, loop: _LoopClip);
	}
#endif
}

public enum AudioType
{
	Music = 0,
	Sfx = 1,
	Dialogue = 2
}
}