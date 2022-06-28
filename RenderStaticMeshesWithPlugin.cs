using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.Runtime.InteropServices;
using System.IO;
#if UNITY_EDITOR
using UnityEditor;
#endif

[Serializable]
public struct CUSTOMTRANSFORMSTRUCT {
   public Vector3 Position;
   public Vector3 Rotation;
   public Vector3 Scale;
   public int MeshIndex;
   public int TextureIndex;
   public int ShaderIndex;
   [Range(0, 1)]
   public float AlphaCutoff;
   public bool IsTransparent;
};

[Serializable]
public struct CUSTOMTREETRANSFORMSTRUCT {
   public Vector3 Position;
   public Vector3 Rotation;
   public Vector3 Scale;
   public int MeshIndex0;
   public int MeshIndex1;
   public int TextureIndex;
   public int ShaderIndex;
   [Range(0, 1)]
   public float AlphaCutoff;
   public bool IsTransparent0;
   public bool IsTransparent1;
};

[Serializable]
public struct RANDOMIZERSTRUCT {
   public int NumberOfTrees;
   public Vector2 BeginCoordinates;
   public Vector2 EndCoordinates;
   public Vector3 Rotation;
   public Vector3 RotationalVariation;
   public Vector3 Scale;
   public Vector3 ScaleVariation;
   [Range(0, 1)]
   public float AlphaCutoff;
   public float AplhaCutoffVariation;
}

public struct CUSTOMTRANSFORM {
   public float PosX, PosY, PosZ;
   public float RotX, RotY, RotZ;
   public float ScaleX, ScaleY, ScaleZ;
   public int meshIndx, textIndx, shaderIndex;
   public float alphaCutoff;
   public bool isTransparent;
};

public struct CUSTOMTREETRANSFORM {
   public float PosX, PosY, PosZ;
   public float RotX, RotY, RotZ;
   public float ScaleX, ScaleY, ScaleZ;
   public int meshIndx0, meshIndx1, textIndx, shaderIndex;
   public float alphaCutoff;
   public bool isTransparent0, isTransparent1;
};

struct CUSTOMVERTEX {
   public float x, y, z; // the untransformed, 3D position for the vertex
   public float nX, nY, nZ;
   public float u, v;
};

struct VECTOR3 {
   public float x, y, z; // the untransformed, 3D position for the vertex
};

struct PER_STATIC_INSTANCE_STRUCT {
   public float _11, _12, _13, _14;
   public float _21, _22, _23, _24;
   public float _31, _32, _33, _34;
   public float _41, _42, _43, _44;

   //public float inv_11, inv_12, inv_13, inv_14;
   //public float inv_21, inv_22, inv_23, inv_24;
   //public float inv_31, inv_32, inv_33, inv_34;
   //public float inv_41, inv_42, inv_43, inv_44;

   public float alphaCutoff;
};

public enum CompileMode { RELEASE, EDIT, COMMIT_DATA_CHANGE };
public enum InstanceMode { EDIT, LOAD, SAVE, DISCARD, RANDOMIZE };

public class RenderStaticMeshesWithPlugin : MonoBehaviour {
   // Native plugin rendering events are only called if a plugin is used
   // by some script. This means we have to DllImport at least
   // one function in some active script.
   // For this example, we'll call into plugin's SetTimeFromUnity
   // function and pass the current time so the plugin can animate.

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetTimeFromUnity(float t);


   // We'll also pass native pointer to a texture in Unity.
   // The plugin will fill texture data from native code.
#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetTextureFromUnity(System.IntPtr texture, int w, int h);

   // We'll pass native pointer to the mesh vertex buffer.
   // Also passing source unmodified mesh data.
   // The plugin will fill vertex data from native code.
#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetMeshBuffersFromUnity(IntPtr vertexBuffer, int vertexCount, IntPtr sourceVertices, IntPtr sourceNormals, IntPtr sourceUVs);

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern IntPtr GetRenderEventFunc();

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetCamera(float aspectRatio, float nearViewPlane, float farViewPlane, float fieldOfView);


#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetViewMatrixParameters(float eyeX, float eyeY, float eyeZ, float yaw, float pitch, float roll);

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetDirectionalLight(float x, float y, float z);

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetDirectionalLightColor(float r, float g, float b, float a);

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetObjectInstantiation(float xStart, float yStart, float zStart, float dX, float dY, float dZ, int rows, int obj_per_row, float ScaleFactor, float rot_speed);

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetStaticObjectInstantiation(IntPtr transforms, int numPositions);

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetStaticMesh(int numIndicies, int numFaces, int numVertecies, IntPtr vertexBuffer, IntPtr indexBuffer, int meshNumber);


#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetMeshTexture(IntPtr texture_pointer, int dataLength, int meshTextureNumber);

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetAnimationMatrices(int numMatrices, IntPtr animationMatricesPointer);

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetAmbientLight(float r, float g, float b, float a);

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetAmbientMaterial(float r, float g, float b, float a);

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetDiffuseMaterial(float r, float g, float b, float a);

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void ActivateMeshAnimation(bool isAnimated);

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void ActivateMode(bool isCommitData);

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetStaticMeshRendering();

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetSkinedMeshRendering();

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetShaders(IntPtr data, int dataLength, int shaderNumber);

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetHardwareInstancing(bool isHWInstancing);

#if(UNITY_IPHONE || UNITY_WEBGL) && !UNITY_EDITOR
	[DllImport ("__Internal")]
#else
   [DllImport("RenderingPlugin")]
#endif
   private static extern void SetStaticObjectInstantiationD3DX(IntPtr transforms, int numPositions, int meshNumber);

#if UNITY_WEBGL && !UNITY_EDITOR
	[DllImport ("__Internal")]
	private static extern void RegisterPlugin();
#endif

   static int MIN_MESH_COUNT = 1;
   static int MAX_MESH_COUNT = 8;

