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

	public float depth_x = 0;
	bool bInit=false;

	protected float pos_x, pos_y, scale_x, scale_y;

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

	protected const float buttonZDepth = 10;
	const float buttonBarZDepth = 20;
	const float buttonBarRatio = 0.1f;
	const float buttonMarginRatio = 0.0125f;
	const float buttonRatio = 0.088f;
	protected float buttonSize;
	protected float buttonMargin;

	public GUIManager mGUIManager;

	const int MAX_BUTTONS=5;

	BasicButton currentSelected=null;

	float[] mSpeedsY;
	int mSpeedPos;
	SmoothStep mSpeed;

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
		float totalHeight = 5*buttonRatio*Screen.width+6*buttonMarginRatio*Screen.width;

		if(totalHeight > Screen.height){
			//The relation between buttons and margins is 7-1
			buttonMargin = Screen.height/41;
			buttonSize = 7*buttonMargin;
			scale_x = 1.13f*buttonSize;
		}
		else{
			buttonMargin = Screen.width*buttonMarginRatio;
			buttonSize = Screen.width*buttonRatio;
			scale_x = Screen.width*buttonBarRatio;
		}
		float buttonsTotalHeight = buttonSize*mButtons.Length + buttonMargin*(mButtons.Length+1);
		scale_y = Mathf.Max(buttonsTotalHeight, Screen.height);

		transform.localScale = new Vector3(scale_x, scale_y, 1);
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void SetPosition()
	{
		//Set the button bar in the initial position and set the buttons
		transform.position = new Vector3(scale_x/2.0f, Screen.height/2.0f, buttonBarZDepth);
		SetButtons();

		//Move the button bar to the correct position (buttons are moved with the button bar)
		if(align==Aligns.left){
			pos_x = (scale_x/2.0f) + scale_x*depth_x + depth_x;
		}
		else if(align==Aligns.right){
			pos_x = Screen.width - (scale_x/2.0f) + scale_x*depth_x + depth_x;
		}

		//If more than 5 buttons, then the first button is top aligned
		if(mButtons.Length>MAX_BUTTONS)
			pos_y = Screen.height/2-(transform.localScale.y-Screen.height)/2;
		else
			pos_y = Screen.height/2.0f;

		transform.position = new Vector3(pos_x, pos_y, buttonBarZDepth);
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Set the position of the first button, then set the position of the other buttons
	protected virtual void SetButtons()
	{
		listButtons.Add(Instantiate(mButtons[0]) as GameObject);
		listButtons[0].transform.position = new Vector3(scale_x/2.0f, (mButtons.Length-1)*(buttonSize/2+buttonMargin/2) + Screen.height/2, buttonZDepth);
		listButtons[0].transform.localScale = new Vector3(buttonSize, buttonSize, 1);
		listButtons[0].transform.parent = transform;
		listButtons[0].GetComponent<BasicButton>().SetID(1);

		if(mButtons.Length>1){
			for(int i=1;i<mButtons.Length;i++){
				listButtons.Add(Instantiate(mButtons[i]) as GameObject);
				listButtons[i].transform.position = new Vector3(scale_x/2.0f, listButtons[0].transform.position.y-(buttonSize+buttonMargin)*i, buttonZDepth);
				listButtons[i].transform.localScale = new Vector3(buttonSize, buttonSize, 1);
				listButtons[i].transform.parent = transform;
				listButtons[i].GetComponent<BasicButton>().SetID(i+1);
			}
		}
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void SetSeparator()
	{
		if(Separator!=null){
			Separator.transform.localScale = new Vector3(1/Separator.transform.lossyScale.x, 1, 1);
			Separator.transform.position = new Vector3(scale_x+0.5f, Screen.height/2, buttonBarZDepth);
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	protected virtual void Update()
	{
		if(state==States.hidden || Camera.main == null)
			return;
		//MoveY
		if(mMoveY.Update() == SmoothStep.State.inFade){
			transform.position = new Vector3(transform.position.x, mMoveY.Value, transform.position.z);
			if(!mMoveY.Ended)
				return;
		}

		//Fade
		if(mFade.Update() == SmoothStep.State.inFade) {
			if(mFade.Ended) {
				if(mFade.Value == 0)
					state = States.hidden;
				else
					state = States.idle;
			}
			Color c = renderer.material.color;
			renderer.material.color = new Color(c.r, c.g, c.b, mFade.Value);
		}

		//Check if user is touching the button bar
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if(state == States.idle && Input.GetMouseButtonDown(0) && collider.Raycast(ray, out hit, 1000.0f)) {
			state = States.touch;
			mSpeedPos = 0;
			for(int i = 0; i < Globals.SPEEDS; i++) {
				mSpeedsY[i] = 0.0f;
			}
			mSpeed.End();
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

	public void EnableButtons()
	{
		foreach(GameObject button in listButtons){
			button.GetComponent<BasicButton>().Enable = true;
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected void GoToButtonPosition(Transform button)
	{
		//Button outside top screen
		if(button.position.y+buttonSize/2 > Screen.height){
			float finalYbutton = Screen.height-buttonMargin-buttonSize/2;
			float finalY = transform.position.y-(button.position.y-finalYbutton);
			mMoveY.Reset(transform.position.y, finalY, Globals.ANIMATIONDURATION);
		}
		//Button outside bottom screen
		else if(button.position.y-buttonSize/2 < 0){
			float finalYbutton = buttonMargin+buttonSize/2;
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



