﻿using UnityEngine;
using System.Collections;
using TVR;

public enum ButtonType{
	MAIN_CHARACTERS,
	MAIN_BACKGROUNDS,
	MAIN_MUSIC,
	MAIN_SHARE,
	MAIN_DELETE,
	CHAR,
	BACKGROUND
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public enum ContentType{
	LIB,
	CHAR_01, CHAR_02, CHAR_03, CHAR_04, CHAR_05, CHAR_06, CHAR_07, CHAR_08, CHAR_09,
	BACKGROUND_01,
	BACKGROUND_02,
	BACKGROUND_03
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class BasicButton : MonoBehaviour
{
	public Texture texChecked;
	public Texture texUnchecked;
	public bool bKeepSt;
	public bool bChecked;
	public ButtonType buttonType;
	public ContentType contentType;

	public delegate void ButtonCallback(ContentType contentType);
	public ButtonCallback buttonCallback;

	int mID;
	ButtonBar mButtonBar;
	GUIManager mGUIManager;

	float startTime;

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		startTime=Time.time;

		renderer.sharedMaterial.mainTexture = texUnchecked;
		mButtonBar = transform.parent.GetComponent<ButtonBar>();
		mGUIManager = mButtonBar.mGUIManager;
		SetCallback();
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void Update()
	{
		//fade_in
		float t = (Time.time - startTime) / Globals.ANIMATIONDURATION;
		Color c = renderer.material.color;
		renderer.material.color = new Color(c.r, c.g, c.b, Mathf.SmoothStep(0.0f, 1.0f, t));

		//Check if user is touching the button
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if(Input.GetMouseButtonUp(0) && collider.Raycast(ray, out hit, 1000.0f)){
			if(!bChecked){
				renderer.sharedMaterial.mainTexture = texChecked;
				bChecked=true;
				mButtonBar.ButtonPressed(mID);
				if(buttonCallback!=null)
					buttonCallback(contentType);
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SetID(int _id)
	{
		mID=_id;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void UnCheck(int current_id)
	{
		if(current_id==mID)
			return;

		if(bChecked){
			bChecked=false;
			renderer.sharedMaterial.mainTexture = texUnchecked;
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void SetCallback()
	{
		if(buttonType==ButtonType.MAIN_CHARACTERS){
			buttonCallback = mGUIManager.OnButtonCharactersPressed;
		}
		else if(buttonType==ButtonType.MAIN_BACKGROUNDS){
			buttonCallback = mGUIManager.OnButtonBackgroundsPressed;
		}
		else if(buttonType==ButtonType.MAIN_MUSIC){
			buttonCallback = mGUIManager.OnButtonMusicPressed;
		}
		else if(buttonType==ButtonType.MAIN_SHARE){
			buttonCallback = mGUIManager.OnButtonSharePressed;
		}
		else if(buttonType==ButtonType.MAIN_DELETE){
			buttonCallback = mGUIManager.OnButtonDeletePressed;
		}
		else if(buttonType==ButtonType.CHAR){
			buttonCallback = mGUIManager.OnButtonCharPressed;
		}
	}
}