   static int MIN_TEXTURE_COUNT = 1;
   static int MAX_TEXTURE_COUNT = 8;

   static int MIN_SHADER_COUNT = 1;
   static int MAX_SHADER_COUNT = 8;

   public bool HardwareInstancing;
   [Header("Script Compile Mode", order = 0)]
   [Space(-15, order = 1)]
   [Header("~~~~~~~~~~~~~~~", order = 2)]
   [Space(10, order = 3)]
   public CompileMode Mode;
   [TextArea]
   public string Message;
   [Header("Lighting Information", order = 0)]
   [Space(-15, order = 1)]
   [Header("~~~~~~~~~~~~~~~", order = 2)]
   [Space(10, order = 3)]
   public Color AmbientLight;
   public Light DirectionalLight;
   public Light[] SpotLights;
   public Light[] PointLights;
   [Header("Material Information", order = 0)]
   [Space(-15, order = 1)]
   [Header("~~~~~~~~~~~~~~~", order = 2)]
   [Space(10, order = 3)]
   public Color AmbientMaterial;
   public Color DiffuseMaterial;
   [Header("Shader Information", order = 0)]
   [Space(-15, order = 1)]
   [Header("~~~~~~~~~~~~~~~", order = 2)]
   [Space(10, order = 3)]
   public TextAsset[] SoftwareInstancingShaders;
   [Space(10, order = 4)]
   public TextAsset HardwareInstancingShader;
   [Header("Camera Information", order = 0)]
   [Space(-15, order = 1)]
   [Header("~~~~~~~~~~~~~~~", order = 2)]
   [Space(10, order = 3)]
   public Camera[] Cameras;
   [Header("Object Information", order = 0)]
   [Space(-15, order = 1)]
   [Header("~~~~~~~~~~~~~~", order = 2)]
   [Space(10, order = 3)]
   public GameObject[] Meshes;
   public Texture2D[] MeshTextures;
   [Header("Instancing Information", order = 0)]
   [Space(-15, order = 1)]
   [Header("~~~~~~~~~~~~~~~~~", order = 2)]
   [Space(10, order = 3)]
   public TextAsset PerInstanceData;
   public InstanceMode InstanceMode;
   [TextArea]
   public string FileName;
   [Header("Instance Randomizer", order = 0)]
   [Space(-15, order = 1)]
   [Header("~~~~~~~~~~~~~~~~", order = 2)]
   [Space(10, order = 3)]
   public RANDOMIZERSTRUCT[] ForestAreaRandomizers;
   [Header("Per Instance Data", order = 0)]
   [Space(-15, order = 1)]
   [Header("~~~~~~~~~~~~~", order = 2)]
   [Space(10, order = 3)]
   public CUSTOMTRANSFORMSTRUCT[] InstanceTransforms;

   float eyeX;
   float eyeY;
   float eyeZ;
   float yaw;
   float pitch;
   float roll;

   CameraEvent camEventAfterEvrything = CameraEvent.AfterEverything;

   CommandBuffer buffer;
   Mesh treeMesh;

   int max;

   CUSTOMVERTEX[] myVertecies;
   CUSTOMTRANSFORM[] instanceTransforms;
   PER_STATIC_INSTANCE_STRUCT[] instanceD3DXTransforms;

   int meshCount;
   int meshTextureCount;
   int shaderCount;

   bool saveInstanceData = false;
   bool discardInstanceData = false;
   bool messageSent = false;

   private void Start() {
#if UNITY_WEBGL && !UNITY_EDITOR
		RegisterPlugin();
#endif

      Mode = CompileMode.RELEASE;
      InstanceMode = InstanceMode.EDIT;

      SetHardwareInstancing(HardwareInstancing);
      SetMode();
      SetShaderDataToPlugin();
      SetStaticMeshRendering();
      AddCommandBuffersToCameras();
      SendMeshDataToPlugin();
      SendTextureDataToPlugin();

      if(PerInstanceData != null) {
         ReadPerInstanceDataFromFile();
      }

      FormatInstanceData();
      SendPerInstanceDataToPlugin();
      SendLightingAndMaterialDataToPlugin();

   }

   private void Update() {

      SetMode();
      SetTimeFromUnity(Time.timeSinceLevelLoad);
      FindActiveCamera();
      SendFrustumDataToPlugin();

      CheckInstancingDataMode();

#if UNITY_EDITOR
      if(Mode == CompileMode.EDIT) {
         SetHardwareInstancing(HardwareInstancing);
         FormatInstanceData();
         SendPerInstanceDataToPlugin();
         SendLightingAndMaterialDataToPlugin();
      }

      if(Mode == CompileMode.COMMIT_DATA_CHANGE) {
         SetHardwareInstancing(HardwareInstancing);
         SetShaderDataToPlugin();
         SendMeshDataToPlugin();
         SendTextureDataToPlugin();
         FormatInstanceData();
         SendPerInstanceDataToPlugin();
         SendLightingAndMaterialDataToPlugin();
      }
#endif
   }

#if UNITY_EDITOR
   void OnApplicationQuit() {
      Debug.Log("Eoga");
   }
#endif

   void AddCommandBuffersToCameras() {

      buffer = new CommandBuffer();
      buffer.IssuePluginEvent(GetRenderEventFunc(), 1);

      for(int i = 0; i < Cameras.Length; i++) {
         if(Cameras[i].renderingPath == RenderingPath.Forward) {
            Cameras[i].AddCommandBuffer(camEventAfterEvrything, buffer);
         }
      }
   }

   void FindActiveCamera() {
      max = 0;
      for(int i = 0; i < Cameras.Length - 1; i++) {
         if(Cameras[i].depth < Cameras[i + 1].depth) {
            max = i + 1;
         }
      }
   }

