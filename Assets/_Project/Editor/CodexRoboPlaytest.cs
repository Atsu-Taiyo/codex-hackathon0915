using System;
using System.Collections;
using System.IO;
using Hackathon.Map;
using UnityEngine;

namespace Hackathon.Editor
{
    public static class CodexRoboPlaytest
    {
        static int checks;
        static void Check(bool ok,string message)
        {
            if(!ok) {File.WriteAllText("Temp/CodexRobo-playtest.txt","FAIL: "+message); throw new Exception(message);}
            checks++;
        }
        public static IEnumerator Run()
        {
            checks=0;
            var robo=UnityEngine.Object.FindAnyObjectByType<CodexRoboMotor>();
            Check(robo!=null,"Walking controller exists");
            robo.acceptInput=false;
            var controller=robo.GetComponent<CharacterController>();
            var anim=robo.GetComponent<Animation>();
            var skin=robo.GetComponentInChildren<SkinnedMeshRenderer>();
            var camera=robo.viewCamera;
            var follow=camera.GetComponent<CodexRoboCamera>();
            var originalCamera=camera.transform.position; var originalRotation=camera.transform.rotation;
            var baked=new Mesh();
            try
            {
                Check(skin.bones.Length==6 && skin.sharedMesh.bindposes.Length==6,"Six bones and bind poses");
                foreach(var weight in skin.sharedMesh.boneWeights)
                    if(Mathf.Abs(weight.weight0+weight.weight1+weight.weight2+weight.weight3-1)>.00001f) throw new Exception("Invalid weight sum");
                checks++;
                Check(anim["Idle"]!=null && anim["Walk"]!=null,"Saved animation clips available");
                Warp(robo,Vector3.zero); yield return new WaitForSeconds(.35f);
                Check(Mathf.Abs(robo.transform.position.y)<.06f,"Standing on floor");
                Check(Vector3.Distance(skin.bones[4].position,skin.bones[5].position)>.18f,"Idle preserves hip separation");
                var start=robo.transform.position;
                robo.WalkTo(new Vector3(0,0,2));
                yield return new WaitForSeconds(.5f);
                Check(robo.IsWalking && robo.transform.position.z > start.z+.3f,"Actual movement and walk state");
                Check(Quaternion.Angle(skin.bones[2].localRotation,skin.bones[3].localRotation)>2,"Arms swing independently");
                Check(Quaternion.Angle(skin.bones[4].localRotation,skin.bones[5].localRotation)>2,"Legs swing independently");
                yield return new WaitForSeconds(1.5f);
                Check(!robo.HasDestination && Vector3.Distance(robo.transform.position,new Vector3(0,0,2))<.12f,"Click destination arrival stops without overshoot");
                yield return new WaitForSeconds(.25f);
                Check(!robo.IsWalking && anim["Idle"].weight>.9f,"Returns to idle");
                Warp(robo,new Vector3(0,0,-2)); robo.Steer(Vector2.up);
                yield return new WaitForSeconds(.4f); float cardinal=new Vector2(robo.Velocity.x,robo.Velocity.z).magnitude;
                robo.Steer(Vector2.one); yield return new WaitForSeconds(.4f);
                float diagonal=new Vector2(robo.Velocity.x,robo.Velocity.z).magnitude;
                Check(Mathf.Abs(cardinal-diagonal)<.05f,"Diagonal movement speed normalized");
                Check(Vector3.Dot(robo.transform.forward,new Vector3(robo.Velocity.x,0,robo.Velocity.z).normalized)>.9f,"Faces travel direction");
                robo.StopWalking(); Warp(robo,new Vector3(7.8f,0,0)); robo.WalkTo(new Vector3(12,0,0));
                yield return new WaitForSeconds(1.7f);
                Check(robo.transform.position.x<8.75f && robo.transform.position.y>-.06f,"Boundary collision prevents falling off map");
                robo.StopWalking(); Warp(robo,new Vector3(0,0,2)); robo.WalkTo(new Vector3(4,0,2));
                yield return new WaitForSeconds(1.5f);
                Check(robo.transform.position.x<1.25f,"Obstacle collision blocks movement");
                robo.StopWalking(); Warp(robo,Vector3.zero); yield return new WaitForSeconds(.2f);
                // Inspect the baked mesh over the entire cycle, including both maximum strides.
                robo.enabled=false; follow.enabled=false;
                robo.transform.rotation=Quaternion.identity;
                camera.transform.position=new Vector3(1.15f,1.05f,2.1f); camera.transform.LookAt(new Vector3(0,.48f,0));
                float minimumY=float.PositiveInfinity; float maximumHeight=0;
                anim.Play("Walk"); anim["Walk"].speed=0;
                for(int frame=0;frame<=16;frame++)
                {
                    anim["Walk"].time=anim["Walk"].length*frame/16; anim.Sample(); skin.BakeMesh(baked);
                    Check(Vector3.Distance(skin.bones[4].position,skin.bones[5].position)>.18f,"Walk preserves hip separation at frame "+frame);
                    foreach(var p in baked.vertices) {minimumY=Mathf.Min(minimumY,p.y); maximumHeight=Mathf.Max(maximumHeight,p.y);}
                    if(frame==0 || frame==4 || frame==12)
                    { yield return null; Capture(camera,"Temp/CodexRobo-walk-"+frame+".png"); }
                }
                Check(minimumY>-.04f && maximumHeight<1.08f,"Skinning bounds and foot penetration: min="+minimumY+" max="+maximumHeight);
                anim.Play("Idle"); anim["Idle"].time=0; anim["Idle"].speed=0; anim.Sample();
                yield return null;
                Capture(camera,"Temp/CodexRobo-idle.png");
                File.WriteAllText("Temp/CodexRobo-playtest.txt","PASS: "+checks+" checks\nGrounded movement, destination stop, independent arm/leg animation, idle blend, normalized diagonals, facing, wall/obstacle collision.\nBaked mesh cycle minY="+minimumY+" maxY="+maximumHeight+"\nSource vertices="+skin.sharedMesh.vertexCount+"\n");
                Debug.Log("[CodexRobo] PASS: "+checks+" runtime checks.");
            }
            finally
            {
                UnityEngine.Object.Destroy(baked);
                robo.StopWalking(); Warp(robo,Vector3.zero); robo.transform.rotation=Quaternion.Euler(0,155,0);
                robo.enabled=true; robo.acceptInput=true; anim["Walk"].speed=1; anim["Idle"].speed=1; anim.Play("Idle");
                follow.enabled=true; camera.transform.position=originalCamera; camera.transform.rotation=originalRotation;
            }
        }
        static void Warp(CodexRoboMotor robo,Vector3 position)
        {
            var c=robo.GetComponent<CharacterController>(); c.enabled=false; robo.transform.position=position+Vector3.up*.02f; c.enabled=true; Physics.SyncTransforms();
        }
        public static void Capture(Camera camera,string path)
        {
            var previous=camera.targetTexture; var active=RenderTexture.active;
            var target=RenderTexture.GetTemporary(1280,960,24); var image=new Texture2D(1280,960,TextureFormat.RGB24,false);
            try {camera.targetTexture=target; camera.Render(); RenderTexture.active=target; image.ReadPixels(new Rect(0,0,1280,960),0,0); image.Apply(); File.WriteAllBytes(path,image.EncodeToPNG());}
            finally {camera.targetTexture=previous; RenderTexture.active=active; RenderTexture.ReleaseTemporary(target); UnityEngine.Object.DestroyImmediate(image);}
        }
    }
}
