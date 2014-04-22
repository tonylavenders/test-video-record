using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour {
	// Update is called once per frame
	private bool click;

	void Start() {
		click = false;
	}

	void Update() {
		if(TVR.Helpers.InputHelp.GetMouseButtonUp(0) && ! click) {
			SceneMgr.Get.SwitchTo("ChapterMgr");
			click = true;
		}
	}
}