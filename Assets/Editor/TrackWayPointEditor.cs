using UnityEngine;
using System.Collections;
using UnityEditorInternal;
using UnityEditor;

[CustomEditor(typeof(TrackWayPoint))]
public class TrackWayPointEditor : Editor
{

	public override void OnInspectorGUI()
	{
		DrawDefaultInspector();
		for (int i = 0; i < ((TrackWayPoint)target).nextPoints.Length; i++)
		{
			if (((TrackWayPoint)target).isNextPointCurve (i))
			{
				if (GUILayout.Button ("Remove Curve From Element " + i))
				{
					((TrackWayPoint)target).removeCurveFromNextPoint (i);
					EditorUtility.SetDirty(((TrackWayPoint)target)); 
				}
			}
			else
			{
				if (GUILayout.Button ("Convert Element " + i + " To Curve"))
				{
					((TrackWayPoint)target).convertNextPointToCurve (i);
					EditorUtility.SetDirty(((TrackWayPoint)target)); 
				}
			}
		}
	}
}
