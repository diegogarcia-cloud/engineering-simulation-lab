using EngineeringSimulationLab.Core;
using EngineeringSimulationLab.Simulations.Common;
using EngineeringSimulationLab.Simulations.Gravity;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem.UI;
#endif

namespace EngineeringSimulationLab.UI
{
    public sealed class DemoLauncherUI : MonoBehaviour
    {
        private static readonly Color PanelColor = new Color(0.01f, 0.018f, 0.027f, 0.88f);
        private static readonly Color PanelBorderColor = new Color(0.18f, 0.35f, 0.46f, 0.55f);
        private static readonly Color ButtonColor = new Color(0.07f, 0.12f, 0.16f, 0.96f);
        private static readonly Color AccentColor = new Color(0.06f, 0.55f, 1f, 1f);
        private static readonly Color MutedTextColor = new Color(0.63f, 0.7f, 0.78f, 1f);

        private SimulationDemoRunner runner;
        private Text pausedText;
        private Text timeScaleText;
        private Text bodyCountText;
        private Text simulationTimeText;
        private Text spawnModeText;
        private Text statusText;
        private InputField velocityXInput;
        private InputField velocityYInput;
        private GravityStatsPanel statsPanel;
        private BodySpawnerPanel spawnerPanel;
        private BodySelectionController selectionController;
        private GravityBodyLabelOverlay labelOverlay;

        public void Initialize(SimulationDemoRunner demoRunner)
        {
            runner = demoRunner;
            BuildEventSystem();
            BuildCanvas();
            BuildSelectionController();
        }

        public void SetActiveDemo(string demoName)
        {
        }

        public void SetPaused(bool paused)
        {
            if (pausedText != null)
            {
                pausedText.text = paused ? "Resume" : "Pause";
            }

            if (statusText != null)
            {
                statusText.text = paused ? "PAUSED" : "RUNNING";
                statusText.color = paused ? new Color(1f, 0.76f, 0.32f) : new Color(0.28f, 1f, 0.42f);
            }
        }

        public void SetTimeScaleLabel(double secondsPerSecond)
        {
            if (timeScaleText != null)
            {
                timeScaleText.text = $"{secondsPerSecond / 86_400d:0.#} days/s";
            }
        }

        public void RefreshReadouts()
        {
            GravitySimulationController gravityDemo = runner.GetGravityDemo();
            if (gravityDemo == null)
            {
                return;
            }

            bodyCountText.text = $"Bodies: {gravityDemo.Bodies.Count}";
            simulationTimeText.text = $"Simulation Time: {UnitFormatter.Time(gravityDemo.SimulationTimeSeconds)}";
            spawnModeText.text = selectionController != null && selectionController.ChooseSpawnPointMode
                ? "Click the space map to set spawn position"
                : "Click a body to inspect live values";
            statsPanel?.Refresh(gravityDemo);
            spawnerPanel?.RefreshSpawnPoint();
            labelOverlay?.Refresh();
        }

        private void BuildCanvas()
        {
            GameObject canvasObject = new GameObject("Gravity Simulator Canvas");
            canvasObject.transform.SetParent(transform);

            Canvas canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1600f, 900f);
            scaler.matchWidthOrHeight = 0.5f;

            canvasObject.AddComponent<GraphicRaycaster>();

            RectTransform root = canvas.GetComponent<RectTransform>();
            BuildTopBar(root);
            BuildLeftControls(root);
            BuildRightPanels(root);
            BuildBottomStatus(root);
            labelOverlay = new GravityBodyLabelOverlay(runner.GetGravityDemo(), runner.SimulationCamera, root);
        }

