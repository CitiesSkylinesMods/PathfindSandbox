using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ColossalFramework;
using ColossalFramework.Math;
using UnityEngine;

namespace PathfindSandbox.UI {
    public class SandboxUi : MonoBehaviour {
        public static SandboxUi Instance { get; set; }

        private static int _popupId = 10;

        //delegate based variable content
        private Window _currentWindow = MainWindow;

        private const int MinMainWindowHeight = 100;
        private static Rect _mainWindowPos = new Rect(50, 50, 150, MinMainWindowHeight);

        private static List<PopupWindow> _popupWindows = new List<PopupWindow>();

        private static List<uint> _citizens = new List<uint>();
        private static List<ushort> _vehicles = new List<ushort>();
        private static bool _selectionToolEnabled;

        private static SimpleSelectionTool _simpleSelectionTool;

        private static GUIStyle _focusedLabelStyle;
        private static GUIStyle _defaultLabelStyle;
        private bool _styleSet;

        public void Awake() {
            GameObject toolModControl = ToolsModifierControl.toolController.gameObject;
            _simpleSelectionTool = toolModControl.GetComponent<SimpleSelectionTool>()
                                   ?? toolModControl.AddComponent<SimpleSelectionTool>();
        }

        public void OnDestroy() {
            Destroy(_simpleSelectionTool);
        }

        private static void EnableTool() {
            ToolsModifierControl.toolController.CurrentTool = _simpleSelectionTool;
            ToolsModifierControl.SetTool<SimpleSelectionTool>();
        }

        private static void DisableTool() {
            if (ToolsModifierControl.toolController.CurrentTool == _simpleSelectionTool) {
                ToolsModifierControl.toolController.CurrentTool = ToolsModifierControl.GetTool<DefaultTool>();
                ToolsModifierControl.SetTool<DefaultTool>();
            }
        }

        public void OnGUI() {
            if (!_styleSet) {
                _focusedLabelStyle = new GUIStyle(GUI.skin.label) {normal = {textColor = Color.green}};
                _defaultLabelStyle = new GUIStyle(GUI.skin.label);
                _styleSet = true;
            }
            _currentWindow?.Invoke();

            bool refreshPopups = false;

            for (int i = 0; i < _popupWindows.Count; i++) {
                if (_popupWindows[i].Open) {
                    _popupWindows[i].RenderWindow();
                } else {
                    refreshPopups = true;
                }
            }

            if (refreshPopups) {
                _popupWindows = _popupWindows.Where(p => p.Open).ToList();
            }
        }

        private static void MainWindow() {
            _mainWindowPos = GUILayout.Window(1, _mainWindowPos, MainWindowFunction, "Pathfind Sandbox", GUILayout.Height(MinMainWindowHeight));
        }

        private static void MainWindowFunction(int id) {
            GUI.DragWindow(new Rect(0, 0, 130, 20));
            bool oldEnabled = _selectionToolEnabled;
            _selectionToolEnabled = GUI.Toggle(new Rect(130, 0, 20, 20), _selectionToolEnabled, "");
            if (!oldEnabled && _selectionToolEnabled) {
                EnableTool();
            } else if(oldEnabled && !_selectionToolEnabled) {
                DisableTool();
            }


            GUILayout.BeginVertical();

            if (GUILayout.Button("Spawn Cim", GUILayout.Width(130), GUILayout.Height(20))) {
                PopupWindow popupWindow = SpawnCimPopup();
                _popupWindows.Add(popupWindow);
            }

            if (GUILayout.Button("Spawn Cargo Truck", GUILayout.Width(130), GUILayout.Height(20))) {
                PopupWindow popupWindow = SpawnCargoVehiclePopup();
                _popupWindows.Add(popupWindow);
            }


            if (GUILayout.Button("Spawn Cim Vehicle", GUILayout.Width(130), GUILayout.Height(20))) {
                PopupWindow popupWindow = SpawnPassengerVehiclePopup();
                _popupWindows.Add(popupWindow);
            }

            if (_citizens.Count > 0 && GUILayout.Button("Remove Citizens", GUILayout.Width(130), GUILayout.Height(20))) {
                foreach (uint citizen in _citizens) {
                    CitizenManager.instance.ReleaseCitizen(citizen);
                }

                _citizens.Clear();
            }

            if (_vehicles.Count > 0 && GUILayout.Button("Remove Vehicles", GUILayout.Width(130), GUILayout.Height(20))) {
                foreach (ushort vehicle in _vehicles) {
                    VehicleManager.instance.ReleaseVehicle(vehicle);
                }

                _vehicles.Clear();
            }

            GUILayout.EndVertical();
            if (Event.current.type == EventType.mouseUp) {
                SelectionManager.Manager.ActivateWindow(-1, null);
            }
        }

