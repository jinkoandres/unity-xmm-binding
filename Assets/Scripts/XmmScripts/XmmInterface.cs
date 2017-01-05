using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO; // File operations
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public class XmmInterface : MonoBehaviour {

	//================== XMM lib functions ==================//
	#if UNITY_IOS
	[DllImport ("__Internal")]
	private static extern float addPhrase(String label, String[] colNames, int nCols, IntPtr phrase, int phraseSize);

	[DllImport ("__Internal")]
	private static extern int getSetSize();

	[DllImport ("__Internal")]
	private static extern IntPtr getLastPhrase();

	[DllImport ("__Internal")]
	private static extern void train(int nGaussians);

	[DllImport ("__Internal")]
	private static extern int getNbOfModels();

	//========= XMM import / export : (TODO implement this in the lib)

	[DllImport ("__Internal")]
	private static extern IntPtr getModels();

	[DllImport ("__Internal")]
	private static extern void setModels(string smodels);

	[DllImport ("__Internal")]
	private static extern void clearModels();

	[DllImport ("__Internal")]
	private static extern IntPtr getTrainingSet();

	[DllImport ("__Internal")]
	private static extern void setTrainingSet(string strainingset);

	[DllImport ("__Internal")]
	private static extern void clearTrainingSet();

	[DllImport ("__Internal")]
	private static extern void clearLabel(string label);

	//======== use XMM library :

	[DllImport ("__Internal")]
	private static extern void setLikelihoodWindow(int w);

	[DllImport ("__Internal")]
	private static extern void filter(IntPtr obs, int obsSize);

	[DllImport("__Internal")]
	private static extern IntPtr getLikeliest();
	/*
	[DllImport ("__Internal")]
	private static extern int setString(string s);

	[DllImport ("__Internal")]
	private static extern int getString(StringBuilder s);
	*/
	#else 
	[DllImport ("XMMEngine", EntryPoint = "addPhrase")]
	private static extern float addPhrase(String label, String[] colNames, int nCols, IntPtr phrase, int phraseSize);

	[DllImport ("XMMEngine", EntryPoint = "getSetSize")]
	private static extern int getSetSize();

	[DllImport ("XMMEngine", EntryPoint = "getLastPhrase")]
	private static extern IntPtr getLastPhrase();

	[DllImport ("XMMEngine", EntryPoint = "train")]
	private static extern void train(int nGaussians);

	[DllImport ("XMMEngine", EntryPoint = "getNbOfModels")]
	private static extern int getNbOfModels();

	//========= XMM import / export : (TODO implement this in the lib)

	[DllImport ("XMMEngine", EntryPoint = "getModels")]
	private static extern IntPtr getModels();

	[DllImport ("XMMEngine", EntryPoint = "setModels")]
	private static extern void setModels(string smodels);

	[DllImport ("XMMEngine", EntryPoint = "clearModels")]
	private static extern void clearModels();

	[DllImport ("XMMEngine", EntryPoint = "getTrainingSet")]
	private static extern IntPtr getTrainingSet();

	[DllImport ("XMMEngine", EntryPoint = "setTrainingSet")]
	private static extern void setTrainingSet(string strainingset);

	[DllImport ("XMMEngine", EntryPoint = "clearTrainingSet")]
	private static extern void clearTrainingSet();

	[DllImport ("XMMEngine", EntryPoint = "clearLabel")]
	private static extern void clearLabel(string label);

	//======== use XMM library :

	[DllImport ("XMMEngine", EntryPoint = "setLikelihoodWindow")]
	private static extern void setLikelihoodWindow(int w);

	[DllImport ("XMMEngine", EntryPoint = "filter")]
	private static extern void filter(IntPtr obs, int obsSize);

	[DllImport("XMMEngine", EntryPoint = "getLikeliest")]
	private static extern IntPtr 
	();
	
	[DllImport ("XMMEngine", EntryPoint = "setString")]
	private static extern int setString(string s);

	[DllImport ("XMMEngine", EntryPoint = "getString")]
	private static extern int getString(StringBuilder s);
	#endif
	//======================= variables ======================//

	Stopwatch sw;
	InputProcessingChain proc;
	
	public Dropdown LabelsDropdown;
	String label;
	public Button RecButton;
	bool rec;
	bool newPhrase;
	public Button AddButton;
	public Button ClearButton;
	public Button ClearCurrentButton;
	public Text UIText;
	public Text DebugText;


	List<float> phraseList;
	float[] phrase;
	float[] desc;
	IntPtr unmanagedDesc;
	String[] colNames;
	String lastPhrase;
	String likeliest;
	// training / filtering parameters :
	int nbOfGaussians;
	int likelihoodWindow;

	bool useLocation;

	// for debug :
	string smodels;
	string strainingset;

	//======================== methods ======================//

	void Start () {

		sw = new Stopwatch();
		sw.Start();

		nbOfGaussians = 1;
		likelihoodWindow = 3;
		
		proc = new InputProcessingChain(128, 16);

		// !!! set WriteAccess to "External (SDCard)" in Edit > Project Settings > Player > Other Settings !!!
		try {
			// NOT USED :
			string modelsPath = Application.persistentDataPath + "/models.json";
			if(File.Exists(modelsPath)) {
				smodels = File.ReadAllText(modelsPath);
				// !!!! THIS IS BUGGY FOR NOW IN XMM !!!!
				// setModels(smodels);
				// setLikelihoodWindow(3);
			}
			// USED :
			string setPath = Application.persistentDataPath + "/trainingset.json";
			if(File.Exists(setPath)) {
				strainingset = File.ReadAllText(setPath);
				setTrainingSet(strainingset);
				train(nbOfGaussians); // mixtures with 1 gaussian
				setLikelihoodWindow(likelihoodWindow);
			}
		}
		catch (Exception e) { // any exception
			smodels = "error loading models";
			strainingset = "error loading training set";
		}

		phrase = new float[0];
		desc = new float[0];

		Input.gyro.enabled = true;

		// TODO : add location data to training features
		if (!Input.location.isEnabledByUser) {
			useLocation = false;
		} else {
			useLocation = true;
			//StartLocationService();
		}

		//======================================================//
		//=============== UGLY GUI CODE (BERK !) ===============//
		//======================================================//
		
		// 150 : width of buttons in unity editor
		// 30 : height of buttons in unity editor
		float UISpacer = 10;
		float UIScale = (Screen.width - 2 * UISpacer) / 150f;//(Screen.width / 3f - UISpacer) / 90f;

		float UIHeight = 30 * UIScale;
		float UIWidth = 150 * UIScale;

		GameObject recGOButton = GameObject.Find("RecButton");
		GameObject labelsGODropdown = GameObject.Find("LabelsDropdown");
		GameObject addGOButton = GameObject.Find("AddButton");
		GameObject clearGOButton = GameObject.Find("ClearButton");
		GameObject clearCurrentGOButton = GameObject.Find("ClearCurrentButton");
		GameObject uiGOText = GameObject.Find("UIText");
		GameObject debugGOText = GameObject.Find("DebugText");

		//================== SETUP RECBUTTON ==================//
		RecButton = recGOButton.GetComponent<Button>();
		RecButton.transform.position = new Vector3(UISpacer, Screen.height - UISpacer, 0);
		RecButton.transform.localScale = new Vector3(UIScale, UIScale, 1);
		rec = false;
		newPhrase = false;
		RecButton.onClick.AddListener(delegate {
			rec = !rec;

			ColorBlock cb = new ColorBlock();
			cb.colorMultiplier = 1f;
			cb.normalColor = Color.white;
			cb.pressedColor = Color.white;

			if(rec) {
				// START REC
				phraseList = new List<float>();
				cb.highlightedColor = Color.red;
			} else {
				// STOP REC
				cb.highlightedColor = Color.white;
				if(phraseList.Count > 0) {
					newPhrase = true;
				}
			}
			RecButton.colors = cb;
		});

		//=================== SETUP DROPDOWN ===================//
		LabelsDropdown = labelsGODropdown.GetComponent<Dropdown>();
		LabelsDropdown.transform.position = new Vector3(UISpacer, Screen.height - UISpacer - UIHeight, 0);
		LabelsDropdown.transform.localScale = new Vector3(UIScale, UIScale, 1);

		label = LabelsDropdown.captionText.text;
		LabelsDropdown.onValueChanged.AddListener(delegate {
         	label = LabelsDropdown.captionText.text;
     	});

		//================== SETUP ADDBUTTON ==================//
		AddButton = addGOButton.GetComponent<Button>();
		AddButton.transform.position = new Vector3(UISpacer, Screen.height - UISpacer - 2 * UIHeight, 0);
		AddButton.transform.localScale = new Vector3(UIScale, UIScale, 1);
		
		AddButton.onClick.AddListener(delegate {
			if(newPhrase) {

				newPhrase = false;
				List<String> colNamesList = new List<String>();
				colNamesList.Add("magnitude");
				colNamesList.Add("frequency");
				colNamesList.Add("periodicity");
				colNames = colNamesList.ToArray();

				//========= set phrase ========//
				phrase = phraseList.ToArray();
				IntPtr unmanagedPhrase = Marshal.AllocHGlobal(phrase.Length * sizeof(float));
				Marshal.Copy(phrase, 0, unmanagedPhrase, phrase.Length);

				//=== send everything needed ==//
				addPhrase(label, colNames, colNames.Length, unmanagedPhrase, phrase.Length);

				Marshal.FreeHGlobal(unmanagedPhrase);

				train(nbOfGaussians);
				setLikelihoodWindow(likelihoodWindow);
			}
		});

		//================= SETUP CLEARBUTTON ==================//
		ClearButton = clearGOButton.GetComponent<Button>();
		ClearButton.transform.position = new Vector3(UISpacer, Screen.height - UISpacer - 3 * UIHeight, 0);
		ClearButton.transform.localScale = new Vector3(UIScale, UIScale, 1);
		
		ClearButton.onClick.AddListener(delegate {
			string modelsPath = Application.persistentDataPath + "/models.json";
			if(File.Exists(modelsPath)) {
				File.Delete(modelsPath);
			}
			string setPath = Application.persistentDataPath + "/trainingset.json";
			if(File.Exists(setPath)) {
				File.Delete(setPath);
			}
			clearTrainingSet();
			clearModels();
			//DebugText.text = "training set and models cleared";
		});		

		//============== SETUP CLEARCURRENTBUTTON ==============//
		ClearCurrentButton = clearCurrentGOButton.GetComponent<Button>();
		ClearCurrentButton.transform.position = new Vector3(UISpacer, Screen.height - UISpacer - 4 * UIHeight, 0);
		ClearCurrentButton.transform.localScale = new Vector3(UIScale, UIScale, 1);
		
		ClearCurrentButton.onClick.AddListener(delegate {
			clearLabel(label);
			train(nbOfGaussians);
			setLikelihoodWindow(likelihoodWindow);
		});		

		//================== SETUP UITEXT ===================//
		UIText = uiGOText.GetComponent<Text>();
		UIText.transform.position = new Vector3(UISpacer, Screen.height - UISpacer - 5 * UIHeight, 0);
		UIText.text = "Unknown";

		//================= SETUP DEBUGTEXT =================//
		DebugText = debugGOText.GetComponent<Text>();
		DebugText.transform.position = new Vector3(UISpacer, Screen.height - UISpacer - 6 * UIHeight, 0);
		//DebugText.transform.localScale = new Vector3(UIScale, UIScale, 1); // -> set position and size mannually in editor
		//DebugText.text = smodels + "\n" + strainingset;
		DebugText.text = "";

		phraseList = new List<float>();
	}

	void Destroy() {
		// eventually free some memory here (obtained from Marshaling for example)
	}
	
	void Update () {
		// HOW TO QUIT NORMALLY - SOUTION FOUND HERE :
		//http://answers.unity3d.com/questions/182265/how-to-quit-from-android.html
	    if (Input.GetKey(KeyCode.Escape)) {
	        if (Application.platform == RuntimePlatform.Android) {
	        	// DO STUFF BEFORE QUITTING
				strainingset = Marshal.PtrToStringAnsi(getTrainingSet());
				File.WriteAllText(Application.persistentDataPath + "/trainingset.json", strainingset);
				smodels = Marshal.PtrToStringAnsi(getModels());
				File.WriteAllText(Application.persistentDataPath + "/models.json", smodels);
				// QUIT !
	    	    Application.Quit();
	    	}
	    }

	    if(desc.Length > 0) {
			unmanagedDesc = Marshal.AllocHGlobal(desc.Length * sizeof(float));
			Marshal.Copy(desc, 0, unmanagedDesc, desc.Length);

			filter(unmanagedDesc, desc.Length);

			IntPtr likeliestPtr = getLikeliest();
			likeliest = Marshal.PtrToStringAnsi(likeliestPtr);

			Marshal.FreeHGlobal(unmanagedDesc);	    	
	    }

	    UIText.text = likeliest;
	}

	void FixedUpdate() {

		//print(sw.ElapsedMilliseconds);
     	sw.Reset();
		sw.Start();
		float rr = Mathf.Sqrt(Input.gyro.rotationRate.x * Input.gyro.rotationRate.x 
							+ Input.gyro.rotationRate.y * Input.gyro.rotationRate.y
							+ Input.gyro.rotationRate.z * Input.gyro.rotationRate.z);

		proc.feed(rr);

		if(proc.hasNewFrame()) {

			desc = proc.getLastFrame();

			if(rec) {
				foreach(float f in desc) {
					phraseList.Add(f);
				}
			}

		}
	}

	void OnGUI() {

	// 	GUI.backgroundColor = new Color(0f, 0f, 0f, 1f);
	// 	if(rec) {
	// 		GUI.contentColor = new Color(1f, 0f, 0f, 1f);
	// 	} else {
	// 		GUI.contentColor = new Color(1f, 1f, 1f, 1f);
	// 	}

		// recToggle = GUI.Toggle(new Rect(10,10,100,50), recToggle, "REC", xButtonStyle);
		// if(recToggle != oldRecToggle) {
		// 	if(recToggle) {
		// 		phraseList = new List<float>();
		// 	} else {
		// 		//
		// 	}
		// }

		// GUI.contentColor = new Color(1f, 1f, 1f, 1f);

		/*
		//if(GUI.Button(new Rect(10,70,100,50), "ADD", xButtonStyle)) {
		if(GUI.Button(new Rect(10,70,100,50), "ADD")) {
			// send the bouzin to xmm (add to training set)

			//======== set label(s) =======//
			//String label = "testLabel";

			//======= set colname(s) ======//
			//String[] colNames = colNamesList.ToArray();

			//========= set phrase ========//
			float[] phrase = phraseList.ToArray();
			IntPtr unmanagedPhrase = Marshal.AllocHGlobal(phrase.Length * sizeof(float));
			Marshal.Copy(phrase, 0, unmanagedPhrase, phrase.Length);

			//=== send everything needed ==//
			addPhrase(label, colNames, colNames.Length, unmanagedPhrase, phrase.Length);

			lastPhrase = Marshal.PtrToStringAnsi(getLastPhrase());

			Marshal.FreeHGlobal(unmanagedPhrase);
		}
		*/


		/*
		if(!recToggle) {
			GUI.Label(new Rect(10, 130, 200, 50), "Données enregistrées : " + phraseList.Count + " points", xStyle);
		} else {
			GUI.Label(new Rect(10, 130, 200, 50), "Enregistrement ...", xStyle);		
		}
		//GUI.Label(new Rect(10, 170, Screen.width, 1500), "Test string : " + lastPhrase, xStyle);
		GUI.Label(new Rect(10, 170, Screen.width, 1500), "Stopwatch freq : " + Stopwatch.Frequency, xStyle);
		*/
	}

}
