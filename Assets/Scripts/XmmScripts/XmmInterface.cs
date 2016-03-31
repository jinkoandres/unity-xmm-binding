using UnityEngine;
using UnityEngine.UI;
using System;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

public class XmmInterface : MonoBehaviour {

	//================== XMM lib functions ==================//

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

	[DllImport ("XMMEngine", EntryPoint = "getModel")]
	private static extern IntPtr getModel();

	[DllImport ("XMMEngine", EntryPoint = "setLikelihoodWindow")]
	private static extern void setLikelihoodWindow(int w);

	[DllImport ("XMMEngine", EntryPoint = "filter")]
	private static extern void filter(IntPtr obs, int obsSize);

	[DllImport("XMMEngine", EntryPoint = "getLikeliest")]
	private static extern IntPtr getLikeliest();
	
	[DllImport ("XMMEngine", EntryPoint = "setString")]
	private static extern int setString(string s);

	[DllImport ("XMMEngine", EntryPoint = "getString")]
	private static extern int getString(StringBuilder s);

	//======================= variables ======================//

	Stopwatch sw;
	InputProcessingChain proc;
	
	public Dropdown LabelsDropdown;
	String label;
	public Button RecButton;
	bool rec;
	bool newPhrase;
	public Button AddButton;
	public Text UIText;


	List<float> phraseList;
	float[] phrase;
	float[] desc;
	String[] colNames;
	String lastPhrase;
	//float[] obs;
	IntPtr unmanagedDesc;
	String likeliest;
	bool useLocation;

	//======================== methods ======================//

	void Start () {

		sw = new Stopwatch();
		sw.Start();
		proc = new InputProcessingChain(128, 16);


		Input.gyro.enabled = true;

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
		GameObject uiGOText = GameObject.Find("UIText");

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
				phraseList = new List<float>();
				cb.highlightedColor = Color.red;
				//print("start rec");
			} else {
				cb.highlightedColor = Color.white;
				if(phraseList.Count > 0) {
					newPhrase = true;
				}
				//print("stop rec");
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

				train(1);
				setLikelihoodWindow(3);
				//lastPhrase = Marshal.PtrToStringAnsi(getLastPhrase());
				//lastPhrase = Marshal.PtrToStringAnsi(getModel());
			}
		});

		//================== SETUP UITEXT ===================//
		UIText = uiGOText.GetComponent<Text>();
		UIText.transform.position = new Vector3(UISpacer, Screen.height - UISpacer - 3 * UIHeight, 0);
		UIText.text = "Unknown";

		phraseList = new List<float>();
	}

	void Destroy() {
		// eventually free memory
	}
	
	void Update () {
		// HOW TO QUIT NORMALLY - SOUTION FOUND HERE :
		//http://answers.unity3d.com/questions/182265/how-to-quit-from-android.html
	    if (Input.GetKey(KeyCode.Escape)) {
	        if (Application.platform == RuntimePlatform.Android) {
	    	    Application.Quit();
	    	}
	    }

	    if(desc.Length > 0) {
			unmanagedDesc = Marshal.AllocHGlobal(desc.Length * sizeof(float));
			Marshal.Copy(desc, 0, unmanagedDesc, desc.Length);

			filter(unmanagedDesc, desc.Length);

			IntPtr likeliestPtr = getLikeliest();
			likeliest = Marshal.PtrToStringAnsi(likeliestPtr);
			//likeliest = "Unknown";
			Marshal.FreeHGlobal(unmanagedDesc);	    	
	    }

	    UIText.text = likeliest;
	    //UIText.text = getNbOfModels() + " " + likeliest + " " + lastPhrase;
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

			// float[] lf = proc.getLastFrame();
			// desc = new float[lf.Length];
			// for(int i=0; i<desc.Length; i++) {
			// 	desc[i] = lf[i];
			// }

			//float[] desc = proc.getLastFrame();
			desc = proc.getLastFrame();

			if(rec) {
				foreach(float f in desc) {
					phraseList.Add(f);
				}
			}

		}
	}

	void OnGUI() {

        GUI.backgroundColor = new Color(0f, 0f, 0f, 1f);
        if(rec) {
	        GUI.contentColor = new Color(1f, 0f, 0f, 1f);
        } else {
            GUI.contentColor = new Color(1f, 1f, 1f, 1f);
        }

		// recToggle = GUI.Toggle(new Rect(10,10,100,50), recToggle, "REC", xButtonStyle);
		// if(recToggle != oldRecToggle) {
		// 	if(recToggle) {
		// 		phraseList = new List<float>();
		// 	} else {
		// 		//
		// 	}
		// }

		GUI.contentColor = new Color(1f, 1f, 1f, 1f);

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
