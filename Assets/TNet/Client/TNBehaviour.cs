//---------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//---------------------------------------------

using UnityEngine;
using TNet;

/// <summary>
/// If your MonoBehaviour will need to use a TNObject, deriving from this class will make it easier.
/// </summary>

[RequireComponent(typeof(TNObject))]
public abstract class TNBehaviour : MonoBehaviour
{
	TNObject mTNO;

	public TNObject tno
	{
		get
		{
			if (mTNO == null) mTNO = GetComponent<TNObject>();
			return mTNO;
		}
	}

	protected virtual void OnEnable ()
	{
		if (Application.isPlaying)
		{
			tno.rebuildMethodList = true;
		}
	}

	/// <summary>
	/// Destroy this game object.
	/// </summary>

	public virtual void DestroySelf () { tno.DestroySelf(); }
}
