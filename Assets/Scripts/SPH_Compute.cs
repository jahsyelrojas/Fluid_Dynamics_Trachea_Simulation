// using System.Runtime.InteropServices;
// using UnityEngine;
// using System.Collections.Generic;
// [System.Serializable]
// [StructLayout(LayoutKind.Sequential, Size = 44)]
// public struct Particle
// {
//     public float pressure; // 4
//     public float density; // 8
//     public Vector3 currentForce; // 20
//     public Vector3 velocity; // 32
//     public Vector3 position; // 44
// }

// public class SPH_Compute : MonoBehaviour
// {
//     [Header("General")]
//     public bool showSpheres = false;
//     public Vector3Int numToSpawn = new Vector3Int(10,10,10);
//     public Vector3 boxSize = new Vector3(4,10,3);
//     public Vector3 spawnBoxCenter = new Vector3(0,3,0);
//     public Vector3 spawnBox = new Vector3(4,2,1.5f);
//     public float particleRadius = 0.1f;

//     [Header("Particle Rendering")]
//     public Mesh particleMesh;
//     public float particleRenderSize = 16f;
//     public Material material;

//      private static readonly int SizeProperty = Shader.PropertyToID("_size");
//     private static readonly int ParticlesBufferProperty = Shader.PropertyToID("_particlesBuffer");

//     [Header("Fluid Constants")]
//     public float boundDamping = -0.3f;
//     public float viscosity = -0.003f;
//     public float particleMass = 1f;
//     public float gasConstant = 2f; 
//     public float restingDensity = 1f; 

//     [Header("Time")]
//     public float timestep = 0.0001f;
//     public Transform sphere;


//     [Header("Compute")]
//     public ComputeShader shader;
//     public Particle[] particles;

//     private ComputeBuffer _argsBuffer;
//     public ComputeBuffer _particlesBuffer;

//     private int num = 0;

//     private void Awake()
//     {
//         // Spawn Particles
//         SpawnParticlesInBox();

//         uint[] args = {
//             particleMesh.GetIndexCount(0),
//             (uint) num,
//             particleMesh.GetIndexStart(0),
//             particleMesh.GetBaseVertex(0),
//             0
//         };
//         _argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
//         _argsBuffer.SetData(args);

//         InitializeComputeBuffers();
//     }

//     private int densityPressureKernel;
//     private int computeForceKernel;

//     private int integrateKernel;

//     private void InitializeComputeBuffers()
//     {
//         _particlesBuffer = new ComputeBuffer(num, 44);
//         _particlesBuffer.SetData(particles);

//         densityPressureKernel = shader.FindKernel("ComputeDensityPressure");
//         computeForceKernel = shader.FindKernel("ComputeForces");
//         integrateKernel = shader.FindKernel("Integrate");

//         shader.SetInt("particleLength", num);
//         shader.SetFloat("particleMass", particleMass);
//         shader.SetFloat("viscosity", viscosity);
//         shader.SetFloat("gasConstant", gasConstant);
//         shader.SetFloat("restDensity", restingDensity);
//         shader.SetFloat("boundDamping", boundDamping);

//         shader.SetFloat("radius", particleRadius);
//         shader.SetFloat("radius2", particleRadius * particleRadius);
//         shader.SetFloat("radius3", particleRadius * particleRadius * particleRadius);
//         shader.SetFloat("radius4", particleRadius * particleRadius * particleRadius * particleRadius);
//         shader.SetFloat("radius5", particleRadius * particleRadius * particleRadius * particleRadius * particleRadius);

//         shader.SetFloat("pi", Mathf.PI);
//         shader.SetFloat("densityWeightConstant", 0.00497359197162172924277761760539f);
//         shader.SetFloat("spikyGradient", -0.09947183943243458485555235210782f);
//         shader.SetFloat("viscLaplacian", 0.39788735772973833942220940843129f);


//         shader.SetVector("boxSize", boxSize);

//         shader.SetBuffer(densityPressureKernel, "_particles", _particlesBuffer);
//         shader.SetBuffer(computeForceKernel, "_particles", _particlesBuffer);
//         shader.SetBuffer(integrateKernel, "_particles", _particlesBuffer);

