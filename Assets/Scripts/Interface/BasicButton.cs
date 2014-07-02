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
	EDIT_TIME_TIME, EDIT_TIME_VOICE,
	EDIT_TIME_TIME_INCR, EDIT_TIME_TIME_DECR, EDIT_TIME_TIME_SAVE,
	EDIT_TIME_VOICE_PLAY, EDIT_TIME_VOICE_REC, EDIT_TIME_VOICE_FX, EDIT_TIME_VOICE_SAVE,
	EDIT_TIME_VOICE_FX_MONSTER, EDIT_TIME_VOICE_FX_SMURF, EDIT_TIME_VOICE_FX_ECHO, EDIT_TIME_VOICE_FX_OFF,
	EDIT_TIME_VOICE_FX_MONSTER_PRO, EDIT_TIME_VOICE_FX_SMURF_PRO, EDIT_TIME_VOICE_FX_ROBOT, EDIT_TIME_VOICE_FX_DIST,
	EDIT_TIME_VOICE_FX_NOISE, EDIT_TIME_VOICE_FX_COMPRESS,
	CAM_PARAM,
	PLAYER_PLAY, PLAYER_EDIT, EXPORT_EDIT,
	MAIN_HELP
}

//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

public class BasicButton : MonoBehaviour
{
	const int MAXDISABLEBUTTONS = 450; //((15^2)*2) Máximo desplazamiento antes de desactivar los botones 15 pixeles.

	public Texture texChecked;
	public Texture texUnchecked;
	public Texture texDisabled;
	public bool bKeepSt = true;
	public bool bUnselectable = true;
	public bool bClickable = true;

	public float fAlphaDisabled=0.3f;
	bool bReactivate=true;

