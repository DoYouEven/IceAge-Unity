//------------------------------------------
//            Tasharen Network
// Copyright © 2012-2014 Tasharen Entertainment
//------------------------------------------

using UnityEngine;
using TNet;

/// <summary>
/// Very simple event manager script that sends out basic touch and mouse-based notifications using NGUI's syntax.
/// </summary>

[RequireComponent(typeof(Camera))]
public class TouchHandler : MonoBehaviour
{
	static public Vector3 worldPos;
	static public Vector2 screenPos;

	public LayerMask eventReceiverMask = -1;

	Camera mCam;
	GameObject mGo;

	void Awake () { mCam = camera; }

	/// <summary>
	/// Update the touch and mouse position and send out appropriate events.
	/// </summary>

	void Update ()
	{
		// Touch notifications
		if (Input.touchCount > 0)
		{
			Touch touch = Input.GetTouch(0);

			if (touch.phase == TouchPhase.Began)
			{
				screenPos = touch.position;
				SendPress(touch.position);
			}
			else if (touch.phase == TouchPhase.Moved)
			{
				SendDrag(touch.position);
			}
			else if (touch.phase != TouchPhase.Stationary)
			{
				SendRelease(touch.position);
			}
		}
		else
		{
			// Mouse notifications
			if (Input.GetMouseButtonDown(0))
			{
				screenPos = Input.mousePosition;
				SendPress(Input.mousePosition);
			}
			if (Input.GetMouseButtonUp(0)) SendRelease(Input.mousePosition);
			if (mGo != null && Input.GetMouseButton(0)) SendDrag(Input.mousePosition);
		}
	}

	/// <summary>
	/// Send out a press notification.
	/// </summary>

	void SendPress (Vector2 pos)
	{
		worldPos = pos;
		mGo = Raycast(pos);
		if (mGo != null) mGo.SendMessage("OnPress", true, SendMessageOptions.DontRequireReceiver);
	}

	/// <summary>
	/// Send out a release notification.
	/// </summary>

	void SendRelease (Vector2 pos)
	{
		worldPos = pos;

		if (mGo != null)
		{
			GameObject go = Raycast(pos);
			if (mGo == go) mGo.SendMessage("OnClick", SendMessageOptions.DontRequireReceiver);
			mGo.SendMessage("OnPress", false, SendMessageOptions.DontRequireReceiver);
			mGo = null;
		}
	}

	/// <summary>
	/// Send out a drag notification.
	/// </summary>

	void SendDrag (Vector2 pos)
	{
		Vector2 delta = pos - screenPos;

		if (delta.sqrMagnitude > 0.001f)
		{
			Raycast(pos);
			mGo.SendMessage("OnDrag", delta, SendMessageOptions.DontRequireReceiver);
			screenPos = pos;
		}
	}

	/// <summary>
	/// Helper function that raycasts into the screen to determine what's underneath the specified position.
	/// </summary>

	GameObject Raycast (Vector2 pos)
	{
		RaycastHit hit;

		if (Physics.Raycast(mCam.ScreenPointToRay(pos), out hit, 300f, eventReceiverMask))
		{
			worldPos = hit.point;
			return hit.collider.gameObject;
		}
		return null;
	}
}