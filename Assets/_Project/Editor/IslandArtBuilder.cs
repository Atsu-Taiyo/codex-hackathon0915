using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Hackathon.Editor
{
    /// <summary>Shared, saved meshes and materials for the map and rendered Room 1 workshop.</summary>
    public static class IslandArtBuilder
    {
        public const string Root = "Assets/_Project/Content/Features/Map/IslandArt";
        const string Terrain = "Assets/_Project/Content/Common/TerrainKit/Prefabs/";
        public static Color Color(string hex) { ColorUtility.TryParseHtmlString("#" + hex, out var c); return c; }
        public static Material Mat(string name, string hex, bool glow = false)
        {
            string path = Root + "/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material) return material;
            material = new Material(Shader.Find("Standard")) { name = name, color = Color(hex) };
            material.SetFloat("_Glossiness", .16f);
            if (glow) { material.EnableKeyword("_EMISSION"); material.SetColor("_EmissionColor", Color(hex) * .65f); }
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        public static Transform Group(string name, Transform parent)
        {
            var t = new GameObject(name).transform; t.SetParent(parent, false); return t;
        }

        public static GameObject Shape(string name, Transform parent, Vector3 position, Vector3 scale, Material mat,
            PrimitiveType type = PrimitiveType.Cube, bool solid = false)
        {
            var go = GameObject.CreatePrimitive(type); go.name = name;
            go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = mat;
            if (!solid) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }

        public static GameObject Part(string name, Transform parent, Vector3 position, bool walkable = false, float yaw = 0)
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Terrain + name + ".prefab");
            if (!prefab) throw new InvalidOperationException("Missing PR #2 terrain: " + name);
            var go = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            go.transform.localPosition = position; go.transform.localRotation = Quaternion.Euler(0, yaw, 0);
            foreach (var t in go.GetComponentsInChildren<Transform>()) t.gameObject.layer = walkable ? 8 : 0;
            if (!walkable) foreach (var c in go.GetComponentsInChildren<Collider>()) Object.DestroyImmediate(c);
            return go;
        }

        // Flat faces make rounded, low-poly silhouettes without per-instance meshes.
        public static Mesh Pebble()
        {
            string path = Root + "/FacetedPebble.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path); if (mesh) return mesh;
            var vertices = new List<Vector3>(); var triangles = new List<int>();
            Vector3[] rings = new Vector3[4 * 8];
            float[] heights = { -.5f, -.27f, .22f, .5f }, radii = { .18f, .48f, .43f, .15f };
            for (int ring = 0; ring < 4; ring++) for (int i = 0; i < 8; i++)
            {
                float a = i * Mathf.PI / 4 + ring * .12f;
                rings[ring * 8 + i] = new Vector3(Mathf.Cos(a) * radii[ring], heights[ring], Mathf.Sin(a) * radii[ring]);
            }
            void Tri(Vector3 a, Vector3 b, Vector3 c) { int n = vertices.Count; vertices.Add(a); vertices.Add(b); vertices.Add(c); triangles.Add(n); triangles.Add(n + 1); triangles.Add(n + 2); }
            for (int ring = 0; ring < 3; ring++) for (int i = 0; i < 8; i++)
            {
                int a = ring * 8 + i, b = ring * 8 + (i + 1) % 8;
                Tri(rings[a], rings[a + 8], rings[b]); Tri(rings[b], rings[a + 8], rings[b + 8]);
            }
            for (int i = 0; i < 8; i++) { Tri(Vector3.down * .5f, rings[i], rings[(i + 1) % 8]); Tri(Vector3.up * .5f, rings[24 + (i + 1) % 8], rings[24 + i]); }
            mesh = new Mesh { name = "Faceted pebble" }; mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh, path); return mesh;
        }

        public static GameObject Rock(string name, Transform parent, Vector3 position, Vector3 scale, Material mat)
        {
            var go = new GameObject(name); go.transform.SetParent(parent, false); go.transform.localPosition = position; go.transform.localScale = scale;
            go.AddComponent<MeshFilter>().sharedMesh = Pebble(); go.AddComponent<MeshRenderer>().sharedMaterial = mat; return go;
        }

        public static void Tree(Transform parent, Vector3 position, float size, int variant)
        {
            var tree = Group("Coastal tree", parent); tree.localPosition = position; tree.localScale = Vector3.one * size;
            var bark = Mat("Bark", "866549"); var leaves = Mat(variant % 2 == 0 ? "Pine" : "Sage", variant % 2 == 0 ? "4B8065" : "7DA86C");
            Shape("Trunk", tree, new Vector3(0, .75f, 0), new Vector3(.22f, .75f, .22f), bark, PrimitiveType.Cylinder, true);
            for (int i = 0; i < 4; i++)
            {
                float angle = i * 2.4f + variant;
                Rock("Faceted foliage", tree, new Vector3(Mathf.Cos(angle) * .36f, 1.4f + i * .28f, Mathf.Sin(angle) * .28f), new Vector3(1.3f, 1.25f, 1.2f), leaves);
            }
            Rock("Crown", tree, new Vector3(0, 2.35f, 0), new Vector3(1, 1.25f, 1), Mat("Leaf tips", "A9C77D"));
        }

        public static void Crate(Transform parent, Vector3 p, float size)
        {
            var root = Group("Joinery crate", parent); root.localPosition = p; root.localScale = Vector3.one * size;
            var wood = Mat("Warm oak", "B98B58"); var edge = Mat("Oak edges", "DEC18C");
            Shape("Box", root, new Vector3(0, .5f, 0), Vector3.one, wood);
            for (int i = 0; i < 2; i++)
            {
                float x = i == 0 ? -.41f : .41f;
                Shape("Front brace", root, new Vector3(x, .5f, -.51f), new Vector3(.1f, 1, .035f), edge);
                Shape("Top brace", root, new Vector3(x, 1.02f, 0), new Vector3(.1f, .04f, 1), edge);
            }
            var brace = Shape("Diagonal brace", root, new Vector3(0, .5f, -.535f), new Vector3(.1f, 1.15f, .04f), edge);
            brace.transform.localRotation = Quaternion.Euler(0, 0, -39);
        }

        public static void Pot(Transform parent, Vector3 position, float size)
        {
            var t = Group("Workshop planter", parent); t.localPosition = position; t.localScale = Vector3.one * size;
            Shape("Terracotta pot", t, new Vector3(0, .23f, 0), new Vector3(.5f, .23f, .5f), Mat("Clay", "BC7759"), PrimitiveType.Cylinder);
            Rock("Herb foliage", t, new Vector3(0, .65f, 0), new Vector3(.75f, .8f, .7f), Mat("Sage", "7DA86C"));
        }

        public static void Lantern(Transform parent, Vector3 position)
        {
            var t = Group("Brass lantern", parent); t.localPosition = position;
            var dark = Mat("Iron", "3E5754");
            Shape("Foot", t, new Vector3(0, .06f, 0), new Vector3(.29f, .12f, .29f), dark);
            Shape("Warm glass", t, new Vector3(0, .3f, 0), new Vector3(.19f, .35f, .19f), Mat("Lantern glow", "FFE0A0", true));
            Shape("Cap", t, new Vector3(0, .51f, 0), new Vector3(.3f, .09f, .3f), dark);
            for (int x = -1; x <= 1; x += 2) for (int z = -1; z <= 1; z += 2)
                Shape("Iron frame", t, new Vector3(x * .12f, .3f, z * .12f), new Vector3(.028f, .4f, .028f), dark);
        }

        public static void Workshop(Transform parent, bool interior)
        {
            var oak = Mat("Oak edges", "DEC18C"); var timber = Mat("Bark", "866549"); var teal = Mat("Workshop teal", "426F67");
            // Origin is the entrance; the 4 x 4 metre footprint matches the PR flooring.
            if (!interior)
            {
                Shape("Door lintel", parent, new Vector3(0, 2.55f, 0), new Vector3(4.5f, .18f, .24f), timber);
                for (int side = -1; side <= 1; side += 2)
                    Shape("Structural oak post", parent, new Vector3(side * 2.05f, 1.3f, 0), new Vector3(.16f, 2.6f, .16f), timber, PrimitiveType.Cube, true);
            }
            Shape("Back wall beam", parent, new Vector3(0, 2.55f, 3.7f), new Vector3(4.35f, .18f, .22f), timber);
            Shape("Window frame", parent, new Vector3(-.9f, 1.5f, 3.54f), new Vector3(1.35f, 1.3f, .16f), oak);
            Shape("Sea glass", parent, new Vector3(-.9f, 1.5f, 3.43f), new Vector3(1.15f, 1.08f, .055f), Mat("Window glass", "A6D7D0", true));
            Shape("Window mullion", parent, new Vector3(-.9f, 1.5f, 3.38f), new Vector3(.07f, 1.1f, .07f), oak);
            Shape("Window transom", parent, new Vector3(-.9f, 1.5f, 3.38f), new Vector3(1.2f, .07f, .07f), oak);
            Shape("Work bench", parent, new Vector3(.9f, .91f, 2.7f), new Vector3(1.7f, .14f, .72f), oak);
            for (int side = -1; side <= 1; side += 2)
                Shape("Bench legs", parent, new Vector3(.9f + side * .66f, .43f, 2.7f), new Vector3(.13f, .85f, .52f), teal);
            Shape("Tool board", parent, new Vector3(.92f, 1.87f, 3.52f), new Vector3(1.55f, .75f, .09f), teal);
            for (int i = 0; i < 4; i++)
            {
                Shape("Hanging tool", parent, new Vector3(.43f + i * .32f, 1.84f, 3.43f), new Vector3(.055f, .4f, .055f), oak);
                Shape("Tool head", parent, new Vector3(.43f + i * .32f, 1.99f, 3.4f), new Vector3(.20f, .07f, .08f), Mat("Iron", "3E5754"));
            }
            Lantern(parent, new Vector3(1.45f, .99f, 2.65f));
            Crate(parent, new Vector3(-1.4f, .05f, 2.85f), .55f);
            Pot(parent, new Vector3(-1.75f, .03f, .7f), .72f);
            if (!interior)
            {
                // A narrow tiled canopy leaves the workshop interior visible from the map camera.
                for (int i = 0; i < 12; i++)
                {
                    var tile = Shape("Glazed canopy tile", parent, new Vector3(-2.05f + i * .37f, 2.63f, .24f), new Vector3(.35f, .1f, .98f), i % 3 == 0 ? Mat("Canopy light", "73A49A") : teal);
                    tile.transform.localRotation = Quaternion.Euler(-12, 0, 0);
                }
                Shape("Workshop plaque", parent, new Vector3(0, 2.1f, -.15f), new Vector3(1.05f, .4f, .08f), teal);
                var label = new GameObject("Workshop number").AddComponent<TextMesh>();
                label.transform.SetParent(parent, false); label.transform.localPosition = new Vector3(0, 2.1f, -.2f);
                label.anchor = TextAnchor.MiddleCenter; label.characterSize = .14f; label.fontSize = 48; label.text = "01"; label.color = Color("FFF1CB");
                Pot(parent, new Vector3(2.4f, .05f, .2f), .8f);
                Lantern(parent, new Vector3(-2.4f, .06f, -.25f));
            }
        }
    }
}
