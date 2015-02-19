//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using UnityEngine;
using TNet;

/// <summary>
/// This tiny script sets the value of a Text Mesh.
/// It's used to replace the "connecting..." text with an error message when connection fails.
/// </summary>

[RequireComponent(typeof(TextMesh))]
public class TNAutoJoinError : MonoBehaviour
{
	void AutoJoinError (string message)
	{
		TextMesh tm = GetComponent<TextMesh>();
		tm.text = message;
	}
}