//         // Add the tube collider
//         MeshCollider tubeCollider = FindObjectOfType<TubeGenerator>().GetComponent<MeshCollider>();
//         if (tubeCollider != null)
//         {
//             shader.SetBuffer(densityPressureKernel, "_tubeVertices", new ComputeBuffer(tubeCollider.sharedMesh.vertexCount, sizeof(float) * 3));
//             shader.SetBuffer(densityPressureKernel, "_tubeTriangles", new ComputeBuffer(tubeCollider.sharedMesh.triangles.Length, sizeof(int)));
//             shader.SetBuffer(computeForceKernel, "_tubeVertices", new ComputeBuffer(tubeCollider.sharedMesh.vertexCount, sizeof(float) * 3));
//             shader.SetBuffer(computeForceKernel, "_tubeTriangles", new ComputeBuffer(tubeCollider.sharedMesh.triangles.Length, sizeof(int)));
//             shader.SetBuffer(integrateKernel, "_tubeVertices", new ComputeBuffer(tubeCollider.sharedMesh.vertexCount, sizeof(float) * 3));
//             shader.SetBuffer(integrateKernel, "_tubeTriangles", new ComputeBuffer(tubeCollider.sharedMesh.triangles.Length, sizeof(int)));

//             shader.SetInt("tubeVertexCount", tubeCollider.sharedMesh.vertexCount);
//             shader.SetInt("tubeTriangleCount", tubeCollider.sharedMesh.triangles.Length);
//         }

//     }


//     private void SpawnParticlesInBox()
//     {
//         Vector3 spawnTopLeft = spawnBoxCenter - spawnBox / 2;
//         List<Particle> _particles = new List<Particle>();

//         for (int x = 0; x < numToSpawn.x; x++)
//         {
//             for (int y = 0; y < numToSpawn.y; y++)
//             {
//                 for (int z = 0; z < numToSpawn.z; z++)
//                 {
//                     Vector3 spawnPosition = spawnTopLeft + new Vector3(x * particleRadius * 2, y * particleRadius * 2, z * particleRadius * 2) + Random.onUnitSphere * particleRadius * 0.1f;
//                     Particle p = new Particle
//                     {
//                         position = spawnPosition
//                     };

//                     _particles.Add(p);
//                 }
//             }
//         }

//         num = _particles.Count;
//         particles = _particles.ToArray();
//     }

//     private void FixedUpdate()
//     {

//         shader.SetVector("boxSize", boxSize);
//         shader.SetFloat("timestep", timestep);
//         shader.SetVector("spherePos", sphere.transform.position);
//         shader.SetFloat("sphereRadius", sphere.transform.localScale.x/2);

//         shader.Dispatch(densityPressureKernel, num / 100, 1, 1);
//         shader.Dispatch(computeForceKernel, num / 100, 1, 1);
//         shader.Dispatch(integrateKernel, num / 100, 1, 1);

//         material.SetFloat(SizeProperty, particleRenderSize);
//         material.SetBuffer(ParticlesBufferProperty, _particlesBuffer);



//         // if (tubeCollider != null)
//         // {
//         //     shader.SetBuffer(densityPressureKernel, "_tubeVertices", new ComputeBuffer(tubeCollider.sharedMesh.vertexCount, sizeof(float) * 3));
//         //     shader.SetBuffer(densityPressureKernel, "_tubeTriangles", new ComputeBuffer(tubeCollider.sharedMesh.triangles.Length, sizeof(int)));
//         //     shader.SetBuffer(computeForceKernel, "_tubeVertices", new ComputeBuffer(tubeCollider.sharedMesh.vertexCount, sizeof(float) * 3));
//         //     shader.SetBuffer(computeForceKernel, "_tubeTriangles", new ComputeBuffer(tubeCollider.sharedMesh.triangles.Length, sizeof(int)));
//         //     shader.SetBuffer(integrateKernel, "_tubeVertices", new ComputeBuffer(tubeCollider.sharedMesh.vertexCount, sizeof(float) * 3));
//         //     shader.SetBuffer(integrateKernel, "_tubeTriangles", new ComputeBuffer(tubeCollider.sharedMesh.triangles.Length, sizeof(int)));

