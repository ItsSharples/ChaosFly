using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

[ExecuteInEditMode]
public class LorenzParent : MonoBehaviour
{
	public Material particleMaterial;
	[Range(0, 100)]
	public int currentAttractors = 0;

    internal List<LorenzAttractor> attractors;

	/*
	[SerializeField]
	[HideInInspector]
	internal float minX;
	[SerializeField]
	[HideInInspector]
	internal float maxX;
	[SerializeField]
	[HideInInspector]
	internal float minY;
	[SerializeField]
	[HideInInspector]
	internal float maxY;
	*/

	public float PrandtlNumber = 14;
	public float RayleighNumber = 10;
	public float physicalDimensions = 8 / 3;
	[Range(0.0001f, 0.001f)]
	public float stepSize = 0.001f;

	// Start is called before the first frame update
	private void Awake()
	{
		if (attractors == null || attractors.Count != currentAttractors)
		{
			DestroyAllChildren();
			RegenerateChildren();
		}
	}
	public void DestroyAllChildren()
	{
		if (attractors == null) { attractors = new List<LorenzAttractor>(); }
		foreach (var attractor in attractors)
		{
			if (attractor.IsDestroyed()) { continue; }
			DestroyImmediate(attractor.transform.gameObject);
		}
		attractors.Clear();
	}
	public void RegenerateChildren()
    {
		
		for(int i = 0; i< gameObject.transform.childCount; i++)
		{
			var child = gameObject.transform.GetChild(i);
			if(child.TryGetComponent(typeof(LorenzAttractor), out var attractor))
			{
				attractors.Add(attractor as LorenzAttractor);
			}
			else
			{
				DestroyImmediate(child);
			}
		}


        for(int i = 0; i < currentAttractors; i++)
        {
			if (attractors.Count > i)
			{
				if (attractors[i] != null) { continue; }
			}

            var lorenz = new GameObject();
            lorenz.name = $"Attractor {attractors.Count + 1}";
            lorenz.transform.parent = transform;

			
            var attractor = lorenz.AddComponent<LorenzAttractor>();
			
            var config = Instantiate(LorenzConfiguration.zero);

			//attractor.originPoint = config.originPoint;
            attractor.config = config;
			attractor.particleMaterial = particleMaterial;
			attractor.lineRenderer = lorenz.AddComponent<LineRenderer>();

			attractors.Add(attractor);
        }
        
		if(attractors.Count > currentAttractors)
		{
			for (int i = currentAttractors; i < attractors.Count; i++)
			{
				var attractor = attractors[i];
				if (attractor.IsDestroyed()) { continue; }
				DestroyImmediate(attractor.transform.gameObject);
				attractors.RemoveAt(i);
			}
		}
		
		currentAttractors = attractors.Count;
    }
	private void Start()
	{
		if (Application.isPlaying)
		{
			foreach(var attractor in attractors)
			{
				attractor.PrandtlNumber = PrandtlNumber;
				attractor.RayleighNumber = RayleighNumber;
				attractor.physicalDimensions = physicalDimensions;
				attractor.stepSize = stepSize;
			}
		}
	}
	private void Update()
	{
		if(attractors.Count != currentAttractors) { RegenerateChildren(); }
	}
}

[CustomEditor(typeof(LorenzParent))]
public class LorenzParentEditor : Editor
{
	public override void OnInspectorGUI()
    {
		var obj = target as LorenzParent;
		base.OnInspectorGUI();

		//EditorGUILayout.PropertyField(serializedObject.FindProperty(nameof(obj.configurations)));

		if (GUILayout.Button("Regenerate Children")){
            obj.RegenerateChildren();
        }

		if (GUILayout.Button("Clear All"))
		{
			if (obj.attractors != null)
			{
				obj.attractors.Clear();
			}
			else
			{
				obj.attractors = new List<LorenzAttractor>();
			}
		}

		/*
		using (var scope = new EditorGUILayout.VerticalScope("X Range"))
		{
			EditorGUILayout.BeginHorizontal();
			obj.minX = EditorGUILayout.FloatField(obj.minX);
			obj.maxX = EditorGUILayout.FloatField(obj.maxX);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.MinMaxSlider(ref obj.minX, ref obj.maxX, -1, 1);
		}

		using (var scope = new EditorGUILayout.VerticalScope("Y Range"))
		{
			EditorGUILayout.BeginHorizontal();
			obj.minY = EditorGUILayout.FloatField(obj.minY);
			obj.maxY = EditorGUILayout.FloatField(obj.maxY);
			EditorGUILayout.EndHorizontal();
			EditorGUILayout.MinMaxSlider(ref obj.minY, ref obj.maxY, -1, 1);
		}
		*/

		if (obj.attractors != null)
		{
			for (int i = 0; i < obj.attractors.Count; i++)
			{
				var attractor = obj.attractors[i];

				

				var editor = Editor.CreateEditor(attractor.config);
				editor.DrawDefaultInspector();

				obj.attractors[i] = attractor;
			}
		}
	}

}


