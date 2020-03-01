using ICities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PathfindSandbox {
    public class LoadingExtensions : ILoadingExtension {

        // public static AppMode? CurrentMode = SimulationManager.instance.m_ManagersWrapper?.loading.currentMode;
        public static bool GameLoaded { get; set; }
        
        public void OnCreated(ILoading loading) {
            Debug.Log("PfS: OnCreated");
            if (LoadingManager.instance.m_loadingComplete) {
                Debug.Log("PfS: Hot-Reloading");
                InitUi();
            }
        }

        public void OnReleased() {
            Debug.Log("PfS: Released");
            if (SandboxUi.Instance) {
                GameObject.Destroy(SandboxUi.Instance.gameObject);
                SandboxUi.Instance = null;
            }
        }

        public void OnLevelLoaded(LoadMode mode) {
            Debug.Log("PfS: OnLevelLoaded");
            InitUi();
            GameLoaded = true;
        }

        public void OnLevelUnloading() {
            Debug.Log("PfS: OnLevelUnloading");
            if (SandboxUi.Instance) {
                GameObject.Destroy(SandboxUi.Instance.gameObject);
                SandboxUi.Instance = null;
            }
        }

        private void InitUi() {
            if (SandboxUi.Instance == null) {
                Debug.Log("PfS: Initializing UI");
                SandboxUi.Instance = new GameObject("PF_SandboxUI").AddComponent<SandboxUi>();
            }
        }
    }
}