//         //     shader.SetInt("tubeVertexCount", tubeCollider.sharedMesh.vertexCount);
//         //     shader.SetInt("tubeTriangleCount", tubeCollider.sharedMesh.triangles.Length);
//         // }

       
//     }

//     private void InitializeComputeBuffers()
// {
//     _particlesBuffer = new ComputeBuffer(num, 44);
//     _particlesBuffer.SetData(particles);

//     densityPressureKernel = shader.FindKernel("ComputeDensityPressure");
//     computeForceKernel = shader.FindKernel("ComputeForces");
//     integrateKernel = shader.FindKernel("Integrate");

//     shader.SetInt("particleLength", num);
//     shader.SetFloat("particleMass", particleMass);
//     shader.SetFloat("viscosity", viscosity);
//     shader.SetFloat("gasConstant", gasConstant);
//     shader.SetFloat("restDensity", restingDensity);
//     shader.SetFloat("boundDamping", boundDamping);

//     shader.SetFloat("radius", particleRadius);
//     shader.SetFloat("radius2", particleRadius * particleRadius);
//     shader.SetFloat("radius3", particleRadius * particleRadius * particleRadius);
//     shader.SetFloat("radius4", particleRadius * particleRadius * particleRadius * particleRadius);
//     shader.SetFloat("radius5", particleRadius * particleRadius * particleRadius * particleRadius * particleRadius);

//     shader.SetFloat("pi", Mathf.PI);
//     shader.SetFloat("densityWeightConstant", 0.00497359197162172924277761760539f);
//     shader.SetFloat("spikyGradient", -0.09947183943243458485555235210782f);
//     shader.SetFloat("viscLaplacian", 0.39788735772973833942220940843129f);

//     shader.SetVector("boxSize", boxSize);

//     shader.SetBuffer(densityPressureKernel, "_particles", _particlesBuffer);
//     shader.SetBuffer(computeForceKernel, "_particles", _particlesBuffer);
//     shader.SetBuffer(integrateKernel, "_particles", _particlesBuffer);

//     // Add the tube collider
//     MeshCollider tubeCollider = FindObjectOfType<TubeGenerator>().GetComponent<MeshCollider>();
//     if (tubeCollider != null)
//     {
//         var tubeMesh = tubeCollider.sharedMesh;
//         var vertices = tubeMesh.vertices;
//         var triangles = tubeMesh.triangles;

//         ComputeBuffer verticesBuffer = new ComputeBuffer(vertices.Length, sizeof(float) * 3);
//         ComputeBuffer trianglesBuffer = new ComputeBuffer(triangles.Length, sizeof(int));

//         verticesBuffer.SetData(vertices);
//         trianglesBuffer.SetData(triangles);

//         shader.SetBuffer(densityPressureKernel, "_tubeVertices", verticesBuffer);
//         shader.SetBuffer(densityPressureKernel, "_tubeTriangles", trianglesBuffer);
//         shader.SetBuffer(computeForceKernel, "_tubeVertices", verticesBuffer);
//         shader.SetBuffer(computeForceKernel, "_tubeTriangles", trianglesBuffer);
//         shader.SetBuffer(integrateKernel, "_tubeVertices", verticesBuffer);
//         shader.SetBuffer(integrateKernel, "_tubeTriangles", trianglesBuffer);

//         shader.SetInt("tubeVertexCount", vertices.Length);
//         shader.SetInt("tubeTriangleCount", triangles.Length);

//         buffersToDispose.Add(verticesBuffer);
//         buffersToDispose.Add(trianglesBuffer);
//     }
// }


//     private void Update() {
//         if (showSpheres) Graphics.DrawMeshInstancedIndirect(particleMesh, 0, material, new Bounds(Vector3.zero, boxSize), _argsBuffer, castShadows: UnityEngine.Rendering.ShadowCastingMode.Off);
//     }

//     private void OnDrawGizmos()
//     {
//         Gizmos.color = Color.blue;
//         Gizmos.DrawWireCube(Vector3.zero, boxSize);

//         if (!Application.isPlaying)
//         {
//             Gizmos.color = Color.cyan;
//             Gizmos.DrawWireCube(spawnBoxCenter, spawnBox);
//         }
//     }

// }


using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Runtime.InteropServices;

