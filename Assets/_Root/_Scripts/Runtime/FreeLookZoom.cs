using EditorAttributes;
using Unity.Cinemachine;
using UnityEngine;

namespace Mummoth
{
	public class FreeLookZoom : MonoBehaviour
	{
		[SerializeField, FoldoutGroup("Settings", true, nameof(_ZoomDistance),
			 nameof(_ZoomSpeed))]
		private Void _ZoomSettingsGroup;
		[SerializeField, HideInInspector, MinMaxRangeSlider(0f, 100f)]
		private Vector2 _ZoomDistance = new(20f, 80f);
		[SerializeField, HideInInspector]
		private float _ZoomSpeed = 15f;

		private CinemachineCamera _Camera;


		private void Awake()
		{
			_Camera = GetComponent<CinemachineCamera>();
		}

		private void Start()
		{
			if (!_Camera)
				_Camera = GetComponent<CinemachineCamera>();
		}

		private void Update()
		{
			var scrollInput = Input.GetAxis("Mouse ScrollWheel");
			if (scrollInput != 0f) AdjustZoom(scrollInput);
		}

		private void AdjustZoom(float scroll)
		{
			if (_Camera)
			{
				_Camera.Lens.FieldOfView = Mathf.Clamp(
					_Camera.Lens.FieldOfView - scroll * _ZoomSpeed,
					_ZoomDistance.x, _ZoomDistance.y);
			}
		}
	}
}