using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using Unity.VisualScripting.FullSerializer;
using UnityEditor;
using UnityEngine;
using static UnityEditor.Experimental.GraphView.GraphView;


public struct Particle
{
	public float x;
	public float y;
	public float z;
	//
	public float t;

	public static Particle zero => new() { x = 0, y = 0, z = 0, t = 0 };
}

public class LorenzParticle
{
	public Particle particle;
	public Color color;
	public Vector3 originPosition;
	public float[] packedOrigin {
		get {
			float[] packed = new float[3 * 4];
			for (int i = 0; i < 3; i++)
			{
				packed[i * 4] = originPosition[i];
			}
			return packed;
		}
	}
}


[ExecuteInEditMode]
public class LayeredWindParticles : MonoBehaviour
{
	[SerializeField]
	int initKernel = 0;
	[SerializeField]
	int updateKernel = 1;
	[Header("Settings")]
	public int trailLength = 10;
	public int numParticles = 2;

	int IntendedParticleBufferSize => trailLength * numParticles;

	public float size = 0.1f;
	public float duration;
	public float stretch = 0;
	public float scale = 1;
	public float timeScale = 1;

	//public float[] heights;

	//public Dictionary<float, Elevation> elevationDict;
	//Dictionary<float, ComputeBuffer> dictionaryParticleBuffers;

	[Header("References")]
	public Mesh mesh;
	public Shader instanceShader;
	Material particleMaterial;

	public ComputeShader compute;

	//Material[] materials;
	
	ComputeBuffer particleBuffer;
	ComputeBuffer argsBuffer;
	Bounds bounds;

	public float[] originPoint = new float[] { 1, 100, 1 };
	public float PrandtlNumber = 14;
	public float RayleighNumber = 10;
	public float physicalDimensions = 8/3;

	
	float[] lorenz { get => ComputeHelper.PackFloats(new float[] { PrandtlNumber, RayleighNumber, physicalDimensions }); }

	public List<LorenzParticle> particles;
	int currentStage;

	//ComputeBuffer[] layeredParticleBuffers;

	public int currLayer;
	int numBuffers;
	int numLayers;

	void Start()
	{
		rebuildBuffers();

		bounds = new Bounds(Vector3.zero, Vector3.one * 1000);

		// Create args buffer
		argsBuffer = ComputeHelper.CreateArgsBuffer(mesh, numParticles);

		if (particles == null)
		{
			LorenzParticle Default = new()
			{
				particle = Particle.zero,
				originPosition = new Vector3(1,0,0),
				color = Color.white
			};

			particles = new List<LorenzParticle>();
			for (int i = 0; i < numParticles; i++)
			{

				particles.Add(Default);
			}
		}

		initKernel = compute.FindKernel("Init");
		updateKernel = compute.FindKernel("Update");

		InitParticles();
	}

	void rebuildBuffers()
	{

		particleMaterial = new Material(instanceShader);
		particleMaterial.enableInstancing = true;
		particleMaterial.SetColor("_particleColour", Color.red);

		if (particleBuffer != null)
		{
			ComputeHelper.Release(particleBuffer);
		}

		particleBuffer = ComputeHelper.CreateStructuredBuffer<Particle>(1);
	}

	private void Update()
	{
		//if (numBuffers != numLayers)
		//{
		//	rebuildBuffers();
		//}


		if (Application.isPlaying)
		{
			if(particleBuffer.count != IntendedParticleBufferSize)
			{
				particleBuffer = ComputeHelper.CreateStructuredBuffer<Particle>(IntendedParticleBufferSize);

				LorenzParticle Default = new()
				{
					particle = Particle.zero,
					originPosition = new Vector3(1, 0, 0),
					color = Color.white
				};

				particles = new List<LorenzParticle>();
				for (int i = 0; i <= numParticles; i++)
				{
					particles.Add(Default);
				}

				InitParticles();
			}

			UpdateParticles();

			particleMaterial.SetBuffer("Particles", particleBuffer);

			particleMaterial.SetInt("stage", currentStage * numParticles);
			particleMaterial.SetInt("numParticles", numParticles);

			particleMaterial.SetFloat("size", size);
			particleMaterial.SetFloat("stretch", stretch);

			particleMaterial.SetColor("_particleColour", Color.green);
			particleMaterial.SetColor("_trailColour", Color.red);
			particleMaterial.SetFloat("scale", scale);

			Graphics.DrawMeshInstancedIndirect(mesh, 0, particleMaterial, bounds, argsBuffer);

			currentStage = ((int)Time.unscaledTime) % trailLength;//  (currentTrailCount + 1) % trailLength;
			
			var data = Array.CreateInstance(typeof(Particle), IntendedParticleBufferSize);
			particleBuffer.GetData(data);

			string outString = "";
			foreach (Particle item in data)
			{
				outString += $"{item.x}, ";
			}
			Debug.Log(outString);
		}
	}
	void InitParticles()
	{
		var origins = new ComputeBuffer(particles.Count, 4 * 3, ComputeBufferType.Default, ComputeBufferMode.Immutable);
		var originData = new List<Vector3>();
		foreach (var particle in particles)
		{
			originData.Add(particle.originPosition);
		}
		origins.SetData(originData);

		compute.SetBuffer(initKernel, "Particles", particleBuffer);
		compute.SetBuffer(initKernel, "originPoints", origins);
		compute.SetInt("trailLength", trailLength);
		compute.SetInt("trailPosition", 0);

		ComputeHelper.Dispatch(compute, numParticles, kernelIndex: initKernel);
	}
	void UpdateParticles()
	{
		//elevation.ActivateMaterial();
		
		ComputeHelper.AssignBuffer(compute, particleBuffer, "Particles", initKernel, updateKernel);


		for (int i = 0; i < particles.Count; i++)
		{
			var currentParticle = particles[i];
			compute.SetFloats("originPoint", currentParticle.packedOrigin);
			compute.SetFloats("lorenz", lorenz);

			compute.SetFloat("deltaTime", timeScale * 0.0001f);

			compute.SetInt("trailLength", trailLength);
			compute.SetInt("trailPosition", 0);

			ComputeHelper.Dispatch(compute, numParticles, kernelIndex: updateKernel);
		}
	}

	void OnDestroy()
	{
		if (argsBuffer != null)
		{
			argsBuffer.Release();
		}
	}
}