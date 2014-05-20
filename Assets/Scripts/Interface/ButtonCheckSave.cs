using UnityEngine;
using System.Collections;
using TVR.Helpers;
using TVR;

public class ButtonCheckSave : MonoBehaviour
{
	BasicButton mBasicButton;
	ButtonBar mButtonBar;
	bool bInit=false;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Init()
	{
		mBasicButton = GetComponent<BasicButton>();
		mButtonBar = mBasicButton.mButtonBar;
		bInit=true;
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Update()
	{
		if(!bInit)
			Init();

		if(Camera.main==null){
			return;
		}

		if(!mBasicButton.bClickable && TVR.Utils.Message.State==TVR.Utils.Message.States.Hide){
			if(((GUIManagerBlocks)mBasicButton.mGUIManager).LastSaved){
				mBasicButton.bClickable=true;
				mBasicButton.Checked=true;
				if(mBasicButton.clickedCallback!=null){
					mBasicButton.clickedCallback(null);
				}
			}else{
				return;
			}
		}

		//Check if button is clicked and SoundRecorder haven't saved the changes yet
		if(mBasicButton.Enable && mBasicButton.state==BasicButton.States.idle /*&& mSharedTime != Time.time*/){
			RaycastHit hit;
			Ray ray = Camera.main.ScreenPointToRay(InputHelp.mousePositionYDown);

			if(InputHelp.GetMouseButtonDown(0)){
				if(collider.Raycast(ray, out hit, 1000.0f)){
					if(mBasicButton.mGUIManager is GUIManagerBlocks){
						if(!((GUIManagerBlocks)mBasicButton.mGUIManager).LastSaved){
							mBasicButton.bClickable=false;
							if(mButtonBar!=null && mButtonBar.elementType==ButtonBar.ElementTypes.blocks){
								if(Data.selChapter!=null && Data.selChapter.selBlock!=null){
									mBasicButton.mGUIManager.SaveWarning(Data.selChapter.selBlock, mButtonBar.currentSelected);
								}
							}else if(mButtonBar==null || mButtonBar.elementType==ButtonBar.ElementTypes.main || mButtonBar.elementType==ButtonBar.ElementTypes.time){
								mBasicButton.mGUIManager.SaveWarning(null, null);
							}
						}
					}
				}
			}
		}
	}
}