   void SendFrustumDataToPlugin() {

      eyeX = Cameras[max].transform.position.x;
      eyeY = Cameras[max].transform.position.y;
      eyeZ = Cameras[max].transform.position.z;

      yaw = Cameras[max].transform.rotation.eulerAngles.y * (2 * Mathf.PI / 360);
      pitch = Cameras[max].transform.rotation.eulerAngles.x * (2 * Mathf.PI / 360);
      roll = Cameras[max].transform.rotation.eulerAngles.z * (2 * Mathf.PI / 360);

      SetViewMatrixParameters(eyeX, eyeY, eyeZ, yaw, pitch, roll);

      SetCamera(Cameras[max].aspect, Cameras[max].nearClipPlane, Cameras[max].farClipPlane, Cameras[max].fieldOfView * (2 * Mathf.PI / 360));
   }

   void SendMeshDataToPlugin() {

      MeshFilter meshFilter;
      GCHandle gcVertices = new GCHandle();
      GCHandle gcIndicies = new GCHandle();

      for(int i = 0; i < ((Meshes.Length > MAX_MESH_COUNT) ? MAX_MESH_COUNT : Meshes.Length); i++) {
         meshFilter = (MeshFilter)Meshes[i].GetComponentInChildren(typeof(MeshFilter));
         treeMesh = meshFilter.sharedMesh;
         treeMesh.MarkDynamic();
         var verticies = treeMesh.vertices;
         var indicies = treeMesh.triangles;
         var normals = treeMesh.normals;
         var uvs = treeMesh.uv;

         myVertecies = new CUSTOMVERTEX[treeMesh.vertexCount];


         for(int j = 0; j < treeMesh.vertexCount; j++) {
            myVertecies[j].x = verticies[j].x;
            myVertecies[j].y = verticies[j].y;
            myVertecies[j].z = verticies[j].z;

            myVertecies[j].nX = normals[j].x;
            myVertecies[j].nY = normals[j].y;
            myVertecies[j].nZ = normals[j].z;

            myVertecies[j].u = uvs[j].x;
            myVertecies[j].v = 1.0f - uvs[j].y;
         }

         gcVertices = GCHandle.Alloc(myVertecies, GCHandleType.Pinned);
         gcIndicies = GCHandle.Alloc(indicies, GCHandleType.Pinned);

         SetStaticMesh(indicies.Length, treeMesh.triangles.Length, treeMesh.vertexCount, gcVertices.AddrOfPinnedObject(), gcIndicies.AddrOfPinnedObject(), i);
      }
      gcVertices.Free();
      gcIndicies.Free();

      meshCount = Meshes.Length;
      if(meshCount > MAX_MESH_COUNT) {
         meshCount = MAX_MESH_COUNT;
      }
      meshTextureCount = MeshTextures.Length;
      if(meshTextureCount > MAX_TEXTURE_COUNT) {
         meshTextureCount = MAX_TEXTURE_COUNT;
      }
      shaderCount = SoftwareInstancingShaders.Length;
      if(shaderCount > MAX_SHADER_COUNT) {
         shaderCount = MAX_SHADER_COUNT;
      }
      if(Mode == CompileMode.COMMIT_DATA_CHANGE) {
         Message += "Meshe, texture, and shader data succesfuly reloaded! Returning to RELEASE mode.\n";
         if(Meshes.Length > MAX_MESH_COUNT) {
            Message += "Maximum number of meshes is 8. Please reduce the array length.\n";
         }
         if(MeshTextures.Length > MAX_TEXTURE_COUNT) {
            Message += "Maximum number of mesh textures is 8. Please reduce the array length.\n";
         }
         if(SoftwareInstancingShaders.Length > MAX_SHADER_COUNT) {
            Message += "Maximum number of shaders is 8. Please reduce the array length.\n";
         }
         Mode = CompileMode.RELEASE;
      }
   }

   void SendTextureDataToPlugin() {
      byte[] textureData;
      GCHandle gcTextureData;
      for(int i = 0; i < ((MeshTextures.Length > MAX_TEXTURE_COUNT) ? MAX_TEXTURE_COUNT : MeshTextures.Length); i++) {

         textureData = MeshTextures[i].EncodeToPNG();

         gcTextureData = GCHandle.Alloc(textureData, GCHandleType.Pinned);
         SetMeshTexture(gcTextureData.AddrOfPinnedObject(), textureData.Length, i);
         gcTextureData.Free();
      }
   }

   void SetShaderDataToPlugin() {
      byte[] shaderData;
      GCHandle gcShaderData;
      for(int i = 0; i < ((SoftwareInstancingShaders.Length > MAX_SHADER_COUNT) ? MAX_SHADER_COUNT : SoftwareInstancingShaders.Length); i++) {

         shaderData = SoftwareInstancingShaders[i].bytes;

         gcShaderData = GCHandle.Alloc(shaderData, GCHandleType.Pinned);
         SetShaders(gcShaderData.AddrOfPinnedObject(), shaderData.Length, i);
         gcShaderData.Free();
      }

      shaderData = HardwareInstancingShader.bytes;
      gcShaderData = GCHandle.Alloc(shaderData, GCHandleType.Pinned);
      SetShaders(gcShaderData.AddrOfPinnedObject(), shaderData.Length, 1024);
      gcShaderData.Free();
   }

   void SendLightingAndMaterialDataToPlugin() {

      Vector3 direction = DirectionalLight.transform.forward.normalized * DirectionalLight.intensity;
      Color lightColor = DirectionalLight.color;

      SetDirectionalLight(-1.0f * direction.x, -1.0f * direction.y, -1.0f * direction.z);
      SetDirectionalLightColor(lightColor.r, lightColor.g, lightColor.b, lightColor.a);
      SetAmbientLight(AmbientLight.r, AmbientLight.g, AmbientLight.b, AmbientLight.a);

      SetAmbientMaterial(AmbientMaterial.r, AmbientMaterial.g, AmbientMaterial.b, AmbientMaterial.a);
      SetDiffuseMaterial(DiffuseMaterial.r, DiffuseMaterial.g, DiffuseMaterial.b, DiffuseMaterial.a);
   }

