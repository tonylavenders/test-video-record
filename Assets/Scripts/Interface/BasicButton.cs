using UnityEngine;
using System.Collections;
using TVR;
using TVR.Utils;
using TVR.Helpers;

public enum ButtonType{
	MAIN_CHARACTERS, MAIN_BACKGROUNDS, MAIN_MUSIC, MAIN_SHARE, MAIN_DEL_ELEM,
	ADD_ELEM, CHAPTER,
	CHAR, BACKGROUND,
	MAIN_EDIT, MAIN_PLAY,
	EDIT_TIME, EDIT_EXPR, EDIT_ANIM, EDIT_CAM,
	MUSIC, ANIM, EXPR, BLOCK,
	EDIT_TIME_TIME, EDIT_TIME_VOICE
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
			if(bChecked!=value){
				bChecked=value;
				if(value){
					renderer.material.mainTexture = texChecked;
					if(mButtonBar != null)
						mButtonBar.ButtonPressed(this);
				}else{
					renderer.material.mainTexture = texUnchecked;
				}
				if(checkedCallback!=null)
					checkedCallback(this);
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

	public delegate void ButtonCallback(BasicButton sender);
	public ButtonCallback clickedCallback;
	public ButtonCallback checkedCallback;
	public ButtonCallback enabledCallback;

	SmoothStep mFade;
	SmoothStep mMoveY;

	public iObject iObj;
	public int ID;

	public string sPrefab;
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
		renderer.material.mainTexture = texUnchecked;

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
		if(mButtonBar!=null)
			mGUIManager = mButtonBar.mGUIManager;
		else
			mGUIManager = transform.parent.GetComponent<GUIManager>();

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
			Ray ray = Camera.main.ScreenPointToRay(InputHelp.mousePositionYDown);

			if(InputHelp.GetMouseButtonDown(0)) {
				if(collider.Raycast(ray, out hit, 1000.0f)) {
					mMouseInitPos = InputHelp.mousePosition;
					mSharedTime = Time.time;
					bClicked = true;
					if(!bKeepSt)
						renderer.material.mainTexture = texChecked;
				}
			} else if(bClicked) {
				if(InputHelp.GetMouseButton(0)) {
					Vector2 mMovement = InputHelp.mousePosition - mMouseInitPos;
					if(mMovement.sqrMagnitude > MAXDISABLEBUTTONS || InputHelp.fingerChange) {
						if(!bKeepSt)
							renderer.material.mainTexture = texUnchecked;
						bClicked = false;
					}
				} else if(InputHelp.GetMouseButtonUp(0)) {
					if(bKeepSt)
						Checked = !Checked || bUnselectable;
					else
						renderer.material.mainTexture = texUnchecked;
					if(clickedCallback != null)
						clickedCallback(this);
					bClicked = false;
				}
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void Init(Vector3 pos, Vector3 scale)
	{
		transform.position = pos;
		transform.localScale = scale;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void GoToPosition(float finalY, float delay=0, float duration=Globals.ANIMATIONDURATION)
	{
		mMoveY.Reset(transform.position.y, finalY, duration, true, delay);
		state = States.moving;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void Refresh()
	{
		if(buttonType==ButtonType.CHAPTER){
			Text = "vid"+iObj.Number.ToString("00");
		}else{
			Text = iObj.Number.ToString("00");
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Button callbacks
	void SetCallback()
	{
		if(buttonType == ButtonType.MAIN_CHARACTERS) {
			checkedCallback = ((GUIManagerChapters)mGUIManager).OnButtonCharactersPressed;
		}
		else if(buttonType == ButtonType.MAIN_BACKGROUNDS) {
			checkedCallback = ((GUIManagerChapters)mGUIManager).OnButtonBackgroundsPressed;
		}
		else if(buttonType == ButtonType.MAIN_MUSIC) {
			checkedCallback = ((GUIManagerChapters)mGUIManager).OnButtonMusicsPressed;
		}
		else if(buttonType == ButtonType.MAIN_SHARE) {
			clickedCallback = ((GUIManagerChapters)mGUIManager).OnButtonSharePressed;
		}
		else if(buttonType == ButtonType.MAIN_DEL_ELEM) {
			clickedCallback = mGUIManager.mRightButtonBar.OnButtonDeleteElementPressed;
		}
		else if(buttonType == ButtonType.ADD_ELEM) {
			clickedCallback = ((ButtonBarElements)mButtonBar).OnButtonAddElementPressed;
		}
		else if(buttonType == ButtonType.CHAPTER) {
			clickedCallback = ((GUIManagerChapters)mGUIManager).OnButtonChapterPressed;
		}
		else if(buttonType == ButtonType.CHAR) {
			clickedCallback = ((GUIManagerChapters)mGUIManager).OnButtonCharacterPressed;
		}
		else if(buttonType == ButtonType.BACKGROUND) {
			clickedCallback = ((GUIManagerChapters)mGUIManager).OnButtonBackgroundPressed;
		}
		else if(buttonType == ButtonType.MUSIC) {
			clickedCallback = ((GUIManagerChapters)mGUIManager).OnButtonMusicPressed;
		}
		else if(buttonType == ButtonType.MAIN_EDIT) {
			clickedCallback = mGUIManager.OnButtonEditPressed;
		}
		else if(buttonType == ButtonType.MAIN_PLAY) {
			clickedCallback = mGUIManager.OnButtonPlayPressed;
		}
		else if(buttonType == ButtonType.BLOCK) {
			clickedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonBlockPressed;
		}
		else if(buttonType == ButtonType.EDIT_ANIM) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonAnimationsPressed;
		}
		else if(buttonType == ButtonType.EDIT_EXPR) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonExpressionsPressed;
		}
		else if(buttonType == ButtonType.EDIT_TIME) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonTimePressed;
		}
		else if(buttonType == ButtonType.EDIT_TIME_TIME) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonTimeTimePressed;
		}
		else if(buttonType == ButtonType.EDIT_TIME_VOICE) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonTimeVoicePressed;
		}
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Show(float delay = 0, float duration = Globals.ANIMATIONDURATION)
	{
		mFade.Reset(Enable ? 1f : 0.3f, duration, true, delay);

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



