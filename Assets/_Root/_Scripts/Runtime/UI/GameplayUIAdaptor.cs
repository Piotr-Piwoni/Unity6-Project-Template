using PROJECTNAME.Managers;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PROJECTNAME.UI
{
public class GameplayUIAdaptor : UIAdaptorBase
{
	[SerializeField]
	private GameObject _CrosshairCanvasPrefab;
	[SerializeField, ReadOnly,]
	private Canvas _CrosshairCanvas;


	protected override void Start()
	{
		base.Start();

		// If the player exists and the prefab to the crosshair canvas was provide, spawn it.
		if (GameManager.Instance.Player && _CrosshairCanvasPrefab)
			_CrosshairCanvas = Instantiate(_CrosshairCanvasPrefab, transform)
					.GetComponent<Canvas>();
	}
}
}