   void SendPerInstanceDataToPlugin() {

      GCHandle gcTransforms = GCHandle.Alloc(instanceTransforms, GCHandleType.Pinned);
      SetStaticObjectInstantiation(gcTransforms.AddrOfPinnedObject(), instanceTransforms.Length);
      gcTransforms.Free();

      if(Mode == CompileMode.EDIT) {

         Message += "Per instance data loaded! Returning to RELEASE mode.\n";
         Mode = CompileMode.RELEASE;
      }
   }

   void FormatInstanceData() {

      instanceTransforms = new CUSTOMTRANSFORM[InstanceTransforms.Length];
      Matrix4x4 mat, invMat;

      int[] instancesPerMesh = new int[Meshes.Length];

      for(int i = 0; i < Meshes.Length; i++) {
         instancesPerMesh[i] = 0;
      }

      for(int i = 0; i < InstanceTransforms.Length; i++) {
         instancesPerMesh[InstanceTransforms[i].MeshIndex]++;
      }

      for(int i = 0; i < instanceTransforms.Length; i++) {
         instanceTransforms[i].PosX = InstanceTransforms[i].Position.x;
         instanceTransforms[i].PosY = InstanceTransforms[i].Position.y;
         instanceTransforms[i].PosZ = InstanceTransforms[i].Position.z;

         instanceTransforms[i].RotX = InstanceTransforms[i].Rotation.x * (2 * Mathf.PI / 360);
         instanceTransforms[i].RotY = InstanceTransforms[i].Rotation.y * (2 * Mathf.PI / 360);
         instanceTransforms[i].RotZ = InstanceTransforms[i].Rotation.z * (2 * Mathf.PI / 360);

         instanceTransforms[i].ScaleX = InstanceTransforms[i].Scale.x;
         instanceTransforms[i].ScaleY = InstanceTransforms[i].Scale.y;
         instanceTransforms[i].ScaleZ = InstanceTransforms[i].Scale.z;

         if(InstanceTransforms[i].MeshIndex < MIN_MESH_COUNT) {
            InstanceTransforms[i].MeshIndex = MIN_MESH_COUNT - 1;
         }
         if(InstanceTransforms[i].MeshIndex > meshCount - 1) {
            InstanceTransforms[i].MeshIndex = meshCount - 1;
         }

         if(InstanceTransforms[i].TextureIndex < MIN_TEXTURE_COUNT) {
            InstanceTransforms[i].TextureIndex = MIN_TEXTURE_COUNT - 1;
         }
         if(InstanceTransforms[i].TextureIndex > meshTextureCount - 1) {
            InstanceTransforms[i].TextureIndex = meshTextureCount - 1;
         }

         if(InstanceTransforms[i].ShaderIndex < MIN_SHADER_COUNT) {
            InstanceTransforms[i].ShaderIndex = MIN_SHADER_COUNT - 1;
         }
         if(InstanceTransforms[i].ShaderIndex > shaderCount - 1) {
            InstanceTransforms[i].ShaderIndex = shaderCount - 1;
         }

         instanceTransforms[i].meshIndx = InstanceTransforms[i].MeshIndex;
         instanceTransforms[i].textIndx = InstanceTransforms[i].TextureIndex;
         instanceTransforms[i].shaderIndex = InstanceTransforms[i].ShaderIndex;
         instanceTransforms[i].alphaCutoff = InstanceTransforms[i].AlphaCutoff;
         instanceTransforms[i].isTransparent = InstanceTransforms[i].IsTransparent;
      }

      for(int i = 0; i < Meshes.Length; i++) {

         instanceD3DXTransforms = new PER_STATIC_INSTANCE_STRUCT[instancesPerMesh[i]];

         int k = 0;

         for(int j = 0; j < InstanceTransforms.Length; j++) {

            if(InstanceTransforms[j].MeshIndex == i) {

               mat = CreateD3DXMatrix(InstanceTransforms[j].Position, InstanceTransforms[j].Rotation, InstanceTransforms[j].Scale);
               invMat = CreateInverseMatrix(mat);

               instanceD3DXTransforms[k].alphaCutoff = InstanceTransforms[j].AlphaCutoff;

               instanceD3DXTransforms[k]._11 = mat.m00;
               instanceD3DXTransforms[k]._12 = mat.m01;
               instanceD3DXTransforms[k]._13 = mat.m02;
               instanceD3DXTransforms[k]._14 = mat.m03;

               instanceD3DXTransforms[k]._21 = mat.m10;
               instanceD3DXTransforms[k]._22 = mat.m11;
               instanceD3DXTransforms[k]._23 = mat.m12;
               instanceD3DXTransforms[k]._24 = mat.m13;

               instanceD3DXTransforms[k]._31 = mat.m20;
               instanceD3DXTransforms[k]._32 = mat.m21;
               instanceD3DXTransforms[k]._33 = mat.m22;
               instanceD3DXTransforms[k]._34 = mat.m23;

               instanceD3DXTransforms[k]._41 = mat.m30;
               instanceD3DXTransforms[k]._42 = mat.m31;
               instanceD3DXTransforms[k]._43 = mat.m32;
               instanceD3DXTransforms[k]._44 = mat.m33;

               //instanceD3DXTransforms[i].inv_11 = invMat.m00;
               //instanceD3DXTransforms[i].inv_12 = invMat.m01;
               //instanceD3DXTransforms[i].inv_13 = invMat.m02;
               //instanceD3DXTransforms[i].inv_14 = invMat.m03;

               //instanceD3DXTransforms[i].inv_21 = invMat.m10;
               //instanceD3DXTransforms[i].inv_22 = invMat.m11;
               //instanceD3DXTransforms[i].inv_23 = invMat.m12;
               //instanceD3DXTransforms[i].inv_24 = invMat.m13;

               //instanceD3DXTransforms[i].inv_31 = invMat.m20;
               //instanceD3DXTransforms[i].inv_32 = invMat.m21;
               //instanceD3DXTransforms[i].inv_33 = invMat.m22;
               //instanceD3DXTransforms[i].inv_34 = invMat.m23;

               //instanceD3DXTransforms[i].inv_41 = invMat.m30;
               //instanceD3DXTransforms[i].inv_42 = invMat.m31;
               //instanceD3DXTransforms[i].inv_43 = invMat.m32;
               //instanceD3DXTransforms[i].inv_44 = invMat.m33;
               k++;
            }
         }
         if(instanceD3DXTransforms.Length > 0) {
            GCHandle gcTransformsD3DX = GCHandle.Alloc(instanceD3DXTransforms, GCHandleType.Pinned);
            SetStaticObjectInstantiationD3DX(gcTransformsD3DX.AddrOfPinnedObject(), instanceD3DXTransforms.Length, i);
            gcTransformsD3DX.Free();
         }
      }
   }

