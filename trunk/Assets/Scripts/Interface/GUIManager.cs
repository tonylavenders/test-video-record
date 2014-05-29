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
	public ButtonBar LeftButtonBar;
	public ButtonBarElements RightButtonBar;

	public BasicButton EditButton;
	public BasicButton PlayButton;

	public InputText inputText;
	
	GameObject mCurrentCharacter;
	public GameObject CurrentCharacter{
		get{
			return mCurrentCharacter;
		}set{
			Destroy(mCurrentCharacter);
			mCurrentCharacter=value;
			EditButton.Enable=(mCurrentCharacter!=null && mCurrentBackground!=null);
		}
	}
	GameObject mCurrentBackground;
	public GameObject CurrentBackground{
		get{
			return mCurrentBackground;
		}set{
			Destroy(mCurrentBackground);
			mCurrentBackground=value;
			EditButton.Enable=(mCurrentCharacter!=null && mCurrentBackground!=null);
		}
	}

	const float cameraZDepth = 0;
	public int Counter = 0;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public virtual bool blur {
		get {
			return CameraBlur.Blur.Enable;
		}
		set {
			if(value != CameraBlur.Blur.Enable) {
				/*foreach(BasicButton b in transform.GetComponentsInChildren<BasicButton>()) {
					b.Blur = value;
				}*/
				CameraBlur.Blur.Enable = value;
				//TODO: Desactivar animaciones, sistemas de particulas, ...
			}
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected virtual void Start()
	{
		QueueManager.pauseOnButtonDown = true;
		SetGUICamera();

		if(Data.selChapter!=null && Data.selChapter.IdCharacter!=-1){
			CurrentCharacter = ResourcesLibrary.getCharacter(Data.selChapter.IdCharacter).getInstance("ChapterMgr");
		}
		if(Data.selChapter!=null && Data.selChapter.IdBackground!=-1){
			CurrentBackground = ResourcesLibrary.getBackground(Data.selChapter.IdBackground).getInstance("ChapterMgr");
		}

		LeftButtonBar.Show(false);
		RightButtonBar.Show(true);

		InitButtons();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected virtual void InitButtons()
	{
		//Edit button
		float pos_x = Screen.width-ButtonProperties.buttonBarScaleX-ButtonProperties.buttonMargin-ButtonProperties.buttonSize/2.0f;
		float pos_y = ButtonProperties.buttonMargin+ButtonProperties.buttonSize/2.0f;
		Vector3 pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		Vector3 scale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);

		EditButton.Init(pos, scale);
		EditButton.Show(0, Globals.ANIMATIONDURATION, CurrentCharacter!=null && CurrentBackground!=null);

		//Play button
		pos_x -= ButtonProperties.buttonMargin+ButtonProperties.buttonSize;
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);

		PlayButton.Init(pos, scale);
		PlayButton.Show(0, Globals.ANIMATIONDURATION, Data.selChapter!=null && Data.selChapter.Blocks.Count>0);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public virtual void EnableButtons(ButtonBar.ElementTypes elemType)
	{
		LeftButtonBar.EnableButtons();
		if(elemType==ButtonBar.ElementTypes.blocks){
			PlayButton.Enable=true;
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public virtual void DisableButtons(ButtonBar.ElementTypes elemType)
	{
		LeftButtonBar.DisableButtons();
		if(elemType==ButtonBar.ElementTypes.blocks){
			PlayButton.Enable=false;
		}
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

	protected virtual void Update()
	{
		TVR.Utils.Message.update();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected virtual void OnGUI()
	{
		TVR.Utils.Message.OnGUI();

		//if(GUI.Button(new Rect(Screen.width / 2 - 50, 70, 100, 50), "Blur")) {
		//	blur = !blur;
		//}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected virtual void OnApplicationPause(bool pauseStatus)
	{/*
		if(Application.platform == RuntimePlatform.Android){
			Application.Quit();
		}*/
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected virtual void OnApplicationQuit()
	{
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	protected void Count(bool bCount)
	{
		if(bCount) Counter++;
		else Counter--;

		if(Counter!=0){
			LeftButtonBar.Separator.GetComponent<SeparatorController>().Show();
		}else{
			LeftButtonBar.Separator.GetComponent<SeparatorController>().Hide();
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public virtual void SaveWarning(Data.Chapter.Block previousBlock, BasicButton previousButton)
	{
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Edit button
	public virtual void OnButtonEditClicked(BasicButton sender)
	{
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Play button
	public virtual void OnButtonPlayClicked(BasicButton sender)
	{
	}
}






