using UnityEngine;
using System.Collections;
using TVR;

public class Player_Main : GUIManager
{
	bool mPlay;
	float mTime;
	public Camera mainCamera;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void Start()
	{
		mPlay=true;
		mTime=0;

		SetGUICamera();
		LoadChapterElements();

		LetterboxManager.Start();
		LetterboxManager.Init();
		LetterboxManager.InitQuads();
		mainCamera.rect = LetterboxManager.GetRectPercent();

		float pos_x = Screen.width/2.0f;
		float pos_y = ButtonProperties.buttonSize/2.0f + ButtonProperties.buttonMargin;

		Vector3 pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);
		Vector3 scale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);

		PlayButton.Init(pos, scale);
		PlayButton.Show(0, Globals.ANIMATIONDURATION, true);

		pos_x = Screen.width - ButtonProperties.buttonSize/2.0f - ButtonProperties.buttonMargin*2;
		pos = new Vector3(pos_x, pos_y, ButtonProperties.buttonZDepth);

		EditButton.Init(pos, scale);
		EditButton.Show(0, Globals.ANIMATIONDURATION, true);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void LoadChapterElements()
	{
		mCamera = GameObject.Find("CameraMain").transform;
		mCamera.gameObject.AddComponent<SceneCameraManager>();
		Data.selChapter.Camera = mCamera.gameObject;

		CurrentCharacter = ResourcesLibrary.getCharacter(Data.selChapter.IdCharacter).getInstance("Player");
		CurrentCharacter.AddComponent<DataManager>();
		Data.selChapter.Character = CurrentCharacter;

		CurrentBackground = ResourcesLibrary.getBackground(Data.selChapter.IdBackground).getInstance("Player");
		CurrentBackground.AddComponent<DataManager>();
		Data.selChapter.BackGround = CurrentBackground;
	}
	/* No podemos usar OnGUI para Letterbox pq se pinta encima de los botones 3D
	 * Usamos un letterbox hecho con un quad igual que los botones
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void OnGUI()
	{
		LetterboxManager.OnGUI();
	}
	*/
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void Update()
	{
		base.Update();

		if(mPlay){
			mTime += Time.deltaTime;
			if(mTime>Data.selChapter.totalTime){
				Data.selChapter.Stop();
				Data.selChapter.Reset();
				Data.selChapter.Frame(0,false);
				PlayButton.Checked=true;
				mTime=0;
				mPlay=false;
			}else{
				Data.selChapter.Frame(mTime, true);
			}
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonPlayerPlayChecked(BasicButton sender)
	{
		//Pause
		if(mPlay){
			Data.selChapter.Stop();
			mPlay=false;
		}
		//Continue
		else{
			Data.selChapter.Reset();
			mPlay=true;
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void OnButtonPlayerEditClicked(BasicButton sender)
	{
		mPlay = false;
		Data.selChapter.Stop();
		SceneMgr.Get.SwitchTo("ChapterMgr");
	}
}




