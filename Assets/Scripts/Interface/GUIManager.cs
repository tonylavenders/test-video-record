//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//http://forum.unity3d.com/threads/88485-2D-Quad-In-Camera-Space
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using TVR.Helpers;
using TVR;
using TVR.Button;

public class GUIManager : MonoBehaviour
{
	public ButtonBar mLeftButtonBar;
	public ButtonBarElements mRightButtonBar;

	public BasicButton mEditButton;
	public BasicButton mPlayButton;

	public InputText mInput;
	
	GameObject mCurrentCharacter;
	public GameObject CurrentCharacter{
		get{
			return mCurrentCharacter;
		}set{
			Destroy(mCurrentCharacter);
			mCurrentCharacter=value;
			mEditButton.Enable=(mCurrentCharacter!=null && mCurrentBackground!=null);
		}
	}
	GameObject mCurrentBackground;
	public GameObject CurrentBackground{
		get{
			return mCurrentBackground;
		}set{
			Destroy(mCurrentBackground);
			mCurrentBackground=value;
			mEditButton.Enable=(mCurrentCharacter!=null && mCurrentBackground!=null);
		}
	}

	const float cameraZDepth = 0;
	public int Counter = 0;
	iBlur mBlur;
	public virtual bool blur {
		get {
			return mBlur.Enable;
		}
		set {
			if(value != mBlur.Enable) {
				foreach(BasicButton b in transform.GetComponentsInChildren<BasicButton>()) {
					b.Blur = value;
				}
				mBlur.Enable = value;
				//TODO: Desactivar animaciones, sistemas de particulas, ...
			}
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected virtual void Start()
	{
		SetGUICamera();

		mLeftButtonBar.Show();
		mRightButtonBar.Show();

		InitButtons();

		if(transform.GetComponent<cBlur>().isSupported())
			mBlur = transform.GetComponent<cBlur>();
		else
			mBlur = transform.GetComponent<cBlur2>();
		mBlur.enabled = true;

		if(Data.selChapter!=null && Data.selChapter.IdCharacter!=-1){
			CurrentCharacter = ResourcesLibrary.getCharacter(Data.selChapter.IdCharacter).getInstance("ChapterMgr");
		}
		if(Data.selChapter!=null && Data.selChapter.IdBackground!=-1){
			CurrentBackground = ResourcesLibrary.getBackground(Data.selChapter.IdBackground).getInstance("ChapterMgr");
		}
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

		//Play button
		pos_x -= ButtonProperties.buttonMargin+ButtonProperties.buttonSize;
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);

		mPlayButton.Init(pos, scale);
		mPlayButton.Enable = false;
		mPlayButton.Show();
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

	public virtual void HideAllButtonBars()
	{
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void SetGUICamera()
	{
		transform.position = new Vector3(Screen.width/2.0f, Screen.height/2.0f, cameraZDepth);
		camera.orthographicSize = Screen.height/2.0f;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected virtual void Update() {
		TVR.Utils.Message.update();
	}
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected virtual void OnGUI() {
		TVR.Utils.Message.OnGUI();
		//This is necessary for the Samsung Galaxy S (Android 2.3)
		//Pressing HOME button freezes the device
		if(Application.platform == RuntimePlatform.Android) {
			if(GUI.Button(new Rect(Screen.width / 2 - 50, Screen.height - 120, 100, 100), "QUIT")) {
				Application.Quit();
			}
		}
		//if(GUI.Button(new Rect(Screen.width / 2 - 50, 70, 100, 50), "Blur")) {
		//	blur = !blur;
		//}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected virtual void OnApplicationPause(bool pauseStatus)
	{
		if(Application.platform == RuntimePlatform.Android){
			Application.Quit();
		}
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






