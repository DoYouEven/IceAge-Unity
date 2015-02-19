using UnityEngine;
using UnityEditor;
using System.Collections;

namespace RootMotion.FinalIK {

	// Custom scene view helpers for the InteractionTrigger
	[CustomEditor(typeof(InteractionTrigger))]
	public class InteractionTriggerInspector : Editor {

		private InteractionTrigger script { get { return target as InteractionTrigger; }}

		void OnSceneGUI() {
			if (!Application.isPlaying) {
				Quaternion q = Quaternion.FromToRotation(script.transform.up, Vector3.up);
				script.transform.rotation = q * script.transform.rotation;
			}

			if (script.target == null) {
				if (Application.isPlaying) Warning.Log("InteractionTrigger has no target Transform.", script.transform);

				return;
			}

			if (script.collider != null) script.collider.isTrigger = true;
			else {
				Warning.Log("InteractionTrigger requires a Collider component.", script.transform, true);
				return;
			}

			for (int i = 0; i < script.ranges.Length; i++) {
				DrawRange(script.ranges[i], i);
			}
		}

		private void DrawRange(InteractionTrigger.Range range, int index) {
			Color color = GetColor(index);
			Handles.color = color;
			GUI.color = color;

			Vector3 position = script.transform.position + script.transform.rotation * range.positionOffset;
			Vector3 direction = script.target.position - position;
			direction.y = 0f;

			range.maxDistance = Mathf.Clamp(range.maxDistance, 0f, range.maxDistance);

			bool noDirection = direction == Vector3.zero;
			if (noDirection) {
				range.angleOffset = 0f;
				range.maxAngle = 180f;
			}

			Quaternion rotation = noDirection? Quaternion.identity: Quaternion.LookRotation(direction);

			Vector3 up = rotation * Vector3.up;
			Vector3 forward = rotation * Vector3.forward;

			Handles.DrawWireDisc(position, up, range.maxDistance);

			if (range.orbit) {
				float mag = range.positionOffset.magnitude;

				if (mag - range.maxDistance > 0f) Handles.DrawWireDisc(script.transform.position, up, mag - range.maxDistance);
				Handles.DrawWireDisc(script.transform.position, up, mag + range.maxDistance);
			}

			Vector3 x = Quaternion.AngleAxis(range.angleOffset, up) * forward * range.maxDistance;
			Quaternion q = Quaternion.AngleAxis(-range.maxAngle, up);
			
			Vector3 dir = q * x;

			if (!noDirection && range.maxAngle < 180f) {
				Handles.DrawLine(position, position + x);
				Handles.DotCap(0, position + x, Quaternion.identity, range.maxDistance * 0.01f);
			}

			string name = range.interactions.Length > 0 && range.interactions[0].effectors.Length > 0? " (" + range.interactions[0].effectors[0].ToString() + ")": string.Empty;

			GUI.color = color;
			Handles.Label(position - up * index * 0.05f, "Character Position for Range " + index.ToString() + name);

			color.a = 0.3f;
			Handles.color = color;

			Handles.DrawSolidArc(position, up, dir, range.maxAngle * 2f, range.maxDistance);

			Handles.color = Color.white;
			GUI.color = Color.white;
		}

		private static Color GetColor(int index) {
			float i = (float)index + 1f;
			return new Color(1f / i, i * 0.1f, (i * i) + 0.1f);
		}
	}
}
