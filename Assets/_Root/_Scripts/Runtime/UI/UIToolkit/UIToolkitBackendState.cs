using System;
using Sirenix.OdinInspector;
using UnityEngine.UIElements;

namespace PROJECTNAME.UI.UIToolkit
{
[Serializable, InlineProperty(LabelWidth = 120),]
public class UIToolkitBackendState
{
	[ShowInInspector]
	public int ReactiveUICount { get; }
	[ShowInInspector]
	public UIDocument UIDocument { get; }


	public UIToolkitBackendState(UIDocument document, int reactiveUICount)
	{
		UIDocument = document;
		ReactiveUICount = reactiveUICount;
	}
}
}
