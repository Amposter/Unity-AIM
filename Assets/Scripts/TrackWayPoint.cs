using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class TrackWayPoint : MonoBehaviour
{
	//keeps track of what kind of node this is
	public enum Type{START, END, INTERSECTION_BORDER, NORMAL}
	public Type type = Type.NORMAL;

	public TrackWayPoint[] nextPoints;



	[HideInInspector]
	public TrackWayPoint prevPoint;

	//the key is the index of the TrackWayPoint (from nextPoints) that has been converted to a curve
	//the value is the bezier curve that connects the next point
	[System.Serializable]
	public class TypedDictionary : SerializableDictionary<int, BezierCurve> {}
	public TypedDictionary curveList;


	// Use this for initialization
	void Start ()
	{

	}
	
	// Update is called once per frame
	void Update ()
	{
	
	}

	void OnDrawGizmos()
	{
		switch (type)
		{
		case Type.START:
			Gizmos.color=Color.green;
			break;
		case Type.END:
			Gizmos.color=Color.red;
			break;
		case Type.INTERSECTION_BORDER:
			Gizmos.color=Color.cyan;
			break;
		case Type.NORMAL:
			Gizmos.color=Color.blue;
			break;
		}
		Gizmos.DrawWireSphere(transform.position,.4f);

		if (Application.isPlaying)
		{
			return;
		}

		for(int i = 0; i < nextPoints.Length; i++)
		{
			if (nextPoints[i] == null)
			{
				continue;
			}
			//only directly connect the next point visually if its not a curve
			if(!isNextPointCurve(i))
			{
				Gizmos.color = Color.green;
				Gizmos.DrawLine (transform.position, (transform.position+nextPoints[i].transform.position)/2f);
				Gizmos.color = Color.blue;
				Gizmos.DrawLine ((transform.position+nextPoints[i].transform.position)/2f, nextPoints[i].transform.position);
			}
		}

	}
		
	public bool isNextPointCurve(int nextPointIndex)
	{
		if (curveList == null) {
			return false;
		}
		if (curveList.ContainsKey(nextPointIndex))
		{
			return true;
		}
		else
		{
			return false;
		}
	}

	public void convertNextPointToCurve(int nextPointIndex)
	{
		GameObject newCurveObj = new GameObject ("Curve"+nextPointIndex);
		newCurveObj.transform.position = transform.position;
		newCurveObj.transform.parent = transform;

		BezierCurve curve = newCurveObj.AddComponent<BezierCurve> ();
		curve.drawColor = Color.yellow;
		curve.AddPointAt (transform.position);
		curve.GetAnchorPoints () [0].handle1 = (transform.position - nextPoints[nextPointIndex].transform.position).normalized*2;
		curve.AddPointAt (nextPoints[nextPointIndex].transform.position);
		curve.GetAnchorPoints () [1].handle1 = (transform.position - nextPoints[nextPointIndex].transform.position).normalized*2;
		curveList.Add (nextPointIndex, curve);
	}

	public void removeCurveFromNextPoint(int nextPointIndex)
	{
		
		foreach (Transform child in transform)
		{
			if(child.name.Equals("Curve"+nextPointIndex))
			{
				DestroyImmediate (child.gameObject);
				break;
			}
		}
		curveList.Remove (nextPointIndex);
	}
}
