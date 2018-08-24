using UnityEngine;
using System;

public class RadialMenuController : MonoBehaviour {

	public RadialMenu radialMenu;

	#region UnityEvent

	private void Start() {
		radialMenu.AddClickCallback("Exit.OK", OnCLickedExitOK);
	}

	private void Update() {
		if(Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.V) ) {
			if(radialMenu.Visibled) {
				radialMenu.Hide();
			} else {
				radialMenu.Visible();
			}
		}
	}

	#endregion

	#region Callback

	private void OnCLickedExitOK(GameObject gObj) {
		Debug.Log("Exit OK!");
	}

	#endregion
}