        private void BuildTopBar(RectTransform root)
        {
            RectTransform title = CreatePanel("Title", root, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(18f, -18f), new Vector2(390f, 58f));
            HorizontalLayoutGroup titleLayout = title.gameObject.AddComponent<HorizontalLayoutGroup>();
            titleLayout.padding = new RectOffset(14, 14, 9, 9);
            titleLayout.spacing = 12f;
            titleLayout.childAlignment = TextAnchor.MiddleLeft;

            Text icon = AddLabel(title, "G", 26, FontStyle.Bold, AccentColor);
            icon.alignment = TextAnchor.MiddleCenter;
            icon.GetComponent<LayoutElement>().preferredWidth = 42f;
            Text titleText = AddLabel(title, "Engineering Simulation Lab\nGravity Simulator", 18, FontStyle.Bold);
            titleText.lineSpacing = 0.78f;
            titleText.GetComponent<LayoutElement>().preferredHeight = 44f;

            RectTransform time = CreatePanel("Simulation Time Badge", root, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(0f, -18f), new Vector2(360f, 48f));
            HorizontalLayoutGroup timeLayout = time.gameObject.AddComponent<HorizontalLayoutGroup>();
            timeLayout.padding = new RectOffset(16, 16, 8, 8);
            timeLayout.childAlignment = TextAnchor.MiddleCenter;
            simulationTimeText = AddLabel(time, "Simulation Time: 0 seconds", 15, FontStyle.Bold);
            simulationTimeText.alignment = TextAnchor.MiddleCenter;
        }

        private void BuildLeftControls(RectTransform root)
        {
            RectTransform column = CreateEmptyRect("Left Column", root);
            column.anchorMin = new Vector2(0f, 0.5f);
            column.anchorMax = new Vector2(0f, 0.5f);
            column.pivot = new Vector2(0f, 0.5f);
            column.anchoredPosition = new Vector2(16f, 5f);
            column.sizeDelta = new Vector2(270f, 760f);
            VerticalLayoutGroup layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            RectTransform simulation = CreatePanel("Simulation Controls", column, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(270f, 390f));
            AddPanelLayout(simulation, 14, 7);
            AddSectionHeader(simulation, "SIMULATION CONTROLS");
            Button pauseButton = AddButton(simulation, "Pause", () => runner.TogglePaused(), true);
            pausedText = pauseButton.GetComponentInChildren<Text>();
            AddButton(simulation, "Step 1 hour", () => runner.StepActiveDemo(), false);
            AddButton(simulation, "Reset Solar System", () => runner.ResetActiveDemo(), false);
            AddRowLabel(simulation, "Time Scale", "20 days/s", out timeScaleText);
            Slider timeSlider = AddSlider(simulation, 0.1f, 365f, (float)(runner.TimeScaleSecondsPerSecond / 86_400d));
            timeSlider.onValueChanged.AddListener(runner.SetTimeScale);
            AddScaleLabels(simulation, "0.1", "1", "30", "365 days/s");
            Toggle trailsToggle = AddToggle(simulation, "Show Trails", true);
            trailsToggle.onValueChanged.AddListener(value => runner.GetGravityDemo().SetShowTrails(value));
            Toggle vectorsToggle = AddToggle(simulation, "Show Vectors (Selected)", true);
            vectorsToggle.onValueChanged.AddListener(value => runner.GetGravityDemo().SetShowVectors(value));
            AddRowLabel(simulation, "Trail Length", runner.GetGravityDemo().TrailLength.ToString(), out _);
            Slider trailSlider = AddSlider(simulation, 60f, 2400f, runner.GetGravityDemo().TrailLength);
            trailSlider.onValueChanged.AddListener(value => runner.GetGravityDemo().SetTrailLength(value));

            RectTransform display = CreatePanel("Display Options", column, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(270f, 225f));
            AddPanelLayout(display, 14, 7);
            AddSectionHeader(display, "DISPLAY OPTIONS");
            Toggle labelsToggle = AddToggle(display, "Planet Labels", true);
            labelsToggle.onValueChanged.AddListener(value => labelOverlay?.SetVisible(value));
            AddRowLabel(display, "Vector Scale", $"{runner.GetGravityDemo().VectorScale:0.0}x", out Text vectorValue);
            Slider vectorSlider = AddSlider(display, 0.1f, 8f, runner.GetGravityDemo().VectorScale);
            vectorSlider.onValueChanged.AddListener(value =>
            {
                runner.GetGravityDemo().SetVectorScale(value);
                vectorValue.text = $"{value:0.0}x";
            });
            AddScaleLabels(display, "0.1x", "", "", "8x");

            RectTransform tip = CreatePanel("Tip", column, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(270f, 118f));
            AddPanelLayout(tip, 14, 8);
            AddSectionHeader(tip, "TIP");
            AddLabel(tip, "Middle-drag to pan. Scroll to zoom.\nClick a body to select it.\nValues update in real time.", 13, FontStyle.Normal, MutedTextColor).GetComponent<LayoutElement>().preferredHeight = 58f;
        }