[System.Serializable]
[StructLayout(LayoutKind.Sequential, Size = 44)]
public struct Particle
{
    public float pressure; // 4
    public float density; // 8
    public Vector3 currentForce; // 20
    public Vector3 velocity; // 32
    public Vector3 position; // 44
}

public class SPH_Compute : MonoBehaviour
{
    [Header("General")]
    public bool showSpheres = false;
    public Vector3Int numToSpawn = new Vector3Int(10,10,10);
    public Vector3 boxSize = new Vector3(4,10,3);
    public Vector3 spawnBoxCenter = new Vector3(0,3,0);
    public Vector3 spawnBox = new Vector3(4,2,1.5f);
    public float particleRadius = 0.1f;

    [Header("Particle Rendering")]
    public Mesh particleMesh;
    public float particleRenderSize = 16f;
    public Material material;

    private static readonly int SizeProperty = Shader.PropertyToID("_size");
    private static readonly int ParticlesBufferProperty = Shader.PropertyToID("_particlesBuffer");

    [Header("Fluid Constants")]
    public float boundDamping = -0.3f;
    public float viscosity = -0.003f;
    public float particleMass = 1f;
    public float gasConstant = 2f;
    public float restingDensity = 1f;

    [Header("Time")]
    public float timestep = 0.0001f;
    public Transform sphere;

    [Header("Compute")]
    public ComputeShader shader;
    public Particle[] particles;

    private ComputeBuffer _argsBuffer;
    public ComputeBuffer _particlesBuffer;

    private int num = 0;

    private void Awake()
    {
        // Spawn Particles
        SpawnParticlesInBox();

        uint[] args = {
            particleMesh.GetIndexCount(0),
            (uint) num,
            particleMesh.GetIndexStart(0),
            particleMesh.GetBaseVertex(0),
            0
        };
        _argsBuffer = new ComputeBuffer(1, args.Length * sizeof(uint), ComputeBufferType.IndirectArguments);
        _argsBuffer.SetData(args);

        InitializeComputeBuffers();
    }

    private int densityPressureKernel;
    private int computeForceKernel;
    private int integrateKernel;

    private void InitializeComputeBuffers()
    {
        _particlesBuffer = new ComputeBuffer(num, 44);
        _particlesBuffer.SetData(particles);

        densityPressureKernel = shader.FindKernel("ComputeDensityPressure");
        computeForceKernel = shader.FindKernel("ComputeForces");
        integrateKernel = shader.FindKernel("Integrate");

        // Debug to ensure kernels are found
        Debug.Log($"DensityPressureKernel: {densityPressureKernel}");
        Debug.Log($"ComputeForceKernel: {computeForceKernel}");
        Debug.Log($"IntegrateKernel: {integrateKernel}");

        if (densityPressureKernel < 0 || computeForceKernel < 0 || integrateKernel < 0)
        {
        Debug.LogError("One or more kernels are invalid!");
        return;
        }

        shader.SetInt("particleLength", num);
        shader.SetFloat("particleMass", particleMass);
        shader.SetFloat("viscosity", viscosity);
        shader.SetFloat("gasConstant", gasConstant);
        shader.SetFloat("restDensity", restingDensity);
        shader.SetFloat("boundDamping", boundDamping);

        shader.SetFloat("radius", particleRadius);
        shader.SetFloat("radius2", particleRadius * particleRadius);
        shader.SetFloat("radius3", particleRadius * particleRadius * particleRadius);
        shader.SetFloat("radius4", particleRadius * particleRadius * particleRadius * particleRadius);
        shader.SetFloat("radius5", particleRadius * particleRadius * particleRadius * particleRadius * particleRadius);

        shader.SetFloat("pi", Mathf.PI);
        shader.SetFloat("densityWeightConstant", 0.00497359197162172924277761760539f);
        shader.SetFloat("spikyGradient", -0.09947183943243458485555235210782f);
        shader.SetFloat("viscLaplacian", 0.39788735772973833942220940843129f);

        shader.SetVector("boxSize", boxSize);

        shader.SetBuffer(densityPressureKernel, "_particles", _particlesBuffer);
        shader.SetBuffer(computeForceKernel, "_particles", _particlesBuffer);
        shader.SetBuffer(integrateKernel, "_particles", _particlesBuffer);

        // Add the tube collider
        MeshCollider tubeCollider = FindObjectOfType<TubeGenerator>().GetComponent<MeshCollider>();
        if (tubeCollider != null)
        {
            shader.SetBuffer(densityPressureKernel, "_tubeVertices", new ComputeBuffer(tubeCollider.sharedMesh.vertexCount, sizeof(float) * 3));
            shader.SetBuffer(densityPressureKernel, "_tubeTriangles", new ComputeBuffer(tubeCollider.sharedMesh.triangles.Length, sizeof(int)));
            shader.SetBuffer(computeForceKernel, "_tubeVertices", new ComputeBuffer(tubeCollider.sharedMesh.vertexCount, sizeof(float) * 3));
            shader.SetBuffer(computeForceKernel, "_tubeTriangles", new ComputeBuffer(tubeCollider.sharedMesh.triangles.Length, sizeof(int)));
            shader.SetBuffer(integrateKernel, "_tubeVertices", new ComputeBuffer(tubeCollider.sharedMesh.vertexCount, sizeof(float) * 3));
            shader.SetBuffer(integrateKernel, "_tubeTriangles", new ComputeBuffer(tubeCollider.sharedMesh.triangles.Length, sizeof(int)));

            shader.SetInt("tubeVertexCount", tubeCollider.sharedMesh.vertexCount);
            shader.SetInt("tubeTriangleCount", tubeCollider.sharedMesh.triangles.Length);
        }
    }

