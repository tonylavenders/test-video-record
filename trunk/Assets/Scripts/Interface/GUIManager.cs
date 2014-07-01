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
	public BasicButton HelpButton;

	public InputText inputText;
	public bool bShowHelp;

	public Texture texHelp01;
	public Texture texHelp02;
	float texHelpW;
	float texHelpH;
	float texHelpY01;
	float texHelpY02;
	
	GameObject mCurrentCharacter;
	public GameObject CurrentCharacter{
		get{
			return mCurrentCharacter;
		}set{
			Destroy(mCurrentCharacter);
			mCurrentCharacter=value;
			if(EditButton!=null){
				EditButton.Enable=(mCurrentCharacter!=null && mCurrentBackground!=null);
			}
		}
	}
	GameObject mCurrentBackground;
	public GameObject CurrentBackground{
		get{
			return mCurrentBackground;
		}set{
			Destroy(mCurrentBackground);
			mCurrentBackground=value;
			if(EditButton!=null){
				EditButton.Enable=(mCurrentCharacter!=null && mCurrentBackground!=null);
			}
		}
	}

	public Transform mCamera;
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
		mCamera = GameObject.Find("CameraMain").transform;
		mCamera.gameObject.AddComponent<SceneCameraManager>();

		QueueManager.pauseOnButtonDown = true;
		SetGUICamera();

		if(Data.selChapter!=null && Data.selChapter.IdCharacter!=-1){
			CurrentCharacter = ResourcesLibrary.getCharacter(Data.selChapter.IdCharacter).getInstance("ChapterMgr");
			CurrentCharacter.AddComponent<DataManager>();
		}
		if(Data.selChapter!=null && Data.selChapter.IdBackground!=-1){
			CurrentBackground = ResourcesLibrary.getBackground(Data.selChapter.IdBackground).getInstance("ChapterMgr");
		}
		SetDataGameObjects();

		if(Data.selChapter!=null){
			Data.selChapter.Reset();
		}
		LeftButtonBar.Show(false);
		RightButtonBar.Show(true);

		InitButtons();

		//Calculate W,H of help texture
		float originalW = texHelp01.width;
		float originalH = texHelp01.height;
		texHelpW = Screen.width;
		float ratio = originalW/Screen.width;
		texHelpH = originalH/ratio;
		texHelpY01 = (Screen.height-texHelpH)/2.0f;
		texHelpY02 = Screen.height-texHelpH;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected void SetDataGameObjects()
	{
		if(Data.selChapter!=null){
			if(CurrentCharacter!=null){
				Data.selChapter.Character=CurrentCharacter;
			}
			if(CurrentBackground!=null){
				Data.selChapter.BackGround=CurrentBackground;
			}
			if(mCamera!=null){
				Data.selChapter.Camera=mCamera.gameObject;
			}
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	protected void ClearDataGameObjects()
	{
		if(Data.selChapter!=null){
			Data.selChapter.Character=null;
			Data.selChapter.BackGround=null;
			Data.selChapter.Camera=null;
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	protected void SampleAnimation()
	{
		Transform mMesh = CurrentCharacter.transform.Find("mesh");
		mMesh.animation["Idle"].time = 0.0f;
		mMesh.animation["Idle"].weight = 1.0f;
		mMesh.animation["Idle"].enabled = true;
		mMesh.animation.Sample();
		mMesh.animation["Idle"].enabled = false;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected virtual void InitButtons()
	{
		//Help button
		float pos_x = Screen.width-ButtonProperties.buttonBarScaleX-ButtonProperties.buttonMargin-ButtonProperties.buttonSize/2.0f;
		float pos_y = ButtonProperties.buttonMargin+ButtonProperties.buttonSize/2.0f;
		Vector3 pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		Vector3 scale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);

		HelpButton.Init(pos, scale);
		HelpButton.Show(0, Globals.ANIMATIONDURATION, true);

		//Edit button
		pos_x -= ButtonProperties.buttonMargin+ButtonProperties.buttonSize;
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);

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
		//if(elemType==ButtonBar.ElementTypes.blocks){
			PlayButton.Enable=false;
		//}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public virtual void HideAllButtonBars()
	{
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected void SetGUICamera()
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

		if(bShowHelp){
			GUI.DrawTexture(new Rect(0,texHelpY01,texHelpW,texHelpH), texHelp01);
			GUI.DrawTexture(new Rect(0,texHelpY02,texHelpW,texHelpH), texHelp02);
		}

		//if(GUI.Button(new Rect(Screen.width / 2 - 50, 70, 100, 50), "Blur")) {
		//	blur = !blur;
		//}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected virtual void OnDestroy()
	{
		ClearDataGameObjects();
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
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Help button
	public void OnButtonHelpCheched(BasicButton sender)
	{
		if(sender.Checked){
			bShowHelp=true;
		}else{
			bShowHelp=false;
		}
	}
}






