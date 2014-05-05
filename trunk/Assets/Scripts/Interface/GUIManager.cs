//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//http://forum.unity3d.com/threads/88485-2D-Quad-In-Camera-Space
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using TVR.Helpers;

//Script attached to the GUICamera object
public class GUIManager : MonoBehaviour
{
	public ButtonBar mLeftButtonBar;
	public ButtonBarElements mRightButtonBar;

	public BasicButton mEditButton;
	public BasicButton mPlayButton;
	
	GameObject mCurrentCharacter;
	public GameObject CurrentCharacter{
		get{
			return mCurrentCharacter;
		}
		set{
			mCurrentCharacter=value;
			if(mCurrentBackground!=null){
				mEditButton.Enable=true;
			}
		}
	}
	GameObject mCurrentBackground;
	public GameObject CurrentBackground{
		get{
			return mCurrentBackground;
		}
		set{
			mCurrentBackground=value;
			if(mCurrentCharacter!=null){
				mEditButton.Enable=true;
			}
		}
	}

	const float cameraZDepth = 0;
	public int Counter = 0;
	iBlur mBlur;
	public bool blur {
		get {
			return mBlur.Enable;
		}
		set {
			if(value != mBlur.Enable) {
				foreach(BasicButton b in transform.GetComponentsInChildren<BasicButton>()) {
					b.Blur = value;
				}
				mBlur.Enable = value;
			}
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected virtual void Start()
	{
		ButtonProperties.Init();

		SetGUICamera();

		mLeftButtonBar.Show();
		mRightButtonBar.Show();

		InitButtons();

		if(transform.GetComponent<cBlur>().isSupported())
			mBlur = transform.GetComponent<cBlur>();
		else
			mBlur = transform.GetComponent<cBlur2>();
		mBlur.enabled = true;

		if(SceneMgr.Get.sCurrentCharacter!="")
			CurrentCharacter = Instantiate(ResourcesManager.LoadModel("Characters/Prefabs/"+SceneMgr.Get.sCurrentCharacter, "ChapterEditor")) as GameObject;
		if(SceneMgr.Get.sCurrentBackground!="")
			CurrentBackground = Instantiate(ResourcesManager.LoadModel("Backgrounds/Prefabs/"+SceneMgr.Get.sCurrentBackground, "ChapterEditor")) as GameObject;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected virtual void InitButtons()
	{
		//Edit button
		float pos_x = Screen.width-ButtonProperties.buttonBarScaleX-ButtonProperties.buttonMargin-ButtonProperties.buttonSize/2.0f;
		float pos_y = ButtonProperties.buttonMargin+ButtonProperties.buttonSize/2.0f;
		Vector3 pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		Vector3 scale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);

		mEditButton.Init(pos, scale);
		mEditButton.Show();

		//Play button
		pos_x = Screen.width-ButtonProperties.buttonBarScaleX-ButtonProperties.buttonMargin*2-ButtonProperties.buttonSize*1.5f;
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);

		mPlayButton.Init(pos, scale);
		mPlayButton.Show();
		mPlayButton.Enable=false;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public virtual void EnableButtons()
	{
		mLeftButtonBar.EnableButtons();
		mPlayButton.Enable=true;
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public virtual void DisableButtons()
	{
		mLeftButtonBar.DisableButtons();
		mPlayButton.Enable=false;
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
		//mBlur.render();
		if(GUI.Button(new Rect(Screen.width / 2 - 50, 10, 100, 50), "QUIT")) {
			Application.Quit();
		}
		if(GUI.Button(new Rect(Screen.width / 2 - 50, 70, 100, 50), "Blur")) {
			blur = !blur;
		}
	}

	void OnApplicationPause(bool pauseStatus) {
		//Application.Quit();
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	protected void Count(bool bCount)
	{
		if(bCount) Counter++;
		else Counter--;
		mLeftButtonBar.Separator.SetActive(Counter!=0);
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Edit button
	public virtual void OnButtonEditPressed(BasicButton sender)
	{
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Play button
	public virtual void OnButtonPlayPressed(BasicButton sender)
	{
	}
}






