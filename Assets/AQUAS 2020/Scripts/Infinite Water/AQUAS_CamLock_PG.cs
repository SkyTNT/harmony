using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace AQUAS {
    public class AQUAS_CamLock_PG : MonoBehaviour {
        
        [Range(32, 256)]
        public int resolution = 64;

        public GameObject MainCamera;

        public Camera[] cameras;
        public Camera[] sceneCameras;

        // Use this for initialization...
        void Start() {

#if UNITY_EDITOR

            if (GetComponent<Renderer>().sharedMaterials[0].shader.name != "AQUAS/Desktop/Front/Default" && GetComponent<Renderer>().sharedMaterials[0].shader.name != "Hidden/AQUAS/Desktop/Front/Opaque")
            {
                if (GetComponent<Renderer>().sharedMaterials[0].shader.name == "AQUAS/Desktop/Front/Shallow")
                {
                    EditorUtility.DisplayDialog("Unsupported Shader!", "AQUAS is currently using a shader for shallow water. This shader does not support dynamic meshes - Aborting grid projection.", "OK");
                }
                else
                {
                    EditorUtility.DisplayDialog("Unsupported Shader", "The shader in use on the waterplane does not support dynamic meshes - Aborting grid projection", "OK");
                }
                return;
            }
#endif

            transform.localScale = new Vector3(1, 1, 1);

            GetComponent<MeshFilter>().mesh = CreateMesh();

            Material[] materials = GetComponent<Renderer>().sharedMaterials;

            gameObject.AddComponent<SkinnedMeshRenderer>();
            GetComponent<SkinnedMeshRenderer>().localBounds = new Bounds(Vector3.zero, new Vector3(1000000000, 1000000000, 1000000000));
            GetComponent<SkinnedMeshRenderer>().sharedMaterials = materials;

            GetComponent<AQUAS_Reflection>().overrideWaterLevel = true;
            GetComponent<AQUAS_Reflection>().waterLevel = transform.position.y;

            MainCamera.AddComponent<AQUAS_ProjectedGrid>();
            MainCamera.GetComponent<AQUAS_ProjectedGrid>().hideFlags = HideFlags.HideInInspector;
            MainCamera.GetComponent<AQUAS_ProjectedGrid>().waterplane = gameObject;
    
        }

        ///<summary>
        ///Planned to hook up all cameras including the scene camera, but underwater effects fail, so the feature is limited to one camera
        ///</summary>
        /*private void Update()
        {
            if (Camera.allCameras.Length != cameras.Length)
            {
                GetCameras();
                HookUpCameras();
            }

#if UNITY_EDITOR
            if (SceneView.GetAllSceneCameras().Length != sceneCameras.Length)
            {
                GetSceneCameras();
                HookUpSceneCameras();
            }
#endif
        }*/

        Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();

            Vector3[] vertices = new Vector3[resolution * resolution];
            Vector2[] uvs = new Vector2[resolution * resolution];
            int[] triangles = new int[(resolution - 1) * (resolution - 1) * 6];

            float incrementX = 1 / (float)(resolution - 1);
            float incrementY = 1 / (float)(resolution - 1);

            for (int i = 0; i < resolution; i++)
            {
                float x = incrementX * i - 0.5f;

                for (int j = 0; j < resolution; j++)
                {
                    float y = incrementY * j - 0.5f;

                    vertices[i * resolution + j] = new Vector3(x, 0, y);
                    uvs[i * resolution + j] = new Vector2(x, y);
                }
            }

            for (int i = 0; i < (resolution - 1) * (resolution - 1); i++)
            {
                int column = (int)Mathf.Floor(i / (resolution - 1));

                triangles[i * 6] = column * (resolution) + i % (resolution - 1);
                triangles[i * 6 + 1] = column * (resolution) + i % (resolution - 1) + 1;
                triangles[i * 6 + 2] = (column + 1) * (resolution) + i % (resolution - 1);
                triangles[i * 6 + 3] = (column + 1) * (resolution) + i % (resolution - 1);
                triangles[i * 6 + 4] = column * (resolution) + i % (resolution - 1) + 1;
                triangles[i * 6 + 5] = (column + 1) * (resolution) + i % (resolution - 1) + 1;
            }

            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();

            return mesh;
        }

        void GetCameras()
        {
            cameras = Camera.allCameras;
        }

#if UNITY_EDITOR
        void GetSceneCameras()
        {
            sceneCameras = SceneView.GetAllSceneCameras();
        }
#endif

        void HookUpCameras()
        {
            foreach (Camera camera in cameras)
            {
                if (camera.gameObject.GetComponent<AQUAS_ProjectedGrid>() == null && camera.transform.parent == null)
                {
#if UNITY_EDITOR
                    if (camera.GetComponent<AQUAS_UnderWaterEffect>() == null)
                    {
                        EditorUtility.DisplayDialog("No Advanced Underwater Effects", "No AQUAS_UnderwaterEffects Component detected on " + camera.name + ". " + camera.name + " won't benefit from the dynamic mesh.", "OK");
                    }
#endif
                    camera.gameObject.AddComponent<AQUAS_ProjectedGrid>();
                    camera.gameObject.GetComponent<AQUAS_ProjectedGrid>().waterplane = gameObject;
                    //camera.gameObject.GetComponent<AQUAS_ProjectedGrid>().hideFlags = HideFlags.HideAndDontSave;
                }
            }
        }

        void HookUpSceneCameras()
        {
            foreach (Camera camera in sceneCameras)
            {
                if (camera.gameObject.GetComponent<AQUAS_ProjectedGrid>() == null)
                {
                    camera.gameObject.AddComponent<AQUAS_ProjectedGrid>();
                    camera.gameObject.GetComponent<AQUAS_ProjectedGrid>().waterplane = gameObject;
                    //camera.gameObject.GetComponent<AQUAS_ProjectedGrid>().hideFlags = HideFlags.HideAndDontSave;
                }
            }
        }
    }
}