        private void BuildRightPanels(RectTransform root)
        {
            RectTransform column = CreateEmptyRect("Right Column", root);
            column.anchorMin = new Vector2(1f, 0.5f);
            column.anchorMax = new Vector2(1f, 0.5f);
            column.pivot = new Vector2(1f, 0.5f);
            column.anchoredPosition = new Vector2(-16f, 0f);
            column.sizeDelta = new Vector2(380f, 770f);
            VerticalLayoutGroup layout = column.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.spacing = 10f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            RectTransform statsPanelRect = CreatePanel("Selected Body Panel", column, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(380f, 360f));
            AddPanelLayout(statsPanelRect, 14, 6);
            Text statsHeader = AddSectionHeader(statsPanelRect, "SELECTED BODY: EARTH");
            Text stats = AddLabel(statsPanelRect, "Selected: Earth", 11, FontStyle.Normal);
            stats.alignment = TextAnchor.UpperLeft;
            stats.supportRichText = true;
            stats.GetComponent<LayoutElement>().preferredHeight = 306f;
            statsPanel = new GravityStatsPanel(statsHeader, stats);

            RectTransform equations = CreatePanel("Equations", column, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(380f, 140f));
            AddPanelLayout(equations, 14, 6);
            AddSectionHeader(equations, "EQUATIONS");
            AddEquationGlyphLabel(equations,
                "F = G · (m₁ · m₂) / r²\n" +
                "a = F / m\n" +
                "v_circ = √(G · M☉ / r)\n" +
                "G = 6.67430 × 10⁻¹¹  N·m²/kg²");

            RectTransform spawn = CreatePanel("Spawn New Body", column, Vector2.zero, Vector2.zero, Vector2.zero, new Vector2(380f, 255f));
            AddPanelLayout(spawn, 14, 4);
            AddSectionHeader(spawn, "SPAWN NEW BODY");
            InputField nameInput = AddInput(spawn, "Name", "New Body");
            InputField massInput = AddInput(spawn, "Mass kg", "5.97237e24");
            InputField radiusInput = AddInput(spawn, "Visual size", "0.16");
            Dropdown presetDropdown = AddDropdown(spawn, "Preset", "Custom", "Earth mass", "Moon mass", "Small asteroid", "Jupiter mass");
            velocityXInput = CreateHiddenInput(spawn, "0");
            velocityYInput = CreateHiddenInput(spawn, "0");
            Toggle autoOrbitToggle = AddToggle(spawn, "Auto Orbit Sun", true);
            spawnModeText = AddLabel(spawn, "Click a body to inspect live values", 11, FontStyle.Normal, MutedTextColor);
            Text spawnPointText = AddLabel(spawn, "Spawn point", 10, FontStyle.Normal, MutedTextColor);
            spawnerPanel = new BodySpawnerPanel(runner.GetGravityDemo(), nameInput, massInput, radiusInput, velocityXInput, velocityYInput, autoOrbitToggle, presetDropdown, spawnPointText);
            presetDropdown.onValueChanged.AddListener(spawnerPanel.ApplyMassPreset);
            RectTransform buttonRow = CreateEmptyRect("Spawn Buttons", spawn);
            buttonRow.sizeDelta = new Vector2(0f, 34f);
            buttonRow.gameObject.AddComponent<LayoutElement>().preferredHeight = 34f;
            HorizontalLayoutGroup rowLayout = buttonRow.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.spacing = 8f;
            rowLayout.childControlWidth = true;
            rowLayout.childForceExpandWidth = true;
            AddButton(buttonRow, "Choose Point", () => selectionController?.BeginChooseSpawnPoint(), false);
            AddButton(buttonRow, "Add Body", () =>
            {
                spawnerPanel.AddBody();
                RefreshReadouts();
            }, true);
        }

        private void BuildBottomStatus(RectTransform root)
        {
            RectTransform bar = CreatePanel("Bottom Status", root, new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 48f));
            bar.pivot = new Vector2(0.5f, 0f);
            HorizontalLayoutGroup layout = bar.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(26, 26, 10, 10);
            layout.spacing = 24f;
            layout.childAlignment = TextAnchor.MiddleLeft;

