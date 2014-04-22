using UnityEngine;
using System.Collections;
using TVR;
using TVR.Utils;
using TVR.Helpers;

public enum ButtonType{
	MAIN_CHARACTERS,
	MAIN_BACKGROUNDS,
	MAIN_MUSIC,
	MAIN_SHARE,
	MAIN_DELETE,
	CHAR,
	BACKGROUND
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public enum ContentType{
	LIB,
	CHAR_01, CHAR_02, CHAR_03, CHAR_04, CHAR_05, CHAR_06, CHAR_07, CHAR_08, CHAR_09,
	BACKGROUND_01, BACKGROUND_02, BACKGROUND_03, BACKGROUND_04, BACKGROUND_05, BACKGROUND_06,BACKGROUND_07
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class BasicButton : MonoBehaviour
{
	const int MAXDISABLEBUTTONS = 450; //((15^2)*2) Máximo desplazamiento antes de desactivar los botones 15 pixeles.

	public Texture texChecked;
	public Texture texUnchecked;
	public bool bKeepSt;
	public bool bChecked;
	public ButtonType buttonType;
	public ContentType contentType;

	public delegate void ButtonCallback(ContentType contentType);
	public ButtonCallback buttonCallback;

	SmoothStep mFade;

	int mID;
	ButtonBar mButtonBar;
	GUIManager mGUIManager;
	Transform mGUIText;

	Vector2 mMouseInitPos;

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
 
	void Awake()
	{
		renderer.sharedMaterial.mainTexture = texUnchecked;
		mGUIText = transform.FindChild("GUI Text");
		mFade = new SmoothStep(0.0f,1.0f,1.0f,false);
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		mButtonBar = transform.parent.GetComponent<ButtonBar>();
		mGUIManager = mButtonBar.mGUIManager;
		SetCallback();
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void Update()
	{
		//Fade
		if(mFade.Enable)
			mFade.Update();
		Color c = renderer.material.color;
		renderer.material.color = new Color(c.r, c.g, c.b, mFade.Value);

		//Check if user is touching the button
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

		if(collider.Raycast(ray, out hit, 1000.0f)){
			if(Input.GetMouseButtonDown(0)){
				mMouseInitPos=InputHelp.mousePosition;
			}
			if(Input.GetMouseButtonUp(0)){
				Vector2 mMovement = InputHelp.mousePosition-mMouseInitPos;
				if(mMovement.sqrMagnitude < MAXDISABLEBUTTONS){
					if(!bChecked){
						renderer.sharedMaterial.mainTexture = texChecked;
						bChecked=true;
						mButtonBar.ButtonPressed(mID);
						if(buttonCallback!=null)
							buttonCallback(contentType);
					}
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SetID(int _id)
	{
		mID=_id;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void UnCheck(int current_id)
	{
		if(current_id==mID)
			return;

		if(bChecked){
			bChecked=false;
			renderer.sharedMaterial.mainTexture = texUnchecked;
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void SetCallback()
	{
		if(buttonType==ButtonType.MAIN_CHARACTERS){
			buttonCallback = mGUIManager.OnButtonCharactersPressed;
		}
		else if(buttonType==ButtonType.MAIN_BACKGROUNDS){
			buttonCallback = mGUIManager.OnButtonBackgroundsPressed;
		}
		else if(buttonType==ButtonType.MAIN_MUSIC){
			buttonCallback = mGUIManager.OnButtonMusicPressed;
		}
		else if(buttonType==ButtonType.MAIN_SHARE){
			buttonCallback = mGUIManager.OnButtonSharePressed;
		}
		else if(buttonType==ButtonType.MAIN_DELETE){
			buttonCallback = mGUIManager.OnButtonDeletePressed;
		}
		else if(buttonType==ButtonType.CHAR){
			buttonCallback = mGUIManager.OnButtonCharacterPressed;
		}
		else if(buttonType==ButtonType.BACKGROUND){
			buttonCallback = mGUIManager.OnButtonBackgroundPressed;
		}
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Show()
	{
		mFade.Reset(1.0f, Globals.ANIMATIONDURATION);
		if(mGUIText)
			mGUIText.gameObject.GetComponent<GUITextController>().Show();
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Hide()
	{
		mFade.Reset(0.0f, Globals.ANIMATIONDURATION);
		if(mGUIText)
			mGUIText.gameObject.GetComponent<GUITextController>().Hide();
	}
}



