//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//http://forum.unity3d.com/threads/88485-2D-Quad-In-Camera-Space
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using TVR.Helpers;
using TVR;

//Script attached to the GUICamera object
public class GUIManagerChapters : GUIManager
{
	private const string NEW_CHAPTER_TEXT = "< Video Name >";
	public ButtonBar mCharactersButtonBar;
	public ButtonBar mBackgroundsButtonBar;
	public ButtonBar mMusicButtonBar;
	private TVR.Button.InputText mInput;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void Start() {
		Font fontArial = (Font)ResourcesManager.LoadResource("Interface/Fonts/Futura Oblique", "Chapter");
		Texture white = (Texture)ResourcesManager.LoadResource("Shared/white_pixel", "Chapter");
		Rect rectFileName = new Rect(0, 20, Screen.width, 20);

		mInput = new TVR.Button.InputText(rectFileName, null, null, null, null, fontArial, white, NEW_CHAPTER_TEXT, false);
		mInput.TextSize = 25;
		mInput.TextPosition = TextAnchor.MiddleCenter;
		mInput.TextColor = Color.white;
		mInput.specialCharacters = new char[]{ ' ', '-', '_', '.' };
		mInput.maxLength = 14;
		mInput.Alpha = 1;
		mInput.Text = "";
		mInput.selectedCallBack = inputSelected;
		mInput.unSelectedCallBack = inputUnSelected;

		//mInput.enable = false;
		//mInput.Fade(1, TVR.Globals.ANIMATIONDURATION, false, true, 0);
		base.Start();
	}

	protected virtual void Update() {
		mInput.Update();
	}

	protected override void OnGUI() {
		mInput.OnGUIAllEvents();

		if(Event.current.type == EventType.Repaint) {
			mInput.OnGUI();
		}
		base.OnGUI();
	}

	protected override void InitButtons()
	{
		base.InitButtons();
		mEditButton.Enable=false;
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
		SceneMgr.Get.SwitchTo("ChapterEditor");
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Play button
	public override void OnButtonPlayPressed(BasicButton sender)
	{
		Debug.Log("play chapters");
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Chapter button
	public void OnButtonChapterPressed(BasicButton sender)
	{
		HideAllButtonBars();

		Data.selChapter = sender.iObj as Data.Chapter;

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

	void OnDestroy() {
		TVR.Helpers.ResourcesManager.UnloadScene("Chapter");
	}

	private void inputSelected(TVR.Button.ExtendedButton sender) {
		blur  = true;
	}
	private void inputUnSelected(TVR.Button.ExtendedButton sender) {
		blur  = false;
		/*Data.selChapter.Title = sender.Text;
		Data.selChapter.Save();*/
	}
}






