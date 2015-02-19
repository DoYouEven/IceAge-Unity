//------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//------------------------------------------

using UnityEngine;
using TNet;

/// <summary>
/// This simple script shows how to change the color of an object on all connected clients.
/// You can see it used in Example 1.
/// </summary>

[RequireComponent(typeof(TNObject))]
public class ColoredObject : MonoBehaviour
{
	/// <summary>
	/// This function is called by the server when one of the players sends an RFC call.
	/// </summary>

	[RFC] void OnColor (Color c)
	{
		renderer.material.color = c;
	}

	/// <summary>
	/// Clicking on the object should change its color.
	/// </summary>

	void OnClick ()
	{
		Color color = Color.red;

		if		(renderer.material.color == Color.red)	 color = Color.green;
		else if (renderer.material.color == Color.green) color = Color.blue;

		TNObject tno = GetComponent<TNObject>();
		tno.Send("OnColor", Target.AllSaved, color);
	}
}