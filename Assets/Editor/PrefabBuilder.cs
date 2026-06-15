using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.ARFoundation;
using ARUnity.AR;

namespace ARUnity.Editor
{
    public static class PrefabBuilder
    {
        // ── Public paths (used by ARSceneBuilder) ─────────────────────────

        public const string PlaneVisualizationPrefabPath = "Assets/Prefabs/AR/PlaneVisualization.prefab";
        public const string PlacedCubePrefabPath    = "Assets/Prefabs/AR/PlacedCube.prefab";
        public const string PlacedSpherePrefabPath  = "Assets/Prefabs/AR/PlacedSphere.prefab";
        public const string PlacedCapsulePrefabPath = "Assets/Prefabs/AR/PlacedCapsule.prefab";

        // Keep this alias so earlier wiring in ARSceneBuilder still resolves
        public const string PlacedObjectPrefabPath  = PlacedCubePrefabPath;

        private const string PlaneMaterialPath    = "Assets/Art/Materials/PlaneVisualizationMat.mat";
        private const string CubeMaterialPath     = "Assets/Art/Materials/PlacedCubeMat.mat";
        private const string SphereMaterialPath   = "Assets/Art/Materials/PlacedSphereMat.mat";
        private const string CapsuleMaterialPath  = "Assets/Art/Materials/PlacedCapsuleMat.mat";

        // ── Menu entry ────────────────────────────────────────────────────

        [MenuItem("ARUnity/Build Assets/Build AR Prefabs")]
        public static void BuildARPrefabs()
        {
            EnsureFolders();

            var planeMat    = EnsureMaterial(PlaneMaterialPath,   BuildPlaneMaterial);
            var cubeMat     = EnsureMaterial(CubeMaterialPath,    () => BuildPlacedMaterial("PlacedCubeMat",    new Color(0.20f, 0.50f, 1.00f)));
            var sphereMat   = EnsureMaterial(SphereMaterialPath,  () => BuildPlacedMaterial("PlacedSphereMat",  new Color(1.00f, 0.38f, 0.28f)));
            var capsuleMat  = EnsureMaterial(CapsuleMaterialPath, () => BuildPlacedMaterial("PlacedCapsuleMat", new Color(1.00f, 0.75f, 0.10f)));

            EnsurePrimitivePrefab(PlaneVisualizationPrefabPath,  BuildPlaneVisualizationPrefab,  planeMat);
            EnsurePrimitivePrefab(PlacedCubePrefabPath,          BuildCubePrefab,                cubeMat);
            EnsurePrimitivePrefab(PlacedSpherePrefabPath,        BuildSpherePrefab,              sphereMat);
            EnsurePrimitivePrefab(PlacedCapsulePrefabPath,       BuildCapsulePrefab,             capsuleMat);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ARUnity] AR prefabs built in Assets/Prefabs/AR/");
        }

        public static void EnsurePrefabsExist()
        {
            var missing =
                AssetDatabase.LoadAssetAtPath<GameObject>(PlaneVisualizationPrefabPath) == null ||
                AssetDatabase.LoadAssetAtPath<GameObject>(PlacedCubePrefabPath)    == null ||
                AssetDatabase.LoadAssetAtPath<GameObject>(PlacedSpherePrefabPath)  == null ||
                AssetDatabase.LoadAssetAtPath<GameObject>(PlacedCapsulePrefabPath) == null;

            if (missing) BuildARPrefabs();
        }

        // ── Material builders ─────────────────────────────────────────────

        private static Material BuildPlaneMaterial()
        {
            var shader = URPLitShader();
            var mat = new Material(shader) { name = "PlaneVisualizationMat" };

            // Semi-transparent teal grid to indicate detected AR surfaces
            mat.SetColor("_BaseColor", new Color(0.05f, 0.80f, 0.55f, 0.28f));
            mat.SetFloat("_Metallic",   0f);
            mat.SetFloat("_Smoothness", 0.2f);
            SetURPTransparent(mat);
            return mat;
        }

