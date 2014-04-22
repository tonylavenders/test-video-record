using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using TVR;
using TVR.Utils;

public class SceneMgr : MonoBehaviour  {
	enum States {
		NONE,
		IN,
		OUT
	}
	public delegate void GenericDelegate();
	public GenericDelegate OnFinished;
	
	States mState;
	
	string mSceneCurrent;
	string mSceneNext;
	
	static SceneMgr mInstance = null;
	
	private Texture mBlack;
	
	Rect mScreenRect;
	
	Color mOriginalColor;		
	Color mCurrentColor;
	
	SmoothStep mAlpha;
	
	AsyncOperation mAsync;
	
	public static SceneMgr Get() {
		return mInstance;
	}
	
	void Init() {
		Application.targetFrameRate = 60;
		//BRB.Utils.Message.Init(BRBRec.Globals.ANIMATIONDURATION);
		//BRBRec.ResourcesLibrary.Init();
		Data.Init();
		
		mSceneCurrent = "Menus";
		Application.LoadLevel(mSceneCurrent);
		
		mState = States.OUT;
		mBlack = (Texture)TVR.Helpers.ResourcesManager.LoadResource("Shared/black_pixel", "SceneMgr");
	}
	
	void Awake () {
		DontDestroyOnLoad(this);
		mInstance = this;
		mOriginalColor = new Color(1.0f, 1.0f, 1.0f, 1.0f);
		mCurrentColor = new Color(1.0f, 1.0f, 1.0f, 0.0f);
		mScreenRect = new Rect(0.0f, 0.0f, Screen.width, Screen.height);
		mState = States.NONE;
		mAlpha = new SmoothStep(0f, 1f, Globals.ANIMATIONDURATION, false);
		Init();
	}
	
	bool SetScene(string sNewScene) {
		if(mAsync.progress >= 0.9f) {
			mAsync.allowSceneActivation = true;
			mSceneCurrent = sNewScene;
			return true;
		} else
			return false;
	}
	
	public bool IsFadeFinished() {
		return (mState == States.NONE);
	}

	public void SwitchTo(string sNewScene) {
		if (mSceneCurrent == sNewScene )
			return;
		
		mSceneNext = sNewScene;
		mAsync = Application.LoadLevelAsync(mSceneNext);
		mAsync.allowSceneActivation=false;
		
		mState = States.IN;			
		mAlpha.Reset(1, Globals.ANIMATIONDURATION);
	}
	
	void Update () {
		if(mState == States.NONE)
			return;

		mAlpha.Update();
		switch(mState) {
		case States.IN:
			if(mAlpha.Ended) {
				if(SetScene(mSceneNext)) {
					mState = States.OUT;
				}
			}
			break;
		case States.OUT:
			/*if(mSceneCurrent == "Player" || mSceneCurrent == "Export")
				mAlpha.End();*/
		
			if(mAlpha.Ended) {
				mState = States.NONE;
			
				if(OnFinished != null) {
					OnFinished();
					OnFinished = null;
				}
			}					
			break;				
		}
		mCurrentColor.a = mAlpha.Value;
	}
	
	void OnGUI () {
		if(mState == States.NONE || Event.current.type != EventType.Repaint)
			return;

		GUI.depth = -1000;
		GUI.color = mCurrentColor;
		GUI.DrawTexture(mScreenRect, mBlack, ScaleMode.StretchToFill, false); 
		GUI.color = mOriginalColor;
	}
	
	void OnDestroy() {
		//BRB.Helpers.ResourcesManager.UnloadScene("Menus");
		TVR.Helpers.ResourcesManager.UnloadScene("SceneMgr");
		Data.closeDB();
	}
	
	void OnApplicationPause(bool pauseStatus) {

	}
}