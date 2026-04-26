using System;
using System.Linq;
using ColossalFramework;
using ColossalFramework.UI;
using UnityEngine;
using System.IO;
using Mapper.OSM;

namespace Mapper
{
    public class MapperWindow7 : UIPanel
    {
        UILabel title;

        UITextField pathTextBox;
        UILabel pathTextBoxLabel;
        UIButton loadMapButton;

        UITextField scaleTextBox;
        UILabel scaleTextBoxLabel;

        UITextField tolerance;
        UILabel toleranceLabel;

        UITextField curveTolerance;
        UILabel curveToleranceLabel;

        UITextField tiles;
        UILabel tilesLabel;

        UILabel errorLabel;

        UIButton okButton;

        UICustomCheckbox3 motorwaysCheck;
        UILabel motorwaysLabel;
        UICustomCheckbox3 majorRoadsCheck;
        UILabel majorRoadsLabel;
        UICustomCheckbox3 minorRoadsCheck;
        UILabel minorRoadsLabel;
        UICustomCheckbox3 residentialCheck;
        UILabel residentialLabel;
        UICustomCheckbox3 serviceCheck;
        UILabel serviceLabel;
        UICustomCheckbox3 railCheck;
        UILabel railLabel;
        UICustomCheckbox3 pedestrianCheck;
        UILabel pedestrianLabel2;

        public ICities.LoadMode mode;
        RoadMaker2 roadMaker;
        bool createRoads;
        Way[] runWays;
        int runIndex;
        int frameCooldown;

        const int DenseBatchSize = 10;
        const int DenseFrameGap  = 5;

        public override void Awake()
        {
            this.isInteractive = true;
            this.enabled = true;
            
            width = 500;

            title = AddUIComponent<UILabel>();

            pathTextBox = AddUIComponent<UITextField>();
            pathTextBoxLabel = AddUIComponent<UILabel>();
            loadMapButton = AddUIComponent<UIButton>();

            scaleTextBox = AddUIComponent<UITextField>();
            scaleTextBoxLabel = AddUIComponent<UILabel>();


            tolerance = AddUIComponent<UITextField>();
            toleranceLabel = AddUIComponent<UILabel>();

            curveTolerance = AddUIComponent<UITextField>();
            curveToleranceLabel = AddUIComponent<UILabel>();

            tiles = AddUIComponent<UITextField>();
            tilesLabel = AddUIComponent<UILabel>();

            errorLabel = AddUIComponent<UILabel>();

            motorwaysCheck = AddUIComponent<UICustomCheckbox3>();
            motorwaysLabel = AddUIComponent<UILabel>();
            majorRoadsCheck = AddUIComponent<UICustomCheckbox3>();
            majorRoadsLabel = AddUIComponent<UILabel>();
            minorRoadsCheck = AddUIComponent<UICustomCheckbox3>();
            minorRoadsLabel = AddUIComponent<UILabel>();
            residentialCheck = AddUIComponent<UICustomCheckbox3>();
            residentialLabel = AddUIComponent<UILabel>();
            serviceCheck = AddUIComponent<UICustomCheckbox3>();
            serviceLabel = AddUIComponent<UILabel>();
            railCheck = AddUIComponent<UICustomCheckbox3>();
            railLabel = AddUIComponent<UILabel>();
            pedestrianCheck = AddUIComponent<UICustomCheckbox3>();
            pedestrianLabel2 = AddUIComponent<UILabel>();

            okButton = AddUIComponent<UIButton>();

            base.Awake();

        }
        public override void Start()
        {
            base.Start();

            relativePosition = new Vector3(396, 58);
            backgroundSprite = "MenuPanel2";
            isInteractive = true;
            SetupControls();
        }

