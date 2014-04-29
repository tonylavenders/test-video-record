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

		stateChapter=StatesChapter.deleting_chapter;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void Update()
	{
		base.Update();

		//Adding chapter
		if(stateChapter==StatesChapter.adding_chapter && listButtons[0].GetComponent<BasicButton>().state == BasicButton.States.hidden){
			listButtons[0].transform.position = listButtons[numChapters].transform.position - new Vector3(0, buttonMargin+buttonSize, 0);
			listButtons[0].GetComponent<BasicButton>().Show(0, 0.2f);
			listButtons[numChapters].GetComponent<BasicButton>().Checked=true;
			ResizeButtonBarAfterAdd();
			stateChapter=StatesChapter.idle;
		}
		//Deleting chapter
		else if(stateChapter==StatesChapter.deleting_chapter){
			mGUIManager.mMainButtonBar.GetComponent<ButtonBar>().DisableButtons();
			if(currentSelected.state == BasicButton.States.hidden){
				MoveOtherButtons();
			}
		}
		//Moving buttons after delete chapter
		else if((stateChapter==StatesChapter.moving_buttons_up && listButtons[0].GetComponent<BasicButton>().state == BasicButton.States.idle) ||
		        (stateChapter==StatesChapter.moving_buttons_down && listButtons[1].GetComponent<BasicButton>().state == BasicButton.States.idle)){
			listButtons.RemoveAt(currentSelected.mID);
			numChapters--;
			ResizeButtonBarAfterDelete();
			if(stateChapter!=StatesChapter.button_adjusment){
				Destroy(currentSelected.gameObject);
				currentSelected=null;
				stateChapter=StatesChapter.idle;
			}
		}
		//special case deleting chapter
		else if(stateChapter==StatesChapter.button_adjusment && listButtons[0].GetComponent<BasicButton>().state == BasicButton.States.idle){
			//Attach buttons
			foreach(GameObject button in listButtons){
				button.transform.parent=transform;
			}
			Destroy(currentSelected.gameObject);
			currentSelected=null;
			stateChapter=StatesChapter.idle;
		}
		Debug.Log(listButtons[0].GetComponent<BasicButton>().state);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Move buttons after delete chapter
	void MoveOtherButtons()
	{
		//Move buttons up
		if(transform.position.y<=Screen.height/2){
			if(currentSelected.mID<numChapters){
				for(int i=currentSelected.mID+1;i<=numChapters;i++){
					listButtons[i].GetComponent<BasicButton>().GoToPosition(listButtons[i-1].transform.position.y);
					listButtons[i].GetComponent<BasicButton>().ChangeID(i-1);
				}
			}
			listButtons[0].GetComponent<BasicButton>().GoToPosition(listButtons[numChapters].transform.position.y);
			stateChapter=StatesChapter.moving_buttons_up;
		}
		//Move buttons down
		else{
			if(currentSelected.mID>1){
				for(int i=currentSelected.mID+1;i<=numChapters;i++){
					listButtons[i].GetComponent<BasicButton>().ChangeID(i-1);
				}
				for(int i=currentSelected.mID-1;i>=1;i--){
					listButtons[i].GetComponent<BasicButton>().GoToPosition(listButtons[i+1].transform.position.y);
				}
			}
			stateChapter=StatesChapter.moving_buttons_down;
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
		float buttonsTotalHeight = buttonSize*listButtons.Count + buttonMargin*(listButtons.Count+1);

		//button bar is still longer than screen height
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
				button.transform.parent=transform; 
			}
		}
		//button bar is shorter than screen height just after delete one button
		else if(buttonsTotalHeight+buttonSize+buttonMargin > Screen.height)
		{
			//Detach buttons
			foreach(GameObject button in listButtons){
				button.transform.parent=null;
			}
			//Resize button bar
			scale_y = Screen.height;
			transform.localScale = new Vector3(scale_x, scale_y, 1);

			//Move buttonbar to the correct position
			float correct_y = (listButtons[1].transform.position.y+listButtons[0].transform.position.y)/2.0f;
			transform.position = new Vector3(transform.position.x, correct_y, transform.position.z);

			AlignButtonsTop();

			//Center button bar
			transform.position = new Vector3(transform.position.x, Screen.height/2.0f, transform.position.z);
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	//Align buttons on top screen after delete chapter (only special case)
	void AlignButtonsTop()
	{
		float init_y = Screen.height-buttonMargin-buttonSize/2.0f;
		listButtons[1].GetComponent<BasicButton>().GoToPosition(init_y);

		for(int i=2;i<=numChapters;i++){
			listButtons[i].GetComponent<BasicButton>().GoToPosition(init_y-(buttonMargin+buttonSize)*(i-1));
		}
		listButtons[0].GetComponent<BasicButton>().GoToPosition(init_y-(buttonMargin+buttonSize)*numChapters);

		stateChapter=StatesChapter.button_adjusment;
	}
}




