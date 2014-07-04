using UnityEngine;
using System.Collections;

//We adjust the camera rect to panoramic 16/9 format with Letterbox or Pillarbox
using TVR.Helpers;

public class LetterboxManager
{
	static float targetRatio = 1.777f; //16/9
	static float displayRatio;

	static float camHpix;
	static float camHpercent;
	static float letterboxPercent;
	static float letterboxPix;

	static float camWpix;
	static float camWpercent;
	static float pillarboxPercent;
	static float pillarboxPix;

	static Texture texBlackPixel;

	static Transform Letterbox_down;
	static Transform Letterbox_up;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static void Start()
	{
		texBlackPixel = (Texture)ResourcesManager.LoadResource("Interface/Textures/black_pixel", "Scene");
		Letterbox_down = GameObject.Find("Letterbox_down").transform;
		Letterbox_up = GameObject.Find("Letterbox_up").transform;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static void Init()
	{
		displayRatio = Screen.width/Screen.height;

		//letterboxing
		if(displayRatio<targetRatio){
			camHpix = Screen.width/targetRatio;
			camHpercent = camHpix/Screen.height;
			letterboxPercent = (1-camHpercent)/2.0f;
			letterboxPix = letterboxPercent*Screen.height;
		}
		//pillarboxing
		else{
			camWpix = Screen.height*targetRatio;
			camWpercent = camWpix/Screen.width;
			pillarboxPercent = (1-camWpercent)/2.0f;
			pillarboxPix = pillarboxPercent*Screen.width;
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public static void InitQuads()
	{
		Letterbox_down.position = new Vector3(Screen.width/2.0f, letterboxPix/2.0f, 20);
		Letterbox_down.localScale = new Vector3(Screen.width,letterboxPix,0);
		
		Letterbox_up.position = new Vector3(Screen.width/2.0f, Screen.height-letterboxPix/2.0f, 20);
		Letterbox_up.localScale = Letterbox_down.localScale;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static Rect GetRectPercent()
	{
		float x, y, h, w;

		//letterboxing
		if(displayRatio<targetRatio){
			x = 0;
			y = letterboxPercent;
			w = Screen.width;
			h = camHpercent;
		}
		//pillarboxing
		else{
			x = pillarboxPercent;
			y = 0;
			w = camWpercent;
			h = 1;
		}

		if(x<0.01f)
			x=0.0f;
		if(y<0.01f)
			y=0.0f;
		if(w>0.99f)
			w=1.0f;
		if(h>0.99f)
			h=1.0f;

		return new Rect(x,y,w,h);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	// No se usa pq los controles GUI son elementos 3D y el OnGUI se pintaria encima
	public static void OnGUI()
	{
		//if(Scene_Main.getEditorInterface==null || Scene_Main.getEditorInterface.buttonEyeCamera.Checked)
		{
			//Add 2 pixels to avoid artifacts
			//letterboxing
			if(displayRatio<targetRatio){
				if(letterboxPix>2){
					GUI.DrawTexture(new Rect(0,0,Screen.width,letterboxPix+2), texBlackPixel);
					GUI.DrawTexture(new Rect(0,letterboxPix+camHpix-2,Screen.width,letterboxPix+2), texBlackPixel);
				}
			}
			//pillarboxing
			else{
				if(pillarboxPix>2){
					GUI.DrawTexture(new Rect(0,0,pillarboxPix+2,Screen.height), texBlackPixel);
					GUI.DrawTexture(new Rect(pillarboxPix+camWpix-2, 0, pillarboxPix+2, Screen.height), texBlackPixel);
				}
			}
		}
	}
}



