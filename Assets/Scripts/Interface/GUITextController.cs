﻿using UnityEngine;
using System.Collections;
using TVR;
using TVR.Utils;

public class GUITextController : MonoBehaviour
{
	Transform mParent;
	//int font_size;

	SmoothStep mFade;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Awake()
	{
		mFade = new SmoothStep(0.0f,1.0f,1.0f,false);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		mParent = transform.parent;
		guiText.fontSize = (int)(mParent.lossyScale.x*(26.0f/90.0f)); //para button.scale=90 ==> font_size=26
		Color c = guiText.color;
		guiText.color = new Color(c.r, c.g, c.b, mFade.Value);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void Update()
	{
		SmoothStep.State state = mFade.Update();
		if(state == SmoothStep.State.inFade || state == SmoothStep.State.justEnd) {
			Color c = guiText.color;
			guiText.color = new Color(c.r, c.g, c.b, mFade.Value);
		}

		//position
		float pos_x = mParent.position.x/Screen.width;
		float pos_y = mParent.position.y/Screen.height;
		guiText.transform.position = new Vector3(pos_x, pos_y, 0.0f);
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Show(float delay = 0, float duration = Globals.ANIMATIONDURATION)
	{
		mFade.Reset(1.0f, duration, true, delay);
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Hide(float delay = 0, float duration = Globals.ANIMATIONDURATION)
	{
		mFade.Reset(0.0f, duration, true, delay);
	}
}


