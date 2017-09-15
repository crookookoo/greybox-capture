using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Greybox))]
public class CustomGreyboxInspector : Editor {

	private string status = "";

	public override void OnInspectorGUI(){
		// base.OnInspectorGUI();
		DrawDefaultInspector();

		// GUILayoutButton Capture = new GUILayoutButton("Capture Screenshot");
		
		GUILayout.Space(5);

		Greybox g = target as Greybox;

		GUI.enabled = Application.isPlaying && TokenValid(g.token) && g.canTakeScreenshot;

		if(GUILayout.Button("Capture Screenshot", GUILayout.Height(24))){
			g.TakeScreenShot();
		}

		GUI.enabled = true;

		status = "Visit Greybox.it to get your token";
		
		if(Application.isPlaying && !TokenValid(g.token)) status = "Token invalid!";
		
		if(TokenValid(g.token)) status = "";

		if(Application.isPlaying && TokenValid(g.token)){
			status = g.state;
		}

		GUILayout.Label(status);
		GUILayout.Space(5);

		// if(Application.isPlaying) GUILayout.Label("lols");
		foreach (string u in g.urls)
		{
			GUILayout.BeginHorizontal();
			GUILayout.TextField(u);
			if(GUILayout.Button("Copy", GUILayout.Width(60))){
				CopyToClipboard(u);
			}
			GUILayout.EndHorizontal();
			CopyToClipboard(u);
		}

		if(g.urls.Count > 1){
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Copy all", GUILayout.Width(60))){
				status = "All URLs copied to clipboard!";
			}
			GUILayout.EndHorizontal();
		}

		GUILayout.Space(5);
	

	}

	private bool TokenValid(string str){
		if(str.Length == 28) return true;
		else return false;
	}

	private void CopyToClipboard(string str){
		Greybox g = target as Greybox;

		TextEditor te = new TextEditor();
		te.text = str;
		te.SelectAll();
		te.Copy();
		
		g.state = "URL copied to clipboard!";
	}
}
