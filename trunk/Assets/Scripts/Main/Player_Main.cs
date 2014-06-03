using UnityEngine;
using System.Collections;
using TVR;

public class Player_Main : MonoBehaviour
{
	bool mPlay;
	float mTime;

	GameObject mCurrentCharacter;
	GameObject mCurrentBackground;
	GameObject mCamera;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		mPlay=true;
		LoadChapterElements();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void LoadChapterElements()
	{
		mCamera = GameObject.Find("CameraMain");
		mCamera.AddComponent<SceneCameraManager>();
		Data.selChapter.Camera = mCamera;

		mCurrentCharacter = ResourcesLibrary.getCharacter(Data.selChapter.IdCharacter).getInstance("Player");
		mCurrentCharacter.AddComponent<DataManager>();
		Data.selChapter.Character = mCurrentCharacter;

		mCurrentBackground = ResourcesLibrary.getBackground(Data.selChapter.IdBackground).getInstance("Player");
		mCurrentBackground.AddComponent<DataManager>();
		Data.selChapter.BackGround = mCurrentBackground;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Update()
	{
		if(mPlay){
			mTime += Time.deltaTime;
			Data.selChapter.Frame(mTime, true);
		}
	}
}