   void SetMode() {

      if(Mode == CompileMode.COMMIT_DATA_CHANGE) {
         ActivateMode(true);
      }
      if(Mode == CompileMode.EDIT) {
         ActivateMode(false);
      }
      if(Mode == CompileMode.RELEASE) {
         ActivateMode(false);
      }
   }

   void CheckInstancingDataMode() {
      if(InstanceMode == InstanceMode.LOAD) {
         ReadPerInstanceDataFromFile();
         Mode = CompileMode.EDIT;
      }
      if(InstanceMode == InstanceMode.SAVE) {
         saveInstanceData = true;
         messageSent = false;
      }
      if(InstanceMode == InstanceMode.DISCARD) {
         discardInstanceData = true;
         messageSent = false;
      }
      if(InstanceMode == InstanceMode.RANDOMIZE) {
         RandomizePerInstanceData();
         Mode = CompileMode.EDIT;
      }
      InstanceMode = InstanceMode.EDIT;

      if(saveInstanceData == true && discardInstanceData == true) {
         Message += "Please select an Instance Mode that represents what you want to do.\n";
         saveInstanceData = false;
         discardInstanceData = false;
         messageSent = false;
      }
      if(saveInstanceData == true && discardInstanceData == false && !messageSent) {
         Message += "Per instance data will be SAVED.\n";
         messageSent = true;
         SavePerInstanceData();
      }
      if(saveInstanceData == false && discardInstanceData == true && !messageSent) {
         Message += "Per instance data will be DISCARDED.\n";
         messageSent = true;
      }
   }

   void SavePerInstanceData() {

      string perInstData = string.Empty;

      for(int i = 0; i < ForestAreaRandomizers.Length; i++) {
         perInstData += ForestAreaRandomizers[i].NumberOfTrees.ToString();
         perInstData += "\n";

         perInstData += ForestAreaRandomizers[i].BeginCoordinates.x.ToString();
         perInstData += "\n";
         perInstData += ForestAreaRandomizers[i].BeginCoordinates.y.ToString();
         perInstData += "\n";

         perInstData += ForestAreaRandomizers[i].EndCoordinates.x.ToString();
         perInstData += "\n";
         perInstData += ForestAreaRandomizers[i].EndCoordinates.y.ToString();
         perInstData += "\n";

         perInstData += ForestAreaRandomizers[i].Rotation.x.ToString();
         perInstData += "\n";
         perInstData += ForestAreaRandomizers[i].Rotation.y.ToString();
         perInstData += "\n";
         perInstData += ForestAreaRandomizers[i].Rotation.z.ToString();
         perInstData += "\n";

         perInstData += ForestAreaRandomizers[i].RotationalVariation.x.ToString();
         perInstData += "\n";
         perInstData += ForestAreaRandomizers[i].RotationalVariation.y.ToString();
         perInstData += "\n";
         perInstData += ForestAreaRandomizers[i].RotationalVariation.z.ToString();
         perInstData += "\n";

         perInstData += ForestAreaRandomizers[i].Scale.x.ToString();
         perInstData += "\n";
         perInstData += ForestAreaRandomizers[i].Scale.y.ToString();
         perInstData += "\n";
         perInstData += ForestAreaRandomizers[i].Scale.z.ToString();
         perInstData += "\n";

         perInstData += ForestAreaRandomizers[i].ScaleVariation.x.ToString();
         perInstData += "\n";
         perInstData += ForestAreaRandomizers[i].ScaleVariation.y.ToString();
         perInstData += "\n";
         perInstData += ForestAreaRandomizers[i].ScaleVariation.z.ToString();
         perInstData += "\n";

         perInstData += ForestAreaRandomizers[i].AlphaCutoff.ToString();
         perInstData += "\n";
         perInstData += ForestAreaRandomizers[i].AplhaCutoffVariation.ToString();
         perInstData += "\n";
      }

      perInstData += "END_OF_RANDOMIZERS";
      perInstData += "\n";

      for(int i = 0; i < InstanceTransforms.Length; i++) {
         perInstData += InstanceTransforms[i].Position.x.ToString();
         perInstData += "\n";
         perInstData += InstanceTransforms[i].Position.y.ToString();
         perInstData += "\n";
         perInstData += InstanceTransforms[i].Position.z.ToString();
         perInstData += "\n";

         perInstData += InstanceTransforms[i].Rotation.x.ToString();
         perInstData += "\n";
         perInstData += InstanceTransforms[i].Rotation.y.ToString();
         perInstData += "\n";
         perInstData += InstanceTransforms[i].Rotation.z.ToString();
         perInstData += "\n";

         perInstData += InstanceTransforms[i].Scale.x.ToString();
         perInstData += "\n";
         perInstData += InstanceTransforms[i].Scale.y.ToString();
         perInstData += "\n";
         perInstData += InstanceTransforms[i].Scale.z.ToString();
         perInstData += "\n";

         perInstData += InstanceTransforms[i].ShaderIndex.ToString();
         perInstData += "\n";
         perInstData += InstanceTransforms[i].MeshIndex.ToString();
         perInstData += "\n";
         perInstData += InstanceTransforms[i].TextureIndex.ToString();
         perInstData += "\n";
         perInstData += InstanceTransforms[i].IsTransparent.ToString();
         perInstData += "\n";
         perInstData += InstanceTransforms[i].AlphaCutoff.ToString();
         perInstData += "\n";
         if(i == InstanceTransforms.Length - 1) {
            perInstData += "END_OF_FILE";
         }
      }

      TextAsset textAsset = Resources.Load("PerInstanceData/" + FileName) as TextAsset;
      if(textAsset == null && !FileName.Equals(string.Empty)) {
         File.WriteAllText(Application.dataPath + "/Resources/PerInstanceData/" + FileName + ".txt", perInstData);
#if UNITY_EDITOR
         AssetDatabase.SaveAssets();
         AssetDatabase.Refresh();
#endif
      } else {
         Message += "File already exists or has an invalid name, please change the file name in the inspector!\n";
      }
   }

