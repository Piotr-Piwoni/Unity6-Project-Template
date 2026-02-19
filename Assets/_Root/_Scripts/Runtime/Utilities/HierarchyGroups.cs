using System;
using Unity.Cinemachine;
using UnityEngine;

namespace PROJECTNAME.Utilities
{
[Serializable]
public class HierarchyGroups
{
	public Group ActorGroup = new("ActorGroup", "Actors");
	public Group SystemGroup = new("SystemGroup", "Systems");
	public Group WorldGroup = new("WorldGroup", "World");
	public Group UIGroup = new("UIGroup", "UI");
	public Group LightsGroup = new("LightsGroup", "Lights");
	public Group CameraGroup = new("CameraGroup", "Cameras");
	public Group MiscGroup = new("MiscGroup", "Miscellaneous");

	public void Initialize()
	{
		ActorGroup.Obtain();
		SystemGroup.Obtain();
		WorldGroup.Obtain();
		UIGroup.Obtain();
		LightsGroup.Obtain();
		CameraGroup.Obtain();
		MiscGroup.Obtain();
	}


	[Serializable]
	public struct Group
	{
		public Transform Object;
		[TagField,]
		public string Tag;
		public string Name;

		public Group(string tag, string name = "New Group")
		{
			Object = null;
			Tag = tag;
			Name = name;
		}

		public Transform TryFind()
		{
			var obj = GameObject.FindGameObjectWithTag(Tag);
			if (obj) return obj.transform;

			Debug.LogError("No valid object found! Try checking if the tag is correct.");
			return null;
		}

		public void Obtain()
		{
			// If the object was not found, create it.
			Transform group = TryFind();
			Object = !group ? new GameObject(Name) { tag = Tag, }.transform : group;
		}
	}
}
}
