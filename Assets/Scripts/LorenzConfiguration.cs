using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu]
public class LorenzConfiguration : ScriptableObject
{
	public Vector3 originPoint;
	public Color startColour;
	public Color endColour;
	public bool useEndColour;

	public static LorenzConfiguration zero => ScriptableObject.CreateInstance<LorenzConfiguration>();
}


[CustomEditor(typeof(LorenzConfiguration))]
public class LorenzConfigurationEditor : Editor
{
	public override void OnInspectorGUI()
	{
		var obj = target as LorenzConfiguration;
		//base.OnInspectorGUI();

		var editor = Editor.CreateEditor(obj);
		editor.DrawDefaultInspector();
	}

}
