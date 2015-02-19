//------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//------------------------------------------

using UnityEngine;

/// <summary>
/// This script shows how to destroy objects dynamically over the network.
/// The same Destroy call will work perfectly fine even if you're not currently connected.
/// This script is attached to the "Created Cube" prefab instantiated by ExampleCreate script in Example 3.
/// 
/// Destroying the object that has a TNObject script attached will automatically destroy this
/// object on all connected clients and will remove all the RFCs associated with this object.
/// </summary>

public class ExampleDestroy : MonoBehaviour
{
	float mDestroyTime = 0f;

	void Awake ()
	{
		mDestroyTime = Time.time + 5f;
	}

	void Update ()
	{
		if (mDestroyTime < Time.time)
		{
			TNManager.Destroy(gameObject);
		}
	}

	void OnClick ()
	{
		TNManager.Destroy(gameObject);
	}
}