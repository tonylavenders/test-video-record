using UnityEngine;
using System.Collections;
using TVR;
using TVR.Utils;

public class ButtonBar : MonoBehaviour
{
	public GameObject[] mButtons;
	public GameObject mSeparator;
	GameObject[] mButtonsInstances;

	public float depth_x = 0;
	public float vSpeed = 15;
	public float smoothTime = 0.8f;

	float pos_x, pos_y, scale_x, scale_y;
	float desplY = 0.0f;
	float velY = 0.0f;

	SmoothStep mFade;
	float mFadeStartValue;
	float mFadeEndValue;

	enum States{
		idle,
		touch,
	}
	States state;

	const float buttonZDepth = 10;
	const float buttonBarZDepth = 20;
	float buttonBarRatio = 0.1f;
	float buttonMarginRatio = 0.0125f;
	float buttonRatio = 0.088f;
	float buttonSize;
	float buttonMargin;

	public GUIManager mGUIManager;

	const int MAX_BUTTONS=5;

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Awake()
	{
		mButtonsInstances = new GameObject[mButtons.Length];
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		mGUIManager = transform.parent.GetComponent<GUIManager>();

		SetScale ();
		SetPosition();
		SetSeparator();

		mFadeStartValue=0.0f;
		mFadeEndValue=1.0f;
		mFade = new SmoothStep(mFadeStartValue,mFadeEndValue,1.0f,false);

		Show();
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
		if(mSeparator!=null){
			mSeparator.transform.localScale = new Vector3(1/mSeparator.transform.lossyScale.x, 1, 1);
			mSeparator.transform.position = new Vector3(scale_x+0.5f, Screen.height/2, buttonBarZDepth);
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void Update()
	{
		//Fade
		if(mFade.Enable)
			mFade.Update();

		Color c = renderer.material.color;
		renderer.material.color = new Color(c.r, c.g, c.b, mFade.Value);

		if(mFadeEndValue==0.0f && mFade.Value<0.001f)
			Destroy(gameObject);

		//Check if user is touching the button bar
		float v = Input.GetAxis("Mouse Y");

		RaycastHit hit;
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		if(Input.GetMouseButton(0) && collider.Raycast(ray, out hit, 1000.0f)){
			state=States.touch;
		}
		else{
			state=States.idle;
		}

		//Move the button bar
		if(state==States.touch){
			float desplYcopia = desplY;
			desplY = v * vSpeed;
			desplY = Mathf.Lerp(desplY, desplYcopia, smoothTime);
		}

		desplY = Mathf.SmoothDamp(desplY, 0.0f, ref velY, smoothTime);
		float min_y = Screen.height/2-(transform.localScale.y-Screen.height)/2;
		float max_y = Screen.height/2+(transform.localScale.y-Screen.height)/2;
		float new_pos_y = Mathf.Clamp(transform.position.y+desplY, min_y, max_y);
		transform.position = new Vector3(transform.position.x, new_pos_y, transform.position.z);

		if(mSeparator!=null)
			mSeparator.SetActive(mGUIManager.bChildActive);
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//A button informs to the ButtonBar that is pressed, ButtonBar informs all buttons to get unchecked
	public void ButtonPressed(int id)
	{
		foreach(GameObject button in mButtonsInstances){
			button.GetComponent<BasicButton>().UnCheck(id);
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Show()
	{
		mFadeEndValue=1.0f;
		mFade.Reset(mFadeEndValue, Globals.ANIMATIONDURATION);

		foreach(GameObject button in mButtonsInstances){
			button.GetComponent<BasicButton>().Show();
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void Hide()
	{
		mFadeEndValue=0.0f;
		mFade.Reset(mFadeEndValue, Globals.ANIMATIONDURATION);

		foreach(GameObject button in mButtonsInstances){
			button.GetComponent<BasicButton>().Hide();
		}
	}
}



