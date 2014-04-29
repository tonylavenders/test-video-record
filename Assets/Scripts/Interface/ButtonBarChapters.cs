using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonBarChapters : ButtonBar
{
	int numChapters;
	enum StatesChapter{
		idle,
		adding_chapter,
		deleting_chapter,
		moving_buttons_up,
		moving_buttons_down,
		button_adjusment
	}
	StatesChapter stateChapter=StatesChapter.idle;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void SetButtons()
	{
		//No chapters in DB yet
		if(/*num. chapters in DB==0*/ true){
			listButtons.Add(Instantiate(mButtons[0]) as GameObject);
			listButtons[0].transform.position = new Vector3(scale_x/2.0f, Screen.height-buttonMargin-buttonSize/2, buttonZDepth);
			listButtons[0].transform.localScale = new Vector3(buttonSize, buttonSize, 1);
			listButtons[0].transform.parent = transform;
			numChapters=0;
		}
		//Create buttons for chapters in DB
		else{
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonAddChapterPressed(BasicButton sender)
	{
		numChapters++;

		//Add chapter button
		listButtons.Add(Instantiate(mButtons[1]) as GameObject);
		listButtons[numChapters].transform.position = listButtons[0].transform.position;
		listButtons[numChapters].transform.localScale = new Vector3(buttonSize, buttonSize, 1);
		listButtons[numChapters].transform.parent = transform;
		listButtons[numChapters].GetComponent<BasicButton>().Show(0.2f, 0.2f);
		listButtons[numChapters].GetComponent<BasicButton>().Text+=numChapters.ToString("00");
		listButtons[numChapters].GetComponent<BasicButton>().SetID(numChapters);

		ButtonPressed(listButtons[numChapters].GetComponent<BasicButton>());
		listButtons[0].GetComponent<BasicButton>().Hide(0 ,0.2f);
		listButtons[0].GetComponent<BasicButton>().Checked=true;

		if(numChapters==1)
			mGUIManager.mMainButtonBar.GetComponent<ButtonBar>().EnableButtons();

		stateChapter=StatesChapter.adding_chapter;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonDeleteChapterPressed(BasicButton sender)
	{
		if(currentSelected==null)
			return;

		currentSelected.Hide(0, 0.2f);
		MoveButtonsAfterDelete();
		stateChapter=StatesChapter.deleting_chapter;
		mGUIManager.mMainButtonBar.GetComponent<ButtonBar>().DisableButtons();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void MoveButtonsAfterDelete()
	{
		float dist_y_above = (buttonSize+buttonMargin)/2.0f;
		float dist_y_below = (buttonSize+buttonMargin)/2.0f;
		float first_min_y = Screen.height-buttonMargin-buttonSize/2.0f;
		float last_max_y = buttonMargin+buttonSize/2.0f;

		//First button y min limit
		if(listButtons[1].transform.position.y-dist_y_above <= first_min_y){
			dist_y_above = listButtons[1].transform.position.y - first_min_y;
			dist_y_below += (buttonSize+buttonMargin)/2.0f - dist_y_above;
		}
		//Last button y max limit
		else if(listButtons[0].transform.position.y+dist_y_below >= last_max_y){
			dist_y_below = last_max_y - listButtons[0].transform.position.y;
			dist_y_above += (buttonSize+buttonMargin)/2.0f - dist_y_below;

			//Check don't break the previous calculation for the first button
			if(listButtons[1].transform.position.y - dist_y_above <= first_min_y){
				dist_y_above = (buttonSize+buttonMargin)/2.0f;
				dist_y_below = (buttonSize+buttonMargin)/2.0f;
			}
		}
		//Move buttons above selected chapter
		for(int i=currentSelected.mID-1;i>=1;i--){
			float new_y = listButtons[i].transform.position.y - dist_y_above;
			listButtons[i].GetComponent<BasicButton>().GoToPosition(new_y, 0.2f);
		}
		//Move buttons below selected chapter
		for(int i=currentSelected.mID+1;i<=numChapters;i++){
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

		//Adding chapter (show add chapter button)
		if(stateChapter==StatesChapter.adding_chapter && listButtons[0].GetComponent<BasicButton>().state == BasicButton.States.hidden){
			listButtons[0].transform.position = listButtons[numChapters].transform.position - new Vector3(0, buttonMargin+buttonSize, 0);
			listButtons[0].GetComponent<BasicButton>().Show(0, 0.2f);
			listButtons[numChapters].GetComponent<BasicButton>().Checked=true;
			ResizeButtonBarAfterAdd();
			stateChapter=StatesChapter.idle;
		}
		//Deleting chapter (destroy button object)
		else if(stateChapter==StatesChapter.deleting_chapter && 
		        listButtons[0].GetComponent<BasicButton>().state == BasicButton.States.idle &&
		        (listButtons[1].GetComponent<BasicButton>().state == BasicButton.States.idle || 
		         listButtons[1].GetComponent<BasicButton>().state == BasicButton.States.hidden)){
			listButtons.RemoveAt(currentSelected.mID);
			Destroy(currentSelected.gameObject);
			numChapters--;
			currentSelected=null;
			stateChapter=StatesChapter.idle;
			ResizeButtonBarAfterDelete();
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void ResizeButtonBarAfterAdd()
	{
		float buttonsTotalHeight = buttonSize*listButtons.Count + buttonMargin*(listButtons.Count+1);
		if(buttonsTotalHeight > Screen.height)
		{
			//Detach buttons
			foreach(GameObject button in listButtons){
				button.transform.parent=null;
			}
			//Resize button bar
			scale_y = buttonsTotalHeight;
			transform.localScale = new Vector3(scale_x, scale_y, 1);
			
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
		float buttonsTotalHeight = buttonSize*listButtons.Count + buttonMargin*(listButtons.Count+1);
		scale_y = Mathf.Max(Screen.height, buttonsTotalHeight);
		transform.localScale = new Vector3(scale_x, scale_y, 1);
			
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




