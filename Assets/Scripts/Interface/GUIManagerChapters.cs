//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//http://forum.unity3d.com/threads/88485-2D-Quad-In-Camera-Space
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using TVR.Helpers;
using TVR.Button;
using TVR;

//Script attached to the GUICamera object
public class GUIManagerChapters : GUIManager
{
	private const float STANDARD_HEIGHT = 768f / 12;
	private const int MARGIN = 10;
	public ButtonBar mCharactersButtonBar;
	public ButtonBar mBackgroundsButtonBar;
	public ButtonBar mMusicButtonBar;

	public override bool blur {
		set {
			inputText.enable = !value;
			base.blur = value;
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void Start()
	{
		Font fontArial = (Font)ResourcesManager.LoadResource("Interface/Fonts/Futura Oblique", "Chapter");
		Texture white = (Texture)ResourcesManager.LoadResource("Shared/white_pixel", "Chapter");
		float width = (Screen.width - (ButtonProperties.buttonBarScaleX * 2)) - (MARGIN * 2);
		float height = Screen.height / 12; 
		float pos_y = Screen.height / 76.8f; //10; //Screen.height / 18; 
		Rect rectFileName = new Rect((ButtonProperties.buttonBarScaleX) + MARGIN, pos_y, width, height);

		//mInput = new InputText(rectFileName, white, white, white, white, fontArial, white, Globals.NEW_CHAPTER_TEXT, false, 2);
		inputText = new InputText(rectFileName, null, null, null, null, fontArial, white, Globals.NEW_CHAPTER_TEXT, false, 2);
		inputText.TextSize = Mathf.RoundToInt(50 * (height / STANDARD_HEIGHT));
		inputText.TextPosition = TextAnchor.MiddleCenter;
		inputText.TextColor = Color.white;
		inputText.specialCharacters = new char[]{ ' ', '-', '_', '.', '/', ',' };
		inputText.maxLength = 20;
		inputText.Text = "";
		inputText.shadow=true;
		inputText.TextStyle=FontStyle.Bold;
		inputText.selectedCallBack = inputSelected;
		inputText.unSelectedCallBack = inputUnSelected;
		inputText.scaleMode = ScaleMode.StretchToFill;
		inputText.Alpha = 0;
		inputText.enable = false;

		if(Data.selChapter!=null){
			inputText.Text = Data.selChapter.Title;
			inputText.Fade(1, Globals.ANIMATIONDURATION, true, true, -2);
		}

		base.Start();

		mCamera.audio.loop = true;
		mCamera.audio.playOnAwake = false;
		mCamera.audio.clip = null;

		//SetCurrentChapterElements();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void Update()
	{
		inputText.Update();
		base.Update();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void OnGUI()
	{
		inputText.OnGUIAllEvents();

		if(Event.current.type == EventType.Repaint){
			inputText.OnGUI();
		}
		base.OnGUI();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public override void EnableButtons(ButtonBar.ElementTypes elemType)
	{
		base.EnableButtons(elemType);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public override void DisableButtons(ButtonBar.ElementTypes elemType)
	{
		base.DisableButtons(elemType);
		EditButton.Enable=false;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public override void HideAllButtonBars()
	{
		mCharactersButtonBar.Hide();
		mBackgroundsButtonBar.Hide();
		mMusicButtonBar.Hide();

		mCharactersButtonBar.UncheckButtons();
		mBackgroundsButtonBar.UncheckButtons();
		mMusicButtonBar.UncheckButtons();

		LeftButtonBar.UncheckButtons();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void SetCurrentChapterElements()
	{
		if(Data.selChapter==null){
			return;
		}
		//Character
		if(Data.selChapter.IdCharacter!=-1){
			CurrentCharacter = ResourcesLibrary.getCharacter(Data.selChapter.IdCharacter).getInstance("ChapterMgr");
			CurrentCharacter.AddComponent<DataManager>();
			mCharactersButtonBar.SetCurrentButton(Data.selChapter.IdCharacter);
		}else{
			CurrentCharacter=null;
			mCharactersButtonBar.UncheckButtons();
		}
		//Background
		if(Data.selChapter.IdBackground!=-1){
			CurrentBackground = ResourcesLibrary.getBackground(Data.selChapter.IdBackground).getInstance("ChapterMgr");
			mBackgroundsButtonBar.SetCurrentButton(Data.selChapter.IdBackground);
		}else{
			CurrentBackground=null;
			mBackgroundsButtonBar.UncheckButtons();
		}
		//Music
		if(Data.selChapter.IdMusicNotNullable!=-1){
			mMusicButtonBar.SetCurrentButton(Data.selChapter.IdMusicNotNullable);
			mCamera.audio.Stop();
			mCamera.audio.clip = ResourcesManager.LoadResource(ResourcesLibrary.getMusic(Data.selChapter.IdMusicNotNullable).ResourceName, "Chapter") as AudioClip;
		}else{
			mMusicButtonBar.SetCurrentButton(-1);
			mCamera.audio.clip = null;
		}

		if(Data.selChapter.Blocks.Count>0){
			PlayButton.Enable=true;
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Characters button
	public void OnButtonCharactersChecked(BasicButton sender)
	{
		if(sender.Checked){
			mCharactersButtonBar.Show(true);
		}else{
			mCharactersButtonBar.Hide();
		}
		Count(sender.Checked);
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Backgrounds button
	public void OnButtonBackgroundsChecked(BasicButton sender)
	{
		if(sender.Checked){
			mBackgroundsButtonBar.Show(true);
		}else{
			mBackgroundsButtonBar.Hide();
		}
		Count(sender.Checked);
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Music button
	public void OnButtonMusicsChecked(BasicButton sender)
	{
		if(sender.Checked){
			mMusicButtonBar.Show(true);
			if(mCamera.audio.clip!=null){
				mCamera.audio.Play();
			}
		}else{
			mMusicButtonBar.Hide();
			mCamera.audio.Stop();
		}
		Count(sender.Checked);
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Share button
	public void OnButtonShareClicked(BasicButton sender)
	{
		Data.selChapter.Save();
		SceneMgr.Get.SwitchTo("Export");
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Edit button
	public override void OnButtonEditClicked(BasicButton sender)
	{
		if(Data.selChapter!=null){
			Data.selChapter.Save();
		}
		QueueManager.pauseOnButtonDown = false;
		Data.selChapter.loadBlocks();
		SceneMgr.Get.SwitchTo("ChapterEditor");
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Play button
	public override void OnButtonPlayClicked(BasicButton sender)
	{
		if(Data.selChapter!=null){
			Data.selChapter.Save();
			SetDataGameObjects();
			Data.selChapter.Reset();
		}
		SceneMgr.Get.SwitchTo("Player");
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Chapter button
	public void OnButtonChapterChecked(BasicButton sender)
	{
		if(sender.Checked){
			Data.selChapter = sender.iObj as Data.Chapter;
			Data.selChapter.loadBlocks();
			HideAllButtonBars();
			SetCurrentChapterElements();
			SetDataGameObjects();
			Data.selChapter.Frame(0,false);
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Character button
	public void OnButtonCharacterChecked(BasicButton sender)
	{
		if(sender.Checked){
			if(sender.sPrefab!=""){
				Data.selChapter.IdCharacter = sender.ID;
				CurrentCharacter = ResourcesLibrary.getCharacter(Data.selChapter.IdCharacter).getInstance("ChapterMgr");
				CurrentCharacter.AddComponent<DataManager>();
				SampleAnimation();
			}else{
				Debug.Log("El boton no tiene prefab asociado!");
			}
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Background button
	public void OnButtonBackgroundChecked(BasicButton sender)
	{
		if(sender.Checked){
			if(sender.sPrefab!=""){
				Data.selChapter.IdBackground = sender.ID;
				CurrentBackground = ResourcesLibrary.getBackground(Data.selChapter.IdBackground).getInstance("ChapterMgr");
			}else{
				Debug.Log("El boton no tiene prefab asociado!");
			}
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Music button
	public void OnButtonMusicChecked(BasicButton sender)
	{
		if(sender.Checked){
			Data.selChapter.IdMusicNotNullable = sender.ID;
			if(Data.selChapter.IdMusicNotNullable>0){
				mCamera.audio.clip = ResourcesManager.LoadResource(ResourcesLibrary.getMusic(Data.selChapter.IdMusicNotNullable).ResourceName, "Chapter") as AudioClip;
				mCamera.audio.Play();
			}else{
				mCamera.audio.clip=null;
			}
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void OnDestroy()
	{
		ResourcesManager.UnloadScene("Chapter");
		base.OnDestroy();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	private void inputSelected(ExtendedButton sender)
	{
		//#if UNITY_IOS
		blur = true;
		inputText.enable = true;
		//#endif
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	 
	private void inputUnSelected(ExtendedButton sender)
	{
		//#if UNITY_IOS
		blur = false;
		//#endif
		Data.selChapter.Title = inputText.Text;
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void OnApplicationPause(bool pauseStatus)
	{
		if(pauseStatus && Data.selChapter!=null){
			Data.selChapter.Save();
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void OnApplicationQuit()
	{
		if(Data.selChapter!=null){
			Data.selChapter.Save();
		}
	}
}