        private static PopupWindow SpawnPassengerVehiclePopup() {
            PopupWindow p = new PopupWindow {
                Id = _popupId++,
                Open = true,
                Position = new Rect(320, 75, 200, 100),
                Data = new WindowData()
            };
            SelectionManager.Manager.ActivateWindow(p.Id, p);
            p.RenderWindow = () => {
                p.Position = GUI.Window(p.Id, p.Position, delegate(int id) {
                    GUI.DragWindow(new Rect(0, 0, 180, 20));
                    if (GUI.Button(new Rect(180, 0, 20, 16), "X")) {
                        ClosePopup(p);
                        return;
                    }

                    GUI.Label(new Rect(15, 30, 20, 20), "Src.", p.Data.CurrentInput == 0 ? _focusedLabelStyle : _defaultLabelStyle);
                    p.Data.InputSrc = GUI.TextField(new Rect(40, 30, 60, 20), p.Data.InputSrc);
                    GUI.Label(new Rect(15, 50, 20, 20), "Dst.", p.Data.CurrentInput == 1 ? _focusedLabelStyle : _defaultLabelStyle);
                    p.Data.InputDst = GUI.TextField(new Rect(40, 50, 60, 20), p.Data.InputDst);
                    if (GUI.Button(new Rect(120, 30, 60, 40), "Spawn")) {
                        if (p.Data.ParseInputs(out ushort srcId, out ushort dstId)) {
                            Debug.Log("Spawning passenger vehicle " + srcId + ", " + dstId);
                            SpawnPassengerVehicle(srcId, dstId, p.Data.IsNode);
                        }
                    }

                    p.Data.IsNode = GUI.Toggle(new Rect(15, 70, 150, 20), p.Data.IsNode, "Target is Node");
                    if (Event.current.button == 0 && Event.current.type == EventType.mouseUp) {
                        SelectionManager.Manager.ActivateWindow(id, p);
                    }
                }, "Spawn Passenger Vehicle");
            };
            return p;
        }

        private static PopupWindow SpawnCargoVehiclePopup() {
            PopupWindow p = new PopupWindow {
                Id = _popupId++,
                Open = true,
                Position = new Rect(310, 55, 200, 90),
                Data = new WindowData()
            };
            SelectionManager.Manager.ActivateWindow(p.Id, p);
            p.RenderWindow = () => {
                p.Position = GUI.Window(p.Id, p.Position, delegate(int id) {
                    GUI.DragWindow(new Rect(0, 0, 180, 20));
                    if (GUI.Button(new Rect(180, 0, 20, 16), "X")) {
                        ClosePopup(p);
                        return;
                    }

                    GUI.Label(new Rect(15, 30, 20, 20), "Src.", p.Data.CurrentInput == 0 ? _focusedLabelStyle : _defaultLabelStyle);
                    p.Data.InputSrc = GUI.TextField(new Rect(40, 30, 60, 20), p.Data.InputSrc);
                    GUI.Label(new Rect(15, 50, 20, 20), "Dst.", p.Data.CurrentInput == 1 ? _focusedLabelStyle : _defaultLabelStyle);
                    p.Data.InputDst = GUI.TextField(new Rect(40, 50, 60, 20), p.Data.InputDst);
                    if (GUI.Button(new Rect(120, 30, 60, 40), "Spawn")) {
                        if (p.Data.ParseInputs(out ushort srcId, out ushort dstId)) {
                            Debug.Log("PfS: Spawning cargo vehicle " + srcId + ", " + dstId);
                            SpawnCargoVehicle(srcId, dstId);
                        }
                    }
                    if (Event.current.button == 0 && Event.current.type == EventType.mouseUp) {
                        SelectionManager.Manager.ActivateWindow(id, p);
                    }
                }, "Spawn Cargo Vehicle");
            };
            return p;
        }

