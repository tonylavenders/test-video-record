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
			mInput.enable = !value;
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
		mInput = new InputText(rectFileName, null, null, null, null, fontArial, white, Globals.NEW_CHAPTER_TEXT, false, 2);
		mInput.TextSize = Mathf.RoundToInt(50 * (height / STANDARD_HEIGHT));
		mInput.TextPosition = TextAnchor.MiddleCenter;
		mInput.TextColor = Color.white;
		mInput.specialCharacters = new char[]{ ' ', '-', '_', '.', '/', ',' };
		mInput.maxLength = 20;
		mInput.Text = "";
		mInput.shadow=true;
		mInput.TextStyle=FontStyle.Bold;
		mInput.selectedCallBack = inputSelected;
		mInput.unSelectedCallBack = inputUnSelected;
		mInput.scaleMode = ScaleMode.StretchToFill;
		mInput.Alpha = 0;
		mInput.enable = false;

		if(Data.selChapter!=null){
			mInput.Text = Data.selChapter.Title;
			mInput.Fade(1, Globals.ANIMATIONDURATION, true, true, -2);
		}

		base.Start();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void Update()
	{
		mInput.Update();
		base.Update();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void OnGUI()
	{
		mInput.OnGUIAllEvents();

		if(Event.current.type == EventType.Repaint){
			mInput.OnGUI();
		}
		base.OnGUI();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void InitButtons()
	{
		base.InitButtons();
		mEditButton.Enable=false;
		mEditButton.Show();
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public override void EnableButtons()
	{
		base.EnableButtons();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public override void DisableButtons()
	{
		base.DisableButtons();
		mEditButton.Enable=false;
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

		mLeftButtonBar.UncheckButtons();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Characters button
	public void OnButtonCharactersPressed(BasicButton sender)
	{
		if(sender.Checked){
			mCharactersButtonBar.Show();
		}else{
			mCharactersButtonBar.Hide();
		}
		Count(sender.Checked);
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Backgrounds button
	public void OnButtonBackgroundsPressed(BasicButton sender)
	{
		if(sender.Checked){
			mBackgroundsButtonBar.Show();
		}else{
			mBackgroundsButtonBar.Hide();
		}
		Count(sender.Checked);
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Music button
	public void OnButtonMusicsPressed(BasicButton sender)
	{
		if(sender.Checked){
			mMusicButtonBar.Show();
		}else{
			mMusicButtonBar.Hide();
		}
		Count(sender.Checked);
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Share button
	public void OnButtonSharePressed(BasicButton sender)
	{
		Debug.Log("share");
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Edit button
	public override void OnButtonEditPressed(BasicButton sender)
	{
		if(Data.selChapter!=null){
			Data.selChapter.Save();
		}
		SceneMgr.Get.SwitchTo("ChapterEditor");
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Play button
	public override void OnButtonPlayPressed(BasicButton sender)
	{
		if(Data.selChapter!=null){
			Data.selChapter.Save();
		}
		Debug.Log("play chapters");
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Chapter button
	public void OnButtonChapterPressed(BasicButton sender)
	{
		HideAllButtonBars();

		//Data.selChapter = sender.iObj as Data.Chapter;

		//mInput.Text = Data.selChapter.Title;
		//mInput.enable = true;

		if(Data.selChapter.IdCharacter!=-1){
			CurrentCharacter = ResourcesLibrary.getCharacter(Data.selChapter.IdCharacter).getInstance("ChapterMgr");
			mCharactersButtonBar.SetCurrentButton(Data.selChapter.IdCharacter);
		}else{
			CurrentCharacter=null;
			mCharactersButtonBar.UncheckButtons();
		}
		if(Data.selChapter.IdBackground!=-1){
			CurrentBackground = ResourcesLibrary.getBackground(Data.selChapter.IdBackground).getInstance("ChapterMgr");
			mBackgroundsButtonBar.SetCurrentButton(Data.selChapter.IdBackground);
		}else{
			CurrentBackground=null;
			mBackgroundsButtonBar.UncheckButtons();
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Character button
	public void OnButtonCharacterPressed(BasicButton sender)
	{
		if(sender.sPrefab!=""){
			Data.selChapter.IdCharacter = sender.ID;
			CurrentCharacter = ResourcesLibrary.getCharacter(Data.selChapter.IdCharacter).getInstance("ChapterMgr");
		}
		else{
			Debug.Log("El boton no tiene prefab asociado!");
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Background button
	public void OnButtonBackgroundPressed(BasicButton sender)
	{
		if(sender.sPrefab!=""){
			Data.selChapter.IdBackground = sender.ID;
			CurrentBackground = ResourcesLibrary.getBackground(Data.selChapter.IdBackground).getInstance("ChapterMgr");
		}
		else{
			Debug.Log("El boton no tiene prefab asociado!");
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Music button
	public void OnButtonMusicPressed(BasicButton sender)
	{
		//Debug.Log("music: " + sender.iObj.Number);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void OnDestroy()
	{
		ResourcesManager.UnloadScene("Chapter");
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	private void inputSelected(ExtendedButton sender)
	{
		#if UNITY_IOS
		blur = true;
		mInput.enable = true;
		#endif
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	 
	private void inputUnSelected(ExtendedButton sender)
	{
		#if UNITY_IOS
		blur = false;
		#endif
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	protected override void OnApplicationPause(bool pauseStatus)
	{
		base.OnApplicationPause(pauseStatus);

		if(Data.selChapter!=null){
			Data.selChapter.Save();
		}
	}
}






