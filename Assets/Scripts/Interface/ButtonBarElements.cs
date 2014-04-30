using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonBarElements : ButtonBar
{
	int numElements;
	enum StatesElements{
		idle,
		adding_element,
		deleting_element,
		moving_buttons_up,
		moving_buttons_down,
		button_adjusment
	}
	StatesElements stateElements=StatesElements.idle;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void SetButtons()
	{
		//No elementss in DB yet
		if(/*num. elementss in DB==0*/ true){
			listButtons.Add(Instantiate(mButtons[0]) as GameObject);
			listButtons[0].transform.position = new Vector3(ButtonProperties.buttonBarScaleX/2.0f, Screen.height-ButtonProperties.buttonMargin-ButtonProperties.buttonSize/2, ButtonProperties.buttonZDepth);
			listButtons[0].transform.localScale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);
			listButtons[0].transform.parent = transform;
			numElements=0;
		}
		//Create buttons for elementss in DB
		else{
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonAddElementPressed(BasicButton sender)
	{
		numElements++;

		//Add elements button
		listButtons.Add(Instantiate(mButtons[1]) as GameObject);
		listButtons[numElements].transform.position = listButtons[0].transform.position;
		listButtons[numElements].transform.localScale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);
		listButtons[numElements].transform.parent = transform;
		listButtons[numElements].GetComponent<BasicButton>().Show(0.2f, 0.2f);
		listButtons[numElements].GetComponent<BasicButton>().Text+=numElements.ToString("00");
		listButtons[numElements].GetComponent<BasicButton>().SetID(numElements);

		ButtonPressed(listButtons[numElements].GetComponent<BasicButton>());
		listButtons[0].GetComponent<BasicButton>().Hide(0 ,0.2f);
		listButtons[0].GetComponent<BasicButton>().Checked=true;

		stateElements=StatesElements.adding_element;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonDeleteElementPressed(BasicButton sender)
	{
		if(currentSelected==null)
			return;

		currentSelected.Hide(0, 0.2f);
		MoveButtonsAfterDelete();
		stateElements=StatesElements.deleting_element;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void MoveButtonsAfterDelete()
	{
		float dist_y_above = (ButtonProperties.buttonSize+ButtonProperties.buttonMargin)/2.0f;
		float dist_y_below = (ButtonProperties.buttonSize+ButtonProperties.buttonMargin)/2.0f;
		float first_min_y = Screen.height-ButtonProperties.buttonMargin-ButtonProperties.buttonSize/2.0f;
		float last_max_y = ButtonProperties.buttonMargin+ButtonProperties.buttonSize/2.0f;

		//First button y min limit
		if(listButtons[1].transform.position.y-dist_y_above <= first_min_y){
			dist_y_above = listButtons[1].transform.position.y - first_min_y;
			dist_y_below += (ButtonProperties.buttonSize+ButtonProperties.buttonMargin)/2.0f - dist_y_above;
		}
		//Last button y max limit
		else if(listButtons[0].transform.position.y+dist_y_below >= last_max_y){
			dist_y_below = last_max_y - listButtons[0].transform.position.y;
			dist_y_above += (ButtonProperties.buttonSize+ButtonProperties.buttonMargin)/2.0f - dist_y_below;

			//Check don't break the previous calculation for the first button
			if(listButtons[1].transform.position.y - dist_y_above <= first_min_y){
				dist_y_above = (ButtonProperties.buttonSize+ButtonProperties.buttonMargin)/2.0f;
				dist_y_below = (ButtonProperties.buttonSize+ButtonProperties.buttonMargin)/2.0f;
			}
		}
		//Move buttons above selected element
		for(int i=currentSelected.mID-1;i>=1;i--){
			float new_y = listButtons[i].transform.position.y - dist_y_above;
			listButtons[i].GetComponent<BasicButton>().GoToPosition(new_y, 0.2f);
		}
		//Move buttons below selected element
		for(int i=currentSelected.mID+1;i<=numElements;i++){
			float new_y = listButtons[i].transform.position.y + dist_y_below;
			listButtons[i].GetComponent<BasicButton>().GoToPosition(new_y, 0.2f);
			listButtons[i].GetComponent<BasicButton>().ChangeID(i-1);
		}
		//Move add button
		float new_y_0 = listButtons[0].transform.position.y + dist_y_below;
		listButtons[0].GetComponent<BasicButton>().GoToPosition(new_y_0, 0.2f);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void Update()
	{
		base.Update();

		//Adding elements (show add elements button)
		if(stateElements==StatesElements.adding_element && listButtons[0].GetComponent<BasicButton>().state == BasicButton.States.hidden){
			listButtons[0].transform.position = listButtons[numElements].transform.position - new Vector3(0, ButtonProperties.buttonMargin+ButtonProperties.buttonSize, 0);
			listButtons[0].GetComponent<BasicButton>().Show(0, 0.2f);
			listButtons[numElements].GetComponent<BasicButton>().Checked=true;
			ResizeButtonBarAfterAdd();
			stateElements=StatesElements.idle;
		}
		//Deleting elements (destroy button object)
		else if(stateElements==StatesElements.deleting_element && 
		        listButtons[0].GetComponent<BasicButton>().state == BasicButton.States.idle &&
		        (listButtons[1].GetComponent<BasicButton>().state == BasicButton.States.idle || 
		         listButtons[1].GetComponent<BasicButton>().state == BasicButton.States.hidden)){
			listButtons.RemoveAt(currentSelected.mID);
			Destroy(currentSelected.gameObject);
			numElements--;
			currentSelected=null;
			stateElements=StatesElements.idle;
			ResizeButtonBarAfterDelete();
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void ResizeButtonBarAfterAdd()
	{
		float buttonsTotalHeight = ButtonProperties.buttonSize*listButtons.Count + ButtonProperties.buttonMargin*(listButtons.Count+1);
		if(buttonsTotalHeight > Screen.height)
		{
			//Detach buttons
			foreach(GameObject button in listButtons){
				button.transform.parent=null;
			}
			//Resize button bar
			scale_y = buttonsTotalHeight;
			transform.localScale = new Vector3(ButtonProperties.buttonBarScaleX, scale_y, 1);
			
			//Move buttonbar to the correct position
			float correct_y = (listButtons[1].transform.position.y+listButtons[0].transform.position.y)/2.0f;
			transform.position = new Vector3(transform.position.x, correct_y, transform.position.z);
			
			//Attach buttons
			foreach(GameObject button in listButtons){
				button.transform.parent=transform; }
			
			//Align to bottom
			GoToButtonPosition(listButtons[0].transform);
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void ResizeButtonBarAfterDelete()
	{
		//Detach buttons
		foreach(GameObject button in listButtons){
			button.transform.parent=null;
		}
		//Resize button bar
		float buttonsTotalHeight = ButtonProperties.buttonSize*listButtons.Count + ButtonProperties.buttonMargin*(listButtons.Count+1);
		scale_y = Mathf.Max(Screen.height, buttonsTotalHeight);
		transform.localScale = new Vector3(ButtonProperties.buttonBarScaleX, scale_y, 1);
			
		//Move buttonbar to the correct position
		if(buttonsTotalHeight<=Screen.height){
			transform.position = new Vector3(transform.position.x, Screen.height/2.0f, transform.position.z);
		}else{
			float correct_y = (listButtons[1].transform.position.y+listButtons[0].transform.position.y)/2.0f;
			transform.position = new Vector3(transform.position.x, correct_y, transform.position.z);
		}
		//Attach buttons
		foreach(GameObject button in listButtons){
			button.transform.parent=transform;
		}
	}
}




