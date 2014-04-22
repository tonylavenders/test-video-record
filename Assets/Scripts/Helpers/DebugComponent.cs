using UnityEngine;
using System.Collections;

public class DebugComponent : MonoBehaviour
{
	public const string FRAMESSECOND = "#FRAMES#";
	public const string DEFAULTMESSAGE = "Development Build - FPS: " + FRAMESSECOND;
	private const int RANGE = 10;
	private GUIStyle mStyle;
	private float mSeconds;
	private float mFrames;
	private int mCounter;
	
	public static string Message;

	// Use this for initialization
	void Start() {
		mStyle=new GUIStyle();
//		mStyle.font=(Font)BRB.Helpers.ResourcesManager.LoadResource ("Fonts/arial", "Debug");
		mStyle.clipping=TextClipping.Clip;
		mStyle.wordWrap=true;
		mStyle.fontSize=12;
		mStyle.alignment=TextAnchor.LowerRight;
		mStyle.fontStyle=FontStyle.Normal;
		mStyle.normal.textColor=Color.black;
		
		Message=DEFAULTMESSAGE;
		mSeconds=0;
		mCounter=0;
	}
	
	// Update is called once per frame
	void Update() {
		mSeconds+=Time.deltaTime;
		mCounter++;
		if(mCounter==RANGE) {
			mFrames=RANGE/mSeconds;
			mCounter=0;
			mSeconds=0;
		}
	}
	
	void OnGUI() {
		if (Event.current.type != EventType.Repaint)
			return;
		
		string mes = Message;
		mes=mes.Replace(FRAMESSECOND, mFrames.ToString ("f2"));
		
		//GUI.depth = -1000;
		
		mStyle.normal.textColor=new Color(0,0,0,1.0f);
		GUI.Label(new Rect(0,0,Screen.width-3,Screen.height-3), mes, mStyle);
		mStyle.normal.textColor=Color.white;
		GUI.Label (new Rect(0,0,Screen.width-4,Screen.height-4),mes,mStyle);
	}
	
	void OnDestroy() {
		//BRB.Helpers.ResourcesManager.UnloadScene("Debug");
	}
}