        private static Material BuildPlacedMaterial(string matName, Color baseColor)
        {
            var shader = URPLitShader();
            var mat = new Material(shader) { name = matName };
            mat.SetColor("_BaseColor",  baseColor);
            mat.SetFloat("_Metallic",   0.15f);
            mat.SetFloat("_Smoothness", 0.65f);
            // Opaque — writes depth, casts and receives shadows for a grounded AR feel
            mat.SetFloat("_Surface", 0f); // 0 = Opaque
            return mat;
        }

        private static void SetURPTransparent(Material mat)
        {
            mat.SetFloat("_Surface", 1f);
            mat.SetFloat("_Blend",   0f);
            mat.SetFloat("_ZWrite",  0f);
            mat.renderQueue = (int)RenderQueue.Transparent;
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }

        private static Shader URPLitShader()
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                Debug.LogWarning("[ARUnity] URP Lit shader not found — falling back to Standard.");
                shader = Shader.Find("Standard");
            }
            return shader;
        }

        // ── Prefab builders ───────────────────────────────────────────────

        private static void BuildPlaneVisualizationPrefab(GameObject go, Material mat)
        {
            go.name = "PlaneVisualization";
            go.AddComponent<ARPlaneMeshVisualizer>();
            go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = ShadowCastingMode.Off;
            mr.receiveShadows = true; // show shadow cast by placed virtual objects
        }

        private static void BuildCubePrefab(GameObject go, Material mat)
        {
            go.name = "PlacedCube";
            BuildPlacedPrimitive(go, PrimitiveType.Cube, mat);
        }

        private static void BuildSpherePrefab(GameObject go, Material mat)
        {
            go.name = "PlacedSphere";
            BuildPlacedPrimitive(go, PrimitiveType.Sphere, mat);
        }

        private static void BuildCapsulePrefab(GameObject go, Material mat)
        {
            go.name = "PlacedCapsule";
            BuildPlacedPrimitive(go, PrimitiveType.Capsule, mat);
        }

        private static void BuildPlacedPrimitive(GameObject root, PrimitiveType type, Material mat)
        {
            // Create the primitive as a child so ObjectScaleAnimation can scale from 0
            // without affecting the anchor root transform.
            var mesh = GameObject.CreatePrimitive(type);
            mesh.name = "Mesh";
            mesh.transform.SetParent(root.transform, false);
            mesh.transform.localScale = Vector3.one * 0.1f; // 10 cm

            mesh.GetComponent<MeshRenderer>().sharedMaterial = mat;

            // Remove collider — AR placed objects don't need physics
            var col = mesh.GetComponent<Collider>();
            if (col != null) Object.DestroyImmediate(col);

            // Scale-in animation on the mesh child, not the root
            mesh.AddComponent<ObjectScaleAnimation>();
        }

        // ── Helpers ───────────────────────────────────────────────────────

        private delegate void PrefabSetup(GameObject go, Material mat);

        private static Material EnsureMaterial(string path, System.Func<Material> builder)
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing != null) return existing;
            var mat = builder();
            AssetDatabase.CreateAsset(mat, path);
            return mat;
        }

        private static void EnsurePrimitivePrefab(string path, PrefabSetup setup, Material mat)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;

            var go = new GameObject();
            setup(go, mat);
            PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
        }

        private static void EnsureFolders()
        {
            EnsureFolder("Assets", "Art");
            EnsureFolder("Assets/Art", "Materials");
            EnsureFolder("Assets", "Prefabs");
            EnsureFolder("Assets/Prefabs", "AR");
        }

        private static void EnsureFolder(string parent, string name)
        {
            var path = $"{parent}/{name}";
            if (!AssetDatabase.IsValidFolder(path))
                AssetDatabase.CreateFolder(parent, name);
        }
    }
}
