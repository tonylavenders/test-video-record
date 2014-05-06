using UnityEngine;
using System.Collections;
using TVR.Helpers;

public class Updater : MonoBehaviour {
	// Update is called once per frame
	void Update() {
		InputHelp.Update();
		//InputBRBMulti.Update();
	}

	void LateUpdate() {
		if(InputHelp.GetMouseButtonUp(0)) {
			TVR.Button.BasicButton.mouseUp();
			//System.GC.Collect();
		}
	}
}