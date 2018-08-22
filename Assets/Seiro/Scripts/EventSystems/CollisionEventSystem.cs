using UnityEngine;
using System;
using System.Collections.Generic;

namespace Seiro.Scripts.EventSystems {

	/// <summary>
	/// Event notifications for colliders
	/// </summary>
	public class CollisionEventSystem : MonoBehaviour {

		//Raycast camera
		[SerializeField]
		private Camera camera;

		//Click detection
		[SerializeField]
		private int mouseButton = 0;
		private Collider downCollider;

		private Collider prevCollider;
		private RaycastHit hitInfo;
		private const float EPSILON = 0.001f;
		private Dictionary<Collider, ICollisionEventHandler[]> cache;

		public bool useMouse = true;
		public GameObject pointer;

		#region UnityEvent

		private void Awake() {
			cache = new Dictionary<Collider, ICollisionEventHandler[]>();
		}

		private void Update() {

			Vector2 screenPos = Input.mousePosition;

			// Hover check
			CheckHighlight(screenPos);

			// Click check
			CheckClick();
		}

		#endregion

		#region Function

		/// <summary>
		/// Overlap check
		/// </summary>
		private void CheckHighlight(Vector2 screenPos) {

			Ray ray;

			if (useMouse)
			{
				ray = camera.ScreenPointToRay(screenPos);
			}
			else
			{
				ray = new Ray(pointer.transform.position, transform.forward);
			}

			RaycastHit hit;
			if(Physics.Raycast(ray, out hit, 100f)) {
				//Hit test
				Collider hitCollider = hit.collider;
				hitInfo = hit;
				if(prevCollider != hitCollider) {
					if(prevCollider == null) {
						//Enter
						EnterCollider(hitCollider);
					} else {
						//Exit & Enter
						ExitCollider(prevCollider);
						EnterCollider(hitCollider);
					}
				}
			} else {
				if(prevCollider != null) {
					//Exit
					ExitCollider(prevCollider);
				}
				hitInfo = hit;
			}
		}

		/// <summary>
		/// Click check
		/// </summary>
		private void CheckClick() {
			if(Input.GetMouseButtonUp(mouseButton) || Input.GetKeyUp(KeyCode.Space)) {
				if(downCollider == prevCollider) {
					UpCollider(downCollider);
					ClickCollider(downCollider);
				}
				downCollider = null;
			}
			if(Input.GetMouseButtonDown(mouseButton) || Input.GetKeyDown(KeyCode.Space)) {
				if(prevCollider != null) {
					downCollider = prevCollider;
					DownCollider(downCollider);
				}
			}
		}


		private void EnterCollider(Collider col) {
			ICollisionEventHandler[] handlers = GetHandlers(col);
			if(handlers != null) {
				foreach(var e in handlers) {
					e.OnPointerEnter(hitInfo);
				}
			}
			prevCollider = col;
		}

		private void ExitCollider(Collider col) {
			ICollisionEventHandler[] handlers = GetHandlers(col);
			if(handlers != null) {
				foreach(var e in handlers) {
					e.OnPointerExit(hitInfo);
				}
			}
			prevCollider = null;
		}

		private void DownCollider(Collider col) {
			ICollisionEventHandler[] handlers = GetHandlers(col);
			if(handlers != null) {
				foreach(var e in handlers) {
					e.OnPointerDown(hitInfo);
				}
			}
		}

		private void UpCollider(Collider col) {
			ICollisionEventHandler[] handlers = GetHandlers(col);
			if(handlers != null) {
				foreach(var e in handlers) {
					e.OnPointerUp(hitInfo);
				}
			}
		}

		private void ClickCollider(Collider col) {
			ICollisionEventHandler[] handlers = GetHandlers(downCollider);
			if(handlers != null) {
				foreach(var e in handlers) {
					e.OnPointerClick(hitInfo);
				}
			}
		}

		private ICollisionEventHandler[] GetHandlers(Collider col) {
			if(col == null) return null;
			if(cache.ContainsKey(col)) {
				return cache[col];
			}
			ICollisionEventHandler[] handlers = col.GetComponents<ICollisionEventHandler>();
			if(handlers != null) {
				cache.Add(col, handlers);
			}
			return handlers;
		}

		#endregion
	}
}