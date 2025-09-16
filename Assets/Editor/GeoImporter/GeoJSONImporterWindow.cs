#if UNITY_EDITOR
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


namespace GeoImport.Editor
{
    using EditorUtil;
    using LibTessDotNet;
    using System;

    /// <summary>
    /// Unity Editor window for importing GeoJSON files and converting their features into Unity prefabs.
    /// 
    /// This tool allows you to:
    /// - Assign a GeoJSON file (as a Unity TextAsset).
    /// - Set the geographic origin and scale for projection to Unity world space.
    /// - Configure output folders for generated assets.
    /// - Specify road widths for different highway types.
    /// - Import roads, buildings, and landuse features as prefabs with appropriate materials.
    /// 
    /// Usage:
    /// 1. Open via Tools &gt; Geo Import &gt; GeoJSON Importer.
    /// 2. Assign a GeoJSON TextAsset.
    /// 3. Adjust import settings as needed.
    /// 4. Click "Import → Prefabs" to generate prefabs in the specified output folder.
    /// </summary>
    public class GeoJSONImporterWindow : EditorWindow
    {
        /// <summary>
        /// Shows the GeoJSON Importer window in the Unity Editor.
        /// </summary>
        [MenuItem("Tools/Geo Import/GeoJSON Importer")]
        public static void ShowWindow()
        {
            GetWindow<GeoJSONImporterWindow>("GeoJSON Importer");
        }

        public int batchSize = 1000;

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

        /// <summary>
        /// The material used for rendering imported water meshes.
        /// </summary>
        Material waterMat;

        /// <summary>
        /// Whether to convert imported features into chunk prefabs.
        /// </summary>
        public bool buildChunks = true;

        /// <summary>
        /// The size of each chunk in meters.
        /// </summary>
        public int chunkSizeMeters = 25;

        /// <summary>
        /// Whether to segment long roads into smaller chunks to allow long roads to be part of the chunk it's in.
        /// </summary>
        public bool segmentLongRoads = true;

        /// <summary>
        /// Draws the GUI for the GeoJSON Importer window, allowing users to set import options and trigger the import process.
        /// </summary>
        void OnGUI()
        {
            EditorGUILayout.HelpBox("Requires package: com.unity.nuget.newtonsoft-json", MessageType.Info);
            geojson = (TextAsset)EditorGUILayout.ObjectField("GeoJSON", geojson, typeof(TextAsset), false);
            originLat = EditorGUILayout.DoubleField("Origin Lat", originLat);
            originLon = EditorGUILayout.DoubleField("Origin Lon", originLon);
            metersToUnity = EditorGUILayout.FloatField("Meters → Unity", metersToUnity);
            batchSize = EditorGUILayout.IntField("Mesh Batch Size", batchSize);
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

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Chunking", EditorStyles.boldLabel);
            buildChunks = EditorGUILayout.Toggle("Build Chunks", buildChunks);
            chunkSizeMeters = EditorGUILayout.IntField("Chunk Size (m)", chunkSizeMeters);

            if (GUILayout.Button("Import → Prefabs"))
            {
                if (!geojson) { EditorUtility.DisplayDialog("Error", "Assign a GeoJSON TextAsset", "OK"); return; }
                if (batchSize <= 100) { EditorUtility.DisplayDialog("Warning", "A low batch size will slow down the importing proccess significantly! Only lower it when you run into memory problems", "I understand"); return; }
                Import();
            }

        }

        private readonly List<MeshWorkItem> pendingMeshes = new();
        static string ReadTag(JToken feature, string name) => (string)feature["properties"]?[name];

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
            EnsureMaterials();                          // Make sure materials and folders exist
            string json = geojson.text;                 // Get the GeoJSON text
            var geoJsonRoot = JObject.Parse(json);      // Parse the JSON using Newtonsoft.Json
            var featuresArray = (JArray)geoJsonRoot["features"];    // Read the features array
            if (featuresArray == null) { Debug.LogError("No features found"); return; }

