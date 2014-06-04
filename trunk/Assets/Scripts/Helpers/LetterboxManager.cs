using UnityEngine;
using System.Collections;

//Lower GUI size is 115pix, so we have Screen.height-115 for camera view
//We adjust the camera rect to panoramic 16/9 format with Letterbox or Pillarbox
using TVR.Helpers;

public class LetterboxManager
{
	static float targetRatio = 1.777f; //16/9
	static float displayRatio;
	static float realH;

	static float GUIpercent;
	static float guiHpix;

	static float camHpix;
	static float camHpercent;
	static float letterboxPercent;
	static float letterboxPix;

	static float camWpix;
	static float camWpercent;
	static float pillarboxPercent;
	static float pillarboxPix;

	static Texture texBlackPixel;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static void Start()
	{
		texBlackPixel = (Texture)ResourcesManager.LoadResource("Interface/Textures/black_pixel", "Scene");
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static void Init(float guiH=0)
	{
		guiHpix = guiH;
		GUIpercent = guiH/Screen.height;
		realH = Screen.height-guiH;
		displayRatio = Screen.width/realH;

		//letterboxing
		if(displayRatio<targetRatio){
			camHpix = Screen.width/targetRatio;
			camHpercent = camHpix/Screen.height;
			letterboxPercent = (1-camHpercent-GUIpercent)/2.0f;
			letterboxPix = letterboxPercent*Screen.height;
		}
		//pillarboxing
		else{
			camWpix = realH*targetRatio;
			camWpercent = camWpix/Screen.width;
			pillarboxPercent = (1-camWpercent)/2.0f;
			pillarboxPix = pillarboxPercent*Screen.width;
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public static Rect GetRectPercent()
	{
		float x, y, h, w;

		//letterboxing
		if(displayRatio<targetRatio){
			x = 0;
			y = GUIpercent+letterboxPercent;
			w = Screen.width;
			h = camHpercent;
		}
		//pillarboxing
		else{
			x = pillarboxPercent;
			y = GUIpercent;
			w = camWpercent;
			h = 1 - GUIpercent;
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

	public static Rect GetPixelInset()
	{
		if(displayRatio<targetRatio){
			return new Rect(-Screen.width/2,-camHpix/2,Screen.width,camHpix);
		}
		else{
			return new Rect(-camWpix/2,-realH/2,camWpix,realH);
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
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
					GUI.DrawTexture(new Rect(0,0,pillarboxPix+2,Screen.height-guiHpix), texBlackPixel);
					GUI.DrawTexture(new Rect(pillarboxPix+camWpix-2, 0, pillarboxPix+2, Screen.height-guiHpix), texBlackPixel);
				}
			}
		}
	}
}



