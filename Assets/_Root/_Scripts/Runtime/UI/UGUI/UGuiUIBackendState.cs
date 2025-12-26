using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace PROJECTNAME.UI.UGUI
{
[Serializable, InlineProperty(LabelWidth = 120),]
public class UGuiUIBackendState
{
	[ShowInInspector]
	public Canvas CrosshairCanvas { get; }
	[ShowInInspector]
	public int ReactiveUICount { get; }


	public UGuiUIBackendState(Canvas crosshairCanvas, int reactiveUICount)
	{
		CrosshairCanvas = crosshairCanvas;
		ReactiveUICount = reactiveUICount;
	}
}
}