            string meshFolder = outputFolder + "/Meshes";
            if (!AssetDatabase.IsValidFolder(meshFolder)) AssetDatabase.CreateFolder(outputFolder, "Meshes");
            string prefabFolder = outputFolder + "/Prefabs";
            if (!AssetDatabase.IsValidFolder(prefabFolder)) AssetDatabase.CreateFolder(outputFolder, "Prefabs");
            string chunkFolder = outputFolder + "/Chunks";
            if (!AssetDatabase.IsValidFolder(chunkFolder)) AssetDatabase.CreateFolder(outputFolder, "Chunks");

            var rootGameObject = new GameObject("GeoImported_Scene"); // Doesn't work currently, empty prefab is created and nothing put in.

            int roadCount = 0, buildingCount = 0, landuseCount = 0, plazaCount = 0, waterCount = 0, chunkCount = 0;

            foreach (var featureToken in featuresArray)
            {
                if (pendingMeshes.Count > batchSize)
                {
                    BatchSaveAssets();
                }

                if (featureToken["geometry"] is not JObject geometryObject) continue;
                string geometryType = (string)geometryObject["type"];
                string highway = ReadTag(featureToken, "highway");
                string building = ReadTag(featureToken, "building");
                string landuse = ReadTag(featureToken, "landuse");
                string natural = ReadTag(featureToken, "natural");
                natural ??= ReadTag(featureToken, "waterway");


                if (geometryType == "LineString" && !string.IsNullOrEmpty(highway))
                {
                    var linePoints = ReadLineString(geometryObject);
                    MakeRoad(linePoints, highway, meshFolder, rootGameObject.transform, ref roadCount);
                }
                else if (geometryType == "Polygon")
                {
                    var polygonRings = ReadPolygon(geometryObject);
                    if (!string.IsNullOrEmpty(highway) || ReadTag(featureToken, "area") == "yes") MakeArea(polygonRings, roadMat, "Plaza_", meshFolder, rootGameObject.transform, ref plazaCount);
                    else if (!string.IsNullOrEmpty(building)) MakeArea(polygonRings, buildingMat, "Building_", meshFolder, rootGameObject.transform, ref buildingCount);
                    else if (!string.IsNullOrEmpty(landuse) && landuse == "grass") MakeArea(polygonRings, landuseMat, "Landuse_", meshFolder, rootGameObject.transform, ref landuseCount);
                    else if (!string.IsNullOrEmpty(natural)) MakeArea(polygonRings, waterMat, "WaterBody_", meshFolder, rootGameObject.transform, ref waterCount);
                }
                else if (geometryType == "MultiPolygon")
                {
                    var polygons = ReadMultiPolygon(geometryObject);
                    foreach (var polygonRings in polygons)
                    {
                        if (!string.IsNullOrEmpty(building)) MakeArea(polygonRings, buildingMat, "Building_", meshFolder, rootGameObject.transform, ref buildingCount);
                        else if (!string.IsNullOrEmpty(landuse) && landuse == "grass") MakeArea(polygonRings, landuseMat, "Landuse_", meshFolder, rootGameObject.transform, ref landuseCount);
                        else if (!string.IsNullOrEmpty(natural)) MakeArea(polygonRings, waterMat, "WaterBody_", meshFolder, rootGameObject.transform, ref waterCount);
                    }
                }
            }

            BatchSaveAssets();

            string parentPath = prefabFolder + "/GeoImported_Scene.prefab";
            PrefabUtility.SaveAsPrefabAsset(rootGameObject, parentPath);
            //DestroyImmediate(rootGameObject);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            //if (buildChunks)
            //    ChunkingProcess(chunkSizeMeters, ref chunkCount);

            Debug.Log($"Imported: Roads {roadCount}, Buildings {buildingCount}, Area/Plaza's {plazaCount}, Landuse {landuseCount}" +
                $"\nChunks Created: {chunkCount}");
        }