   void ReadPerInstanceDataFromFile() {

      string[] elements = PerInstanceData.ToString().Split('\n');

      int numberOfInstances = (elements.Length - 1) / 14;
      int numberOfRandomizers = 0;
      int instanceDataStartIndex = 0;

      for(int i = 0; i < elements.Length; i++) {
         if(elements[i] == "END_OF_RANDOMIZERS") {
            instanceDataStartIndex = i;
            numberOfRandomizers = i / 19;
            numberOfInstances = ((elements.Length - 1) - i) / 14;
            break;
         }
      }

      InstanceTransforms = new CUSTOMTRANSFORMSTRUCT[numberOfInstances];
      ForestAreaRandomizers = new RANDOMIZERSTRUCT[numberOfRandomizers];

      for(int i = 0; i < numberOfRandomizers; i++) {

         ForestAreaRandomizers[i].NumberOfTrees = int.Parse(elements[i * 19]);

         ForestAreaRandomizers[i].BeginCoordinates.x = float.Parse(elements[i * 19 + 1]);
         ForestAreaRandomizers[i].BeginCoordinates.y = float.Parse(elements[i * 19 + 2]);

         ForestAreaRandomizers[i].EndCoordinates.x = float.Parse(elements[i * 19 + 3]);
         ForestAreaRandomizers[i].EndCoordinates.y = float.Parse(elements[i * 19 + 4]);

         ForestAreaRandomizers[i].Rotation.x = float.Parse(elements[i * 19 + 5]);
         ForestAreaRandomizers[i].Rotation.y = float.Parse(elements[i * 19 + 6]);
         ForestAreaRandomizers[i].Rotation.z = float.Parse(elements[i * 19 + 7]);

         ForestAreaRandomizers[i].RotationalVariation.x = float.Parse(elements[i * 19 + 8]);
         ForestAreaRandomizers[i].RotationalVariation.y = float.Parse(elements[i * 19 + 9]);
         ForestAreaRandomizers[i].RotationalVariation.z = float.Parse(elements[i * 19 + 10]);

         ForestAreaRandomizers[i].Scale.x = float.Parse(elements[i * 19 + 11]);
         ForestAreaRandomizers[i].Scale.y = float.Parse(elements[i * 19 + 12]);
         ForestAreaRandomizers[i].Scale.z = float.Parse(elements[i * 19 + 13]);

         ForestAreaRandomizers[i].ScaleVariation.x = float.Parse(elements[i * 19 + 14]);
         ForestAreaRandomizers[i].ScaleVariation.y = float.Parse(elements[i * 19 + 15]);
         ForestAreaRandomizers[i].ScaleVariation.z = float.Parse(elements[i * 19 + 16]);

         ForestAreaRandomizers[i].AlphaCutoff = float.Parse(elements[i * 19 + 17]);
         ForestAreaRandomizers[i].AplhaCutoffVariation = float.Parse(elements[i * 19 + 18]);
      }

      for(int i = 0; i < numberOfInstances; i++) {

         InstanceTransforms[i].Position.x = float.Parse(elements[i * 14 + (instanceDataStartIndex + 1)]);
         InstanceTransforms[i].Position.y = float.Parse(elements[i * 14 + 1 + (instanceDataStartIndex + 1)]);
         InstanceTransforms[i].Position.z = float.Parse(elements[i * 14 + 2 + (instanceDataStartIndex + 1)]);

         InstanceTransforms[i].Rotation.x = float.Parse(elements[i * 14 + 3 + (instanceDataStartIndex + 1)]);
         InstanceTransforms[i].Rotation.y = float.Parse(elements[i * 14 + 4 + (instanceDataStartIndex + 1)]);
         InstanceTransforms[i].Rotation.z = float.Parse(elements[i * 14 + 5 + (instanceDataStartIndex + 1)]);

         InstanceTransforms[i].Scale.x = float.Parse(elements[i * 14 + 6 + (instanceDataStartIndex + 1)]);
         InstanceTransforms[i].Scale.y = float.Parse(elements[i * 14 + 7 + (instanceDataStartIndex + 1)]);
         InstanceTransforms[i].Scale.z = float.Parse(elements[i * 14 + 8 + (instanceDataStartIndex + 1)]);

         InstanceTransforms[i].ShaderIndex = int.Parse(elements[i * 14 + 9 + (instanceDataStartIndex + 1)]);
         InstanceTransforms[i].MeshIndex = int.Parse(elements[i * 14 + 10 + (instanceDataStartIndex + 1)]);
         InstanceTransforms[i].TextureIndex = int.Parse(elements[i * 14 + 11 + (instanceDataStartIndex + 1)]);
         InstanceTransforms[i].IsTransparent = bool.Parse(elements[i * 14 + 12 + (instanceDataStartIndex + 1)]);
         InstanceTransforms[i].AlphaCutoff = float.Parse(elements[i * 14 + 13 + (instanceDataStartIndex + 1)]);
      }
   }

