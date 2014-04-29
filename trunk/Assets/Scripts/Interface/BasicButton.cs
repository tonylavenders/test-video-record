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
	CHAPTER,
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
	[HideInInspector] [SerializeField] bool bEnabled=true;
	[ExposeProperty]
	public bool Enable {
		get { return bEnabled; }
		set {
			if(bEnabled != value) {
				bEnabled = value;
				if(enabledCallback != null)
					enabledCallback(this);
				if(state == States.fade_in || state == States.idle) {
					if(value)
						mFade.Reset(1f, Globals.ANIMATIONDURATION);
					else
						mFade.Reset(0.3f, Globals.ANIMATIONDURATION);
				}
				if(!value)
					Checked = false;
			}
		}
	}
	bool bClicked;

	[HideInInspector] [SerializeField] bool bChecked;
	[ExposeProperty]
	public bool Checked {
		get { return bChecked; }
		set {
			if(bChecked!=value) {
				bChecked=value;
				if(checkedCallback!=null)
					checkedCallback(this);
				if(value) {
					renderer.sharedMaterial.mainTexture = texChecked;
					if(mButtonBar != null)
						mButtonBar.ButtonPressed(this);
				} else
					renderer.sharedMaterial.mainTexture = texUnchecked;
			}
		}
	}
	[ExposeProperty]
	public string Text {
		get { 
			if(mGUIText!=null)
				return mGUIText.guiText.text;
			else if(mText3D!=null)
				return mText3D.text;
			else
				return "";
		}
		set {
			if(mGUIText!=null) 
				mGUIText.guiText.text = value;
			if(mText3D!=null)
				mText3D.text = value;
		}
	}
	[ExposeProperty]
	public bool Blur {
		get {
			if(mText3D != null)
				return mText3D.gameObject.activeSelf;
			else
				return false;
		}
		set {
			if(mGUIText != null)
				mGUIText.gameObject.SetActive(!value);
			if(mText3D != null)
				mText3D.gameObject.SetActive(value);
			if(value && mGUIText != null && mText3D != null)
				mText3D.color = mGUIText.guiText.color;
		}
	}
	public ButtonType buttonType;
	public ContentType contentType;

	public delegate void ButtonCallback(BasicButton sender);
	public ButtonCallback clickedCallback;
	public ButtonCallback checkedCallback;
	public ButtonCallback enabledCallback;

	SmoothStep mFade;
	SmoothStep mMoveY;

	public int mID;
	ButtonBar mButtonBar;
	GUIManager mGUIManager;
	Transform mGUIText;
	TextMesh mText3D;
	static float mSharedTime;

	Vector2 mMouseInitPos;

	public enum States{
		fade_out,
		hidden,
		fade_in,
		idle,
		moving,
	}
	public States state;

	public static bool anyButtonJustPressed {
		get { return mSharedTime == Time.time; }
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
 
	void Awake()
	{
		renderer.sharedMaterial.mainTexture = texUnchecked;
		mGUIText = transform.FindChild("GUI Text");
		Transform t = transform.FindChild("New Text");
		if(t != null)
			mText3D = transform.FindChild("New Text").GetComponent<TextMesh>();

		Color c = renderer.material.color;
		renderer.material.color = new Color(c.r, c.g, c.b, 0.0f);

		mFade = new SmoothStep(0.0f, 0.0f, 1.0f, false);
		mMoveY = new SmoothStep(0.0f,0.0f,1.0f,false);
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
		if(state == States.hidden || Camera.main == null)
			return;

		//MoveY
		SmoothStep.State SSState = mMoveY.Update();
		if(SSState == SmoothStep.State.inFade || SSState==SmoothStep.State.justEnd) {
			transform.position = new Vector3(transform.position.x, mMoveY.Value, transform.position.z);
			if(SSState==SmoothStep.State.justEnd)
				state=States.idle;
			else
				return;
		}
		//Fade
		SSState = mFade.Update();
		if(SSState == SmoothStep.State.inFade || SSState==SmoothStep.State.justEnd) {
			if(SSState==SmoothStep.State.justEnd) {
				if(mFade.Value == 0)
					state = States.hidden;
				else
					state = States.idle;
			}
			Color c = renderer.material.color;
			renderer.material.color = new Color(c.r, c.g, c.b, mFade.Value);
		}
		//Check if user is touching the button
		if(bEnabled && state == States.idle && mSharedTime != Time.time) {
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
					if(clickedCallback != null)
						clickedCallback(this);
					bClicked = false;
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void GoToPosition(float finalY, float delay=0, float duration=Globals.ANIMATIONDURATION)
	{
		mMoveY.Reset(transform.position.y, finalY, duration, true, delay);
		state = States.moving;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void ChangeID(int new_id)
	{
		mID = new_id;
		Text = "sc"+new_id.ToString("00");

		//TODO:Reasign chapter data
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SetID(int _id)
	{
		mID = _id;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void SetCallback()
	{
		if(buttonType == ButtonType.MAIN_CHARACTERS) {
			checkedCallback = mGUIManager.OnButtonCharactersPressed;
		}
		else if(buttonType == ButtonType.MAIN_BACKGROUNDS) {
			checkedCallback = mGUIManager.OnButtonBackgroundsPressed;
		}
		else if(buttonType == ButtonType.MAIN_MUSIC) {
			checkedCallback = mGUIManager.OnButtonMusicPressed;
		}
		else if(buttonType == ButtonType.MAIN_SHARE) {
			clickedCallback = mGUIManager.OnButtonSharePressed;
		}
		else if(buttonType == ButtonType.MAIN_DELETE) {
			clickedCallback = mGUIManager.mChaptersButtonBar.GetComponent<ButtonBarChapters>().OnButtonDeleteChapterPressed;
		}
		else if(buttonType == ButtonType.ADD_CHAPTER) {
			clickedCallback = transform.parent.GetComponent<ButtonBarChapters>().OnButtonAddChapterPressed;
		}
		else if(buttonType == ButtonType.CHAR) {
			clickedCallback = mGUIManager.OnButtonCharacterPressed;
		}
		else if(buttonType == ButtonType.BACKGROUND) {
			clickedCallback = mGUIManager.OnButtonBackgroundPressed;
		}
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Show(float delay = 0, float duration = Globals.ANIMATIONDURATION)
	{
		mFade.Reset(bEnabled ? 1f : 0.3f, duration, true, delay);

		if(mGUIText)
			mGUIText.gameObject.GetComponent<GUITextController>().Show(delay, duration);

		state = States.fade_in;
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Hide(float delay = 0, float duration = Globals.ANIMATIONDURATION)
	{
		mFade.Reset(0f, duration, true, delay);

		if(mGUIText)
			mGUIText.gameObject.GetComponent<GUITextController>().Hide(delay, duration);

		state = States.fade_out;
	}
}



