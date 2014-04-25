﻿//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//http://forum.unity3d.com/threads/88485-2D-Quad-In-Camera-Space
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using TVR.Helpers;

//Script attached to the GUICamera object
public class GUIManager : MonoBehaviour
{
	public GameObject mMainButtonBar;
	public GameObject mCharactersButtonBar;
	public GameObject mBackgroundsButtonBar;
	public GameObject mChaptersButtonBar;

	const float cameraZDepth = 0;
	public int Counter = 0;
	cBlur mBlur;
	[ExposeProperty]
	public bool blur {
		get {
			return mBlur.enabled;
		}
		set {
			if(value != mBlur.enabled) {
				mBlur.enabled = value;
				foreach(BasicButton b in transform.GetComponentsInChildren<BasicButton>()) {
					b.Blur = value;
				}
				if(value) {
					Camera[] c = new Camera[2];
					c[0] = GameObject.Find("CameraMain").GetComponent<Camera>();
					c[1] = transform.GetComponent<Camera>();
					mBlur.proccess(c);
				} else {
					mBlur.clear();
				}
			}
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		SetGUICamera();
		mMainButtonBar.GetComponent<ButtonBar>().Show();
		mChaptersButtonBar.GetComponent<ButtonBar>().Show();
		mBlur = transform.GetComponent<cBlur>();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void SetGUICamera()
	{
		transform.position = new Vector3(Screen.width/2.0f, Screen.height/2.0f, cameraZDepth);
		camera.orthographicSize = Screen.height/2.0f;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void OnGUI() {

		//This is necessary for the Samsung Galaxy S (Android 2.3)
		//Pressing HOME button freezes the device
		mBlur.render();
		if(GUI.Button(new Rect(Screen.width / 2 - 50, 10, 100, 50), "QUIT")) {
			Application.Quit();
		}
		if(GUI.Button(new Rect(Screen.width / 2 - 50, 70, 100, 50), "Blur")) {
			blur = !blur;
		}
	}

	void OnApplicationPause() {
		Application.Quit();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonCharactersPressed(BasicButton sender)
	{
		if(sender.Checked)
			mCharactersButtonBar.GetComponent<ButtonBar>().Show();
		else
			mCharactersButtonBar.GetComponent<ButtonBar>().Hide();

		Count(sender.Checked);
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void OnButtonBackgroundsPressed(BasicButton sender)
	{
		if(sender.Checked)
			mBackgroundsButtonBar.GetComponent<ButtonBar>().Show();
		else
			mBackgroundsButtonBar.GetComponent<ButtonBar>().Hide();

		Count (sender.Checked);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Count(bool bCount)
	{
		if(bCount)
			Counter++;
		else
			Counter--;

		mMainButtonBar.GetComponent<ButtonBar>().Separator.SetActive(Counter!=0);
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void OnButtonMusicPressed(BasicButton sender)
	{
		//Debug.Log(contentType + " - music pressed");
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void OnButtonSharePressed(BasicButton sender)
	{
		//Debug.Log(contentType + " - share pressed");
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void OnButtonDeletePressed(BasicButton sender)
	{
		//Debug.Log(contentType + " - delete pressed");
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonCharacterPressed(BasicButton sender)
	{
		//Debug.Log(contentType + " pressed");
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void OnButtonBackgroundPressed(BasicButton sender)
	{
		//Debug.Log(contentType + " pressed");
	}
}






