using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TVR;

public class ButtonBarElements : ButtonBar
{
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

	public override void Show(bool bReactivate)
	{
		base.Show(bReactivate);

		if(elementType==ElementTypes.chapters && Data.selChapter!=null){
			currentSelected=listButtons[Data.selChapter.Number].GetComponent<BasicButton>();
			currentSelected.Checked=true;
			GoToButtonPosition(currentSelected.transform);
		}
		else if(elementType==ElementTypes.blocks && listButtons.Count>1){
			currentSelected=listButtons[1].GetComponent<BasicButton>();
			currentSelected.Checked=true;
		}
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	int GetCount()
	{
		if(elementType==ElementTypes.chapters){
			return Data.Chapters.Count;
		}
		else if(elementType==ElementTypes.blocks){
			return Data.selChapter.Blocks.Count;
		}
		return 0;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void SetPosition()
	{
		//Set the button bar in the initial position and set the buttons
		transform.position = new Vector3(ButtonProperties.buttonBarScaleX/2.0f, Screen.height/2.0f, ButtonProperties.buttonBarZDepth);
		SetButtons();
		
		//Move the button bar to the correct position (buttons are moved with the button bar)
		if(align==Aligns.left){
			pos_x = (ButtonProperties.buttonBarScaleX/2.0f) + ButtonProperties.buttonBarScaleX*depth_x + depth_x;
		}
		else if(align==Aligns.right){
			pos_x = Screen.width - (ButtonProperties.buttonBarScaleX/2.0f) + ButtonProperties.buttonBarScaleX*depth_x + depth_x;
		}
		
		transform.position = new Vector3(pos_x, transform.position.y, ButtonProperties.buttonBarZDepth);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	protected override void SetButtons()
	{
		//Init button bar
		float buttonsTotalHeight = ButtonProperties.buttonSize*((GetCount()+1)) + ButtonProperties.buttonMargin*(GetCount()+2);
		if(buttonsTotalHeight > Screen.height){
			float new_pos_y = Screen.height - buttonsTotalHeight/2.0f;
			transform.position = new Vector3(transform.position.x, new_pos_y, transform.position.z);
			transform.localScale = new Vector3(ButtonProperties.buttonBarScaleX, buttonsTotalHeight, 1);
		}

		Vector3 init_pos = new Vector3(transform.position.x, Screen.height-ButtonProperties.buttonMargin-ButtonProperties.buttonSize/2, ButtonProperties.buttonZDepth);
		int i=1;

		//Add elements button [+]
		listButtons.Add(Instantiate(mButtons[0]) as GameObject);

		//Chapters buttons
		if(elementType==ElementTypes.chapters){
			foreach(Data.Chapter chapter in Data.Chapters){
				listButtons.Add(Instantiate(mButtons[1]) as GameObject);
				listButtons[i].transform.position = init_pos - new Vector3(0, (ButtonProperties.buttonSize + ButtonProperties.buttonMargin)*(i-1), 0);
				listButtons[i].transform.localScale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);
				listButtons[i].transform.parent = transform;
				listButtons[i].GetComponent<BasicButton>().iObj = chapter;
				listButtons[i].GetComponent<BasicButton>().Refresh();
				listButtons[i].GetComponent<BasicButton>().Show(0, 0.2f);
				if(Data.selChapter!=null && Data.selChapter.Id==i){
					currentSelected=listButtons[i].GetComponent<BasicButton>();
					currentSelected.Checked=true;
				}
				i++;
			}
		}
		//Blocks buttons
		else if(elementType==ElementTypes.blocks){
			foreach(Data.Chapter.Block block in Data.selChapter.Blocks){
				listButtons.Add(Instantiate(mButtons[1]) as GameObject);
				listButtons[i].transform.position = init_pos - new Vector3(0, (ButtonProperties.buttonSize + ButtonProperties.buttonMargin)*(i-1), 0);
				listButtons[i].transform.localScale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);
				listButtons[i].transform.parent = transform;
				listButtons[i].GetComponent<BasicButton>().iObj = block;
				listButtons[i].GetComponent<BasicButton>().Refresh();
				listButtons[i].GetComponent<BasicButton>().Show(0, 0.2f);
				i++;
			}
		}

		listButtons[0].transform.position = init_pos - new Vector3(0, (ButtonProperties.buttonSize + ButtonProperties.buttonMargin)*(i-1), 0);
		listButtons[0].transform.localScale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);
		listButtons[0].transform.parent = transform;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonAddElementClicked(BasicButton sender)
	{
		int counter=0;

		if(elementType==ElementTypes.chapters){	
			iObject newChapter;
			newChapter = Data.newChapter("", "", -1, -1, null);
			listButtons.Add(Instantiate(mButtons[1]) as GameObject);
			counter=GetCount();
			listButtons[counter].GetComponent<BasicButton>().iObj = newChapter;
			mGUIManager.CurrentCharacter=null;
			mGUIManager.CurrentBackground=null;
		}
		else if(elementType==ElementTypes.blocks){
			iObject newBlock;
			newBlock = Data.selChapter.newBlock(Data.Chapter.Block.blockTypes.Time, Data.Chapter.Block.shotTypes.CloseUP, Data.Chapter.Block.filterType.Off, 25, 1, 1, null);
			listButtons.Add(Instantiate(mButtons[1]) as GameObject);
			counter=GetCount();
			listButtons[counter].GetComponent<BasicButton>().iObj = newBlock;
		}

		listButtons[counter].transform.position = listButtons[0].transform.position;
		listButtons[counter].transform.localScale = new Vector3(ButtonProperties.buttonSize, ButtonProperties.buttonSize, 1);
		listButtons[counter].transform.parent = transform;
		listButtons[counter].GetComponent<BasicButton>().Refresh();
		listButtons[counter].GetComponent<BasicButton>().Show(0.2f, 0.2f);
		ButtonPressed(listButtons[counter].GetComponent<BasicButton>());

		listButtons[0].GetComponent<BasicButton>().Hide(0 ,0.2f);
		listButtons[0].GetComponent<BasicButton>().Checked=true;

		stateElements = StatesElements.adding_element;

		mGUIManager.HideAllButtonBars();
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	public void OnButtonDeleteElementClicked(BasicButton sender)
	{
		if(currentSelected==null)
			return;
		if(elementType == ElementTypes.chapters) {
			TVR.Utils.Message.Show(1, "AVISO", "\u00BFDesea eliminar el cap\u00EDtulo seleccionado?", TVR.Utils.Message.Type.YesNo, "S\u00ED", "No", Message_Delete);
		} else if(elementType == ElementTypes.blocks) {
			TVR.Utils.Message.Show(1, "AVISO", "\u00BFDesea eliminar el bloque seleccionado?", TVR.Utils.Message.Type.YesNo, "S\u00ED", "No", Message_Delete);
		}
		mGUIManager.blur = true;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	private void Message_Delete(TVR.Utils.Message.ButtonClicked buttonClicked, int Identifier)
	{
		if(buttonClicked == TVR.Utils.Message.ButtonClicked.Yes){
			currentSelected.Hide(0, 0.2f);
			currentSelected.iObj.Delete();
			MoveButtonsAfterDelete();
			stateElements=StatesElements.deleting_element;
			mSpeed.End();

			mGUIManager.DisableButtons();
			mGUIManager.HideAllButtonBars();

			if(elementType==ElementTypes.chapters){
				mGUIManager.CurrentCharacter=null;
				mGUIManager.CurrentBackground=null;
				//mGUIManager.mInput.Text="";
				//mGUIManager.mInput.enable=false;
			}
		}
		mGUIManager.blur = false;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void MoveButtonsAfterDelete()
	{
		float dist_y_above = (ButtonProperties.buttonSize + ButtonProperties.buttonMargin)/2.0f;
		float dist_y_below = (ButtonProperties.buttonSize + ButtonProperties.buttonMargin)/2.0f;
		float first_min_y = Screen.height - ButtonProperties.buttonMargin - ButtonProperties.buttonSize/2.0f;
		float last_max_y = ButtonProperties.buttonMargin + ButtonProperties.buttonSize/2.0f;

		//Last button y max limit
		if(listButtons[0].transform.position.y + dist_y_below >= last_max_y){
			dist_y_below = last_max_y - listButtons[0].transform.position.y;
			dist_y_above += (ButtonProperties.buttonSize+ButtonProperties.buttonMargin)/2.0f - dist_y_below;
		}
		//First button y min limit (first button has priority over last button)
		if(listButtons[1].transform.position.y-dist_y_above <= first_min_y){
			dist_y_above = listButtons[1].transform.position.y - first_min_y;
			dist_y_below = ButtonProperties.buttonSize+ButtonProperties.buttonMargin - dist_y_above;
		}
		//Move buttons above selected element
		for(int i=currentSelected.iObj.Number-1;i>=1;i--){
			float new_y = listButtons[i].transform.position.y - dist_y_above;
			listButtons[i].GetComponent<BasicButton>().GoToPosition(new_y, 0.2f);
		}
		//Move buttons below selected element
		for(int i=currentSelected.iObj.Number+1;i<=GetCount()+1;i++){
			float new_y = listButtons[i].transform.position.y + dist_y_below;
			listButtons[i].GetComponent<BasicButton>().GoToPosition(new_y, 0.2f);
			listButtons[i].GetComponent<BasicButton>().Refresh();
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
			listButtons[0].transform.position = listButtons[GetCount()].transform.position - new Vector3(0, ButtonProperties.buttonMargin+ButtonProperties.buttonSize, 0);
			listButtons[0].GetComponent<BasicButton>().Show(0, 0.2f);
			listButtons[GetCount()].GetComponent<BasicButton>().Checked=true;
			ResizeButtonBarAfterAdd();
			stateElements=StatesElements.idle;
		}
		//Deleting elements (destroy button object)
		else if(stateElements==StatesElements.deleting_element && 
		        listButtons[0].GetComponent<BasicButton>().state == BasicButton.States.idle &&
		        (listButtons[1].GetComponent<BasicButton>().state == BasicButton.States.idle || 
		         listButtons[1].GetComponent<BasicButton>().state == BasicButton.States.hidden)){
			listButtons.RemoveAt(currentSelected.iObj.Number);
			Destroy(currentSelected.gameObject);
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




