using UnityEngine;
using System.Collections;
using TVR.Helpers;

public class InputHelp_Updater : MonoBehaviour {
	// Update is called once per frame
	void Update() {
		InputHelp.Update();
		//InputBRBMulti.Update();
	}

	/*void LateUpdate() {
		if(InputBRB.GetMouseButtonUp(0)) {
			BRB.Button.BasicButton.mouseUp();
			//System.GC.Collect();
		}
	}*/
}