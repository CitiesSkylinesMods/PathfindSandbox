using ICities;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace PathfindSandbox {
    public class LoadingExtensions : ILoadingExtension {
        public void OnCreated(ILoading loading) {
            Debug.Log("PfS: OnCreated");
            if (SceneManager.GetActiveScene().name.Equals("Game")) {
                Debug.Log("PfS: Hot-Reloading");
                InitUi();
            }
        }

        public void OnReleased() {
            Debug.Log("PfS: Released");
            if (SandboxUi.Instance) {
                GameObject.Destroy(SandboxUi.Instance);
                SandboxUi.Instance = null;
            }
        }

        public void OnLevelLoaded(LoadMode mode) {
            Debug.Log("PfS: OnLevelLoaded");
            InitUi();
        }

        public void OnLevelUnloading() {
            Debug.Log("PfS: OnLevelUnloading");
            if (SandboxUi.Instance) {
                GameObject.Destroy(SandboxUi.Instance);
                SandboxUi.Instance = null;
            }
        }

        private void InitUi() {
            if (SandboxUi.Instance) {
                Object.Destroy(SandboxUi.Instance);
                SandboxUi.Instance = null;
            }
            SandboxUi.Instance = new GameObject("PF_SandboxUI").AddComponent<SandboxUi>();
        }
    }
}