        public void SetupControls()
        {
            

            title.text = "Open Street Map Import";
            title.relativePosition = new Vector3(15, 15);
            title.textScale = 0.9f;
            title.size = new Vector2(200, 30);
            var vertPadding = 30;
            var x = 15;
            var y = 50;

            x = 15;
            y += vertPadding;

            SetLabel(scaleTextBoxLabel, "Scale", x, y);
            SetTextBox(scaleTextBox, "1", x + 120, y);
            y += vertPadding;


            SetLabel(toleranceLabel, "Tolerance", x, y);
            SetTextBox(tolerance, "6", x + 120, y);
            y += vertPadding;

            SetLabel(curveToleranceLabel, "Curve Tolerance", x, y);
            SetTextBox(curveTolerance, "6", x + 120, y);
            y += vertPadding;

            SetLabel(tilesLabel, "Tiles to Boundary", x, y);
            SetTextBox(tiles, "4.5", x + 120, y);
            y += vertPadding + 12;

            SetLabel(pathTextBoxLabel, "Path", x, y);
            SetTextBox(pathTextBox, Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "map"), x + 120, y);
            y += vertPadding - 5;
            SetButton(loadMapButton, "Load OSM From File", y);
            loadMapButton.eventClick += LoadMapButton_eventClick;
            y += vertPadding + 5;

            SetLabel(errorLabel, "No OSM data loaded.", x, y);
            errorLabel.textScale = 0.6f;
            y += vertPadding + 12;

            SetCheckbox(motorwaysCheck,  motorwaysLabel,  "Motorways",   x,       y);
            SetCheckbox(majorRoadsCheck, majorRoadsLabel, "Major Roads", x + 155, y);
            SetCheckbox(minorRoadsCheck, minorRoadsLabel, "Minor Roads", x + 310, y);
            y += vertPadding - 2;
            SetCheckbox(residentialCheck, residentialLabel, "Residential", x,       y);
            SetCheckbox(serviceCheck,     serviceLabel,     "Service",     x + 155, y);
            SetCheckbox(pedestrianCheck,  pedestrianLabel2, "Pedestrian",  x + 310, y);
            y += vertPadding - 2;
            SetCheckbox(railCheck,        railLabel,        "Rail",        x,       y);
            y += vertPadding + 5;

            SetButton(okButton, "Make Roads", y);
            okButton.eventClick += OkButton_eventClick;
            okButton.Disable();
            y += vertPadding;


            height = y + vertPadding + 6;
        }

