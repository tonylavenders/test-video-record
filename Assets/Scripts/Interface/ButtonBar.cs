using UnityEngine;
using System.Collections;
using TVR;
using TVR.Utils;
using TVR.Helpers;

public class ButtonBar : MonoBehaviour
{
	public GameObject[] mButtons;
	public GameObject Separator;
	GameObject[] mButtonsInstances;

	public float depth_x = 0;
	//public const float vSpeed = 15;
	//public const float smoothTime = 0.8f;
	bool bInit=false;

	float pos_x, pos_y, scale_x, scale_y;
	//float desplY = 0.0f;
	//float velY = 0.0f;
	//float v;

	SmoothStep mFade;
	float mFadeEndValue;

	enum States{
		fade_out,
		hidden,
		idle,
		touch,
	}
	States state;

	const float buttonZDepth = 10;
	const float buttonBarZDepth = 20;
	const float buttonBarRatio = 0.1f;
	const float buttonMarginRatio = 0.0125f;
	const float buttonRatio = 0.088f;
	float buttonSize;
	float buttonMargin;

	public GUIManager mGUIManager;

	const int MAX_BUTTONS=5;

	public int lastButtonSel=0;

	float[] mSpeedsY;
	int mSpeedPos;
	SmoothStep mSpeed;

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Awake()
	{
		mButtonsInstances = new GameObject[mButtons.Length];
		mGUIManager = transform.parent.GetComponent<GUIManager>();

		mFadeEndValue=1.0f;
		mFade = new SmoothStep(0.0f,0.0f,1.0f,false);

		mSpeedsY = new float[Globals.SPEEDS];
		mSpeedPos = 0;
		mSpeed = new SmoothStep(0, 0, Globals.BRAKEDURATION, false, 0);
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
		pos_x = (scale_x/2.0f) + scale_x*depth_x + depth_x;

		//If more than 5 buttons, then the first button is top aligned
		if(mButtons.Length>MAX_BUTTONS)
			pos_y = Screen.height/2-(transform.localScale.y-Screen.height)/2;
		else
			pos_y = Screen.height/2.0f;

		transform.position = new Vector3(pos_x, pos_y, buttonBarZDepth);
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Set the position of the first button, then set the position of the other buttons
	void SetButtons()
	{
		mButtonsInstances[0] = Instantiate(mButtons[0]) as GameObject;
		mButtonsInstances[0].transform.position = new Vector3(scale_x/2.0f, (mButtons.Length-1)*(buttonSize/2+buttonMargin/2) + Screen.height/2, buttonZDepth);
		mButtonsInstances[0].transform.localScale = new Vector3(buttonSize, buttonSize, 1);
		mButtonsInstances[0].transform.parent = transform;
		mButtonsInstances[0].GetComponent<BasicButton>().SetID(1);

		for(int i=1;i<mButtonsInstances.Length;i++){
			mButtonsInstances[i] = Instantiate(mButtons[i]) as GameObject;
			mButtonsInstances[i].transform.position = new Vector3(scale_x/2.0f, mButtonsInstances[0].transform.position.y-(buttonSize+buttonMargin)*i, buttonZDepth);
			mButtonsInstances[i].transform.localScale = new Vector3(buttonSize, buttonSize, 1);
			mButtonsInstances[i].transform.parent = transform;
			mButtonsInstances[i].GetComponent<BasicButton>().SetID(i+1);
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

		//Check if user is touching the button bar
		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if(Input.GetMouseButtonDown(0) && collider.Raycast(ray, out hit, 1000.0f)) {
			state = States.touch;
			mSpeedPos = 0;
			for(int i = 0; i < Globals.SPEEDS; i++) {
				mSpeedsY[i] = 0.0f;
			}
			mSpeed.End();
		}
		/*else if(InputHelp.GetMouseButtonUp(0)) {
			state = States.idle;
		}*/

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

		//Move the button bar
		/*v = InputHelp.mouseDeltaPositionYDown.y/15.0f;
		if(InputHelp.GetMouseButton(0) && state==States.touch){
			float desplYcopia = desplY;
			desplY = v * vSpeed;
			desplY = Mathf.Lerp(desplY, desplYcopia, smoothTime);
		}

		desplY = Mathf.SmoothDamp(desplY, 0.0f, ref velY, smoothTime);*/
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
	public void ButtonPressed(int id)
	{
		foreach(GameObject button in mButtonsInstances){
			button.GetComponent<BasicButton>().UnCheck(id);
		}
		mSpeed.End();
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Show()
	{
		if(!bInit)
			Init();

		mFadeEndValue=1.0f;
		mFade.Reset(mFadeEndValue, Globals.ANIMATIONDURATION);

		foreach(GameObject button in mButtonsInstances){
			button.GetComponent<BasicButton>().Show();
		}

		state=States.idle;
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void Hide()
	{
		if(!bInit)
			Init();

		mFadeEndValue=0.0f;
		mFade.Reset(mFadeEndValue, Globals.ANIMATIONDURATION);
	
		foreach(GameObject button in mButtonsInstances){
			button.GetComponent<BasicButton>().Hide();
		}

		state=States.fade_out;
	}
}



