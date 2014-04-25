using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonBarChapters : ButtonBar
{
	int numChapters;
	bool bAddingChapter;

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

		ButtonPressed(listButtons[numChapters].GetComponent<BasicButton>());
		listButtons[0].GetComponent<BasicButton>().Hide(0 ,0.2f);
		listButtons[0].GetComponent<BasicButton>().Checked=true;

		if(numChapters==1)
			mGUIManager.mMainButtonBar.GetComponent<ButtonBar>().EnableButtons();

		bAddingChapter=true;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void Update()
	{
		base.Update();

		if(bAddingChapter && listButtons[0].GetComponent<BasicButton>().state == BasicButton.States.hidden){
			listButtons[0].transform.position = listButtons[numChapters].transform.position - new Vector3(0, buttonMargin+buttonSize, 0);
			listButtons[0].GetComponent<BasicButton>().Show(0, 0.2f);
			bAddingChapter=false;
			listButtons[numChapters].GetComponent<BasicButton>().Checked=true;

			ResizeButtonBar();
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void ResizeButtonBar()
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
}



