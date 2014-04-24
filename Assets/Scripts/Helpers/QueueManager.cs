using UnityEngine;
using System.Collections;
//using System.Collections.Generic;
using System;
using TVR.Helpers;

class QueueManager : MonoBehaviour {
	static Queue[] mQueue = new Queue[Enum.GetNames(typeof(Priorities)).Length];
	/*static Queue mQueueHighPriority = new Queue();
	static Queue mQueueLowPriority = new Queue();*/
	static System.Threading.Thread t = null;
	static int mFramesDelayCounter = 0;
	public static int mFramesDelay = 0;
	public static bool pause = false;
	public static bool mPauseOnButtonDown = true;
	public static bool debug = false;
	public static int framesDelay {
		get { return mFramesDelay; }
		set {
			mFramesDelay = value;
			mFramesDelayCounter = value;
		}
	}
	public static bool pauseOnButtonDown {
		get { return mPauseOnButtonDown; }
		set {
			mPauseOnButtonDown = value;
			if(!value)
				mFramesDelayCounter = mFramesDelay;
		}
	}
	public static bool isEmpty { 
		get {
			for(int i = 0; i < mQueue.Length; ++i)
				if(mQueue[i].Count != 0)
					return false;
			return true;
		}
	}
	public static int count {
		get {
			int result = 0;
			for(int i = 0; i < mQueue.Length; ++i)
				result += mQueue[i].Count;
			return result;
		}
	}
	public enum Priorities {
		Highest,
		High,
		Normal,
		Low,
		Lowest
	}

	void Awake() {
		for(int i = 0; i < mQueue.Length; ++i)
			mQueue[i] = new Queue();
	}

	public static bool contains(QueueManagerAction action) {
		for(int i = 0; i < mQueue.Length; ++i)
			if(mQueue[i].Contains(action))
				return true;
		return false;
	}

	public static QueueManagerAction pop() {
		for(int i = 0; i < mQueue.Length; ++i)
			if(mQueue[i].Count > 0)
				return (QueueManagerAction)mQueue[i].Dequeue();
		return null;
	}

	public static void removeAction(QueueManagerAction action) {
		Queue q;
		for(int i = 0; i < mQueue.Length; ++i)
			if(mQueue[i].Contains(action)) {
				q = new Queue();
				foreach(QueueManagerAction act in mQueue[i]) {
					if(action != act)
						q.Enqueue(act);
				}
				mQueue[i] = q;
			}
	}

	public static void add(QueueManagerAction function, Priorities priority = Priorities.Normal) {
		mQueue[(int)priority].Enqueue(function);
	}

	public static void processQueue() {
		while(!isEmpty)
			pop().function.Invoke();
	}

	void Update() {
		if(mPauseOnButtonDown && (InputHelp.GetAnyMouseButton || InputHelp.GetAnyMouseButtonDown || Input.anyKey || Input.anyKeyDown))
			mFramesDelayCounter = 90;

		if(!pause) {
			if(mFramesDelayCounter <= 0) {
				if(t == null) {
					for(int i = 0; i < mQueue.Length; ++i) {
						if(mQueue[i].Count != 0) {
							QueueManagerAction action = (QueueManagerAction)mQueue[i].Dequeue();
							if(debug)
								Debug.Log("Invocado acción " + action + " de prioridad " + i + ".");
							if(action.function.Target is System.Threading.Thread) {
								t = (System.Threading.Thread)action.function.Target;
								t.Start();
								while(!t.IsAlive);
							} else
								action.function.Invoke();
							mFramesDelayCounter = mFramesDelay;
							return;
						}
					}
				} else if(!t.IsAlive)
					t = null;
			} else
				mFramesDelayCounter--;
		}
	}

	public static void clear() {
		for(int i = 0; i < mQueue.Length; ++i)
			mQueue[i].Clear();
	}

	public static void clear(string sender) {
		Queue q;
		for(int i = 0; i < mQueue.Length; ++i) {
			q = new Queue();
			foreach(QueueManagerAction act in mQueue[i]) {
				if(sender != act.sender)
					q.Enqueue(act);
			}
			mQueue[i] = q;
		}
	}

	public static void printActionList() {
		for(int i = 0; i < mQueue.Length; ++i) {
			Debug.Log("Lista de prioridad: " + i);
			foreach(QueueManagerAction act in mQueue[i]) {
				Debug.Log(act);
			}
		}
	}

	public static void addGCCollectionMode(Priorities priority, string Sender = "GC") {
		if(debug)
			QueueManager.add(new QueueManager.QueueManagerAction(Sender, () => Debug.Log("Memory used before collection:     " + System.GC.GetTotalMemory(false)), "GC.GetTotalMemory(false)"), priority);
		QueueManager.add(new QueueManager.QueueManagerAction(Sender, () => GC(), "GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced)"), priority);
		if(debug)
			QueueManager.add(new QueueManager.QueueManagerAction(Sender, () => Debug.Log("Memory used after full collection: " + System.GC.GetTotalMemory(true)), "GC.GetTotalMemory(true)"), priority);
	}

	private static void GC() {
		for(int i = 0; i < 7; ++i)
			System.GC.Collect(System.GC.MaxGeneration, System.GCCollectionMode.Forced);
	}

	public class QueueManagerAction {
		public string sender;
		public Action function;
		private string mDescription;

		public QueueManagerAction(string Sender, Action Function, string Description) {
			sender = Sender;
			function = Function;
			mDescription = Description;
		}

		public override string ToString() {
			return mDescription;
		}
	}
}