   void RandomizePerInstanceData() {

      float xCoordinateRandom;
      float yCoordinateRandom;
      float xRotRandom;
      float yRotRandom;
      float zRotRandom;
      float xScaleRandom;
      float yScaleRandom;
      float zScaleRandom;
      float alphaCutoffRandom;

      int totalNumnerOfTrees = 0;
      int treesSoFar = 0;

      for(int i = 0; i < ForestAreaRandomizers.Length; i++) {
         totalNumnerOfTrees += ForestAreaRandomizers[i].NumberOfTrees;
      }

      InstanceTransforms = new CUSTOMTRANSFORMSTRUCT[totalNumnerOfTrees * 2];

      for(int i = 0; i < ForestAreaRandomizers.Length; i++) {
         for(int j = treesSoFar; j < (treesSoFar + ForestAreaRandomizers[i].NumberOfTrees * 2); j += 2) {

            xCoordinateRandom = UnityEngine.Random.Range(ForestAreaRandomizers[i].BeginCoordinates.x, ForestAreaRandomizers[i].EndCoordinates.x);
            yCoordinateRandom = UnityEngine.Random.Range(ForestAreaRandomizers[i].BeginCoordinates.y, ForestAreaRandomizers[i].EndCoordinates.y);
            xRotRandom = UnityEngine.Random.Range(1 / ForestAreaRandomizers[i].RotationalVariation.x, ForestAreaRandomizers[i].RotationalVariation.x);
            yRotRandom = UnityEngine.Random.Range(1 / ForestAreaRandomizers[i].RotationalVariation.y, ForestAreaRandomizers[i].RotationalVariation.y);
            zRotRandom = UnityEngine.Random.Range(1 / ForestAreaRandomizers[i].RotationalVariation.z, ForestAreaRandomizers[i].RotationalVariation.z);
            xScaleRandom = UnityEngine.Random.Range(1 / ForestAreaRandomizers[i].ScaleVariation.x, ForestAreaRandomizers[i].ScaleVariation.x);
            yScaleRandom = UnityEngine.Random.Range(1 / ForestAreaRandomizers[i].ScaleVariation.y, ForestAreaRandomizers[i].ScaleVariation.y);
            zScaleRandom = UnityEngine.Random.Range(1 / ForestAreaRandomizers[i].ScaleVariation.z, ForestAreaRandomizers[i].ScaleVariation.z);
            alphaCutoffRandom = UnityEngine.Random.Range(1 / ForestAreaRandomizers[i].AplhaCutoffVariation, ForestAreaRandomizers[i].AplhaCutoffVariation);

            InstanceTransforms[j].Position = new Vector3(xCoordinateRandom, 0.0f, yCoordinateRandom);
            InstanceTransforms[j].Rotation = new Vector3(ForestAreaRandomizers[i].Rotation.x * xRotRandom, ForestAreaRandomizers[i].Rotation.y * yRotRandom, ForestAreaRandomizers[i].Rotation.x * zRotRandom);
            InstanceTransforms[j].Scale = new Vector3(ForestAreaRandomizers[i].Scale.x * xScaleRandom, ForestAreaRandomizers[i].Scale.y * yScaleRandom, ForestAreaRandomizers[i].Scale.z * zScaleRandom);
            InstanceTransforms[j].ShaderIndex = 0;
            InstanceTransforms[j].MeshIndex = Mathf.CeilToInt(UnityEngine.Random.Range(-0.999f, Meshes.Length - 1));
            if(InstanceTransforms[j].MeshIndex > 0 && InstanceTransforms[j].MeshIndex % 2 != 0) {
               InstanceTransforms[j].MeshIndex -= 1;
            }
            InstanceTransforms[j].TextureIndex = 0;
            InstanceTransforms[j].IsTransparent = false;
            InstanceTransforms[j].AlphaCutoff = ForestAreaRandomizers[i].AlphaCutoff * UnityEngine.Random.Range(1 / ForestAreaRandomizers[i].AplhaCutoffVariation, ForestAreaRandomizers[i].AplhaCutoffVariation);

            InstanceTransforms[j + 1].Position = InstanceTransforms[j].Position;
            InstanceTransforms[j + 1].Rotation = InstanceTransforms[j].Rotation;
            InstanceTransforms[j + 1].Scale = InstanceTransforms[j].Scale;
            InstanceTransforms[j + 1].ShaderIndex = InstanceTransforms[j].ShaderIndex;
            InstanceTransforms[j + 1].MeshIndex = InstanceTransforms[j].MeshIndex + 1;
            InstanceTransforms[j + 1].TextureIndex = InstanceTransforms[j].TextureIndex;
            InstanceTransforms[j + 1].IsTransparent = !InstanceTransforms[j].IsTransparent;
            InstanceTransforms[j + 1].AlphaCutoff = InstanceTransforms[j].AlphaCutoff;
            if(Meshes.Length > 1) {
               InstanceTransforms[j + 1].Position = InstanceTransforms[j].Position;
               InstanceTransforms[j + 1].Rotation = InstanceTransforms[j].Rotation;
               InstanceTransforms[j + 1].Scale = InstanceTransforms[j].Scale;
               InstanceTransforms[j + 1].ShaderIndex = InstanceTransforms[j].ShaderIndex;
               InstanceTransforms[j + 1].MeshIndex = InstanceTransforms[j].MeshIndex + 1;
               InstanceTransforms[j + 1].TextureIndex = InstanceTransforms[j].TextureIndex;
               InstanceTransforms[j + 1].IsTransparent = !InstanceTransforms[j].IsTransparent;
               InstanceTransforms[j + 1].AlphaCutoff = InstanceTransforms[j].AlphaCutoff;
            } else {
               xCoordinateRandom = UnityEngine.Random.Range(ForestAreaRandomizers[i].BeginCoordinates.x, ForestAreaRandomizers[i].EndCoordinates.x);
               yCoordinateRandom = UnityEngine.Random.Range(ForestAreaRandomizers[i].BeginCoordinates.y, ForestAreaRandomizers[i].EndCoordinates.y);
               xRotRandom = UnityEngine.Random.Range(1 / ForestAreaRandomizers[i].RotationalVariation.x, ForestAreaRandomizers[i].RotationalVariation.x);
               yRotRandom = UnityEngine.Random.Range(1 / ForestAreaRandomizers[i].RotationalVariation.y, ForestAreaRandomizers[i].RotationalVariation.y);
               zRotRandom = UnityEngine.Random.Range(1 / ForestAreaRandomizers[i].RotationalVariation.z, ForestAreaRandomizers[i].RotationalVariation.z);
               xScaleRandom = UnityEngine.Random.Range(1 / ForestAreaRandomizers[i].ScaleVariation.x, ForestAreaRandomizers[i].ScaleVariation.x);
               yScaleRandom = UnityEngine.Random.Range(1 / ForestAreaRandomizers[i].ScaleVariation.y, ForestAreaRandomizers[i].ScaleVariation.y);
               zScaleRandom = UnityEngine.Random.Range(1 / ForestAreaRandomizers[i].ScaleVariation.z, ForestAreaRandomizers[i].ScaleVariation.z);
               alphaCutoffRandom = UnityEngine.Random.Range(1 / ForestAreaRandomizers[i].AplhaCutoffVariation, ForestAreaRandomizers[i].AplhaCutoffVariation);

               InstanceTransforms[j + 1].Position = new Vector3(xCoordinateRandom, 0.0f, yCoordinateRandom);
               InstanceTransforms[j + 1].Rotation = new Vector3(ForestAreaRandomizers[i].Rotation.x * xRotRandom, ForestAreaRandomizers[i].Rotation.y * yRotRandom, ForestAreaRandomizers[i].Rotation.x * zRotRandom);
               InstanceTransforms[j + 1].Scale = new Vector3(ForestAreaRandomizers[i].Scale.x * xScaleRandom, ForestAreaRandomizers[i].Scale.y * yScaleRandom, ForestAreaRandomizers[i].Scale.z * zScaleRandom);
               InstanceTransforms[j + 1].ShaderIndex = 0;
               InstanceTransforms[j + 1].MeshIndex = 0;
               InstanceTransforms[j + 1].TextureIndex = 0;
               InstanceTransforms[j + 1].IsTransparent = false;
               InstanceTransforms[j + 1].AlphaCutoff = ForestAreaRandomizers[i].AlphaCutoff * UnityEngine.Random.Range(1 / ForestAreaRandomizers[i].AplhaCutoffVariation, ForestAreaRandomizers[i].AplhaCutoffVariation);
            }
         }
         treesSoFar += (ForestAreaRandomizers[i].NumberOfTrees * 2);
      }
   }

