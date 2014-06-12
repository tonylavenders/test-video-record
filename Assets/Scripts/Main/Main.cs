using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour
{
	private bool click;
	public Texture mCover;
	const int w=700;
	const int h=400;
	const float delay=3;
	float mTime=0;

	void Start() 
	{
		click = false;
	}

	void Update()
	{
		mTime+=Time.deltaTime;
		//if(TVR.Helpers.InputHelp.GetMouseButtonUp(0) && !click) {
		if(mTime>delay && !click) {
			SceneMgr.Get.SwitchTo("ChapterMgr");
			click = true;
		}
	}

	void OnGUI()
	{
		//GUIStyle style = new GUIStyle(GUI.skin.label);
		//style.alignment = TextAnchor.MiddleCenter;
		//GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "Touch the screen", style);
		if(!click){
			GUI.DrawTexture(new Rect(Screen.width/2.0f-w/2.0f, Screen.height/2.0f-h/2.0f, w, h), mCover);
		}
	}
}