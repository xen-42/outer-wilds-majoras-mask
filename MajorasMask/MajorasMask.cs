using OWML.ModHelper;
using OWML.Common;
using UnityEngine;
using UnityEngine.PostProcessing;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;
using System;

namespace MajorasMask
{
    public class MajorasMask : ModBehaviour
    {
        public static MajorasMask Instance;
        private static readonly Shader standardShader = Shader.Find("Standard");
        private static readonly Shader unlitShader = Shader.Find("Unlit/Fade");

        private GameObject _moonPrefab;
        private GameObject moon;
        private SunController _sunController;
        private TessellatedSphereRenderer _sunSurface;

        private bool _loaded;

        private void Start()
        {
            Instance = this;

            var bundle = ModHelper.Assets.LoadBundle("majoras mask");

            _moonPrefab = LoadPrefab(bundle, "Assets/Prefabs/moon.prefab");

            LoadManager.OnCompleteSceneLoad += OnCompleteSceneLoad;
        }

        private void OnDestroy()
        {
            if(_loaded) _sunController.OnSupernovaStart -= OnSupernovaStart;
            LoadManager.OnCompleteSceneLoad -= OnCompleteSceneLoad;
        }

        private void OnCompleteSceneLoad(OWScene _, OWScene currentScene)
        {
            if (currentScene != OWScene.SolarSystem) 
            {
                _loaded = false;
                return;
            }

            _loaded = true;

            Log("Getting sun controller");
            _sunController = GameObject.Find("Sun_Body").GetComponent<SunController>();
            _sunController.OnSupernovaStart += OnSupernovaStart;

            Log("Disappearing original sun surface");
            var sunSurface = GameObject.Find("Sun_Body/Sector_SUN/Geometry_SUN/Surface").gameObject;
            _sunSurface = sunSurface.GetComponent<TessellatedSphereRenderer>();
            _sunSurface.enabled = false;

            Log("Replacing sun mesh");
            moon = GameObject.Instantiate(_moonPrefab, sunSurface.transform);
            moon.transform.localPosition = Vector3.zero;
            moon.transform.localScale = Vector3.one * 0.0005f;
            foreach(Transform child in moon.transform)
            {
                child.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            }
            moon.SetActive(true);
        }

        private static GameObject LoadPrefab(AssetBundle bundle, string path)
        {
            var prefab = bundle.LoadAsset<GameObject>(path);

            // Repair materials             
            foreach (var renderer in prefab.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (var mat in renderer.materials)
                {
                    mat.shader = standardShader;
                    mat.renderQueue = 2000;
                }
            }

            prefab.SetActive(false);

            return prefab;
        }

        private void OnSupernovaStart()
        {
            //_sunSurface.enabled = true;
            foreach (var renderer in moon.GetComponentsInChildren<MeshRenderer>())
            {
                foreach (var mat in renderer.materials)
                {
                    mat.shader = unlitShader;
                    mat.color = Color.blue;
                }
            }
            moon.transform.localScale = Vector3.one * 0.00045f;
        }

        private void Update()
        {
            if (!_loaded) return;

            try
            {
                moon.transform.LookAt(Locator.GetPlayerTransform(), Vector3.up);
            }
            catch (Exception) { }
        }

        private static void Log(string message)
        {
            MajorasMask.Instance.ModHelper.Console.WriteLine(message, OWML.Common.MessageType.Info);
        }
    }
}
