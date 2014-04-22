//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//http://forum.unity3d.com/threads/88485-2D-Quad-In-Camera-Space
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;

//Script attached to the GUICamera object
public class GUIManager : MonoBehaviour
{
	public GameObject mMainButtonBar;
	public GameObject mCharactersButtonBar;
	public GameObject mBackgroundsButtonBar;

	GameObject mMainButtonBarInstance;
	GameObject mCharactersButtonBarInstance;
	GameObject mBackgroundsButtonBarInstance;

	const float cameraZDepth = 0;

	public bool bChildActive;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		SetGUICamera();

		mMainButtonBarInstance = Instantiate(mMainButtonBar) as GameObject;
		mMainButtonBarInstance.transform.parent = transform;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void SetGUICamera()
	{
		transform.position = new Vector3(Screen.width/2.0f, Screen.height/2.0f, cameraZDepth);
		camera.orthographicSize = Screen.height/2.0f;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void OnGUI()
	{
		//This is necessary for the Samsung Galaxy S (Android 2.3)
		//Pressing HOME button freezes the device
		if(GUI.Button(new Rect(Screen.width/2-50, 10, 100, 50), "QUIT")){
			Application.Quit();
		}
	}

	void OnApplicationPause()
	{
		Application.Quit();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonCharactersPressed(ContentType contentType)
	{
		//Debug.Log(contentType + " - characters pressed");

		if(mBackgroundsButtonBarInstance!=null){
			mBackgroundsButtonBarInstance.GetComponent<ButtonBar>().Hide();
		}

		if(mCharactersButtonBarInstance==null){
			mCharactersButtonBarInstance = Instantiate(mCharactersButtonBar) as GameObject;
			mCharactersButtonBarInstance.transform.parent = transform;
			bChildActive=true;
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void OnButtonBackgroundsPressed(ContentType contentType)
	{
		//Debug.Log(contentType + " - backgrounds pressed");

		if(mCharactersButtonBarInstance!=null){
			mCharactersButtonBarInstance.GetComponent<ButtonBar>().Hide();
		}

		if(mBackgroundsButtonBarInstance==null){
			mBackgroundsButtonBarInstance = Instantiate(mBackgroundsButtonBar) as GameObject;
			mBackgroundsButtonBarInstance.transform.parent = transform;
			bChildActive=true;
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void OnButtonMusicPressed(ContentType contentType)
	{
		Debug.Log(contentType + " - music pressed");
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void OnButtonSharePressed(ContentType contentType)
	{
		Debug.Log(contentType + " - share pressed");
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void OnButtonDeletePressed(ContentType contentType)
	{
		Debug.Log(contentType + " - delete pressed");
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonCharPressed(ContentType contentType)
	{
		Debug.Log(contentType + " pressed");
	}
}






