#if UNITY_EDITOR
using GeoImport.EditorUtil;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace GeoImport.Editor
{
    public class GeoJSONImporterWindow : EditorWindow
    {
        [MenuItem("Tools/Geo Import/GeoJSON Importer")]
        public static void ShowWindow()
        {
            GetWindow<GeoJSONImporterWindow>("GeoJSON Importer");
        }


        /// <summary>
        /// The GeoJSON file to import. Should be assigned as a Unity TextAsset containing valid GeoJSON.
        /// </summary>
        public TextAsset geojson;

        /// <summary>
        /// The latitude of the origin point for projecting geographic coordinates to Unity world space.
        /// Typically set to the center of the bounding box of the imported data.
        /// </summary>
        public double originLat = 52.2210; // set to your bbox center

        /// <summary>
        /// The longitude of the origin point for projecting geographic coordinates to Unity world space.
        /// Typically set to the center of the bounding box of the imported data.
        /// </summary>
        public double originLon = 6.8910;

        /// <summary>
        /// The scale factor to convert meters to Unity units. 
        /// Adjust this value to match the desired scale in your Unity scene.
        /// </summary>
        public float metersToUnity = 1f;

        /// <summary>
        /// The output folder path where imported assets (meshes, prefabs, materials) will be saved.
        /// </summary>
        public string outputFolder = "Assets/GeoImported";

        /// <summary>
        /// The default road width in meters for highway types not explicitly specified.
        /// </summary>
        public float defaultRoadWidth = 6f;

        /// <summary>
        /// The road width in meters for 'primary' highway type.
        /// </summary>
        public float primaryWidth = 10f;

        /// <summary>
        /// The road width in meters for 'secondary' highway type.
        /// </summary>
        public float secondaryWidth = 8f;

        /// <summary>
        /// The road width in meters for 'residential' highway type.
        /// </summary>
        public float residentialWidth = 6f;

        /// <summary>
        /// The road width in meters for 'footway' highway type.
        /// </summary>
        public float footwayWidth = 2.5f;

        /// <summary>
        /// The road width in meters for 'path' highway type.
        /// </summary>
        public float pathWidth = 2f;

        /// <summary>
        /// The road width in meters for 'service' highway type.
        /// </summary>
        public float serviceWidth = 4f;


        /// <summary>
        /// The material used for rendering imported road meshes.
        /// </summary>
        Material roadMat;

        /// <summary>
        /// The material used for rendering imported building meshes.
        /// </summary>
        Material buildingMat;

        /// <summary>
        /// The material used for rendering imported landuse meshes.
        /// </summary>
        Material landuseMat;


        void OnGUI()
        {
            EditorGUILayout.HelpBox("Requires package: com.unity.nuget.newtonsoft-json", MessageType.Info);
            geojson = (TextAsset)EditorGUILayout.ObjectField("GeoJSON", geojson, typeof(TextAsset), false);
            originLat = EditorGUILayout.DoubleField("Origin Lat", originLat);
            originLon = EditorGUILayout.DoubleField("Origin Lon", originLon);
            metersToUnity = EditorGUILayout.FloatField("Meters → Unity", metersToUnity);
            outputFolder = EditorGUILayout.TextField("Output Folder", outputFolder);


            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Road Widths (meters)", EditorStyles.boldLabel);
            primaryWidth = EditorGUILayout.FloatField("primary", primaryWidth);
            secondaryWidth = EditorGUILayout.FloatField("secondary", secondaryWidth);
            residentialWidth = EditorGUILayout.FloatField("residential", residentialWidth);
            serviceWidth = EditorGUILayout.FloatField("service", serviceWidth);
            footwayWidth = EditorGUILayout.FloatField("footway", footwayWidth);
            pathWidth = EditorGUILayout.FloatField("path", pathWidth);
            defaultRoadWidth = EditorGUILayout.FloatField("default", defaultRoadWidth);


            if (GUILayout.Button("Import → Prefabs"))
            {
                if (!geojson) { EditorUtility.DisplayDialog("Error", "Assign a GeoJSON TextAsset", "OK"); return; }
                Import();
            }
        }

        /// <summary>
        /// Ensures that the output folder and its Materials subfolder exist in the Unity project.
        /// Loads or creates the required materials for roads, buildings, and landuse features.
        /// </summary>
        void EnsureMaterials()
        {
            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                var parent = Path.GetDirectoryName(outputFolder);
                var leaf = Path.GetFileName(outputFolder);
                if (!AssetDatabase.IsValidFolder(parent)) AssetDatabase.CreateFolder(Path.GetDirectoryName(parent), Path.GetFileName(parent));
                AssetDatabase.CreateFolder(parent, leaf);
            }
            string matFolder = outputFolder + "/Materials";
            if (!AssetDatabase.IsValidFolder(matFolder)) AssetDatabase.CreateFolder(outputFolder, "Materials");
            roadMat = LoadOrCreateMat(matFolder + "/Road.mat", new Color(0.15f, 0.15f, 0.15f, 1));
            buildingMat = LoadOrCreateMat(matFolder + "/Building.mat", new Color(0.85f, 0.85f, 0.85f, 1));
            landuseMat = LoadOrCreateMat(matFolder + "/Landuse.mat", new Color(0.75f, 0.9f, 0.75f, 1));
        }

        /// <summary>
        /// Imports GeoJSON features and creates Unity prefabs for roads, buildings, and landuse areas.
        /// 
        /// This method performs the following steps:
        /// 1. Ensures required materials and output folders exist.
        /// 2. Parses the GeoJSON text asset and iterates through its features.
        /// 3. For each feature:
        ///    - If it is a LineString with a highway property, creates a road mesh and prefab.
        ///    - If it is a Polygon or MultiPolygon with a building or landuse property, creates an area mesh and prefab.
        /// 4. Parents all created GameObjects under a single root GameObject.
        /// 5. Saves the root GameObject as a prefab for convenience.
        /// 6. Outputs a summary of imported feature counts to the Unity console.
        /// </summary>
        void Import()
        {
            EnsureMaterials();
            string json = geojson.text;
            var root = JObject.Parse(json);
            var features = (JArray)root["features"];
            if (features == null) { Debug.LogError("No features found"); return; }

            string meshFolder = outputFolder + "/Meshes";
            if (!AssetDatabase.IsValidFolder(meshFolder)) AssetDatabase.CreateFolder(outputFolder, "Meshes");
            string prefabFolder = outputFolder + "/Prefabs";
            if (!AssetDatabase.IsValidFolder(prefabFolder)) AssetDatabase.CreateFolder(outputFolder, "Prefabs");

            var parentGO = new GameObject("GeoImported_Scene");

            int roadCount = 0, buildingCount = 0, landuseCount = 0;

            foreach (var f in features)
            {
                var geom = f["geometry"] as JObject; if (geom == null) continue;
                string gtype = (string)geom["type"];
                var props = f["properties"] as JObject ?? new JObject();
                string highway = (string)props["highway"];
                string building = (string)props["building"];
                string landuse = (string)props["landuse"];

                if (gtype == "LineString" && !string.IsNullOrEmpty(highway))
                {
                    var pts = ReadLineString(geom);
                    MakeRoad(pts, highway, prefabFolder, meshFolder, parentGO.transform, ref roadCount);
                }
                else if (gtype == "Polygon")
                {
                    var rings = ReadPolygon(geom);
                    if (!string.IsNullOrEmpty(building)) MakeArea(rings, buildingMat, "Building_", prefabFolder, meshFolder, parentGO.transform, ref buildingCount);
                    else if (!string.IsNullOrEmpty(landuse)) MakeArea(rings, landuseMat, "Landuse_", prefabFolder, meshFolder, parentGO.transform, ref landuseCount);
                }
                else if (gtype == "MultiPolygon")
                {
                    var polys = ReadMultiPolygon(geom);
                    foreach (var rings in polys)
                    {
                        if (!string.IsNullOrEmpty(building)) MakeArea(rings, buildingMat, "Building_", prefabFolder, meshFolder, parentGO.transform, ref buildingCount);
                        else if (!string.IsNullOrEmpty(landuse)) MakeArea(rings, landuseMat, "Landuse_", prefabFolder, meshFolder, parentGO.transform, ref landuseCount);
                    }
                }
            }

            // Save the parent as a prefab for convenience
            string parentPath = prefabFolder + "/GeoImported_Scene.prefab";
            PrefabUtility.SaveAsPrefabAsset(parentGO, parentPath);
            DestroyImmediate(parentGO);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"Imported: Roads {roadCount}, Buildings {buildingCount}, Landuse {landuseCount}");
        }

        /// <summary>
        /// Loads a material asset from the specified path if it exists; otherwise, creates a new material
        /// using the Universal Render Pipeline Unlit shader, sets its color, and saves it as an asset at the given path.
        /// </summary>
        /// <param name="path">The asset path where the material is loaded from or created.</param>
        /// <param name="c">The color to assign to the material if it is created.</param>
        /// <returns>The loaded or newly created Material asset.</returns>
        Material LoadOrCreateMat(string path, Color c)
        {
            var mat = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (!mat)
            {
                mat = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
                mat.color = c;
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        /// <summary>
        /// Reads a GeoJSON LineString geometry and converts it to a list of 2D points in Unity meters.
        /// Each point is projected from geographic coordinates (longitude, latitude) to Unity world space using the specified origin and scale.
        /// </summary>
        /// <param name="geom">The GeoJSON geometry object containing LineString coordinates.</param>
        /// <returns>
        /// A list of Vector2 points representing the LineString in Unity meters.
        /// </returns>
        List<Vector2> ReadLineString(JObject geom)
        {
            var arr = (JArray)geom["coordinates"]; // [[lon,lat], ...]
            var ptsLonLat = new List<Vector2>(arr.Count);
            foreach (var c in arr)
            {
                float lon = (float)c[0];
                float lat = (float)c[1];
                ptsLonLat.Add(new Vector2(lon, lat));
            }
            var meters = GeoProjection.LatLonArrayToMeters(ptsLonLat, originLat, originLon);
            var list = new List<Vector2>(meters.Length);
            foreach (var m in meters) list.Add(m * metersToUnity);
            return list;
        }

        /// <summary>
        /// Reads a GeoJSON Polygon geometry and converts it to a list of rings, 
        /// where each ring is a list of 2D points in Unity meters.
        /// Each ring is projected from geographic coordinates (longitude, latitude) to Unity world space using the specified origin and scale.
        /// </summary>
        /// <param name="geom">The GeoJSON geometry object containing Polygon coordinates.</param>
        /// <returns>
        /// A list of rings, where each ring is a list of Vector2 points in Unity meters.
        /// The first ring is the outer boundary, subsequent rings are holes (if any).
        /// </returns>
        List<List<Vector2>> ReadPolygon(JObject geom)
        {
            // coordinates: [ [ [lon,lat], ... outer ... ], [ ... hole1 ... ], ... ]
            var ringsOut = new List<List<Vector2>>();
            var coords = (JArray)geom["coordinates"];
            if (coords == null) return ringsOut;
            for (int r = 0; r < coords.Count; r++)
            {
                var ring = (JArray)coords[r];
                var ptsLonLat = new List<Vector2>(ring.Count);
                foreach (var c in ring)
                {
                    float lon = (float)c[0];
                    float lat = (float)c[1];
                    ptsLonLat.Add(new Vector2(lon, lat));
                }
                var meters = GeoProjection.LatLonArrayToMeters(ptsLonLat, originLat, originLon);
                var list = new List<Vector2>(meters.Length);
                foreach (var m in meters) list.Add(m * metersToUnity);
                ringsOut.Add(list);
            }
            return ringsOut;
        }

        /// <summary>
        /// Reads a GeoJSON MultiPolygon geometry and converts it to a list of polygons, 
        /// where each polygon is represented as a list of rings, and each ring is a list of 2D points in Unity meters.
        /// Each ring is projected from geographic coordinates (longitude, latitude) to Unity world space using the specified origin and scale.
        /// </summary>
        /// <param name="geom">The GeoJSON geometry object containing MultiPolygon coordinates.</param>
        /// <returns>
        /// A list of polygons, where each polygon is a list of rings, and each ring is a list of Vector2 points in Unity meters.
        /// </returns>
        List<List<List<Vector2>>> ReadMultiPolygon(JObject geom)
        {
            var polys = new List<List<List<Vector2>>>();
            var coords = (JArray)geom["coordinates"]; // [ [ [ [lon,lat],... ] (ring), ... ] (poly), ... ]
            if (coords == null) return polys;
            foreach (var poly in coords)
            {
                var polyRings = new List<List<Vector2>>();
                foreach (var ring in (JArray)poly)
                {
                    var ptsLonLat = new List<Vector2>();
                    foreach (var c in (JArray)ring)
                    {
                        float lon = (float)c[0];
                        float lat = (float)c[1];
                        ptsLonLat.Add(new Vector2(lon, lat));
                    }
                    var meters = GeoProjection.LatLonArrayToMeters(ptsLonLat, originLat, originLon);
                    var list = new List<Vector2>(meters.Length);
                    foreach (var m in meters) list.Add(m * metersToUnity);
                    polyRings.Add(list);
                }
                polys.Add(polyRings);
            }
            return polys;
        }

        /// <summary>
        /// Returns the road width in meters for a given highway type.
        /// Uses preset width values for common highway types, otherwise returns the default road width.
        /// </summary>
        /// <param name="type">Highway type string (e.g., "primary", "residential", "service", etc.).</param>
        /// <returns>Width in meters for the specified highway type.</returns>
        float WidthForHighway(string type)
        {
            switch (type)
            {
                case "motorway":
                case "trunk":
                case "primary": return primaryWidth;
                case "secondary": return secondaryWidth;
                case "residential": return residentialWidth;
                case "service": return serviceWidth;
                case "footway": return footwayWidth;
                case "path": return pathWidth;
                default: return defaultRoadWidth;
            }
        }

        /// <summary>
        /// Creates a road mesh from a polyline, assigns the road material, saves the mesh and prefab assets,
        /// parents the resulting GameObject, and increments the road counter.
        /// </summary>
        /// <param name="pts">List of 2D points representing the road centerline in Unity meters.</param>
        /// <param name="highwayType">Highway type string (e.g., "primary", "residential") used for width and naming.</param>
        /// <param name="prefabFolder">Folder path to save the prefab asset.</param>
        /// <param name="meshFolder">Folder path to save the mesh asset.</param>
        /// <param name="parent">Transform to parent the created GameObject under.</param>
        /// <param name="counter">Reference to an integer counter for unique asset naming; incremented after creation.</param>
        void MakeRoad(List<Vector2> pts, string highwayType, string prefabFolder, string meshFolder, Transform parent, ref int counter)
        {
            if (pts.Count < 2) return;
            float w = WidthForHighway(highwayType);
            var mesh = PolylineMeshBuilder.Build(pts, w);
            string meshPath = $"{meshFolder}/Road_{highwayType}_{counter}.asset";
            AssetDatabase.CreateAsset(mesh, meshPath);


            var go = new GameObject($"Road_{highwayType}_{counter}");
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mf.sharedMesh = mesh; mr.sharedMaterial = roadMat;
            go.transform.SetParent(parent, false);


            string prefabPath = $"{prefabFolder}/Road_{highwayType}_{counter}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            counter++;
        }

        /// <summary>
        /// Creates a mesh from the outer ring of a polygon (ignoring holes), assigns the specified material,
        /// saves the mesh and prefab assets, and parents the resulting GameObject.
        /// </summary>
        /// <param name="rings">List of rings, where each ring is a list of 2D points (outer ring at index 0).</param>
        /// <param name="mat">Material to assign to the generated mesh.</param>
        /// <param name="prefix">Prefix for naming the mesh and prefab assets.</param>
        /// <param name="prefabFolder">Folder path to save the prefab asset.</param>
        /// <param name="meshFolder">Folder path to save the mesh asset.</param>
        /// <param name="parent">Transform to parent the created GameObject under.</param>
        /// <param name="counter">Reference to an integer counter for unique asset naming; incremented after creation.</param>
        void MakeArea(List<List<Vector2>> rings, Material mat, string prefix, string prefabFolder, string meshFolder, Transform parent, ref int counter)
        {
            if (rings.Count == 0 || rings[0].Count < 3) return;
            // Outer ring only (index 0). Holes ignored for brevity.
            var outer = rings[0];
            var indices = new List<int>();
            PolygonTriangulator.Triangulate(outer, indices);
            var verts3 = new List<Vector3>(outer.Count);
            foreach (var p in outer) verts3.Add(new Vector3(p.x, p.y, 0));
            var mesh = new Mesh();
            mesh.SetVertices(verts3);
            mesh.SetTriangles(indices, 0);
            mesh.RecalculateBounds(); mesh.RecalculateNormals();


            string meshPath = $"{meshFolder}/{prefix}{counter}.asset";
            AssetDatabase.CreateAsset(mesh, meshPath);


            var go = new GameObject($"{prefix}{counter}");
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mf.sharedMesh = mesh; mr.sharedMaterial = mat;
            go.transform.SetParent(parent, false);


            string prefabPath = $"{prefabFolder}/{prefix}{counter}.prefab";
            PrefabUtility.SaveAsPrefabAsset(go, prefabPath);
            Object.DestroyImmediate(go);
            counter++;
        }
    }
}
#endif