        private static PopupWindow SpawnCimPopup() {
            PopupWindow p = new PopupWindow {
                Id = _popupId++,
                Open = true,
                Position = new Rect(300, 35, 200, 100),
                Data = new WindowData()
            };
            SelectionManager.Manager.ActivateWindow(p.Id, p);
            p.RenderWindow = () => {
                p.Position = GUI.Window(p.Id, p.Position, delegate(int id) {
                    GUI.DragWindow(new Rect(0, 0, 180, 20));
                    if (GUI.Button(new Rect(180, 0, 20, 16), "X")) {
                        ClosePopup(p);
                        return;
                    }

                    GUI.Label(new Rect(15, 30, 20, 20), "Src.");
                    p.Data.InputSrc = GUI.TextField(new Rect(40, 30, 60, 20), p.Data.InputSrc);
                    GUI.Label(new Rect(15, 50, 20, 20), "Dst.");
                    p.Data.InputDst = GUI.TextField(new Rect(40, 50, 60, 20), p.Data.InputDst);
                    if (GUI.Button(new Rect(120, 30, 60, 40), "Spawn")) {
                        if (p.Data.ParseInputs(out ushort srcId, out ushort dstId)) {
                            Debug.Log("PfS: Spawning citizen " + srcId + ", " + dstId);
                            SpawnCitizen(out uint citizenId);
                            if (citizenId != 0) {
                                StartMoving(citizenId, srcId, dstId);
                            }
                        }
                    }

                    p.Data.IsNode = GUI.Toggle(new Rect(15, 70, 150, 20), p.Data.IsNode, "Target is Node");
                    if (Event.current.button == 0 && Event.current.type == EventType.mouseUp) {
                        SelectionManager.Manager.ActivateWindow(id, p);
                    }
                }, "Spawn Cim");
            };
            return p;
        }

        private static void ClosePopup(PopupWindow p) {
            p.Open = false;
            SelectionManager.Manager.ActivateWindow(-1, null);
        }

        private static void SpawnCitizen(out uint citizenId) {
            CitizenManager.instance.CreateCitizen(out citizenId, 45, 0, ref SimulationManager.instance.m_randomizer);
        }

        private static void StartMoving(uint citizenId, ushort sourceBuildingId, ushort targetBuildingId) {
            CitizenInfo citizenInfo = CitizenManager.instance.m_citizens.m_buffer[citizenId].GetCitizenInfo(citizenId);
            Citizen citizen = CitizenManager.instance.m_citizens.m_buffer[citizenId];
            ResidentAI resident = citizenInfo.m_citizenAI as ResidentAI;
            if (resident.StartMoving(citizenId, ref citizen, sourceBuildingId, targetBuildingId)) {
                Debug.Log("PfS: Citizen is moving...");
                _citizens.Add(citizenId);
            }
        }

        private static void SpawnPassengerVehicle(ushort sourceBuildingId, ushort targetBuildingId, bool isNode) {
            Randomizer r = SimulationManager.instance.m_randomizer;
            VehicleInfo vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Residential, ItemClass.SubService.ResidentialLow, ItemClass.Level.Level1, VehicleInfo.VehicleType.Car);
            Vector3 position = BuildingManager.instance.m_buildings.m_buffer[sourceBuildingId].m_position;

            if (Singleton<VehicleManager>.instance.CreateVehicle(out ushort vehicleId, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, position, TransferManager.TransferReason.DummyCar, false, true)) {
                Debug.Log("PfS: Vehicle created " + vehicleId);
                _vehicles.Add(vehicleId);
                ref Vehicle vehicle = ref VehicleManager.instance.m_vehicles.m_buffer[vehicleId];
                vehicleInfo.m_vehicleAI.SetSource(vehicleId, ref vehicle, sourceBuildingId);
                vehicleInfo.m_vehicleAI.SetTarget(vehicleId, ref vehicle, targetBuildingId);

                SpawnCitizen(out uint citizenId);
                ref Citizen citizen = ref CitizenManager.instance.m_citizens.m_buffer[citizenId];
                CitizenInfo citizenInfo = CitizenManager.instance.m_citizens.m_buffer[citizenId].GetCitizenInfo(citizenId);
                if (CitizenManager.instance.CreateCitizenInstance(out ushort cimInstanceId, ref r, citizenInfo, citizenId)) {
                    citizen.SetVehicle(citizenId, vehicleId, 0);
                    Vector3 endPos = Vector3.zero;
                    if (isNode) {
                        NetManager netManager = Singleton<NetManager>.instance;
                        endPos = netManager.m_nodes.m_buffer[(int) targetBuildingId].m_position;
                    } else {
                        BuildingManager buildingManager = Singleton<BuildingManager>.instance;
                        endPos = buildingManager.m_buildings.m_buffer[targetBuildingId].m_position;
                    }

                    if (PassengerCarStartPathFind(vehicleInfo.m_vehicleAI as PassengerCarAI, vehicleId, ref vehicle, vehicle.m_targetPos3, endPos, true, true, false)) {
                        Debug.Log("PfS: PathFind started");
                        if (vehicleInfo.m_vehicleAI.TrySpawn(vehicleId, ref VehicleManager.instance.m_vehicles.m_buffer[vehicleId])) {
                            Debug.Log("PfS: Spawned " + vehicleId + " " + VehicleManager.instance.m_vehicles.m_buffer[vehicleId].m_flags);
                        }
                    }
                }
            }
        }

