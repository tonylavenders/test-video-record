using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TVR;
using TVR.Utils;
using TVR.Helpers;

public class ButtonBar : MonoBehaviour
{
	public GameObject[] mButtons;
	protected List<GameObject> listButtons;
	public GameObject Separator;
	public bool bIsMain;

	public float depth_x = 0;
	bool bInit=false;

	protected float pos_x, pos_y, scale_y;

	SmoothStep mFade;
	SmoothStep mMoveY;

	public enum Aligns{
		left,
		right
	}
	public Aligns align;

	enum States{
		fade_out,
		hidden,
		fade_in,
		idle,
		touch,
	}
	States state;

	public enum ElementTypes{
		main,
		chapters,
		blocks,
		characters,
		backgrounds,
		animations,
		expressions
	}
	public ElementTypes elementType;

	public GUIManager mGUIManager;

	const int MAX_BUTTONS=5;

	BasicButton mCurrentSelected=null;
	protected BasicButton currentSelected{
		get { return mCurrentSelected; }
		set {
			if(mCurrentSelected==null && value!=null)
				mGUIManager.EnableButtons();
			mCurrentSelected=value;
		}
	}

	float[] mSpeedsY;
	int mSpeedPos;
	protected SmoothStep mSpeed;

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Awake()
	{
		listButtons = new List<GameObject>();
		mGUIManager = transform.parent.GetComponent<GUIManager>();

		mFade = new SmoothStep(0.0f,0.0f,1.0f,false);
		mMoveY = new SmoothStep(0.0f,0.0f,1.0f,false);

		mSpeedsY = new float[Globals.SPEEDS];
		mSpeedPos = 0;
		mSpeed = new SmoothStep(0, 0, Globals.BRAKEDURATION, false, 0);

		state=States.hidden;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Init()
	{
		SetScale();
		SetPosition();
		SetSeparator();

		bInit=true;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Main button bar must have 5 buttons, if it doesn't fit then it's necessary to recalculate the ratios
	//This happens with very large ratios -> 2:1
	void SetScale()
	{
		float buttonsTotalHeight = ButtonProperties.buttonSize*mButtons.Length + ButtonProperties.buttonMargin*(mButtons.Length+1);
		float scale_y = Mathf.Max(buttonsTotalHeight, Screen.height);

		transform.localScale = new Vector3(ButtonProperties.buttonBarScaleX, scale_y, 1);
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected virtual void SetPosition()
	{
		//Set the button bar in the initial position and set the buttons
		transform.position = new Vector3(ButtonProperties.buttonBarScaleX/2.0f, Screen.height/2.0f, ButtonProperties.buttonBarZDepth);
		SetButtons();

		//Move the button bar to the correct position (buttons are moved with the button bar)
		if(align==Aligns.left){
			pos_x = (ButtonProperties.buttonBarScaleX/2.0f) + ButtonProperties.buttonBarScaleX*depth_x + depth_x;
		}
		else if(align==Aligns.right){
			pos_x = Screen.width - (ButtonProperties.buttonBarScaleX/2.0f) + ButtonProperties.buttonBarScaleX*depth_x + depth_x;
		}

		//If more than 5 buttons, then the first button is top aligned
		if(mButtons.Length>MAX_BUTTONS)
			pos_y = Screen.height/2-(transform.localScale.y-Screen.height)/2;
		else
			pos_y = Screen.height/2.0f;

		transform.position = new Vector3(pos_x, pos_y, ButtonProperties.buttonBarZDepth);
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Set the position of the first button, then set the position of the other buttons
	protected virtual void SetButtons()
	{
		float buttonsTotalHeight = ButtonProperties.buttonSize*mButtons.Length + ButtonProperties.buttonMargin*(mButtons.Length+1);
		float y_pos=0;
		if(buttonsTotalHeight>=Screen.height || bIsMain){
			y_pos = (mButtons.Length-1)*(ButtonProperties.buttonSize/2+ButtonProperties.buttonMargin/2) + Screen.height/2;
		}else{
			y_pos = Screen.height-ButtonProperties.buttonMargin-ButtonProperties.buttonSize/2.0f;
		}
		
		listButtons.Add(Instantiate(mButtons[0]) as GameObject);
		listButtons[0].transform.position = new Vector3(ButtonProperties.buttonBarScaleX/2.0f, y_pos, ButtonProperties.buttonZDepth);
		listButtons[0].transform.localScale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);
		listButtons[0].transform.parent = transform;

		if(mButtons.Length>1){
			for(int i=1;i<mButtons.Length;i++){
				listButtons.Add(Instantiate(mButtons[i]) as GameObject);
				listButtons[i].transform.position = new Vector3(ButtonProperties.buttonBarScaleX/2.0f, listButtons[0].transform.position.y-(ButtonProperties.buttonSize+ButtonProperties.buttonMargin)*i, ButtonProperties.buttonZDepth);
				listButtons[i].transform.localScale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);
				listButtons[i].transform.parent = transform;
			}
		}
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void SetSeparator()
	{
		if(Separator!=null){
			Separator.transform.localScale = new Vector3(1/Separator.transform.lossyScale.x, 1, 1);
			Separator.transform.position = new Vector3(ButtonProperties.buttonBarScaleX+0.5f, Screen.height/2, ButtonProperties.buttonBarZDepth);
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	protected virtual void Update()
	{
		if(state==States.hidden || Camera.main == null)
			return;

		//MoveY
		SmoothStep.State SSState = mMoveY.Update();
		if(SSState == SmoothStep.State.inFade || SSState == SmoothStep.State.justEnd) {
			transform.position = new Vector3(transform.position.x, mMoveY.Value, transform.position.z);
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
		//Check if user is touching the button bar
		if(state == States.idle && Input.GetMouseButtonDown(0)){
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(InputHelp.mousePositionYDown);
			if(collider.Raycast(ray, out hit, 1000.0f)){
				state = States.touch;
				mSpeedPos = 0;
				for(int i = 0; i < Globals.SPEEDS; i++) {
					mSpeedsY[i] = 0.0f;
				}
				mSpeed.End();
				mMoveY.End();
			}
		}

		if(state == States.touch) {
			if(InputHelp.GetMouseButtonUp(0)) {
				for(int i = 0; i < Globals.SPEEDS; i++)
					mSpeed.Value += mSpeedsY[i];

				mSpeed.Value = mSpeed.Value / Globals.SPEEDS;
				if(Mathf.Abs(mSpeed.Value) < Globals.MINIMUMSPEED)
					mSpeed.Value = 0;
				else
					mSpeed.Reset(0.0f, Globals.BRAKEDURATION, true);
				state = States.idle;
			} else {
				mSpeedsY[mSpeedPos] = InputHelp.mouseDeltaPositionYDown.y;
				mSpeed.Value = InputHelp.mouseDeltaPositionYDown.y;
				mSpeedPos++;
				if(mSpeedPos == Globals.SPEEDS)
					mSpeedPos = 0;
			}
		} else
			mSpeed.Update();

		if(mSpeed.Value != 0) {
			float desplY = mSpeed.Value;
			float min_y = Screen.height / 2 - (transform.localScale.y - Screen.height) / 2;
			float max_y = Screen.height / 2 + (transform.localScale.y - Screen.height) / 2;
			float new_pos_y = Mathf.Clamp(transform.position.y + desplY, min_y, max_y);
			transform.position = new Vector3(transform.position.x, new_pos_y, transform.position.z);
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//A button informs to the ButtonBar that is pressed, ButtonBar informs all buttons to get unchecked
	public void ButtonPressed(BasicButton sender)
	{
		currentSelected=sender;

		foreach(GameObject button in listButtons){
			BasicButton b = button.GetComponent<BasicButton>();
			if(sender!=b)
				b.Checked=false;
		}
		mSpeed.End();
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void SetCurrentButton(int id_button)
	{
		if(!bInit)
			Init();

		foreach(GameObject button in listButtons){
			BasicButton b = button.GetComponent<BasicButton>();
			if(b.ID==id_button){
				currentSelected=b;
				b.Checked=true;
			}else{
				b.Checked=false;
			}
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void EnableButtons()
	{
		foreach(GameObject button in listButtons){
			button.GetComponent<BasicButton>().Enable = true;
		}
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void DisableButtons()
	{
		foreach(GameObject button in listButtons){
			button.GetComponent<BasicButton>().Enable = false;
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void UncheckButtons()
	{
		foreach(GameObject button in listButtons){
			button.GetComponent<BasicButton>().Checked=false;
		}
		currentSelected=null;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected void GoToButtonPosition(Transform button)
	{
		//Button outside top screen
		if(button.position.y+ButtonProperties.buttonSize/2 > Screen.height){
			float finalYbutton = Screen.height-ButtonProperties.buttonMargin-ButtonProperties.buttonSize/2;
			float finalY = transform.position.y-(button.position.y-finalYbutton);
			mMoveY.Reset(transform.position.y, finalY, Globals.ANIMATIONDURATION);
		}
		//Button outside bottom screen
		else if(button.position.y-ButtonProperties.buttonSize/2 < 0){
			float finalYbutton = ButtonProperties.buttonMargin+ButtonProperties.buttonSize/2;
			float finalY = transform.position.y+(finalYbutton-button.position.y);
			mMoveY.Reset(transform.position.y, finalY, Globals.ANIMATIONDURATION);
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Show()
	{
		if(!bInit)
			Init();

		//First time buttonbar is opened
		if(mGUIManager.Counter==0){
			mFade.Reset(1f, Globals.ANIMATIONDURATION);
			state=States.fade_in;
			foreach(GameObject button in listButtons){
				button.GetComponent<BasicButton>().Show(); 
			}
		}
		else{
			mFade.Value=1f;
			Color c = renderer.material.color;
			renderer.material.color = new Color(c.r, c.g, c.b, 1.0f); 
			state=States.idle;
			foreach(GameObject button in listButtons){
				button.GetComponent<BasicButton>().Show(0.2f, 0.2f); 
			}
		}

		if(currentSelected!=null)
			GoToButtonPosition(currentSelected.transform);

		mSpeed.End();
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void Hide()
	{
		if(mGUIManager.Counter==1){
			mFade.Reset(0f, Globals.ANIMATIONDURATION);
			state=States.fade_out;
		}
		else{
			mFade.Value=0f;
			Color c = renderer.material.color;
			renderer.material.color = new Color(c.r, c.g, c.b, 0.0f);
			state=States.hidden;
		}

		foreach(GameObject button in listButtons){
			button.GetComponent<BasicButton>().Hide(0, 0.2f);
		}
	}
}