            AddLabel(bar, "GRAVITY SIMULATOR v1.0.0", 12, FontStyle.Bold, MutedTextColor);
            AddLabel(bar, "2D SANDBOX MODE", 12, FontStyle.Bold, MutedTextColor);
            AddLabel(bar, "SOLAR SYSTEM", 12, FontStyle.Bold, MutedTextColor);
            AddFlexibleSpace(bar);
            bodyCountText = AddLabel(bar, "Bodies: 9", 12, FontStyle.Bold, MutedTextColor);
            AddLabel(bar, "FPS: 60", 12, FontStyle.Bold, MutedTextColor);
            statusText = AddLabel(bar, "RUNNING", 12, FontStyle.Bold, new Color(0.28f, 1f, 0.42f));
        }

        private void BuildSelectionController()
        {
            selectionController = runner.GetGravityDemo().gameObject.AddComponent<BodySelectionController>();
            selectionController.Initialize(runner.GetGravityDemo(), runner.SimulationCamera, RefreshReadouts);
        }

        private static RectTransform CreatePanel(string name, RectTransform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 anchoredPosition, Vector2 size)
        {
            GameObject panelObject = new GameObject(name);
            panelObject.transform.SetParent(parent, false);
            Image image = panelObject.AddComponent<Image>();
            image.color = PanelColor;

            Outline outline = panelObject.AddComponent<Outline>();
            outline.effectColor = PanelBorderColor;
            outline.effectDistance = new Vector2(1f, -1f);

            RectTransform rect = panelObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = anchorMin;
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            if (parent.GetComponent<LayoutGroup>() != null)
            {
                LayoutElement layout = panelObject.AddComponent<LayoutElement>();
                layout.preferredWidth = size.x;
                layout.preferredHeight = size.y;
            }
            return rect;
        }

        private static void AddPanelLayout(RectTransform panel, int padding, float spacing)
        {
            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(padding, padding, padding, padding);
            layout.spacing = spacing;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;
        }

        private static Text AddSectionHeader(Transform parent, string text)
        {
            Text label = AddLabel(parent, text, 14, FontStyle.Bold, AccentColor);
            label.GetComponent<LayoutElement>().preferredHeight = 24f;
            return label;
        }

        private static Text AddLabel(Transform parent, string text, int size, FontStyle style)
        {
            return AddLabel(parent, text, size, style, new Color(0.9f, 0.94f, 1f));
        }

        private static Text AddEquationGlyphLabel(Transform parent, string glyphText)
        {
            GameObject labelObject = new GameObject("Equations Glyph");
            labelObject.transform.SetParent(parent, false);
            Text label = labelObject.AddComponent<Text>();
            Font mono = Font.CreateDynamicFontFromOSFont(new[] { "Consolas", "Courier New", "Menlo", "Liberation Mono" }, 14);
            label.font = mono ?? Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 14;
            label.fontStyle = FontStyle.Bold;
            label.color = new Color(0.9f, 0.94f, 1f);
            label.alignment = TextAnchor.MiddleCenter;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.supportRichText = true;
            label.lineSpacing = 1.15f;
            label.raycastTarget = false;
            label.text = glyphText;

            LayoutElement layout = labelObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 92f;
            return label;
        }

        private static Text AddLabel(Transform parent, string text, int size, FontStyle style, Color color)
        {
            GameObject labelObject = new GameObject(text);
            labelObject.transform.SetParent(parent, false);
            Text label = labelObject.AddComponent<Text>();
            label.text = text;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = size;
            label.fontStyle = style;
            label.color = color;
            label.alignment = TextAnchor.MiddleLeft;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;

            LayoutElement layout = labelObject.AddComponent<LayoutElement>();
            layout.preferredHeight = Mathf.Max(22f, size + 8f);
            return label;
        }

        private static void AddRowLabel(Transform parent, string left, string right, out Text rightText)
        {
            RectTransform row = CreateEmptyRect(left + " Row", parent);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 24f;
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            AddLabel(row, left, 12, FontStyle.Normal, MutedTextColor);
            rightText = AddLabel(row, right, 12, FontStyle.Bold, new Color(0.52f, 0.78f, 1f));
            rightText.alignment = TextAnchor.MiddleRight;
        }

        private static Button AddButton(Transform parent, string label, UnityEngine.Events.UnityAction action, bool primary)
        {
            GameObject buttonObject = new GameObject(label + " Button");
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.AddComponent<Image>();
            image.color = primary ? new Color(0.04f, 0.36f, 0.68f, 0.98f) : ButtonColor;
            Outline outline = buttonObject.AddComponent<Outline>();
            outline.effectColor = new Color(0.25f, 0.48f, 0.62f, 0.35f);
            outline.effectDistance = new Vector2(1f, -1f);

            Button button = buttonObject.AddComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = image.color;
            colors.highlightedColor = AccentColor;
            colors.pressedColor = new Color(0.03f, 0.25f, 0.46f);
            button.colors = colors;
            button.onClick.AddListener(action);

            LayoutElement layout = buttonObject.AddComponent<LayoutElement>();
            layout.preferredHeight = 34f;

            Text text = AddLabel(buttonObject.transform, label, 13, FontStyle.Bold);
            text.alignment = TextAnchor.MiddleCenter;
            Stretch(text.GetComponent<RectTransform>(), 0f, 0f);
            return button;
        }

        private static InputField AddInput(Transform parent, string label, string value)
        {
            RectTransform row = CreateEmptyRect(label + " Row", parent);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 28f;
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;

            Text labelText = AddLabel(row, label, 11, FontStyle.Normal, MutedTextColor);
            labelText.GetComponent<LayoutElement>().preferredWidth = 105f;

            GameObject inputObject = new GameObject(label + " Input");
            inputObject.transform.SetParent(row, false);
            Image image = inputObject.AddComponent<Image>();
            image.color = new Color(0.045f, 0.075f, 0.095f, 0.96f);
            InputField input = inputObject.AddComponent<InputField>();

            Text text = AddLabel(inputObject.transform, value, 12, FontStyle.Normal);
            text.color = Color.white;
            Stretch(text.GetComponent<RectTransform>(), 8f, 2f);
            input.textComponent = text;
            input.text = value;
            inputObject.AddComponent<LayoutElement>().preferredHeight = 28f;
            return input;
        }

        private static InputField CreateHiddenInput(Transform parent, string value)
        {
            GameObject inputObject = new GameObject("Hidden Input");
            inputObject.transform.SetParent(parent, false);
            inputObject.SetActive(false);
            InputField input = inputObject.AddComponent<InputField>();
            Text text = inputObject.AddComponent<Text>();
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            input.textComponent = text;
            input.text = value;
            return input;
        }

        private static Toggle AddToggle(Transform parent, string label, bool value)
        {
            GameObject toggleObject = new GameObject(label + " Toggle");
            toggleObject.transform.SetParent(parent, false);
            Toggle toggle = toggleObject.AddComponent<Toggle>();
            toggleObject.AddComponent<LayoutElement>().preferredHeight = 28f;

            RectTransform box = CreateSliderPart("Track", toggleObject.transform, new Color(0.08f, 0.12f, 0.15f), new Vector2(1f, 0.5f), new Vector2(1f, 0.5f));
            box.sizeDelta = new Vector2(34f, 18f);
            box.anchoredPosition = new Vector2(-17f, 0f);
            RectTransform check = CreateSliderPart("On", box, AccentColor, Vector2.zero, Vector2.one);
            check.offsetMin = new Vector2(4f, 4f);
            check.offsetMax = new Vector2(-4f, -4f);
            Text text = AddLabel(toggleObject.transform, label, 12, FontStyle.Normal, MutedTextColor);
            Stretch(text.GetComponent<RectTransform>(), 0f, 0f);
            text.GetComponent<RectTransform>().offsetMax = new Vector2(-44f, 0f);
            toggle.targetGraphic = box.GetComponent<Image>();
            toggle.graphic = check.GetComponent<Image>();
            toggle.isOn = value;
            return toggle;
        }

        private static Dropdown AddDropdown(Transform parent, string label, params string[] options)
        {
            RectTransform row = CreateEmptyRect(label + " Row", parent);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 28f;
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 8f;
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;

            Text labelText = AddLabel(row, label, 11, FontStyle.Normal, MutedTextColor);
            labelText.GetComponent<LayoutElement>().preferredWidth = 105f;

            GameObject dropdownObject = new GameObject(label + " Dropdown");
            dropdownObject.transform.SetParent(row, false);
            Image image = dropdownObject.AddComponent<Image>();
            image.color = new Color(0.045f, 0.075f, 0.095f, 0.96f);
            Dropdown dropdown = dropdownObject.AddComponent<Dropdown>();
            dropdown.options.Clear();
            for (int i = 0; i < options.Length; i++)
            {
                dropdown.options.Add(new Dropdown.OptionData(options[i]));
            }

            Text caption = AddLabel(dropdownObject.transform, options[0], 12, FontStyle.Normal);
            Stretch(caption.GetComponent<RectTransform>(), 8f, 0f);
            dropdown.captionText = caption;
            dropdownObject.AddComponent<LayoutElement>().preferredHeight = 28f;
            return dropdown;
        }

        private static Slider AddSlider(Transform parent, float min, float max, float value)
        {
            GameObject sliderObject = new GameObject("Slider");
            sliderObject.transform.SetParent(parent, false);
            Slider slider = sliderObject.AddComponent<Slider>();
            slider.minValue = min;
            slider.maxValue = max;
            slider.value = value;
            sliderObject.AddComponent<LayoutElement>().preferredHeight = 24f;

            RectTransform background = CreateSliderPart("Background", sliderObject.transform, new Color(0.09f, 0.13f, 0.16f), Vector2.zero, Vector2.one);
            RectTransform fillArea = CreateEmptyRect("Fill Area", sliderObject.transform);
            fillArea.anchorMin = new Vector2(0f, 0.35f);
            fillArea.anchorMax = new Vector2(1f, 0.65f);
            fillArea.offsetMin = new Vector2(6f, 0f);
            fillArea.offsetMax = new Vector2(-6f, 0f);
            RectTransform fill = CreateSliderPart("Fill", fillArea, AccentColor, Vector2.zero, Vector2.one);

            RectTransform handleArea = CreateEmptyRect("Handle Slide Area", sliderObject.transform);
            handleArea.anchorMin = Vector2.zero;
            handleArea.anchorMax = Vector2.one;
            handleArea.offsetMin = new Vector2(6f, 0f);
            handleArea.offsetMax = new Vector2(-6f, 0f);
            RectTransform handle = CreateSliderPart("Handle", handleArea, new Color(0.85f, 0.94f, 1f), new Vector2(0f, 0.5f), new Vector2(0f, 0.5f));
            handle.sizeDelta = new Vector2(14f, 20f);

            slider.targetGraphic = handle.GetComponent<Image>();
            slider.fillRect = fill;
            slider.handleRect = handle;
            background.SetAsFirstSibling();
            return slider;
        }

        private static void AddScaleLabels(Transform parent, string a, string b, string c, string d)
        {
            RectTransform row = CreateEmptyRect("Scale Labels", parent);
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = 18f;
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.childControlWidth = true;
            layout.childForceExpandWidth = true;
            AddLabel(row, a, 10, FontStyle.Normal, MutedTextColor);
            AddLabel(row, b, 10, FontStyle.Normal, MutedTextColor).alignment = TextAnchor.MiddleCenter;
            AddLabel(row, c, 10, FontStyle.Normal, MutedTextColor).alignment = TextAnchor.MiddleCenter;
            AddLabel(row, d, 10, FontStyle.Normal, MutedTextColor).alignment = TextAnchor.MiddleRight;
        }

        private static void AddFlexibleSpace(Transform parent)
        {
            GameObject spacer = new GameObject("Flexible Space");
            spacer.transform.SetParent(parent, false);
            LayoutElement layout = spacer.AddComponent<LayoutElement>();
            layout.flexibleWidth = 1f;
            layout.preferredHeight = 1f;
        }

        private static RectTransform CreateSliderPart(string name, Transform parent, Color color, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject part = new GameObject(name);
            part.transform.SetParent(parent, false);
            Image image = part.AddComponent<Image>();
            image.color = color;
            RectTransform rect = part.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            return rect;
        }

        private static RectTransform CreateEmptyRect(string name, Transform parent)
        {
            GameObject rectObject = new GameObject(name);
            rectObject.transform.SetParent(parent, false);
            return rectObject.AddComponent<RectTransform>();
        }

        private static void Stretch(RectTransform rect, float xPadding, float yPadding)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = new Vector2(xPadding, yPadding);
            rect.offsetMax = new Vector2(-xPadding, -yPadding);
        }

        private static void BuildEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
#if ENABLE_INPUT_SYSTEM
            eventSystem.AddComponent<InputSystemUIInputModule>();
#else
            eventSystem.AddComponent<StandaloneInputModule>();
#endif
        }
    }
}
