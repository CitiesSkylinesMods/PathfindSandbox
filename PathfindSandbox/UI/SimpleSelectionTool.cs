using System.Collections.Generic;
using UnityEngine;

namespace PathfindSandbox.UI {
    public class SimpleSelectionTool : DefaultTool {
        private static Camera _camera;

        protected override bool CheckBuilding(ushort building, ref ToolErrors errors) => true;
        protected override bool CheckNode(ushort node, ref ToolErrors errors) => true;
        protected override bool CheckVehicle(ushort node, ref ToolErrors errors) => true;
        protected override bool CheckParkedVehicle(ushort node, ref ToolErrors errors) => false;
        protected override bool CheckCitizen(ushort citizenInstance, ref ToolErrors errors) => false;

        public override Building.Flags GetBuildingIgnoreFlags() => Building.Flags.None;
        public override NetNode.Flags GetNodeIgnoreFlags() => NetNode.Flags.None;
        public override Vehicle.Flags GetVehicleIgnoreFlags() => 0;
        public override VehicleParked.Flags GetParkedVehicleIgnoreFlags() => VehicleParked.Flags.All;
        public override CitizenInstance.Flags GetCitizenIgnoreFlags() => CitizenInstance.Flags.All;

        private float _lastClickTime;
        private float _clickInterval = 0.5f;
        protected override void Awake() {
            base.Awake();
            _camera = Camera.main;
        }

        public override void RenderOverlay(RenderManager.CameraInfo cameraInfo) {
            base.RenderOverlay(cameraInfo);
            if (m_hoverInstance.NetNode == 0) {
                return;
            }

            NetNode node = NetManager.instance.m_nodes.m_buffer[m_hoverInstance.NetNode];
            RenderManager.instance.OverlayEffect.DrawCircle(
                cameraInfo,
                GetToolColor(false, m_selectErrors != ToolErrors.None),
                node.m_position,
                node.m_bounds.size.x,
                node.m_position.y - 1f,
                node.m_position.y + 1f,
                true,
                true);
        }

        public override void SimulationStep() {
            base.SimulationStep();
            if (m_hoverInstance.CitizenInstance > 0 || m_hoverInstance.Vehicle > 0 ||
                m_hoverInstance.ParkedVehicle > 0 ||
                m_hoverInstance.District > 0 || m_hoverInstance.Park > 0 || m_hoverInstance.TransportLine > 0 ||
                m_hoverInstance.Prop > 0 || m_hoverInstance.Tree > 0) {
                return;
            }

            if (!RayCastNode(out ushort hoveredNode)) {
                return;
            }

            if (hoveredNode > 0) {
                m_hoverInstance.NetNode = hoveredNode;
            }
        }

        private static bool RayCastNode(out ushort netNode) {
            if (RayCastSegmentAndNode(out RaycastOutput output)) {
                netNode = output.m_netNode;
                return true;
            }

            netNode = 0;
            return false;
        }

        private static bool RayCastSegmentAndNode(out RaycastOutput output) {
            RaycastInput input = new RaycastInput(_camera.ScreenPointToRay(Input.mousePosition), _camera.farClipPlane) {
                m_netService = {m_itemLayers = ItemClass.Layer.Default | ItemClass.Layer.MetroTunnels},
                m_ignoreNodeFlags = NetNode.Flags.None,
                m_ignoreTerrain = true,
            };

            return RayCast(input, out output);
        }

        protected override void OnToolGUI(Event e) {
            DrawLabel();
            if (m_toolController.IsInsideUI || e.type != EventType.MouseDown) {
                base.OnToolGUI(e);
                return;
            }

            if (m_hoverInstance.IsEmpty) {
                return;
            }

            if (Time.time - _lastClickTime < _clickInterval) {
                return;
            }

            if (e.button == 0) { //left click
                _lastClickTime = Time.time;
                if (m_hoverInstance.NetNode != 0) {
                    SelectionManager.Manager.SetValue(m_hoverInstance.NetNode.ToString());
                } else if (m_hoverInstance.Building != 0) {
                    SelectionManager.Manager.SetValue(m_hoverInstance.Building.ToString());
                }
            } else if (e.button == 1) { //right click
                _lastClickTime = Time.time;
                SelectionManager.Manager.MoveFocus(false);
            }
        }

        private void DrawLabel() {
            InstanceID hoverInstance = m_hoverInstance;
            string text = null;
            if (hoverInstance.NetNode != 0) {
                text = $"[Click LMB to {LeftClickText()}]\n[Click RMB to {RightClickText()}]\nNode ID: {hoverInstance.NetNode}";
            } else if (hoverInstance.Building != 0) {
                text = $"[Click LMB to {LeftClickText()}]\n[Click RMB to {RightClickText()}]\nBuilding ID: {hoverInstance.Building}";
            } else if (hoverInstance.Vehicle != 0) {
                text = $"[Click LMB to NOT_ACTION]\n[Click RMB to NOT_ACTION]\nVehicle ID: {hoverInstance.Vehicle}\n" +
                       $"Destination ID: {VehicleManager.instance.m_vehicles.m_buffer[hoverInstance.Vehicle].m_targetBuilding}";
            }

            if (text == null) {
                return;
            }

            Vector3 screenPoint = Input.mousePosition;
            screenPoint.y = Screen.height - screenPoint.y;
            Color color = GUI.color;
            GUI.color = Color.white;
            GUI.skin.label.fontSize = 15;
            DeveloperUI.LabelOutline(new Rect(screenPoint.x + 15, screenPoint.y + 15, 400f, 100f), text, new Color(0.19f, 0.21f, 0.24f), new Color(0.35f, 1f, 0.64f), GUI.skin.label, 1f);
            GUI.color = color;
        }

        private string LeftClickText() {
            return SelectionManager.Manager.IsWindowActive ? "Copy ID" : "NOT_ACTION";
        }

        private string RightClickText() {
            return SelectionManager.Manager.IsWindowActive ? "Change active input" : "NOT_ACTION";
        }
    }
}