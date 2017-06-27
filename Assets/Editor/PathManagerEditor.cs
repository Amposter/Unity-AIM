using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditorInternal;

[CustomEditor(typeof(PathManager))]
public class PathManagerEditor : Editor
{

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		if (GUILayout.Button ("Update Start/End Point List"))
		{
			((PathManager)target).findStartEndPoints();
			EditorUtility.SetDirty(((PathManager)target)); 
		}
		if (GUILayout.Button ("(Debug) Display Random Path"))
		{
			((PathManager)target).showRandomPath();
		}
		if (GUILayout.Button ("(Debug) Clear Path"))
		{
			((PathManager)target).clearDisplayPath();
		}
	}
}
