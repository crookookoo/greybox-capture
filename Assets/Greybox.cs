using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.IO;

public class Greybox : MonoBehaviour {
	public string token;
	public KeyCode screenshotHotkey = KeyCode.F1;
	public bool keepLocalImageCopies = false;

	[HideInInspector]
	public string state = "Ready";

	[HideInInspector]
	public List<string> urls = new List<string>();

	[HideInInspector]
	public bool canTakeScreenshot = true;
   
    [HideInInspector]
    public string FilePath;
    
	private string POSTurl = "https://us-central1-graybox-219f6.cloudfunctions.net/upload";


	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void TakeScreenShot(){
			urls.Add("http://greybox.it/lsdfe4");
			// canTakeScreenshot = false;
	}

	public void ClearURLs(){
		urls.Clear();
	}

	IEnumerator UploadPost(){
	
		WWWForm form = new WWWForm();
				
		form.AddBinaryData("file", File.ReadAllBytes(@FilePath), "screenShot.jpg", "image/jpeg");
		
		Dictionary<string, string> headers = form.headers;
		
		headers["x-token"] = token;
		// headers["debug"] = "lol";
		headers["content-type"] = "application/json; charset=utf-8";

		WWW w = new WWW(POSTurl, form.data, headers);
		// UnityWebRequest www = UnityWebRequest.Put(PUTurl, rawData);

		yield return w;
		if (!string.IsNullOrEmpty(w.error)) {
			print(w.text);
		}
		else {
			print(w.text);
		}
	}

}
