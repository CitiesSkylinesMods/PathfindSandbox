using ICities;
using JetBrains.Annotations;
using PathfindSandbox.UI;
using UnityEngine;

namespace PathfindSandbox {
    [UsedImplicitly]
    public class PathfindSandbox : IUserMod {
        public string Name { get; } = "Pathfind Sandbox";
        public string Description { get; } = "Sandbox for Pathfinder";

        [UsedImplicitly]
        public void OnEnabled() {
            Debug.Log("Pathfinding Sandbox enabled");
        }

        [UsedImplicitly]
        public void OnDisabled() {
            if (SandboxUi.Instance) {
                GameObject.Destroy(SandboxUi.Instance);
            }

            Debug.Log("Pathfinding Sandbox disabled");
        }
    }
}