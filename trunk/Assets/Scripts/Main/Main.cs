using UnityEngine;
using System.Collections;

public class Main : MonoBehaviour
{
	public Texture texLogo;
	public GUIStyle mButtonStyle;
	Rect rectTexLogo;
	Rect rectButton;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Start() 
	{
		//Calculate size and position for background textures and progress bar
		float originalW = 700;
		float originalH = 500;
		float ratio = originalW/originalH;
		float targetW = Screen.width*0.6f;
		float targetH = targetW/ratio;
		float x = (Screen.width-targetW)/2.0f;
		float y = (Screen.height-targetH)*0.1f;
		
		rectTexLogo = new Rect(x,y,targetW,targetH);

		float size = Screen.width*0.1f;
		float x_button = (Screen.width-size)/2.0f;
		float y_button = Screen.height*0.8f;

		rectButton = new Rect(x_button,y_button,size,size);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Update()
	{
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void OnGUI()
	{
		GUI.DrawTexture(rectTexLogo, texLogo);

		if(GUI.Button(rectButton, "", mButtonStyle)){
			SceneMgr.Get.SwitchTo("ChapterMgr");
		}
	}
}