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
	ADD_CHAPTER,
	CHAR,
	BACKGROUND
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public enum ContentType{
	MAIN,
	CHAR_01, CHAR_02, CHAR_03, CHAR_04, CHAR_05, CHAR_06, CHAR_07, CHAR_08, CHAR_09,
	BACKGROUND_01, BACKGROUND_02, BACKGROUND_03, BACKGROUND_04, BACKGROUND_05, BACKGROUND_06,BACKGROUND_07
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class BasicButton : MonoBehaviour
{
	const int MAXDISABLEBUTTONS = 450; //((15^2)*2) Máximo desplazamiento antes de desactivar los botones 15 pixeles.

	public Texture texChecked;
	public Texture texUnchecked;
	public bool bKeepSt = true;
	public bool bUnselectable = true;
	public bool bEnabled=true;
	bool bClicked;

	bool bChecked;
	public bool Checked {
		get { return bChecked; }
		set {
			if(bChecked!=value) {
				bChecked=value;
				if(checkedCallback!=null)
					checkedCallback(this);
				if(value) {
					renderer.sharedMaterial.mainTexture = texChecked;
					mButtonBar.ButtonPressed(this);
				} else
					renderer.sharedMaterial.mainTexture = texUnchecked;
			}
		}
	}
	public ButtonType buttonType;
	public ContentType contentType;

	public delegate void ButtonCallback(BasicButton sender);
	public ButtonCallback clickedCallback;
	public ButtonCallback checkedCallback;

	SmoothStep mFade;
	float mFadeEndValue;

	int mID;
	ButtonBar mButtonBar;
	GUIManager mGUIManager;
	Transform mGUIText;
	static float mSharedTime;

	Vector2 mMouseInitPos;

	enum States{
		fade_out,
		hidden,
		idle,
	}
	States state;

	public static bool anyButtonJustPressed {
		get { return mSharedTime == Time.time; }
	}
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
 
	void Awake()
	{
		renderer.sharedMaterial.mainTexture = texUnchecked;
		mGUIText = transform.FindChild("GUI Text");

		Color c = renderer.material.color;
		renderer.material.color = new Color(c.r, c.g, c.b, 0.0f);

		mFadeEndValue=1.0f;
		mFade = new SmoothStep(0.0f,0.0f,1.0f,false);
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
		if(state==States.hidden)
			return;

		//Fade
		if(mFade.Enable)
			mFade.Update();
		Color c = renderer.material.color;
		renderer.material.color = new Color(c.r, c.g, c.b, mFade.Value);

		if(mFadeEndValue==0.0f && mFade.Value<0.001f)
			state=States.hidden;

		if(bEnabled && mSharedTime != Time.time) {
			//Check if user is touching the button
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if(InputHelp.GetMouseButtonDown(0)) {
				if(collider.Raycast(ray, out hit, 1000.0f)) {
					mMouseInitPos = InputHelp.mousePosition;
					mSharedTime = Time.time;
					bClicked = true;
					if(!bKeepSt)
						renderer.sharedMaterial.mainTexture = texChecked;
				}
			} else if(bClicked) {
				if(InputHelp.GetMouseButton(0)) {
					Vector2 mMovement = InputHelp.mousePosition - mMouseInitPos;
					if(mMovement.sqrMagnitude > MAXDISABLEBUTTONS) {
						if(!bKeepSt)
							renderer.sharedMaterial.mainTexture = texUnchecked;
						bClicked = false;
					}
				} else if(InputHelp.GetMouseButtonUp(0)) {
					if(bKeepSt)
						Checked = !Checked || bUnselectable;
					else
						renderer.sharedMaterial.mainTexture = texUnchecked;
					mButtonBar.ButtonPressed(this);
					if(clickedCallback != null)
						clickedCallback(this);
					bClicked = false;
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void Enable()
	{
		bEnabled=true;
		mFade.Reset(1.0f, Globals.ANIMATIONDURATION);
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SetID(int _id)
	{
		mID=_id;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void SetCallback()
	{
		if(buttonType==ButtonType.MAIN_CHARACTERS){
			checkedCallback = mGUIManager.OnButtonCharactersPressed;
		}
		else if(buttonType==ButtonType.MAIN_BACKGROUNDS){
			checkedCallback = mGUIManager.OnButtonBackgroundsPressed;
		}
		else if(buttonType==ButtonType.MAIN_MUSIC){
			checkedCallback = mGUIManager.OnButtonMusicPressed;
		}
		else if(buttonType==ButtonType.MAIN_SHARE){
			clickedCallback = mGUIManager.OnButtonSharePressed;
		}
		else if(buttonType==ButtonType.MAIN_DELETE){
			clickedCallback = mGUIManager.OnButtonDeletePressed;
		}
		else if(buttonType==ButtonType.ADD_CHAPTER){
			clickedCallback = mGUIManager.OnButtonAddChapterPressed;
		}
		else if(buttonType==ButtonType.CHAR){
			clickedCallback = mGUIManager.OnButtonCharacterPressed;
		}
		else if(buttonType==ButtonType.BACKGROUND){
			clickedCallback = mGUIManager.OnButtonBackgroundPressed;
		}
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Show()
	{
		if(bEnabled)
			mFadeEndValue=1.0f;
		else
			mFadeEndValue=0.3f;
		mFade.Reset(mFadeEndValue, Globals.ANIMATIONDURATION);

		if(mGUIText)
			mGUIText.gameObject.GetComponent<GUITextController>().Show();

		state=States.idle;
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Hide()
	{
		mFadeEndValue=0.0f;
		mFade.Reset(mFadeEndValue, Globals.ANIMATIONDURATION);

		if(mGUIText)
			mGUIText.gameObject.GetComponent<GUITextController>().Hide();

		state=States.fade_out;
	}
}



