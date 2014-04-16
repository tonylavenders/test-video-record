using UnityEngine;
using System.Collections;

public class ButtonBar : MonoBehaviour
{
	public bool isMain;
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
	float buttonBarRatio = 0.1f;
	float buttonMarginRatio = 0.0125f;
	float buttonRatio = 0.088f;

	float buttonSize;
	float buttonMargin;

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Start()
	{
		state=States.idle;

		if(!isMain || (isMain && CheckRatios())){
			buttonSize = Screen.width*buttonRatio;
			buttonMargin = Screen.width*buttonMarginRatio;
			GetScale();
		}

		GetPosition();

		transform.localScale = new Vector3(scale_x, scale_y, 1);
		transform.position = new Vector3(pos_x, pos_y, buttonBarZDepth);

		SetButtons();
	}

	//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Si no caben 5 botones en la botonera principal hay que reajustar los ratios
	//Puede ocurrir con ratios de pantalla muy largos -> 2:1
	bool CheckRatios()
	{
		float totalHeight = 5*buttonRatio*Screen.width+6*buttonMarginRatio*Screen.width;
		if(totalHeight > Screen.height){
			//Los botones son 7 veces mas grandes que los margenes entre ellos
			buttonMargin = Screen.height/41;
			buttonSize = 7*buttonMargin;
			scale_x = 1.13f*buttonSize;
			scale_y = Screen.height;
			return false;
		}
		return true;
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
		pos_x = scale_x/2.0f;
		pos_y = Screen.height/2.0f;
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