        private static void SpawnCargoVehicle(ushort sourceBuildingId, ushort targetBuildingId) {
            VehicleInfo vehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, ItemClass.Service.Industrial, ItemClass.SubService.IndustrialOre, ItemClass.Level.Level2);
            Vector3 position = Vector3.zero;

            if (Singleton<VehicleManager>.instance.CreateVehicle(out ushort vehicleId, ref Singleton<SimulationManager>.instance.m_randomizer, vehicleInfo, position, TransferManager.TransferReason.Ore, false, true)) {
                Debug.Log("PfS: Vehicle created " + vehicleId);
                _vehicles.Add(vehicleId);
                VehicleManager.instance.m_vehicles.m_buffer[vehicleId].m_flags |= Vehicle.Flags.DummyTraffic;
                VehicleManager.instance.m_vehicles.m_buffer[vehicleId].m_flags &= ~Vehicle.Flags.WaitingCargo;
                vehicleInfo.m_vehicleAI.SetSource(vehicleId, ref VehicleManager.instance.m_vehicles.m_buffer[vehicleId], sourceBuildingId);
                vehicleInfo.m_vehicleAI.SetTarget(vehicleId, ref VehicleManager.instance.m_vehicles.m_buffer[vehicleId], targetBuildingId);
            }
        }

        private static bool PassengerCarStartPathFind(PassengerCarAI ai, ushort vehicleId, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget) {
            MethodInfo methodInfo = typeof(PassengerCarAI).GetMethod("StartPathFind",
                BindingFlags.NonPublic | BindingFlags.Instance,
                Type.DefaultBinder,
                new[] {
                    typeof(ushort),
                    typeof(Vehicle).MakeByRefType(),
                    typeof(Vector3),
                    typeof(Vector3),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool)
                },
                null);
            PassengerCarAiStartPathFind startPathFind = (PassengerCarAiStartPathFind) Delegate.CreateDelegate(typeof(PassengerCarAiStartPathFind), ai, methodInfo);
            return (bool) startPathFind?.Invoke(vehicleId, ref vehicleData, startPos, endPos, startBothWays, endBothWays, undergroundTarget);
        }
    }

    public delegate void Window();

    public class PopupWindow {
        public int Id;
        public Window RenderWindow;
        public WindowData Data;
        public bool Open;
        public Rect Position;
    }

    public class WindowData {
        public string InputSrc = string.Empty;
        public string InputDst = string.Empty;
        public bool IsNode;
        public ushort CurrentInput;

        public bool ParseInputs(out ushort srcId, out ushort dstId) {
            srcId = 0;
            dstId = 0;
            if (!InputSrc.IsNullOrWhiteSpace() && !InputDst.IsNullOrWhiteSpace()) {
                if (ushort.TryParse(InputSrc, out srcId) && ushort.TryParse(InputDst, out dstId)) {
                    return true;
                }
            }

            return false;
        }

        public void UpdateInput(string value) {
            if (CurrentInput == 0) {
                InputSrc = value;
                FocusNextInput();
            } else {
                InputDst = value;
            }
        }

        public void FocusNextInput() {
            CurrentInput = CurrentInput == 0 ? (ushort) 1 : (ushort) 0;
        }

        public void FocusPreviousInput() {
            CurrentInput = CurrentInput == 0 ? (ushort) 1: (ushort) 0;
        }
    }

    public delegate bool PassengerCarAiStartPathFind(ushort vehicleId, ref Vehicle vehicleData, Vector3 startPos, Vector3 endPos, bool startBothWays, bool endBothWays, bool undergroundTarget);
}