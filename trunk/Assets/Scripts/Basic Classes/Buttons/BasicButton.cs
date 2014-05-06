using UnityEngine;
using TVR.Helpers;

namespace TVR.Button
{	
	public class BasicButton {	
		private const int MAXDISABLEBUTTONS = 450; //((15^2)*2) Máximo desplazamiento antes de desactivar los botones 15 pixeles.

		public Texture TexDown { get; set; }  
		public Texture TexUp { get; set; }
		protected Texture TexDownDisable { get; set; }
		protected Texture TexUpDisable { get; set; }
		
		protected Rect rec;		
		
		private bool mEnabled = true;
		private bool mPressed = false;
		private bool mChecked = false;
		protected bool mKeepState = true;
		protected bool mJustPressed = false;
		protected bool mJustReleased = false;
		private Vector2 mMouseInit;
		private Vector2 mMovement;
		public ScaleMode scaleMode;
		public bool disableOnMouseMove;

		private static float mSharedTime;
		private static BasicButton mSharedAnyPressed = null;

		public bool enable {
			get {
				return mEnabled;
			}
			set {
				mEnabled = value;
			}
		}
		public bool pressed {
			get {
				return mPressed;
			}
		}
		public bool justPressed {
			get {
				return mJustPressed;// Input.GetMouseButtonUp(0) && rec.Contains (GetMousePos());
			}
		}
		public bool justReleased {
			get {
				return mJustReleased;// Input.GetMouseButtonUp(0) && mPressed;
			}
		}
		public bool Checked {
			get {
				return mChecked;
			}
			set {
				mChecked = value;
			}
		}
		public bool keepState {
			get {
				return mKeepState;
			}
			set {
				mKeepState = value;
			}
		}
		public static bool anyButtonPressed {
			get { return mSharedAnyPressed != null; }
		}
		public static bool anyButtonJustPressed {
			get { return mSharedTime == Time.time; }
		}

		public BasicButton(Rect r, Texture down, Texture up, Texture disableDown, Texture disableUp, bool bKeepSt = true) {
			TexDown = down;
			TexUp = up;
			TexDownDisable = disableDown;
			TexUpDisable = disableUp;
			rec = r;
			mKeepState = bKeepSt;
			scaleMode = ScaleMode.ScaleToFit;
			disableOnMouseMove = true;
		}

		public static void mouseUp() {
			if(mSharedAnyPressed != null) {
				if(QueueManager.isEmpty) {
					Debug.Log("El último botón pulsado no ha recibido el update en el que se ha levantado el botón del mouse. Revisar el código.");
					if(mSharedAnyPressed is TextButton) {
						TextButton b = (TextButton)mSharedAnyPressed;
						Debug.Log("Su texto es: " + b.Text);
					}
					Debug.Log("Su posición es: " + mSharedAnyPressed.rec);
					Debug.Log("Hacerle un reset cuando deje de recibir updates.");
				}
				mSharedAnyPressed.Reset();
			}
		}
							
		public bool IsPressed() {
			/*if(mKeepState)
				return mChecked;*/
			return mPressed;
		}
		
		public virtual void Reset() {
			/*if(mPressed)
				OnButtonReleased();*/
			
			mEnabled = true;
			mPressed = false;
			mChecked = false;
			mJustPressed = false;
			mJustReleased = false;
			mSharedAnyPressed = null;
		}
		
		// Update is called once per frame
		public virtual void Update() {
			mJustPressed = false;
			mJustReleased = false;
			if(!mEnabled || mSharedTime == Time.time || (mSharedAnyPressed != this && mSharedAnyPressed != null))
				return;
			
			Vector2 mousePos = InputHelp.mousePosition;
			if(InputHelp.GetMouseButtonDown(0)) {
				if(rec.Contains(mousePos)) {
					mMouseInit = mousePos;
					mMovement = Vector2.zero;
					mPressed = true;
					mJustPressed = true;
					OnButtonPressed();
					mSharedTime = Time.time;
					mSharedAnyPressed = this;
				}
			} else if(mPressed) {
				if(InputHelp.GetMouseButton(0)) {
					if(!rec.Contains(mousePos) || InputHelp.fingerChange) {
						mPressed = false;
						OnButtonReleasedNotContained();
						mSharedAnyPressed = null;
					} else if(disableOnMouseMove) {
						mMovement += mousePos - mMouseInit;
						if(mMovement.sqrMagnitude > MAXDISABLEBUTTONS) {
							mMovement.x = 300;
							mPressed = false;
							OnButtonReleasedNotContained();
							mSharedAnyPressed = null;
						}
					}
					mMouseInit = mousePos;
				} else if(InputHelp.GetMouseButtonUp(0)) {
					if(mKeepState) {
						mChecked = !mChecked;
					}
					mJustReleased = true;
					mSharedAnyPressed = null;
					mPressed = false;
					OnButtonReleased();
				}
			}
		}
		
		public virtual void OnButtonPressed () {
			if(!mEnabled)
				return;				
		}
		
		public virtual void OnButtonReleased () {
			if(!mEnabled)
				return;				
		}
		
		public virtual void OnButtonReleasedNotContained () {
			if(!mEnabled)
				return;
		}
		
		public void OnGUI () {
			if(Event.current.type != EventType.Repaint)
				return;
			Texture tex;
			if(!mEnabled) {
				if(!mKeepState) {
					if(mPressed) {
						tex = TexDownDisable;
					} else {
						tex = TexUpDisable;
					}
				} else {
					if(mChecked) {
						tex = TexDownDisable;
					} else {
						tex = TexUpDisable;					
					}
				}
			} else {
				if(!mKeepState) {
					if(mPressed) {
						tex = TexDown;
					} else {
						tex = TexUp;
					}
				} else {
					if(mChecked) {
						tex = TexDown;
					} else {
						tex = TexUp;					
					}
				}
			}
			if(tex != null)
				GUI.DrawTexture(rec, tex, scaleMode, true);
		}
	}
}