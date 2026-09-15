using System;
using System.IO;
using Hackathon.Map;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hackathon.Editor
{
    public static class CodexRoboBuilder
    {
        public const string Folder = "Assets/_Project/Content/Features/Map/CodexRobo";
        public const string PrefabPath = Folder + "/CodexRobo.prefab";
        public const string ScenePath = "Assets/_Project/Scenes/CodexRoboMap.unity";
        const string Source = "Assets/_Project/Content/Features/RoomOne/CloudRobot/Models/CloudRobot_Terminal.fbx";
        static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;
            string parent = Path.GetDirectoryName(path).Replace('\\','/'); EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent,Path.GetFileName(path));
        }
        static void Save(UnityEngine.Object asset, string path)
        {
            var previous = AssetDatabase.LoadMainAssetAtPath(path);
            if (previous == null) AssetDatabase.CreateAsset(asset,path);
            else
            {
                EditorUtility.CopySerialized(asset,previous); EditorUtility.SetDirty(previous);
                AssetDatabase.SaveAssetIfDirty(previous); UnityEngine.Object.DestroyImmediate(asset);
            }
        }
        [MenuItem("Tools/Codex Robo/Create Walking Prefab and Map")]
        public static void Build()
        {
            if (EditorApplication.isPlaying) throw new InvalidOperationException("Stop Play mode before generating assets.");
            EnsureFolder(Folder);
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Source);
            if (source == null) throw new InvalidOperationException("Import the source FBX first (git lfs pull).");
            var original = UnityEngine.Object.Instantiate(source);
            var root = new GameObject("codex robo");
            try
            {
                var filters = original.GetComponentsInChildren<MeshFilter>();
                if (filters.Length != 1) throw new InvalidOperationException("Expected the single-mesh codex robo source.");
                var filter = filters[0]; var originalMesh = filter.sharedMesh;
                var matrix = filter.transform.localToWorldMatrix;
                var mesh = UnityEngine.Object.Instantiate(originalMesh); mesh.name = "CodexRobo_Skinned";
                var vertices = mesh.vertices; var normals = mesh.normals; var tangents = mesh.tangents;
                Bounds bounds = original.GetComponentInChildren<Renderer>().bounds;
                Vector3 center = new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
                float scale = 1f / bounds.size.y;
                for (int i=0;i<vertices.Length;i++)
                {
                    vertices[i] = (matrix.MultiplyPoint3x4(vertices[i])-center)*scale;
                    normals[i] = matrix.inverse.transpose.MultiplyVector(normals[i]).normalized;
                    if (tangents.Length == vertices.Length)
                    {
                        Vector3 t = matrix.MultiplyVector(tangents[i]).normalized;
                        tangents[i] = new Vector4(t.x,t.y,t.z,tangents[i].w);
                    }
                }
                mesh.vertices=vertices; mesh.normals=normals; if (tangents.Length==vertices.Length) mesh.tangents=tangents;
                var rig = new GameObject("Rig").transform; rig.SetParent(root.transform,false);
                var bones = new Transform[6];
                string[] names = {"Body","Head","LeftArm","RightArm","LeftLeg","RightLeg"};
                Vector3[] pivots = {new Vector3(0,.21f,0),new Vector3(0,.38f,0),new Vector3(.19f,.335f,0),new Vector3(-.19f,.335f,0),new Vector3(.095f,.135f,0),new Vector3(-.095f,.135f,0)};
                for (int i=0;i<bones.Length;i++)
                {
                    bones[i]=new GameObject(names[i]).transform;
                    bones[i].SetParent(i==0 ? rig : bones[0],false); bones[i].position=pivots[i];
                }
                var weights = new BoneWeight[vertices.Length]; var counts = new int[6];
                var influences = new float[4]; var indices = new int[4];
                for (int i=0;i<vertices.Length;i++)
                {
                    Vector3 p=vertices[i];
                    float head=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.355f,.385f,p.y));
                    float threshold=Mathf.Lerp(.19f,.17f,Mathf.InverseLerp(.16f,.345f,p.y));
                    float arm=Mathf.SmoothStep(0,1,Mathf.InverseLerp(threshold,threshold+.035f,Mathf.Abs(p.x)));
                    float leg=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.115f,.155f,p.y));
                    // Continuous overlaps avoid cutting a wrist where it crosses the hip's height.
                    influences[0]=head; indices[0]=1;
                    influences[1]=(1-head)*arm; indices[1]=p.x>=0?2:3;
                    influences[2]=(1-head)*(1-arm)*leg; indices[2]=p.x>=0?4:5;
                    influences[3]=(1-head)*(1-arm)*(1-leg); indices[3]=0;
                    for(int a=0;a<3;a++) for(int b=a+1;b<4;b++)
                        if(influences[b]>influences[a])
                        {
                            float weight=influences[a]; influences[a]=influences[b]; influences[b]=weight;
                            int index=indices[a]; indices[a]=indices[b]; indices[b]=index;
                        }
                    weights[i]=new BoneWeight {boneIndex0=indices[0],weight0=influences[0],boneIndex1=indices[1],weight1=influences[1],boneIndex2=indices[2],weight2=influences[2],boneIndex3=indices[3],weight3=influences[3]};
                    counts[indices[0]]++;
                }
                foreach (int count in counts) if(count<100) throw new InvalidOperationException("Rig region has insufficient vertices.");
                mesh.boneWeights=weights;
                var bindposes=new Matrix4x4[6]; for(int i=0;i<6;i++) bindposes[i]=bones[i].worldToLocalMatrix * root.transform.localToWorldMatrix;
                mesh.bindposes=bindposes; mesh.RecalculateBounds();
                Save(mesh,Folder+"/CodexRobo_Skinned.asset");
                var material=new Material(Shader.Find("Standard")) {name="CodexRobo"};
                material.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Path.GetDirectoryName(Source).Replace('\\','/')+"/tripo_convert_ed1accbd-4268-4f25-8db6-660239cea543.fbm/CloudRobot_Terminal_basecolor.JPEG");
                material.SetFloat("_Glossiness",.22f); material.SetFloat("_Metallic",0);
                Save(material,Folder+"/CodexRobo.mat");
                var visual=new GameObject("Visual"); visual.transform.SetParent(root.transform,false);
                var skin=visual.AddComponent<SkinnedMeshRenderer>(); skin.sharedMesh=AssetDatabase.LoadAssetAtPath<Mesh>(Folder+"/CodexRobo_Skinned.asset");
                skin.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/CodexRobo.mat"); skin.bones=bones; skin.rootBone=bones[0];
                skin.localBounds=new Bounds(new Vector3(0,.5f,0),new Vector3(1.3f,1.3f,1)); skin.quality=SkinQuality.Bone4;
                Save(CreateClip(false),Folder+"/Idle.anim"); Save(CreateClip(true),Folder+"/Walk.anim");
                var animation=root.AddComponent<Animation>();
                var idle=AssetDatabase.LoadAssetAtPath<AnimationClip>(Folder+"/Idle.anim");
                animation.AddClip(idle,"Idle"); animation.AddClip(AssetDatabase.LoadAssetAtPath<AnimationClip>(Folder+"/Walk.anim"),"Walk");
                animation.clip=idle; animation.playAutomatically=true; animation.cullingType=AnimationCullingType.AlwaysAnimate;
                var controller=root.AddComponent<CharacterController>(); controller.height=1; controller.radius=.26f; controller.center=new Vector3(0,.5f,0);
                controller.stepOffset=.12f; controller.skinWidth=.015f; controller.minMoveDistance=0; controller.slopeLimit=40;
                root.AddComponent<CodexRoboMotor>();
                PrefabUtility.SaveAsPrefabAsset(root,PrefabPath);
                File.WriteAllText("Temp/CodexRobo-rig.txt","Vertices: "+vertices.Length+"\nBone vertex counts: "+string.Join(",",counts)+"\nSix bones, normalized weights, Idle and Walk clips.\n");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); UnityEngine.Object.DestroyImmediate(original); }
            if (!File.Exists(ScenePath)) CreateMap();
            Debug.Log("[CodexRobo] Walking prefab and map generated.");
        }
        static AnimationClip CreateClip(bool walk)
        {
            var clip=new AnimationClip { name=walk?"Walk":"Idle",legacy=true,frameRate=30,wrapMode=WrapMode.Loop };
            float duration=walk?.68f:2.4f;
            Action<string,string,Func<float,float>> curve=(path,property,value)=>
            {
                var keys=new Keyframe[33];
                for(int i=0;i<keys.Length;i++) {float t=(float)i/(keys.Length-1); keys[i]=new Keyframe(t*duration,value(t*2*Mathf.PI));}
                var c=new AnimationCurve(keys); for(int i=0;i<keys.Length;i++) c.SmoothTangents(i,0);
                c.preWrapMode=c.postWrapMode=WrapMode.Loop; clip.SetCurve(path,typeof(Transform),property,c);
            };
            curve("Rig/Body","localPosition.y",p=>.21f+(walk?.008f*(1-Mathf.Cos(p*2)):.002f*Mathf.Sin(p)));
            curve("Rig/Body","localEulerAnglesRaw.z",p=>walk?1.2f*Mathf.Sin(p):0);
            curve("Rig/Body/Head","localEulerAnglesRaw.y",p=>walk?-2*Mathf.Sin(p):1.2f*Mathf.Sin(p));
            foreach(string limb in new[]{"LeftArm","RightArm","LeftLeg","RightLeg"})
            {
                bool left=limb.StartsWith("Left"),leg=limb.EndsWith("Leg");
                float sign=(left?1:-1)*(leg?1:-1);
                curve("Rig/Body/"+limb,"localEulerAnglesRaw.x",p=>(walk?(leg?24:22):1.5f)*sign*Mathf.Sin(p));
                if(leg)
                {
                    // Unity stores position curves as Vector3; preserve the hip spacing explicitly.
                    curve("Rig/Body/"+limb,"localPosition.x",p=>left?.095f:-.095f);
                    curve("Rig/Body/"+limb,"localPosition.y",p=>-.075f+(walk?.026f*Mathf.Abs(Mathf.Sin(p)):0));
                    curve("Rig/Body/"+limb,"localPosition.z",p=>0);
                }
            }
            return clip;
        }
        static Material MapMaterial(string name,Color color)
        {
            var mat=new Material(Shader.Find("Standard")) {name=name,color=color}; mat.SetFloat("_Glossiness",.08f);
            Save(mat,Folder+"/"+name+".mat"); return AssetDatabase.LoadAssetAtPath<Material>(Folder+"/"+name+".mat");
        }
        public static void CreateMap()
        {
            if(File.Exists(ScenePath)) throw new InvalidOperationException("Map already exists; preserve its scene edits.");
            var previous=SceneManager.GetActiveScene();
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive); SceneManager.SetActiveScene(scene);
            try
            {
                RenderSettings.ambientLight=new Color(.6f,.65f,.75f); RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;
                var ground=MapMaterial("Floor",new Color(.16f,.25f,.3f)); var line=MapMaterial("Grid",new Color(.22f,.34f,.38f)); var edge=MapMaterial("Boundary",new Color(.12f,.19f,.25f));
                Action<string,Vector3,Vector3,Material,bool> box=(name,position,scale,mat,collider)=>
                {
                    var go=GameObject.CreatePrimitive(PrimitiveType.Cube); go.name=name; go.transform.position=position; go.transform.localScale=scale;
                    go.GetComponent<Renderer>().sharedMaterial=mat; if(!collider) UnityEngine.Object.DestroyImmediate(go.GetComponent<Collider>());
                    if(name=="Walkable floor") go.layer=8;
                };
                box("Walkable floor",new Vector3(0,-.15f,0),new Vector3(18,.3f,18),ground,true);
                for(int i=-9;i<=9;i++)
                {
                    box("Grid X "+i,new Vector3(i,.001f,0),new Vector3(.012f,.002f,18),line,false);
                    box("Grid Z "+i,new Vector3(0,.001f,i),new Vector3(18,.002f,.012f),line,false);
                }
                box("North wall",new Vector3(0,.35f,9),new Vector3(18,.7f,.2f),edge,true);
                box("South wall",new Vector3(0,.35f,-9),new Vector3(18,.7f,.2f),edge,true);
                box("East wall",new Vector3(9,.35f,0),new Vector3(.2f,.7f,18),edge,true);
                box("West wall",new Vector3(-9,.35f,0),new Vector3(.2f,.7f,18),edge,true);
                box("Obstacle",new Vector3(2,.4f,2),new Vector3(1.2f,.8f,1.2f),edge,true);
                var robo=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath),scene);
                robo.transform.position=new Vector3(0,.02f,0); robo.transform.rotation=Quaternion.Euler(0,155,0);
                var camera=new GameObject("Map Camera").AddComponent<Camera>(); camera.tag="MainCamera";
                camera.clearFlags=CameraClearFlags.SolidColor; camera.backgroundColor=new Color(.08f,.12f,.18f); camera.fieldOfView=40; camera.nearClipPlane=.05f; camera.farClipPlane=100;
                camera.gameObject.AddComponent<AudioListener>(); var follow=camera.gameObject.AddComponent<CodexRoboCamera>(); follow.target=robo.transform;
                camera.transform.position=robo.transform.position+follow.offset; camera.transform.LookAt(robo.transform.position+Vector3.up*.4f);
                robo.GetComponent<CodexRoboMotor>().viewCamera=camera;
                var light=new GameObject("Sun").AddComponent<Light>(); light.type=LightType.Directional; light.intensity=1.3f; light.shadows=LightShadows.Soft; light.transform.rotation=Quaternion.Euler(45,-30,0);
                var hud=new GameObject("Map controls").AddComponent<CodexRoboMapHud>(); hud.robo=robo.GetComponent<CodexRoboMotor>();
                EditorSceneManager.SaveScene(scene,ScenePath);
            }
            finally { SceneManager.SetActiveScene(previous); EditorSceneManager.CloseScene(scene,true); }
        }
    }
}
