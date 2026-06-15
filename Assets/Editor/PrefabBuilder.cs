using UnityEditor;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using ARUnity.AR;

namespace ARUnity.Editor
{
    public static class PrefabBuilder
    {
        public const string PlaneVisualizationPrefabPath = "Assets/Prefabs/AR/PlaneVisualization.prefab";
        public const string PlacedObjectPrefabPath = "Assets/Prefabs/AR/PlacedObject.prefab";

        private const string PlaneMaterialPath = "Assets/Art/Materials/PlaneVisualizationMat.mat";
        private const string PlacedObjectMaterialPath = "Assets/Art/Materials/PlacedObjectMat.mat";

        [MenuItem("ARUnity/Build Assets/Build AR Prefabs")]
        public static void BuildARPrefabs()
        {
            EnsureFolders();
            var planeMat = BuildPlaneMaterial();
            var placedMat = BuildPlacedObjectMaterial();
            BuildPlaneVisualizationPrefab(planeMat);
            BuildPlacedObjectPrefab(placedMat);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[ARUnity] AR prefabs built in Assets/Prefabs/AR/");
        }

        // Called by ARSceneBuilder before building the scene so prefabs exist for wiring.
        public static void EnsurePrefabsExist()
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PlaneVisualizationPrefabPath) == null ||
                AssetDatabase.LoadAssetAtPath<GameObject>(PlacedObjectPrefabPath) == null)
            {
                BuildARPrefabs();
            }
        }

        // ── Materials ─────────────────────────────────────────────────────

        private static Material BuildPlaneMaterial()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(PlaneMaterialPath);
            if (existing != null) return existing;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null)
            {
                shader = Shader.Find("Standard");
                Debug.LogWarning("[ARUnity] URP Lit shader not found — falling back to Standard.");
            }

            var mat = new Material(shader) { name = "PlaneVisualizationMat" };

            // Semi-transparent green to indicate detected AR surfaces
            mat.SetColor("_BaseColor", new Color(0.1f, 0.85f, 0.4f, 0.35f));
            SetURPTransparent(mat);

            AssetDatabase.CreateAsset(mat, PlaneMaterialPath);
            return mat;
        }

        private static Material BuildPlacedObjectMaterial()
        {
            var existing = AssetDatabase.LoadAssetAtPath<Material>(PlacedObjectMaterialPath);
            if (existing != null) return existing;

            var shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Standard");

            var mat = new Material(shader) { name = "PlacedObjectMat" };
            mat.SetColor("_BaseColor", new Color(0.2f, 0.5f, 1.0f, 1.0f));
            mat.SetFloat("_Metallic", 0.1f);
            mat.SetFloat("_Smoothness", 0.6f);

            AssetDatabase.CreateAsset(mat, PlacedObjectMaterialPath);
            return mat;
        }

        private static void SetURPTransparent(Material mat)
        {
            mat.SetFloat("_Surface", 1f);       // 1 = Transparent
            mat.SetFloat("_Blend", 0f);          // Alpha blend
            mat.SetFloat("_ZWrite", 0f);
            mat.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent;
            mat.SetOverrideTag("RenderType", "Transparent");
            mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
        }

        // ── Prefabs ───────────────────────────────────────────────────────

        private static void BuildPlaneVisualizationPrefab(Material mat)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PlaneVisualizationPrefabPath) != null)
                return;

            var go = new GameObject("PlaneVisualization");

            // ARPlaneMeshVisualizer drives MeshFilter and MeshRenderer each frame
            go.AddComponent<ARPlaneMeshVisualizer>();
            go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = mat;
            mr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            mr.receiveShadows = false;

            PrefabUtility.SaveAsPrefabAsset(go, PlaneVisualizationPrefabPath);
            Object.DestroyImmediate(go);
        }

        private static void BuildPlacedObjectPrefab(Material mat)
        {
            if (AssetDatabase.LoadAssetAtPath<GameObject>(PlacedObjectPrefabPath) != null)
                return;

            // 10 cm cube — representative AR scale for a small object
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = "PlacedObject";
            go.transform.localScale = Vector3.one * 0.1f;

            go.GetComponent<MeshRenderer>().sharedMaterial = mat;

            // Remove the Collider added by CreatePrimitive — not needed for AR placement
            Object.DestroyImmediate(go.GetComponent<BoxCollider>());

            // Scale-in animation plays automatically on Start
            go.AddComponent<ObjectScaleAnimation>();

            PrefabUtility.SaveAsPrefabAsset(go, PlacedObjectPrefabPath);
            Object.DestroyImmediate(go);
        }

        // ── Folder setup ──────────────────────────────────────────────────

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
