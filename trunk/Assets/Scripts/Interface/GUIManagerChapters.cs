﻿//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
//http://forum.unity3d.com/threads/88485-2D-Quad-In-Camera-Space
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using UnityEngine;
using System.Collections;
using TVR.Helpers;

//Script attached to the GUICamera object
public class GUIManagerChapters : GUIManager
{
	public ButtonBar mCharactersButtonBar;
	public ButtonBar mBackgroundsButtonBar;
	public ButtonBar mMusicButtonBar;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

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
	//Main: Characters button
	public void OnButtonCharactersPressed(BasicButton sender)
	{
		if(sender.Checked) mCharactersButtonBar.Show();
		else mCharactersButtonBar.Hide();
		Count(sender.Checked);
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Backgrounds button
	public void OnButtonBackgroundsPressed(BasicButton sender)
	{
		if(sender.Checked) mBackgroundsButtonBar.Show();
		else mBackgroundsButtonBar.Hide();
		Count(sender.Checked);
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main: Music button
	public void OnButtonMusicsPressed(BasicButton sender)
	{
		if(sender.Checked) mMusicButtonBar.Show();
		else mMusicButtonBar.Hide();
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
	//Character button
	public void OnButtonCharacterPressed(BasicButton sender)
	{
		if(sender.sPrefab!=""){
			if(CurrentCharacter!=null){
				Destroy(CurrentCharacter);
			}
			CurrentCharacter = Instantiate(ResourcesManager.LoadModel("Characters/Prefabs/"+sender.sPrefab, "ChapterMgr")) as GameObject;
			SceneMgr.Get.sCurrentCharacter = sender.sPrefab;
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
			if(CurrentBackground!=null){
				Destroy(CurrentBackground);
			}
			CurrentBackground = Instantiate(ResourcesManager.LoadModel("Backgrounds/Prefabs/"+sender.sPrefab, "ChapterMgr")) as GameObject;
			SceneMgr.Get.sCurrentBackground = sender.sPrefab;
		}
		else{
			Debug.Log("El boton no tiene prefab asociado!");
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Music button
	public void OnButtonMusicPressed(BasicButton sender)
	{
		Debug.Log("music: " + sender.mID);
	}
}






