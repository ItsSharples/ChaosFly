using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class LorenzAttractor : MonoBehaviour
{
	[Header("References")]
	//public Mesh mesh;
	//public Shader instanceShader;
	public Material particleMaterial;
	internal LineRenderer lineRenderer;

	[SerializeField]
	internal LorenzConfiguration config;

	[SerializeField]
	List<Vector3> path;

	[DoNotSerialize]
	public float smallestDelta;

	public bool verySmall;

	public float PrandtlNumber;// = 14;
	public float RayleighNumber;// = 10;
	public float physicalDimensions;// = 8 / 3;
	public float stepSize;// = 0.001f;

	// Start is called before the first frame update
	void Start()
    {
		smallestDelta = float.MaxValue;
		path = new()
		{
			config.originPoint
		};

		lineRenderer = gameObject.GetOrAddComponent<LineRenderer>();
		lineRenderer.enabled = true;
		lineRenderer.material = particleMaterial;
		lineRenderer.startWidth = lineRenderer.endWidth = 0.1f;
		//lineRenderer.startColor = config.startColour;
		//lineRenderer.endColor = config.useEndColour ? config.endColour : config.startColour;

		var gradient = new Gradient();
		var startKey = new GradientColorKey() { color = config.startColour, time = 0f };
		var startAlpha = new GradientAlphaKey() { alpha = config.startColour.a, time = 0f };

		var midKey = new GradientColorKey() { color = config.startColour, time = 0.5f };
		var midAlpha = new GradientAlphaKey() { alpha = config.startColour.a, time = 0.5f };

		var endColour = config.useEndColour ? config.endColour : config.startColour;
		var endKey = new GradientColorKey() { color = endColour, time = 1f };
		var endAlpha = new GradientAlphaKey() { alpha = endColour.a, time = 1f };

		gradient.colorKeys = new GradientColorKey[] {startKey, midKey, endKey};
		gradient.alphaKeys = new GradientAlphaKey[] { startAlpha, midAlpha, endAlpha };
		gradient.mode = GradientMode.PerceptualBlend; 

		lineRenderer.colorGradient = gradient;

		lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
		lineRenderer.receiveShadows = false;
	}

	// Update is called once per frame
	void FixedUpdate()
	{

		var step = stepSize;
		for (int i = 0; i < 10; i++)
		{
			var nextStep = path.Last();

			float dx = PrandtlNumber * (nextStep.y - nextStep.x);
			float dy = nextStep.x * (RayleighNumber - nextStep.z) - nextStep.y;
			float dz = nextStep.x * nextStep.y - physicalDimensions * nextStep.z;

			smallestDelta =  Mathf.Min(Mathf.Abs(smallestDelta), Mathf.Abs(dx), Mathf.Abs(dy), Mathf.Abs(dz));
			if(smallestDelta < 0.0001f) { verySmall = true; }

			nextStep.x += dx * step;
			nextStep.y += dy * step;
			nextStep.z += dz * step;

			path.Add(nextStep);
		}

	}
	void Update() {
		lineRenderer.positionCount = path.Count;
		lineRenderer.SetPositions(path.ToArray());
		return;
		/*
		RenderParams renderParams = new(particleMaterial);
		for (int i = 0; i < path.Count; i++)
		{
			Vector3 current = path[i];
			Vector3? previous = i != 0 ? path[i - 1] : null;
			Graphics.RenderMesh(renderParams, mesh, 0, Matrix4x4.Translate(current), previous != null ? Matrix4x4.Translate(previous.Value) : null);
		}
		*/
    }
}

[CustomEditor(typeof(LorenzAttractor))]
public class LorenzAttractorEditor : Editor
{
	public override void OnInspectorGUI()
	{
		var obj = target as LorenzAttractor;
		base.OnInspectorGUI();

		if (obj.config != null)
		{
			var editor = Editor.CreateEditor(obj.config);
			editor.DrawDefaultInspector();
		}
	}
}