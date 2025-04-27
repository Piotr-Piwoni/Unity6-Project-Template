using EditorAttributes;
using PROJECTNAME.Systems;
using UnityEngine;
using AudioType = PROJECTNAME.Systems.AudioType;

namespace Demos
{
public class AudioDemoScript : MonoBehaviour
{
	private const bool ShowMessageBoxes = true;

	[MessageBox("For the Audio System all you need is an Audio Clip.",
		nameof(ShowMessageBoxes))]
	public AudioClip m_Clip;
	[MessageBox(
		"You can set the clip's position to make it work as a 3D sound. Keeping it at zero or NULL will tell the system to treat the clip normally.",
		nameof(ShowMessageBoxes))]
	public Vector3 m_ClipPosition = Vector3.zero;
	[MessageBox(
		"You can also specify if you wish for the clip to be looping. By default, it doesn't loop the audio clip.",
		nameof(ShowMessageBoxes))]
	public bool m_IsLooping;
	[MessageBox(
		"You can also tell the system to specifically treat the clip as a music clip, this will plat the clip as a non-3D sound on loop in its own dedicated music AudioSource.",
		nameof(ShowMessageBoxes))]
	public bool m_IsMusic;
	[MessageBox(
		 "For any clip including music, you can specify a volume for that clip, by default it's 1F as the Audio System's volume is managed by the AudioMixer in th Resources folder. The clip's volume can only be between 0F - 1F.",
		 nameof(ShowMessageBoxes)), Clamp(0f, 1f)]
	public float m_Volume = 1f;
	[MessageBox(
		"The Audio System allows you to define what type of audio is being played. That determines what slider in the AudioMixer controls the clip's volume. By default audio clips are all treated as \"SFX\".",
		nameof(ShowMessageBoxes))]
	public AudioType m_AudioType = AudioType.Sfx;
	private AudioSource _AudioSource;


	[Button("Play Clip")]
	private void PlayClip()
	{
		// AudioSystem's "PlayClip()" returns AudioSource, which can be used to stop the clip. But it can be ignored.
		_AudioSource = AudioSystem.Instance.PlayClip(m_Clip, m_ClipPosition,
			m_AudioType, m_Volume, m_IsLooping);
	}

	[Button("Play Music")]
	private void PlayMusic()
	{
		if (!m_IsMusic)
		{
			Debug.Log($"You need to set {nameof(m_IsMusic)} to true.");
			return;
		}

		AudioSystem.Instance.PlayMusic(m_Clip);
	}

	[Button("Stop Clip")]
	private void StopClip()
	{
		AudioSystem.Instance.StopClip(_AudioSource);
	}

	[Button("Stop Music")]
	private void StopMusic()
	{
		AudioSystem.Instance.StopMusic();
	}
}
}