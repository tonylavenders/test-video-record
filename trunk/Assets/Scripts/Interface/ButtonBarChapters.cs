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
		listButtons[numChapters].GetComponent<BasicButton>().mGUIText.guiText.text+=numChapters.ToString("00");

		ButtonPressed(listButtons[numChapters].GetComponent<BasicButton>());
		listButtons[0].GetComponent<BasicButton>().Hide(0 ,0.2f);
		listButtons[0].GetComponent<BasicButton>().Checked=true;

		if(numChapters==1)
			mGUIManager.mMainButtonBar.GetComponent<ButtonBar>().EnableButtons();

		bAddingChapter=true;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void ResizeButtonBar()
	{
		float buttonsTotalHeight = buttonSize*listButtons.Count + buttonMargin*(listButtons.Count+1);
		if(buttonsTotalHeight > Screen.height)
		{
			//Move buttons to correct place
			listButtons[1].transform.position = new Vector3(listButtons[1].transform.position.x, (listButtons.Count-1)*(buttonSize/2+buttonMargin/2) + Screen.height/2, buttonZDepth);
			for(int i=2;i<listButtons.Count;i++){
				listButtons[i].transform.position = new Vector3(listButtons[1].transform.position.x, listButtons[1].transform.position.y-(buttonSize+buttonMargin)*(i-1), buttonZDepth);
			}
			listButtons[0].transform.position = new Vector3(listButtons[1].transform.position.x, listButtons[1].transform.position.y-(buttonSize+buttonMargin)*(listButtons.Count-1), buttonZDepth);

			//Detach buttons
			foreach(GameObject button in listButtons){
				button.transform.parent=null;
			}
			//Resize button bar
			scale_y = buttonsTotalHeight;
			transform.localScale = new Vector3(scale_x, scale_y, 1);
			transform.position = new Vector3(transform.position.x, Screen.height/2, transform.position.z);

			//Attach buttons
			foreach(GameObject button in listButtons){
				button.transform.parent=transform;
			}

			//Align to bottom
			pos_y = Screen.height/2+(transform.localScale.y-Screen.height)/2;
			transform.position = new Vector3(transform.position.x, pos_y, transform.position.z);
		}
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
}