   Matrix4x4 CreateD3DXMatrix(Vector3 position, Vector3 rotation, Vector3 scale) {

      Matrix4x4 matRotateX = new Matrix4x4();
      Matrix4x4 matRotateY = new Matrix4x4();
      Matrix4x4 matRotateZ = new Matrix4x4();
      Matrix4x4 matScale = new Matrix4x4();
      Matrix4x4 matWorldTransform = new Matrix4x4();

      matRotateX = Matrix4x4.identity;
      matRotateX.m11 = Mathf.Cos(rotation.x * (2 * Mathf.PI) / 360);
      matRotateX.m12 = Mathf.Sin(rotation.x * (2 * Mathf.PI) / 360);
      matRotateX.m21 = -Mathf.Sin(rotation.x * (2 * Mathf.PI) / 360);
      matRotateX.m22 = Mathf.Cos(rotation.x * (2 * Mathf.PI) / 360);

      matRotateY = Matrix4x4.identity;
      matRotateY.m00 = Mathf.Cos(rotation.y * (2 * Mathf.PI) / 360);
      matRotateY.m02 = -Mathf.Sin(rotation.y * (2 * Mathf.PI) / 360);
      matRotateY.m20 = Mathf.Sin(rotation.y * (2 * Mathf.PI) / 360);
      matRotateY.m22 = Mathf.Cos(rotation.y * (2 * Mathf.PI) / 360);

      matRotateZ = Matrix4x4.identity;
      matRotateZ.m00 = Mathf.Cos(rotation.z * (2 * Mathf.PI) / 360);
      matRotateZ.m01 = Mathf.Sin(rotation.z * (2 * Mathf.PI) / 360);
      matRotateZ.m10 = -Mathf.Sin(rotation.z * (2 * Mathf.PI) / 360);
      matRotateZ.m11 = Mathf.Cos(rotation.z * (2 * Mathf.PI) / 360);

      matScale = Matrix4x4.identity;
      matScale.m00 = scale.x;
      matScale.m11 = scale.y;
      matScale.m22 = scale.z;

      matWorldTransform = Matrix4x4.identity;
      matWorldTransform.m30 = position.x;
      matWorldTransform.m31 = position.y;
      matWorldTransform.m32 = position.z;

      return matRotateX * matRotateY * matRotateZ * matScale * matWorldTransform;
   }

   Matrix4x4 CreateInverseMatrix(Matrix4x4 inputMat) {

      return inputMat.inverse;
   }
}