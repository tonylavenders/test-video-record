using UnityEngine;
using System.Collections;

public class ButtonBar : MonoBehaviour
{
	public GameObject[] mButtons;

	public float offset_x;
	public float max_y;
	public float min_y;
	public float vSpeed = 15;
	public float smoothTime = 0.8f;

	float pos_x, pos_y, scale_x, scale_y;

	float desplY = 0.0f;
	float velY = 0.0f;
	
	enum States{
		idle,
		touch
	}
	States state;

	const float buttonZDepth = 10;
	const float buttonBarZDepth = 20;
	const float buttonBarRatio = 0.1f;
	const float buttonMarginRatio = 0.0125f;
	const float buttonRatio = 0.09f;

	float buttonSize;
	float buttonMargin;

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		state=States.idle;

		buttonSize = Screen.width*buttonRatio;
		buttonMargin = Screen.width*buttonMarginRatio;

		//(Screen.width*buttonBarRatio)/2, Screen.height/2, Screen.width*buttonBarRatio, Screen.height

		GetScale();
		GetPosition();

		transform.localScale = new Vector3(scale_x, scale_y, 1);
		transform.position = new Vector3(pos_x, pos_y, buttonBarZDepth);

		SetButtons();
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void GetScale()
	{
		scale_x = Screen.width*buttonBarRatio;
		float buttonsTotalHeight = buttonSize*mButtons.Length + buttonMargin*(mButtons.Length+1);
		scale_y = Mathf.Max(buttonsTotalHeight, Screen.height);
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void GetPosition()
	{
		pos_x = scale_x/2;
		pos_y = Screen.height/2;
	}
	
	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void SetButtons()
	{
		GameObject button1 = Instantiate(mButtons[0]) as GameObject;
		button1.transform.position = new Vector3(pos_x, (mButtons.Length-1)*(buttonSize/2+buttonMargin/2) + Screen.height/2, buttonZDepth);
		button1.transform.localScale = new Vector3(buttonSize, buttonSize, 1);
		button1.name = "Button_01";
		button1.transform.parent = transform;

		for(int i=1;i<mButtons.Length;i++){
			GameObject buttonI = Instantiate(mButtons[i]) as GameObject;
			buttonI.transform.position = new Vector3(pos_x, button1.transform.position.y-(buttonSize+buttonMargin)*i, buttonZDepth);
			buttonI.transform.localScale = new Vector3(buttonSize, buttonSize, 1);
			buttonI.name = "Button_0"+(i+1).ToString();
			buttonI.transform.parent = transform;
		}
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void Update()
	{
		float v = Input.GetAxis("Mouse Y");

		//Check if user is touching the button bar
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
		float new_pos_y = Mathf.Clamp(transform.position.y+desplY, Screen.height/2-(transform.localScale.y-Screen.height)/2, Screen.height/2+(transform.localScale.y-Screen.height)/2);
		transform.position = new Vector3(transform.position.x, new_pos_y, transform.position.z);
	}
}








