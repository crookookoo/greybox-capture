using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Greybox : MonoBehaviour {
	public string token;
	public KeyCode screenshotHotkey = KeyCode.F1;
	[HideInInspector]
	public string state = "Ready";

	[HideInInspector]
	public List<string> urls = new List<string>();

	[HideInInspector]
	public bool canTakeScreenshot = true;
	// public bool verbose = true;
	// public bool storeCapturesLocally = true;
	
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void TakeScreenShot(){
			urls.Add("http://greybox.it/lsdfe4");
			canTakeScreenshot = false;
	}

	public void ClearURLs(){
		urls.Clear();
	}
}