    private void SpawnParticlesInBox()
    {
        Vector3 spawnTopLeft = spawnBoxCenter - spawnBox / 2;
        List<Particle> _particles = new List<Particle>();

        for (int x = 0; x < numToSpawn.x; x++)
        {
            for (int y = 0; y < numToSpawn.y; y++)
            {
                for (int z = 0; z < numToSpawn.z; z++)
                {
                    Vector3 spawnPosition = spawnTopLeft + new Vector3(x * particleRadius * 2, y * particleRadius * 2, z * particleRadius * 2) + Random.onUnitSphere * particleRadius * 0.1f;
                    Particle p = new Particle
                    {
                        position = spawnPosition
                    };

                    _particles.Add(p);
                }
            }
        }

        num = _particles.Count;
        particles = _particles.ToArray();
    }

    // private void FixedUpdate()
    // {
    //     shader.SetVector("boxSize", boxSize);
    //     shader.SetFloat("timestep", timestep);
    //     shader.SetVector("spherePos", sphere.transform.position);
    //     shader.SetFloat("sphereRadius", sphere.transform.localScale.x / 2);

    //     shader.Dispatch(densityPressureKernel, num / 100, 1, 1);
    //     shader.Dispatch(computeForceKernel, num / 100, 1, 1);
    //     shader.Dispatch(integrateKernel, num / 100, 1, 1);

    //     material.SetFloat(SizeProperty, particleRenderSize);
    //     material.SetBuffer(ParticlesBufferProperty, _particlesBuffer);
    // }
    private void FixedUpdate()
    {
        shader.SetVector("boxSize", boxSize);
        shader.SetFloat("timestep", timestep);
        shader.SetVector("spherePos", sphere.transform.position);
        shader.SetFloat("sphereRadius", sphere.transform.localScale.x / 2);

        // int threadGroups = Mathf.CeilToInt(num / 100.0f);

        shader.Dispatch(densityPressureKernel, 100, 1, 1);
        shader.Dispatch(computeForceKernel, 100, 1, 1);
        shader.Dispatch(integrateKernel, 100, 1, 1);

        material.SetFloat(SizeProperty, particleRenderSize);
        material.SetBuffer(ParticlesBufferProperty, _particlesBuffer);
    }

    private void Update()
    {
        if (showSpheres) 
        {
            Graphics.DrawMeshInstancedIndirect(particleMesh, 0, material, new Bounds(Vector3.zero, boxSize), _argsBuffer, castShadows: UnityEngine.Rendering.ShadowCastingMode.Off);
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireCube(Vector3.zero, boxSize);

        if (!Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireCube(spawnBoxCenter, spawnBox);
        }
    }
    // private void OnDestroy()
    // {
    //     _particlesBuffer?.Dispose();
    //     _argsBuffer?.Dispose();
    // }
}
