using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

namespace GBXT
{
[CustomEditor(typeof(Greybox))]
public class CustomGreyboxInspector : Editor {

	private string status = "";

	public override void OnInspectorGUI(){

		DrawDefaultInspector();
		
		GUILayout.Space(10);

		Greybox g = target as Greybox;
		
		GUI.enabled = Application.isPlaying && TokenValid(g.token) && g.canTakeScreenshot;

		GUI.contentColor = Color.green;

		if(GUILayout.Button("Capture Screenshot", GUILayout.Height(24))){
			g.TakeScreenShotNative();
		}

		GUI.enabled = true;

		status = "Visit Greybox.it to get your token";
		
		if(Application.isPlaying && !TokenValid(g.token)) status = "Token invalid!";
		
		if(TokenValid(g.token)) status = "";

		if(Application.isPlaying && TokenValid(g.token)){
			status = g.state;
		}
		
		var style = new GUIStyle(GUI.skin.label);
		style.normal.textColor = Color.blue;

		GUILayout.Label(status,style);

		GUILayout.Space(5);

		foreach (string u in g.urls)
		{
			GUILayout.BeginHorizontal();
			GUILayout.TextField(u);
			GUILayout.EndHorizontal();
			//CopyToClipboard(u);
		}

		if(g.urls.Count > 1){
			GUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Copy all", GUILayout.Width(60))){
				CopySetToClipboard(g.urls);
			}
			GUILayout.EndHorizontal();
		}

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

	private void CopySetToClipboard(List<string> lines)
	{
		Greybox g = target as Greybox;
		
		TextEditor te = new TextEditor();

		string sum = "";

		foreach (var line in lines)
		{
			sum += line + "\n";
		}
				
		te.text = sum;
		te.SelectAll();
		te.Copy();
		
		g.state = "All URLs are copied to clipboard!";

	}
}
}