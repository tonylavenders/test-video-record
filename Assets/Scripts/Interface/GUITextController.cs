using UnityEngine;
using System.Collections;
using TVR;

public class GUITextController : MonoBehaviour
{
	Transform mParent;
	int font_size;

	float startTime;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		startTime=Time.time;

		mParent = transform.parent;

		//button.scale=90 ==> font_size=26
		guiText.fontSize = (int)(mParent.lossyScale.x*(26.0f/90.0f));
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void Update()
	{
		//fade_in
		float t = (Time.time - startTime) / Globals.ANIMATIONDURATION;
		Color c = guiText.color;
		guiText.color = new Color(c.r, c.g, c.b, Mathf.SmoothStep(0.0f, 1.0f, t));

		//position
		float pos_x = mParent.position.x/Screen.width;
		float pos_y = mParent.position.y/Screen.height;
		guiText.transform.position = new Vector3(pos_x, pos_y, 0.0f);
	}
}
