using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {
	// Update is called once per frame
	private bool click;

	void Start() {
		click = false;
	}

	void Update() {
		if(TVR.Helpers.InputHelp.GetMouseButtonUp(0) && !click) {
			SceneMgr.Get.SwitchTo("ChapterMgr");
			click = true;
		}
	}

	void OnGUI() {
		GUI.Label(new Rect(Screen.width/2, Screen.height/2, 200, 50), "Touch the screen");
	}
}