        /// <summary>
        /// 
        /// </summary>
        private void BatchSaveAssets()
        {
            AssetDatabase.StartAssetEditing();
            foreach (var item in pendingMeshes)
            {
                AssetDatabase.CreateAsset(item.mesh, item.path);
            }
            AssetDatabase.StopAssetEditing();
            pendingMeshes.Clear();
        }

        /// <summary>
        /// Ensures that the output folder and its Materials subfolder exist in the Unity project.
        /// Loads or creates the required materials for roads, buildings, and landuse features.
        /// </summary>
        void EnsureMaterials()
        {
            if (!AssetDatabase.IsValidFolder(outputFolder))
            {
                var parentFolder = Path.GetDirectoryName(outputFolder);
                var leafFolderName = Path.GetFileName(outputFolder);
                if (!AssetDatabase.IsValidFolder(parentFolder)) AssetDatabase.CreateFolder(Path.GetDirectoryName(parentFolder), Path.GetFileName(parentFolder));
                AssetDatabase.CreateFolder(parentFolder, leafFolderName);
            }
            string materialsFolder = outputFolder + "/Materials";
            if (!AssetDatabase.IsValidFolder(materialsFolder)) AssetDatabase.CreateFolder(outputFolder, "Materials");
            float buildingGray = 100 / 255f;
            float roadGray = 150 / 255f;
            roadMat = LoadOrCreateMat(materialsFolder + "/Road.mat", new Color(roadGray, roadGray, roadGray, 1));
            buildingMat = LoadOrCreateMat(materialsFolder + "/Building.mat", new Color(buildingGray, buildingGray, buildingGray));
            landuseMat = LoadOrCreateMat(materialsFolder + "/Landuse.mat", Color.green);
            waterMat = LoadOrCreateMat(materialsFolder + "/Water.mat", Color.cyan);
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
                mat = new(Shader.Find("Universal Render Pipeline/Unlit"))
                {
                    color = c
                };
                AssetDatabase.CreateAsset(mat, path);
            }
            return mat;
        }

        #region Geometry Readers
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
            var coordinatesArray = (JArray)geom["coordinates"]; // [[lon,lat], ...]
            var lonLatPoints = new List<Vector2>(coordinatesArray.Count);
            foreach (var coordinate in coordinatesArray)
            {
                float lon = (float)coordinate[0];
                float lat = (float)coordinate[1];
                lonLatPoints.Add(new Vector2(lon, lat));
            }
            var metersPoints = GeoProjection.LatLonArrayToMeters(lonLatPoints, originLat, originLon);
            var pointsInUnityMeters = new List<Vector2>(metersPoints.Length);
            foreach (var meterPoint in metersPoints) pointsInUnityMeters.Add(meterPoint * metersToUnity);
            return pointsInUnityMeters;
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
            var ringsInUnityMeters = new List<List<Vector2>>();
            var coordinatesArray = (JArray)geom["coordinates"];
            if (coordinatesArray == null) return ringsInUnityMeters;
            for (int r = 0; r < coordinatesArray.Count; r++)
            {
                var ringArray = (JArray)coordinatesArray[r];
                var ringLonLatPoints = new List<Vector2>(ringArray.Count);
                foreach (var coordinate in ringArray)
                {
                    float lon = (float)coordinate[0];
                    float lat = (float)coordinate[1];
                    ringLonLatPoints.Add(new Vector2(lon, lat));
                }
                var ringMetersPoints = GeoProjection.LatLonArrayToMeters(ringLonLatPoints, originLat, originLon);
                var ringUnityPoints = new List<Vector2>(ringMetersPoints.Length);
                foreach (var meterPoint in ringMetersPoints) ringUnityPoints.Add(meterPoint * metersToUnity);
                ringsInUnityMeters.Add(ringUnityPoints);
            }
            return ringsInUnityMeters;
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
            var polygons = new List<List<List<Vector2>>>();
            var coordinatesArray = (JArray)geom["coordinates"];
            if (coordinatesArray == null) return polygons;
            foreach (var polygon in coordinatesArray)
            {
                var polygonRings = new List<List<Vector2>>();
                foreach (var ringArray in (JArray)polygon)
                {
                    var ringLonLatPoints = new List<Vector2>();
                    foreach (var coordinate in (JArray)ringArray)
                    {
                        float lon = (float)coordinate[0];
                        float lat = (float)coordinate[1];
                        ringLonLatPoints.Add(new Vector2(lon, lat));
                    }
                    var ringMetersPoints = GeoProjection.LatLonArrayToMeters(ringLonLatPoints, originLat, originLon);
                    var ringUnityPoints = new List<Vector2>(ringMetersPoints.Length);
                    foreach (var meterPoint in ringMetersPoints) ringUnityPoints.Add(meterPoint * metersToUnity);
                    polygonRings.Add(ringUnityPoints);
                }
                polygons.Add(polygonRings);
            }
            return polygons;
        }
        #endregion Geometry Readers

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
        /// <param name="polylinePoints">List of 2D points representing the road centerline in Unity meters.</param>
        /// <param name="highwayType">Highway type string (e.g., "primary", "residential") used for width and naming.</param>
        /// <param name="prefabFolder">Folder path to save the prefab asset.</param>
        /// <param name="meshFolder">Folder path to save the mesh asset.</param>
        /// <param name="parentTransform">Transform to parent the created GameObject under.</param>
        /// <param name="counter">Reference to an integer counter for unique asset naming; incremented after creation.</param>
        void MakeRoad(List<Vector2> polylinePoints, string highwayType,
              string meshFolder, Transform parentTransform, ref int counter)
        {
            if (polylinePoints.Count < 2) return;

            float roadWidth = WidthForHighway(highwayType);
            var roadMesh = PolylineMeshBuilder.Build(polylinePoints, roadWidth);

            string meshPath = $"{meshFolder}/Road_{highwayType}_{counter}.asset";

            // Defer asset creation → batch later
            pendingMeshes.Add(new MeshWorkItem { mesh = roadMesh, path = meshPath });

            // Create GameObject immediately (still needed in prefab hierarchy)
            var roadGameObject = new GameObject($"Road_{highwayType}_{counter}");
            roadGameObject.transform.parent = parentTransform;
            var meshFilter = roadGameObject.AddComponent<MeshFilter>();
            var meshRenderer = roadGameObject.AddComponent<MeshRenderer>();
            meshFilter.sharedMesh = roadMesh;
            meshRenderer.sharedMaterial = roadMat;

            counter++;
        }


        /// <summary>
        /// Builds a filled area mesh from polygon rings (outer boundary with optional holes),
        /// tessellates it using LibTessDotNet, creates a GameObject with the given material,
        /// and defers mesh asset creation for batched saving. The resulting mesh lies on the XY plane (Z = 0).
        /// </summary>
        /// <param name="rings">
        /// Polygon rings in Unity meters. The first ring is the outer boundary; subsequent rings are holes.
        /// Each ring is a list of 2D points already projected/scaled into Unity space.
        /// </param>
        /// <param name="material">Material to assign to the created MeshRenderer.</param>
        /// <param name="prefix">
        /// Name prefix used for the generated mesh asset and GameObject (e.g., "Building_", "Landuse_").
        /// </param>
        /// <param name="meshFolder">AssetDatabase path where the mesh asset will be saved.</param>
        /// <param name="parentTransform">
        /// Intended parent Transform for the created GameObject. Note: currently not applied in this implementation.
        /// </param>
        /// <param name="counter">
        /// Reference counter used to produce unique names/paths for assets and objects; incremented upon success.
        /// </param>
        /// <remarks>
        /// Steps:
        /// 1) Validate that an outer ring exists and has at least three points; otherwise, return.
        /// 2) Convert each ring to a LibTessDotNet contour and add it with CounterClockwise orientation.
        /// 3) Tessellate using WindingRule.EvenOdd into triangles (polySize = 3).
        /// 4) Copy tessellated vertices into a Unity Mesh on the XY plane and build triangle indices.
        /// 5) Recalculate bounds (normals are skipped for performance; suitable for unlit materials).
        /// 6) Queue the mesh into a pending list for batched AssetDatabase.CreateAsset later.
        /// 7) Create a GameObject with MeshFilter/MeshRenderer, assign the mesh and provided material.
        /// 8) Increment the provided counter to ensure unique naming on subsequent calls.
        /// 
        /// Notes:
        /// - Input coordinates must already be projected to meters and scaled by the configured meters-to-Unity factor.
        /// - The mesh is suitable for flat areas like building footprints, plazas, or landuse patches.
        /// - Unity Editor APIs are used; execution must occur on the main editor thread.
        /// </remarks>
        void MakeArea(List<List<Vector2>> rings, Material material, string prefix,
              string meshFolder, Transform parentTransform, ref int counter)
        {
            if (rings.Count == 0 || rings[0].Count < 3) return;

            // --- Tessellate with LibTess ---
            var tess = new Tess();
            foreach (var ring in rings)
            {
                if (ring.Count < 3) continue;
                var contour = new ContourVertex[ring.Count];
                for (int i = 0; i < ring.Count; i++)
                    contour[i].Position = new Vec3(ring[i].x, ring[i].y, 0);
                tess.AddContour(contour, ContourOrientation.CounterClockwise);
            }
            tess.Tessellate(WindingRule.EvenOdd, ElementType.Polygons, 3);

            // --- Build Unity mesh ---
            var verts = new Vector3[tess.Vertices.Length];
            for (int i = 0; i < verts.Length; i++)
                verts[i] = new Vector3(tess.Vertices[i].Position.X, tess.Vertices[i].Position.Y, 0);

            var indices = new int[tess.ElementCount * 3];
            for (int i = 0; i < tess.ElementCount; i++)
            {
                indices[i * 3 + 0] = tess.Elements[i * 3 + 0];
                indices[i * 3 + 1] = tess.Elements[i * 3 + 1];
                indices[i * 3 + 2] = tess.Elements[i * 3 + 2];
            }

            var areaMesh = new Mesh
            {
                vertices = verts,
                triangles = indices
            };
            areaMesh.RecalculateBounds();
            // skip normals if you don’t need lighting: areaMesh.RecalculateNormals();

            string meshPath = $"{meshFolder}/{prefix}{counter}.asset";

            // Defer asset creation → batch later
            pendingMeshes.Add(new MeshWorkItem { mesh = areaMesh, path = meshPath });

            // Create GameObject immediately (still needed for prefab structure)
            var areaGameObject = new GameObject($"{prefix}{counter}");
            areaGameObject.transform.parent = parentTransform;
            var meshFilter = areaGameObject.AddComponent<MeshFilter>();
            var meshRenderer = areaGameObject.AddComponent<MeshRenderer>();
            meshFilter.sharedMesh = areaMesh;
            meshRenderer.sharedMaterial = material;

            counter++;
        }


        #region Chunking process
        /// <summary>
        /// 
        /// </summary>
        private Dictionary<Vector2Int, List<GameObject>> cellToObjects = new();

        /// <summary>
        /// 
        /// </summary>
        void ChunkingProcess(int chunkSize, ref int counter)
        {
            CellBucketing();
            CreateChunks();
        }

        /// <summary>
        /// 
        /// </summary>
        void CellBucketing()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        void CreateChunks()
        {
            CombineByMaterial();
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="listOfObjects"></param>
        /// <exception cref="NotImplementedException"></exception>
        void CombineByMaterial()
        {
            throw new NotImplementedException();
        }

        Vector2Int WorldToCell(Vector2 pos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(pos.x / chunkSizeMeters),
                Mathf.FloorToInt(pos.y / chunkSizeMeters)
            );
        }

        /// <summary>
        /// 
        /// </summary>
        private class MeshWorkItem
        {
            public Mesh mesh;
            public string path;
        }

        #endregion Chunking process
    }
}
#endif