//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//http://forum.unity3d.com/threads/88485-2D-Quad-In-Camera-Space
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using TVR.Helpers;

//Script attached to the GUICamera object
public class GUIManager : MonoBehaviour
{
	public ButtonBar mMainButtonBar;
	public ButtonBar mCharactersButtonBar;
	public ButtonBar mBackgroundsButtonBar;
	public ButtonBarChapters mChaptersButtonBar;

	public BasicButton mEditButton;
	public BasicButton mPlayButton;

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

	void Start()
	{
		ButtonProperties.Init();

		SetGUICamera();

		mMainButtonBar.Show();
		mChaptersButtonBar.Show();

		InitButtons();

		/*if(GameObject.Find("CameraMain").GetComponent<cBlur>().isSupported())
			mBlur = GameObject.Find("CameraMain").GetComponent<cBlur>();
		else
			mBlur = GameObject.Find("CameraMain").GetComponent<cBlur2>();*/
		if(transform.GetComponent<cBlur>().isSupported())
			mBlur = transform.GetComponent<cBlur>();
		else
			mBlur = transform.GetComponent<cBlur2>();
		mBlur.enabled = true;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void InitButtons()
	{
		//Edit button
		float pos_x = Screen.width-ButtonProperties.buttonBarScaleX-ButtonProperties.buttonMargin-ButtonProperties.buttonSize/2.0f;
		float pos_y = ButtonProperties.buttonMargin+ButtonProperties.buttonSize/2.0f;
		Vector3 pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		Vector3 scale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);

		mEditButton.Init(pos, scale);
		mEditButton.Enable=false;
		mEditButton.Show();

		//Play button
		pos_x = Screen.width-ButtonProperties.buttonBarScaleX-ButtonProperties.buttonMargin*2-ButtonProperties.buttonSize*1.5f;
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);

		mPlayButton.Init(pos, scale);
		mPlayButton.Enable=false;
		mPlayButton.Show();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void EnableButtons()
	{
		mMainButtonBar.EnableButtons();
		mEditButton.Enable=true;
		mPlayButton.Enable=true;
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void DisableButtons()
	{
		mMainButtonBar.DisableButtons();
		mEditButton.Enable=false;
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

	void OnApplicationPause() {
		Application.Quit();
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void Count(bool bCount)
	{
		if(bCount) Counter++;
		else Counter--;
		mMainButtonBar.Separator.SetActive(Counter!=0);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Characters button bar
	public void OnButtonCharactersPressed(BasicButton sender)
	{
		if(sender.Checked) mCharactersButtonBar.Show();
		else mCharactersButtonBar.Hide();
		Count(sender.Checked);
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Backgrounds button bar
	public void OnButtonBackgroundsPressed(BasicButton sender)
	{
		if(sender.Checked) mBackgroundsButtonBar.Show();
		else mBackgroundsButtonBar.Hide();
		Count(sender.Checked);
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Music button bar
	public void OnButtonMusicPressed(BasicButton sender)
	{
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Share
	public void OnButtonSharePressed(BasicButton sender)
	{
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Character button
	public void OnButtonCharacterPressed(BasicButton sender)
	{
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Background button
	public void OnButtonBackgroundPressed(BasicButton sender)
	{
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Edit button
	public void OnButtonEditPressed(BasicButton sender)
	{
		Debug.Log("edit");
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Play button
	public void OnButtonPlayPressed(BasicButton sender)
	{
		Debug.Log("play");
	}
}






