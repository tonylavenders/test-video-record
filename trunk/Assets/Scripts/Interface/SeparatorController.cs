using UnityEngine;
using System.Collections;
using TVR.Utils;
using TVR;

public class SeparatorController : MonoBehaviour
{
	SmoothStep mFade;

	enum States{
		fade_out,
		hidden,
		fade_in,
		idle,
	}
	States state;

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	void Awake()
	{
		mFade = new SmoothStep(0.0f,0.0f,1.0f,false);
	}

	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

	void Update()
	{
		if(state == States.hidden || Camera.main == null)
			return;

		SmoothStep.State SSState = mFade.Update();
		if(SSState == SmoothStep.State.inFade || SSState==SmoothStep.State.justEnd) {
			if(SSState==SmoothStep.State.justEnd) {
				if(mFade.Value == 0)
					state = States.hidden;
				else
					state = States.idle;
			}
			Color c = renderer.material.color;
			renderer.material.color = new Color(c.r, c.g, c.b, mFade.Value);
		}
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Show(float delay = 0, float duration = Globals.ANIMATIONDURATION)
	{
		mFade.Reset(1f, duration, true, delay);
		state = States.fade_in;
	}
	
	////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
	
	public void Hide(float delay = 0, float duration = Globals.ANIMATIONDURATION)
	{
		mFade.Reset(0f, duration, true, delay);
		state = States.fade_out;
	}
}



