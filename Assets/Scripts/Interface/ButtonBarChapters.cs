using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ButtonBarChapters : ButtonBar
{
	int numChapters;

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
		//listButtons[0].transform.position = listButtons[numChapters].transform.position - new Vector3(0, buttonMargin+buttonSize, 0);
		listButtons[0].GetComponent<BasicButton>().Hide();
		listButtons[numChapters].transform.localScale = new Vector3(buttonSize, buttonSize, 1);
		listButtons[numChapters].transform.parent = transform;
		listButtons[numChapters].GetComponent<BasicButton>().Show();

		mGUIManager.mMainButtonBar.GetComponent<ButtonBar>().EnableButtons();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

}


