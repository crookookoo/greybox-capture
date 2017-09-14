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

		GUI.enabled = Application.isPlaying && TokenValid(g.token);

		if(GUILayout.Button("Capture Screenshot", GUILayout.Height(24))){
		
		}


		status = "Visit Greybox.it to get your token";
		
		if(Application.isPlaying && !TokenValid(g.token)) status = "Token invalid!";
		
		if(TokenValid(g.token)) status = "";

		if(Application.isPlaying && TokenValid(g.token)){
			status = g.state;
		}

		GUILayout.Label(status);

		// if(Application.isPlaying) GUILayout.Label("lols");
		foreach (string u in g.urls)
		{
			GUILayout.TextField(u);
			CopyToClipboard(u);
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