	[HideInInspector] [SerializeField] bool bEnabled=true;
	[ExposeProperty]
	public bool Enable {
		get { return bEnabled; }
		set {
			if(bEnabled != value) {
				bEnabled = value;
				if(enabledCallback != null){
					enabledCallback(this);
				}
				if(state == States.fade_in || state == States.idle) {
					Show(0, Globals.ANIMATIONDURATION, value);
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
				Init();
				bChecked=value;
				if(checkedCallback!=null)
					checkedCallback(this);
				if(value){
					renderer.material.mainTexture = texChecked;
					if(mButtonBar != null)
						mButtonBar.ButtonPressed(this);
				}else{
					renderer.material.mainTexture = texUnchecked;
				}
			}
		}
	}
	[ExposeProperty]
	public string Text {
		get { 
			if(mGUIText!=null)
				return mGUIText.guiText.text;
			/*else if(mText3D!=null)
				return mText3D.text;*/
			else
				return "";
		}
		set {
			if(mGUIText!=null) 
				mGUIText.guiText.text = value;
			/*if(mText3D!=null)
				mText3D.text = value;*/
		}
	}
	/*[ExposeProperty]
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
	}*/
	public ButtonType buttonType;

	public delegate void ButtonCallback(BasicButton sender);
	public ButtonCallback clickedCallback;
	public ButtonCallback checkedCallback;
	public ButtonCallback enabledCallback;

	SmoothStep mFade;
	SmoothStep mMoveY;

	public iObject iObj;
	public int ID;

	bool bInit=false;

	public string sPrefab;
	public ButtonBar mButtonBar;
	public GUIManager mGUIManager;
	Transform mGUIText;
	Transform mGUITextBottom;
	//TextMesh mText3D;
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
		mGUITextBottom = transform.FindChild("GUI Text Bottom");
		/*Transform t = transform.FindChild("New Text");
		if(t != null)
			mText3D = transform.FindChild("New Text").GetComponent<TextMesh>();*/

		Color c = renderer.material.color;
		renderer.material.color = new Color(c.r, c.g, c.b, 0.0f);

		mFade = new SmoothStep(0.0f, 0.0f, 1.0f, false);
		mMoveY = new SmoothStep(0.0f,0.0f,1.0f,false);
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		Init();
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Init()
	{
		if(!bInit){
			mButtonBar = transform.parent.GetComponent<ButtonBar>();
			
			if(mButtonBar!=null){
				mGUIManager = mButtonBar.mGUIManager;
			}else{
				mGUIManager = transform.parent.GetComponent<GUIManager>();
			}
			SetCallback();
			bInit=true;
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void Update()
	{
		if(state == States.hidden || Camera.main == null)
			return;

		if(TVR.Utils.Message.State==TVR.Utils.Message.States.Running)
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
				else{
					state = States.idle;
					Enable = bReactivate;
				}
			}
			Color c = renderer.material.color;
			renderer.material.color = new Color(c.r, c.g, c.b, mFade.Value);
		}

		if(buttonType!=ButtonType.MAIN_HELP && mGUIManager.bShowHelp)
			return;

		if(!bClickable)
			return;

		if(mGUIManager is GUIManagerBlocks && buttonType!=ButtonType.EDIT_TIME_VOICE_PLAY && buttonType!=ButtonType.EDIT_TIME_VOICE_REC){
			if(((GUIManagerBlocks)mGUIManager).soundRecorder.mMode!=SoundRecorder.Modes.Idle)
				return;
		}

		//Check if user is touching the button
		if(bEnabled && state == States.idle && mSharedTime != Time.time) {
			RaycastHit hit;
			Ray ray = mGUIManager.camera.ScreenPointToRay(InputHelp.mousePositionYDown);

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

	public void SetTextBottom(string text="")
	{
		mGUITextBottom.GetComponent<GUITextController>().SetTextBottom(text);
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void SetTextColor(bool bChecked, Color checkedColor)
	{
		if(bChecked){
			mGUIText.guiText.color = checkedColor; //selected
		}else{
			mGUIText.guiText.color = new Color(0.96f,0.96f,0.96f,1); //white -> unselected
		}
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void SetTextBottomColor(bool bChecked, Color checkedColor)
	{
		if(bChecked){
			mGUITextBottom.guiText.color = checkedColor; //selected
		}else{
			mGUITextBottom.guiText.color = new Color(0.96f,0.96f,0.96f,1); //white -> unselected
		}
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void SetBottomTextColor(bool bChecked, Color checkedColor)
	{
		if(bChecked){
			mGUITextBottom.guiText.color = checkedColor; //selected
		}else{
			mGUITextBottom.guiText.color = new Color(0.96f,0.96f,0.96f,1); //white -> unselected
		}
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
		//if(buttonType==ButtonType.CHAPTER){
		//	Text = "vid"+iObj.Number.ToString("00");
		//}else{
			Text = iObj.Number.ToString("00");
		//}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Button callbacks
	void SetCallback()
	{
		if(buttonType == ButtonType.MAIN_CHARACTERS) {
			checkedCallback = ((GUIManagerChapters)mGUIManager).OnButtonCharactersChecked;
		}
		else if(buttonType == ButtonType.MAIN_BACKGROUNDS) {
			checkedCallback = ((GUIManagerChapters)mGUIManager).OnButtonBackgroundsChecked;
		}
		else if(buttonType == ButtonType.MAIN_MUSIC) {
			checkedCallback = ((GUIManagerChapters)mGUIManager).OnButtonMusicsChecked;
		}
		else if(buttonType == ButtonType.MAIN_SHARE) {
			clickedCallback = ((GUIManagerChapters)mGUIManager).OnButtonShareClicked;
		}
		else if(buttonType == ButtonType.MAIN_DEL_ELEM) {
			clickedCallback = mGUIManager.RightButtonBar.OnButtonDeleteElementClicked;
		}
		else if(buttonType == ButtonType.ADD_ELEM) {
			clickedCallback = ((ButtonBarElements)mButtonBar).OnButtonAddElementClicked;
		}
		else if(buttonType == ButtonType.CHAPTER) {
			checkedCallback = ((GUIManagerChapters)mGUIManager).OnButtonChapterChecked;
		}
		else if(buttonType == ButtonType.CHAR) {
			checkedCallback = ((GUIManagerChapters)mGUIManager).OnButtonCharacterChecked;
		}
		else if(buttonType == ButtonType.BACKGROUND) {
			checkedCallback = ((GUIManagerChapters)mGUIManager).OnButtonBackgroundChecked;
		}
		else if(buttonType == ButtonType.MUSIC) {
			checkedCallback = ((GUIManagerChapters)mGUIManager).OnButtonMusicChecked;
		}
		else if(buttonType == ButtonType.MAIN_EDIT) {
			clickedCallback = mGUIManager.OnButtonEditClicked;
		}
		else if(buttonType == ButtonType.MAIN_PLAY) {
			clickedCallback = mGUIManager.OnButtonPlayClicked;
		}
		else if(buttonType == ButtonType.MAIN_HELP) {
			checkedCallback = mGUIManager.OnButtonHelpCheched;
		}
		else if(buttonType == ButtonType.BLOCK) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonBlockChecked;
		}
		else if(buttonType == ButtonType.EDIT_ANIM) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonAnimationsChecked;
		}
		else if(buttonType == ButtonType.EDIT_EXPR) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonExpressionsChecked;
		}
		else if(buttonType == ButtonType.EDIT_CAM) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonCamerasChecked;
		}
		else if(buttonType == ButtonType.ANIM) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonAnimationChecked;
		}
		//else if(buttonType == ButtonType.EXPR) {
		//	checkedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonExpressionChecked;
		//}
		else if(buttonType == ButtonType.CAM_PARAM) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonCameraChecked;
		}

		//TIME SECTION ////////////////////////////////////////////////////////////////////////////////////////////////////////

		else if(buttonType == ButtonType.EDIT_TIME) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonTimeChecked;
		}
		else if(buttonType == ButtonType.EDIT_TIME_TIME) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonTimeTimeChecked;
		}
		else if(buttonType == ButtonType.EDIT_TIME_VOICE) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonTimeVoiceChecked;
		}
		else if(buttonType == ButtonType.EDIT_TIME_TIME_DECR) {
			clickedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonTimeTimeDecrClicked;
		}
		else if(buttonType == ButtonType.EDIT_TIME_TIME_INCR) {
			clickedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonTimeTimeIncrClicked;
		}
		else if(buttonType == ButtonType.EDIT_TIME_TIME_SAVE) {
			clickedCallback = ((GUIManagerBlocks)mGUIManager).OnButtonTimeTimeSaveClicked;
		}

		//VOICE ////////////

		else if(buttonType == ButtonType.EDIT_TIME_VOICE_PLAY) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).soundRecorder.OnButtonTimeVoicePlayChecked;
		}
		else if(buttonType == ButtonType.EDIT_TIME_VOICE_REC) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).soundRecorder.OnButtonTimeVoiceRecChecked;
		}
		else if(buttonType == ButtonType.EDIT_TIME_VOICE_FX) {
			clickedCallback = ((GUIManagerBlocks)mGUIManager).soundRecorder.OnButtonTimeVoiceFxClicked;
		}
		else if(buttonType == ButtonType.EDIT_TIME_VOICE_SAVE) {
			clickedCallback = ((GUIManagerBlocks)mGUIManager).soundRecorder.OnButtonTimeVoiceSaveClicked;
		}

		//SOUND FILTERS ////////////////////////////////////////////////////////////////////////////////////////////////////////

		else if(buttonType == ButtonType.EDIT_TIME_VOICE_FX_MONSTER) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).soundRecorder.OnButtonTimeVoiceFxEffectChecked;
		}
		else if(buttonType == ButtonType.EDIT_TIME_VOICE_FX_SMURF) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).soundRecorder.OnButtonTimeVoiceFxEffectChecked;
		}
		else if(buttonType == ButtonType.EDIT_TIME_VOICE_FX_ECHO) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).soundRecorder.OnButtonTimeVoiceFxEffectChecked;
		}
		else if(buttonType == ButtonType.EDIT_TIME_VOICE_FX_MONSTER_PRO) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).soundRecorder.OnButtonTimeVoiceFxEffectChecked;
		}
		else if(buttonType == ButtonType.EDIT_TIME_VOICE_FX_SMURF_PRO) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).soundRecorder.OnButtonTimeVoiceFxEffectChecked;
		}
		else if(buttonType == ButtonType.EDIT_TIME_VOICE_FX_ROBOT) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).soundRecorder.OnButtonTimeVoiceFxEffectChecked;
		}
		else if(buttonType == ButtonType.EDIT_TIME_VOICE_FX_DIST) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).soundRecorder.OnButtonTimeVoiceFxEffectChecked;
		}
		else if(buttonType == ButtonType.EDIT_TIME_VOICE_FX_NOISE) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).soundRecorder.OnButtonTimeVoiceFxEffectChecked;
		}
		else if(buttonType == ButtonType.EDIT_TIME_VOICE_FX_COMPRESS) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).soundRecorder.OnButtonTimeVoiceFxEffectChecked;
		}
		else if(buttonType == ButtonType.EDIT_TIME_VOICE_FX_OFF) {
			checkedCallback = ((GUIManagerBlocks)mGUIManager).soundRecorder.OnButtonTimeVoiceFxEffectChecked;
		}

		//PLAYER ////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		else if(buttonType == ButtonType.PLAYER_PLAY) {
			checkedCallback = ((Player_Main)mGUIManager).OnButtonPlayerPlayChecked;
		}
		else if(buttonType == ButtonType.PLAYER_EDIT) {
			clickedCallback = ((Player_Main)mGUIManager).OnButtonPlayerEditClicked;
		}
		
		//EXPORT ////////////////////////////////////////////////////////////////////////////////////////////////////////
		
		else if(buttonType == ButtonType.EXPORT_EDIT) {
			clickedCallback = ((Export_Main)mGUIManager).OnButtonExportEditClicked;
		}
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Show(float delay = 0, float duration = Globals.ANIMATIONDURATION, bool reactivate=true)
	{
		if(TVR.Utils.Message.State==TVR.Utils.Message.States.Running)
			return;

		bReactivate = reactivate;

		if(texDisabled!=null){
			if(bReactivate){
				renderer.material.mainTexture = texUnchecked;
			}else{
				renderer.material.mainTexture = texDisabled;
			}
		}
		mFade.Reset(bReactivate ? 1.0f : fAlphaDisabled, duration, true, delay);

		if(mGUIText){
			mGUIText.gameObject.GetComponent<GUITextController>().Show(delay, duration, bReactivate ? 1.0f : fAlphaDisabled);
		}
		if(mGUITextBottom){
			if(renderer.material.color.a!=0.0f){
				mGUITextBottom.gameObject.GetComponent<GUITextController>().Show(0f, 0f, bReactivate ? 1.0f : 0.3f);
			}else{
				mGUITextBottom.gameObject.GetComponent<GUITextController>().Show(delay, duration, bReactivate ? 1.0f : 0.3f);
			}
		}

		state = States.fade_in;
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Hide(float delay = 0, float duration = Globals.ANIMATIONDURATION)
	{
		if(TVR.Utils.Message.State==TVR.Utils.Message.States.Running)
			return;

		mFade.Reset(0f, duration, true, delay);

		if(mGUIText)
			mGUIText.gameObject.GetComponent<GUITextController>().Hide(delay, duration);

		if(mGUITextBottom)
			mGUITextBottom.gameObject.GetComponent<GUITextController>().Hide(delay, duration);

		state = States.fade_out;
	}
}