        private void LoadMapButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            var path = pathTextBox.text.Trim();
            if (!File.Exists(path))
            {
                path += ".osm";
                if (!File.Exists(path))
                {
                    errorLabel.text = "Cannot find osm file: " + path;
                    return;
                }
            }
            try
            {
                var osm = new OSMInterface(path, double.Parse(scaleTextBox.text.Trim()), double.Parse(tolerance.text.Trim()), double.Parse(curveTolerance.text.Trim()), double.Parse(tiles.text.Trim()));
                runWays = null;
                runIndex = 0;
                roadMaker = new RoadMaker2(osm);
                errorLabel.text = "File Loaded.";
                okButton.Enable();
                loadMapButton.Disable();
            }
            catch (Exception ex)
            {
                errorLabel.text = ex.ToString();
            }
        }

        private void SetButton(UIButton okButton, string p1,int x, int y)
        {
            okButton.text = p1;
            okButton.normalBgSprite = "ButtonMenu";
            okButton.hoveredBgSprite = "ButtonMenuHovered";
            okButton.disabledBgSprite = "ButtonMenuDisabled";
            okButton.focusedBgSprite = "ButtonMenuFocused";
            okButton.pressedBgSprite = "ButtonMenuPressed";
            okButton.size = new Vector2(50, 18);
            okButton.relativePosition = new Vector3(x, y - 3);
            okButton.textScale = 0.8f;
        }

        private void SetButton(UIButton okButton, string p1, int y)
        {
            okButton.text = p1;
            okButton.normalBgSprite = "ButtonMenu";
            okButton.hoveredBgSprite = "ButtonMenuHovered";
            okButton.disabledBgSprite = "ButtonMenuDisabled";
            okButton.focusedBgSprite = "ButtonMenuFocused";
            okButton.pressedBgSprite = "ButtonMenuPressed";
            okButton.size = new Vector2(260, 24);
            okButton.relativePosition = new Vector3((int)(width - okButton.size.x) / 2,y);
            okButton.textScale = 0.8f;

        }

        private void SetCheckbox(UICustomCheckbox3 cb, UILabel label, string text, int x, int y)
        {
            cb.size = new Vector2(16, 16);
            cb.relativePosition = new Vector3(x, y + 2);
            cb.IsChecked = false;
            cb.eventClick += (c, p) => { cb.IsChecked = !cb.IsChecked; };
            label.text = text;
            label.textScale = 0.8f;
            label.size = new Vector2(130, 20);
            label.relativePosition = new Vector3(x + 20, y);
        }

        private void SetTextBox(UITextField scaleTextBox, string p, int x, int y)
        {
            scaleTextBox.relativePosition = new Vector3(x, y - 4);
            scaleTextBox.horizontalAlignment = UIHorizontalAlignment.Left;
            scaleTextBox.text = p;
            scaleTextBox.textScale = 0.8f;
            scaleTextBox.color = Color.black;
            scaleTextBox.cursorBlinkTime = 0.45f;
            scaleTextBox.cursorWidth = 1;
            scaleTextBox.selectionBackgroundColor = new Color(233,201,148,255);
            scaleTextBox.selectionSprite = "EmptySprite";
            scaleTextBox.verticalAlignment = UIVerticalAlignment.Middle;
            scaleTextBox.padding = new RectOffset(5, 0, 5, 0);
            scaleTextBox.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            scaleTextBox.normalBgSprite = "TextFieldPanel";
            scaleTextBox.hoveredBgSprite = "TextFieldPanelHovered";
            scaleTextBox.focusedBgSprite = "TextFieldPanel";
            scaleTextBox.size = new Vector3(width - 120 - 30, 20);
            scaleTextBox.isInteractive = true;
            scaleTextBox.enabled = true;
            scaleTextBox.readOnly = false;
            scaleTextBox.builtinKeyNavigation = true;
            
        }

        private void SetLabel(UILabel pedestrianLabel, string p, int x, int y)
        {
            pedestrianLabel.relativePosition = new Vector3(x, y);
            pedestrianLabel.text = p;
            pedestrianLabel.textScale = 0.8f;
            pedestrianLabel.size = new Vector3(120,20);
        }

        private bool IsRoadTypeEnabled(RoadTypes rt)
        {
            switch (rt)
            {
                case RoadTypes.Highway:
                case RoadTypes.HighwayBridge:
                case RoadTypes.HighwayElevated:
                case RoadTypes.HighwayRamp:
                case RoadTypes.HighwayRampElevated:
                case RoadTypes.HighwayBarrier:
                    return motorwaysCheck.IsChecked;

                case RoadTypes.LargeRoad:
                case RoadTypes.LargeRoadDecorationGrass:
                case RoadTypes.LargeRoadDecorationTrees:
                case RoadTypes.LargeRoadBridge:
                case RoadTypes.LargeRoadElevated:
                case RoadTypes.LargeOneway:
                case RoadTypes.LargeOnewayDecorationGrass:
                case RoadTypes.LargeOnewayDecorationTrees:
                case RoadTypes.LargeOnewayBridge:
                case RoadTypes.LargeOnewayElevated:
                case RoadTypes.MediumRoad:
                case RoadTypes.MediumRoadDecorationGrass:
                case RoadTypes.MediumRoadDecorationTrees:
                case RoadTypes.MediumRoadBridge:
                case RoadTypes.MediumRoadElevated:
                    return majorRoadsCheck.IsChecked;

                case RoadTypes.BasicRoad:
                case RoadTypes.BasicRoadDecorationTrees:
                case RoadTypes.BasicRoadDecorationGrass:
                case RoadTypes.BasicRoadBridge:
                case RoadTypes.BasicRoadElevated:
                case RoadTypes.OnewayRoad:
                case RoadTypes.OnewayRoadDecorationTrees:
                case RoadTypes.OnewayRoadDecorationGrass:
                case RoadTypes.OnewayRoadElevated:
                case RoadTypes.OnewayRoadBridge:
                    return minorRoadsCheck.IsChecked;

                case RoadTypes.ResidentialRoad:
                    return residentialCheck.IsChecked;

                case RoadTypes.ServiceRoad:
                case RoadTypes.GravelRoad:
                    return serviceCheck.IsChecked;

                case RoadTypes.TrainTrack:
                case RoadTypes.TrainTrackBridge:
                case RoadTypes.TrainTrackElevated:
                case RoadTypes.TrainConnectionTrack:
                case RoadTypes.TrainOnewayTrack:
                case RoadTypes.TrainOnewayTrackBridge:
                case RoadTypes.TrainOnewayTrackElevated:
                case RoadTypes.TrainCargoTrack:
                case RoadTypes.MetroTrack:
                case RoadTypes.BusLine:
                case RoadTypes.MetroLine:
                case RoadTypes.TrainLine:
                case RoadTypes.AirplaneTaxiway:
                case RoadTypes.Dam:
                    return railCheck.IsChecked;

                case RoadTypes.PedestrianGravel:
                case RoadTypes.PedestrianPavement:
                case RoadTypes.PedestrianElevated:
                    return pedestrianCheck.IsChecked;

                default:
                    return false;
            }
        }

        private void OkButton_eventClick(UIComponent component, UIMouseEventParameter eventParam)
        {
            if (roadMaker != null)
            {
                if (!createRoads)
                {
                    var filtered = new System.Collections.Generic.List<Way>();
                    foreach (var way in roadMaker.osm.ways)
                    {
                        if (IsRoadTypeEnabled(way.roadTypes))
                            filtered.Add(way);
                    }
                    runWays = filtered.ToArray();
                    runIndex = 0;
                }
                createRoads = !createRoads;
            }
        }

        private static bool IsDenseRoadType(RoadTypes rt)
        {
            switch (rt)
            {
                case RoadTypes.BasicRoad:
                case RoadTypes.BasicRoadDecorationTrees:
                case RoadTypes.BasicRoadDecorationGrass:
                case RoadTypes.BasicRoadBridge:
                case RoadTypes.BasicRoadElevated:
                case RoadTypes.OnewayRoad:
                case RoadTypes.OnewayRoadDecorationTrees:
                case RoadTypes.OnewayRoadDecorationGrass:
                case RoadTypes.OnewayRoadElevated:
                case RoadTypes.OnewayRoadBridge:
                case RoadTypes.GravelRoad:
                case RoadTypes.ResidentialRoad:
                case RoadTypes.ServiceRoad:
                    return true;
                default:
                    return false;
            }
        }

        public override void Update()
        {
            if (createRoads && runWays != null)
            {
                if (runIndex < runWays.Length)
                {
                    if (frameCooldown > 0)
                    {
                        frameCooldown--;
                    }
                    else
                    {
                        bool dense = IsDenseRoadType(runWays[runIndex].roadTypes);
                        int count = dense ? DenseBatchSize : 1;
                        for (int i = 0; i < count && runIndex < runWays.Length; i++)
                        {
                            SimulationManager.instance.AddAction(roadMaker.MakeRoad(runWays[runIndex]));
                            runIndex++;
                        }
                        if (dense)
                            frameCooldown = DenseFrameGap;
                    }

                    var nm = Singleton<NetManager>.instance;
                    errorLabel.text = String.Format("{0}/{1} — Nodes: {2}/32768  Segs: {3}/36864",
                        runIndex, runWays.Length,
                        nm.m_nodeCount, nm.m_segmentCount);
                }
                else
                {
                    errorLabel.text = "Done.";
                    createRoads = false;
                }
            }
            base.Update();
        }
    }

    public class UICustomCheckbox3 : UISprite
    {
        public bool IsChecked { get; set; }

        public override void Start()
        {
            base.Start();
            IsChecked = false;
            foreach (var a in Resources.FindObjectsOfTypeAll<UITextureAtlas>())
            {
                if (a["AchievementCheckedTrue"] != null)
                {
                    atlas = a;
                    break;
                }
            }
            spriteName = "AchievementCheckedFalse";
        }

        public override void Update()
        {
            base.Update();
            spriteName = IsChecked ? "AchievementCheckedTrue" : "AchievementCheckedFalse";
        }
    }

}
