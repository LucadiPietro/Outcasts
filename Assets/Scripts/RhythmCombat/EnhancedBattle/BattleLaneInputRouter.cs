using System;
using System.Collections.Generic;
using System.Reflection;
using RhythmCombat.Domain.Timing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

public enum BattleGamepadControl
{
    [InspectorName("None")]
    None,

    [InspectorName("Button South (A / Cross)")]
    South,

    [InspectorName("Button North (Y / Triangle)")]
    North,

    [InspectorName("Button West (X / Square)")]
    West,

    [InspectorName("Button East (B / Circle)")]
    East,

    [InspectorName("Left Shoulder (LB / L1)")]
    LeftShoulder,

    [InspectorName("Right Shoulder (RB / R1)")]
    RightShoulder,

    [InspectorName("Left Trigger (LT / L2)")]
    LeftTrigger,

    [InspectorName("Right Trigger (RT / R2)")]
    RightTrigger,

    [InspectorName("D-Pad Up")]
    DpadUp,

    [InspectorName("D-Pad Down")]
    DpadDown,

    [InspectorName("D-Pad Left")]
    DpadLeft,

    [InspectorName("D-Pad Right")]
    DpadRight,

    [InspectorName("Left Stick Press (L3)")]
    LeftStick,

    [InspectorName("Right Stick Press (R3)")]
    RightStick,

    [InspectorName("Start / Menu")]
    Start,

    [InspectorName("Select / View")]
    Select
}

public enum BattleInputGlyphMode
{
    [InspectorName("Last remapped / used device")]
    LastUsedDevice,

    [InspectorName("Keyboard only")]
    Keyboard,

    [InspectorName("Gamepad only")]
    Gamepad
}

[DefaultExecutionOrder(-250)]
[AddComponentMenu("Rhythm Combat/Battle Lane Input Router")]
public class BattleLaneInputRouter : MonoBehaviour
{
    enum RuntimeDisplayDevice
    {
        Keyboard,
        Gamepad
    }

    [Serializable]
    public class KeyboardIconBinding
    {
        public Key key = Key.None;
        public Sprite sprite;
    }

    [Serializable]
    public class GamepadIconBinding
    {
        public BattleGamepadControl control = BattleGamepadControl.None;
        public Sprite sprite;
    }
    [Serializable]
    public class LaneBinding
    {
        [HideInInspector]
        public BMButtonPrefab.Cell cell;

        [Tooltip("Tasto tastiera assegnato alla lane. None disattiva la tastiera per questa lane.")]
        public Key keyboardKey = Key.None;

        [Tooltip("Controllo gamepad assegnato alla lane. None disattiva il gamepad per questa lane.")]
        public BattleGamepadControl gamepadControl = BattleGamepadControl.None;

        [Tooltip("Etichetta facoltativa. Se vuota viene generata automaticamente dai controlli scelti.")]
        public string displayLabel;

        [Tooltip("Icona di fallback della lane usata quando non esiste una sprite dinamica per il binding.")]
        public Sprite displayIcon;

        public Color displayColor = Color.white;
    }

    sealed class LaneRuntimeState
    {
        public LaneBinding Binding;
        public bool KeyboardPressed;
        public bool GamepadPressed;
        public bool CombinedPressed;
        public double LastAcceptedPressTime = double.NegativeInfinity;
    }

    public static BattleLaneInputRouter Instance { get; private set; }

    public bool IsInputSuppressed => inputSuppressed;

    [Header("Scene References")]
    [SerializeField] BattleManager battleManager;
    [SerializeField] BattleVisualFeedbackController visualFeedback;

    [Header("DESIGNER INPUT - DROPDOWN PER LANE")]
    [SerializeField] List<LaneBinding> laneInputs = CreateDefaultBindings();

    [Header("Dynamic Input Sprites")]
    [Tooltip("Decide quale famiglia di sprite mostrare sulle note quando sono configurati sia tastiera sia gamepad.")]
    [SerializeField] BattleInputGlyphMode inputGlyphMode =
        BattleInputGlyphMode.LastUsedDevice;

    [Tooltip("Usa automaticamente la sprite associata al tasto rimappato. Display Icon resta un override/fallback per la lane.")]
    [SerializeField] bool preferDynamicBindingSprites = true;

    [SerializeField] List<KeyboardIconBinding> keyboardIconBindings =
        new List<KeyboardIconBinding>();

    [SerializeField] List<GamepadIconBinding> gamepadIconBindings =
        new List<GamepadIconBinding>();

    [Header("Trigger / Single Press")]
    [Tooltip("Valore che LT/RT devono raggiungere per produrre la pressione.")]
    [SerializeField, Range(0.01f, 1f)]
    float triggerDownThreshold = 0.55f;

    [Tooltip("Valore sotto cui LT/RT devono tornare prima di poter produrre una nuova pressione.")]
    [SerializeField, Range(0f, 0.99f)]
    float triggerUpThreshold = 0.25f;

    [Tooltip("Tempo minimo tra due pressioni accettate sulla stessa lane.")]
    [SerializeField, Min(0f)]
    float duplicatePressLockSeconds = 0.12f;

    [Header("Runtime Remapping")]
    [Tooltip("Carica le rimappature salvate durante il gioco tramite Esc / Start.")]
    [SerializeField]
    bool loadSavedRuntimeBindings = true;

    const string KeyboardPlayerPrefsPrefix =
        "BattleEnhanced.KeyboardLane.";

    const string GamepadPlayerPrefsPrefix =
        "BattleEnhanced.GamepadLane.";

    readonly Dictionary<BMButtonPrefab.Cell, LaneRuntimeState> runtimeByCell =
        new Dictionary<BMButtonPrefab.Cell, LaneRuntimeState>(6);

    MethodInfo resolvePressedLaneMethod;
    MethodInfo evaluateSpatialJudgmentMethod;
    bool inputSuppressed;
    RuntimeDisplayDevice lastDisplayDevice = RuntimeDisplayDevice.Keyboard;
    BMBattleManager runtimeButtonManager;

    public event Action<BMButtonPrefab.Cell> LanePressed;
    public event Action<BMButtonPrefab.Cell> LaneReleased;

    public static List<LaneBinding> CreateDefaultBindings()
    {
        List<LaneBinding> bindings = new List<LaneBinding>(6);

        for (int laneIndex = 0; laneIndex < 6; laneIndex++)
        {
            bindings.Add(
                new LaneBinding
                {
                    cell = (BMButtonPrefab.Cell)laneIndex,
                    keyboardKey = Key.None,
                    gamepadControl = BattleGamepadControl.None,
                    displayLabel = string.Empty,
                    displayIcon = null,
                    displayColor = Color.white
                });
        }

        return bindings;
    }

    void Reset()
    {
        laneInputs = CreateDefaultBindings();
        ClampThresholds();
    }

    void OnValidate()
    {
        NormalizeLaneInputs();
        NormalizeIconLibraries();
        ClampThresholds();
    }

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogError(
                "BattleLaneInputRouter: esiste più di un router nella scena.");

            enabled = false;
            return;
        }

        Instance = this;

        if (battleManager == null)
        {
            battleManager = FindObjectOfType<BattleManager>();
        }

        if (visualFeedback == null)
        {
            visualFeedback = FindObjectOfType<BattleVisualFeedbackController>();
        }

        runtimeButtonManager = FindObjectOfType<BMBattleManager>();

        NormalizeLaneInputs();
        NormalizeIconLibraries();

        if (loadSavedRuntimeBindings)
        {
            LoadRuntimeBindings();
        }

        CacheBattleManagerMethods();
        BuildRuntimeState();
    }

    void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            enabled = false;
            return;
        }

        NormalizeLaneInputs();
        NormalizeIconLibraries();
        BuildRuntimeState();
    }

    void OnDisable()
    {
        ReleaseAllLanes();
        runtimeByCell.Clear();

        if (Instance == this)
        {
            Instance = null;
        }
    }

    void Update()
    {
        if (inputSuppressed)
        {
            return;
        }

        foreach (KeyValuePair<BMButtonPrefab.Cell, LaneRuntimeState> pair in runtimeByCell)
        {
            ProcessLaneState(pair.Value);
        }
    }

    public bool IsPressed(BMButtonPrefab.Cell cell)
    {
        return runtimeByCell.TryGetValue(cell, out LaneRuntimeState state) &&
               state.CombinedPressed;
    }

    public void SetInputSuppressed(bool suppressed)
    {
        if (inputSuppressed == suppressed)
        {
            return;
        }

        inputSuppressed = suppressed;

        if (inputSuppressed)
        {
            ReleaseAllLanes(false);
            ResetPhysicalState(false);
        }
        else
        {
            ResetPhysicalState(true);
        }
    }

    public Key GetKeyboardKey(BMButtonPrefab.Cell cell)
    {
        LaneBinding binding = GetBinding(cell);
        return binding != null ? binding.keyboardKey : Key.None;
    }

    public BattleGamepadControl GetGamepadControl(BMButtonPrefab.Cell cell)
    {
        LaneBinding binding = GetBinding(cell);
        return binding != null
            ? binding.gamepadControl
            : BattleGamepadControl.None;
    }

    public string GetKeyboardBindingLabel(BMButtonPrefab.Cell cell)
    {
        string label = GetKeyboardDisplayLabel(GetKeyboardKey(cell));
        return string.IsNullOrEmpty(label) ? "None" : label;
    }

    public string GetGamepadBindingLabel(BMButtonPrefab.Cell cell)
    {
        string label = GetGamepadDisplayLabel(GetGamepadControl(cell));
        return string.IsNullOrEmpty(label) ? "None" : label;
    }

    public void SetKeyboardKey(
        BMButtonPrefab.Cell cell,
        Key key,
        bool saveToPlayerPrefs)
    {
        LaneBinding binding = GetBinding(cell);

        if (binding == null)
        {
            return;
        }

        binding.keyboardKey = key;
        lastDisplayDevice = RuntimeDisplayDevice.Keyboard;
        BuildRuntimeState();

        if (saveToPlayerPrefs)
        {
            PlayerPrefs.SetInt(
                KeyboardPlayerPrefsPrefix + (int)cell,
                (int)key);
            PlayerPrefs.Save();
        }

        NotifyBindingVisualsChanged();
    }

    public void SetGamepadControl(
        BMButtonPrefab.Cell cell,
        BattleGamepadControl control,
        bool saveToPlayerPrefs)
    {
        LaneBinding binding = GetBinding(cell);

        if (binding == null)
        {
            return;
        }

        binding.gamepadControl = control;
        lastDisplayDevice = RuntimeDisplayDevice.Gamepad;
        BuildRuntimeState();

        if (saveToPlayerPrefs)
        {
            PlayerPrefs.SetInt(
                GamepadPlayerPrefsPrefix + (int)cell,
                (int)control);
            PlayerPrefs.Save();
        }

        NotifyBindingVisualsChanged();
    }

    public void ClearSavedRuntimeBindings()
    {
        for (int laneIndex = 0; laneIndex < 6; laneIndex++)
        {
            PlayerPrefs.DeleteKey(
                KeyboardPlayerPrefsPrefix + laneIndex);
            PlayerPrefs.DeleteKey(
                GamepadPlayerPrefsPrefix + laneIndex);
        }

        PlayerPrefs.Save();
    }

    public LaneBinding GetBinding(BMButtonPrefab.Cell cell)
    {
        for (int i = 0; i < laneInputs.Count; i++)
        {
            LaneBinding binding = laneInputs[i];

            if (binding != null && binding.cell == cell)
            {
                return binding;
            }
        }

        return null;
    }

    public string GetDisplayLabel(BMButtonPrefab.Cell cell)
    {
        LaneBinding binding = GetBinding(cell);

        if (binding == null)
        {
            return cell.ToString();
        }

        if (!string.IsNullOrWhiteSpace(binding.displayLabel))
        {
            return binding.displayLabel;
        }

        string keyboardLabel = GetKeyboardDisplayLabel(binding.keyboardKey);
        string gamepadLabel = GetGamepadDisplayLabel(binding.gamepadControl);

        if (!string.IsNullOrEmpty(keyboardLabel) &&
            !string.IsNullOrEmpty(gamepadLabel))
        {
            return keyboardLabel + " / " + gamepadLabel;
        }

        if (!string.IsNullOrEmpty(keyboardLabel))
        {
            return keyboardLabel;
        }

        if (!string.IsNullOrEmpty(gamepadLabel))
        {
            return gamepadLabel;
        }

        return cell.ToString();
    }

    public Sprite GetKeyboardBindingSprite(BMButtonPrefab.Cell cell)
    {
        Key key = GetKeyboardKey(cell);

        NormalizeIconLibraries();

        for (int i = 0; i < keyboardIconBindings.Count; i++)
        {
            KeyboardIconBinding iconBinding = keyboardIconBindings[i];

            if (iconBinding != null &&
                iconBinding.key == key &&
                iconBinding.sprite != null)
            {
                return iconBinding.sprite;
            }
        }

        return null;
    }

    public Sprite GetGamepadBindingSprite(BMButtonPrefab.Cell cell)
    {
        BattleGamepadControl control = GetGamepadControl(cell);

        NormalizeIconLibraries();

        for (int i = 0; i < gamepadIconBindings.Count; i++)
        {
            GamepadIconBinding iconBinding = gamepadIconBindings[i];

            if (iconBinding != null &&
                iconBinding.control == control &&
                iconBinding.sprite != null)
            {
                return iconBinding.sprite;
            }
        }

        return null;
    }

    public Sprite GetDisplaySprite(BMButtonPrefab.Cell cell)
    {
        LaneBinding binding = GetBinding(cell);

        if (binding == null)
        {
            return null;
        }

        if (preferDynamicBindingSprites)
        {
            Sprite dynamicSprite = ResolveDynamicBindingSprite(cell);

            if (dynamicSprite != null)
            {
                return dynamicSprite;
            }
        }

        return binding.displayIcon;
    }

    Sprite ResolveDynamicBindingSprite(BMButtonPrefab.Cell cell)
    {
        if (inputGlyphMode == BattleInputGlyphMode.Keyboard)
        {
            return GetKeyboardBindingSprite(cell) ??
                   GetGamepadBindingSprite(cell);
        }

        if (inputGlyphMode == BattleInputGlyphMode.Gamepad)
        {
            return GetGamepadBindingSprite(cell) ??
                   GetKeyboardBindingSprite(cell);
        }

        return lastDisplayDevice == RuntimeDisplayDevice.Gamepad
            ? GetGamepadBindingSprite(cell) ?? GetKeyboardBindingSprite(cell)
            : GetKeyboardBindingSprite(cell) ?? GetGamepadBindingSprite(cell);
    }

    public Color GetDisplayColor(BMButtonPrefab.Cell cell, Color fallback)
    {
        LaneBinding binding = GetBinding(cell);
        return binding != null ? binding.displayColor : fallback;
    }

#if UNITY_EDITOR
    public List<LaneBinding> CreateEditorBindingsSnapshot()
    {
        NormalizeLaneInputs();

        List<LaneBinding> snapshot = new List<LaneBinding>(laneInputs.Count);

        for (int i = 0; i < laneInputs.Count; i++)
        {
            LaneBinding source = laneInputs[i];

            snapshot.Add(
                new LaneBinding
                {
                    cell = source.cell,
                    keyboardKey = source.keyboardKey,
                    gamepadControl = source.gamepadControl,
                    displayLabel = source.displayLabel,
                    displayIcon = source.displayIcon,
                    displayColor = source.displayColor
                });
        }

        return snapshot;
    }

    public List<KeyboardIconBinding> CreateEditorKeyboardIconsSnapshot()
    {
        return CloneKeyboardIconBindings(keyboardIconBindings);
    }

    public List<GamepadIconBinding> CreateEditorGamepadIconsSnapshot()
    {
        return CloneGamepadIconBindings(gamepadIconBindings);
    }

    public void ConfigureEditorIconLibrary(
        List<KeyboardIconBinding> keyboardIcons,
        List<GamepadIconBinding> gamepadIcons)
    {
        keyboardIconBindings = CloneKeyboardIconBindings(keyboardIcons);
        gamepadIconBindings = CloneGamepadIconBindings(gamepadIcons);
    }

    public void ConfigureEditor(
        BattleManager manager,
        BattleVisualFeedbackController feedback,
        List<LaneBinding> bindings)
    {
        battleManager = manager;
        visualFeedback = feedback;
        laneInputs = bindings ?? CreateDefaultBindings();
        NormalizeLaneInputs();
    }
#endif

    void ProcessLaneState(LaneRuntimeState state)
    {
        LaneBinding binding = state.Binding;

        if (binding == null)
        {
            return;
        }

        bool keyboardWasPressed = state.KeyboardPressed;
        bool gamepadWasPressed = state.GamepadPressed;

        state.KeyboardPressed = ReadKeyboard(binding.keyboardKey);
        state.GamepadPressed = ReadGamepad(
            binding.gamepadControl,
            state.GamepadPressed);

        bool pressedNow = state.KeyboardPressed || state.GamepadPressed;

        if (pressedNow && !state.CombinedPressed)
        {
            RuntimeDisplayDevice inputDevice =
                state.GamepadPressed && !gamepadWasPressed
                    ? RuntimeDisplayDevice.Gamepad
                    : RuntimeDisplayDevice.Keyboard;

            if (state.KeyboardPressed && !keyboardWasPressed)
            {
                inputDevice = RuntimeDisplayDevice.Keyboard;
            }

            TryAcceptPress(state, inputDevice);
        }
        else if (!pressedNow && state.CombinedPressed)
        {
            state.CombinedPressed = false;
            LaneReleased?.Invoke(binding.cell);
        }
    }

    void TryAcceptPress(
        LaneRuntimeState state,
        RuntimeDisplayDevice inputDevice)
    {
        double currentTime = Time.unscaledTimeAsDouble;

        if (currentTime - state.LastAcceptedPressTime <
            duplicatePressLockSeconds)
        {
            state.CombinedPressed = true;
            return;
        }

        state.CombinedPressed = true;
        state.LastAcceptedPressTime = currentTime;

        SetLastDisplayDevice(inputDevice);
        PresentPressJudgment(state.Binding);
        ResolveLane(state.Binding);
        LanePressed?.Invoke(state.Binding.cell);
    }

    bool ReadKeyboard(Key key)
    {
        return key != Key.None &&
               Keyboard.current != null &&
               Keyboard.current[key].isPressed;
    }

    bool ReadGamepad(
        BattleGamepadControl configuredControl,
        bool wasPressed)
    {
        if (!TryGetGamepadButton(configuredControl, out GamepadButton button))
        {
            return false;
        }

        float highestValue = 0f;

        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Gamepad gamepad = Gamepad.all[i];

            if (gamepad == null)
            {
                continue;
            }

            highestValue = Mathf.Max(
                highestValue,
                gamepad[button].ReadValue());
        }

        return wasPressed
            ? highestValue > triggerUpThreshold
            : highestValue >= triggerDownThreshold;
    }

    public static bool TryGetGamepadButton(
        BattleGamepadControl configuredControl,
        out GamepadButton button)
    {
        if (configuredControl == BattleGamepadControl.None)
        {
            button = default;
            return false;
        }

        return Enum.TryParse(
            configuredControl.ToString(),
            out button);
    }

    void SetLastDisplayDevice(RuntimeDisplayDevice device)
    {
        if (inputGlyphMode != BattleInputGlyphMode.LastUsedDevice ||
            lastDisplayDevice == device)
        {
            return;
        }

        lastDisplayDevice = device;
        NotifyBindingVisualsChanged();
    }

    void NotifyBindingVisualsChanged()
    {
        if (runtimeButtonManager == null)
        {
            runtimeButtonManager = FindObjectOfType<BMBattleManager>();
        }

        runtimeButtonManager?.RefreshActiveButtonDisplays();
    }

    static List<KeyboardIconBinding> CloneKeyboardIconBindings(
        IEnumerable<KeyboardIconBinding> source)
    {
        var result = new List<KeyboardIconBinding>();

        if (source == null)
        {
            return result;
        }

        foreach (KeyboardIconBinding item in source)
        {
            if (item == null)
            {
                continue;
            }

            result.Add(
                new KeyboardIconBinding
                {
                    key = item.key,
                    sprite = item.sprite
                });
        }

        return result;
    }

    static List<GamepadIconBinding> CloneGamepadIconBindings(
        IEnumerable<GamepadIconBinding> source)
    {
        var result = new List<GamepadIconBinding>();

        if (source == null)
        {
            return result;
        }

        foreach (GamepadIconBinding item in source)
        {
            if (item == null)
            {
                continue;
            }

            result.Add(
                new GamepadIconBinding
                {
                    control = item.control,
                    sprite = item.sprite
                });
        }

        return result;
    }

    void LoadRuntimeBindings()
    {
        for (int laneIndex = 0; laneIndex < 6; laneIndex++)
        {
            BMButtonPrefab.Cell cell = (BMButtonPrefab.Cell)laneIndex;
            LaneBinding binding = GetBinding(cell);

            if (binding == null)
            {
                continue;
            }

            string keyboardKey = KeyboardPlayerPrefsPrefix + laneIndex;
            string gamepadKey = GamepadPlayerPrefsPrefix + laneIndex;

            if (PlayerPrefs.HasKey(keyboardKey))
            {
                int savedValue = PlayerPrefs.GetInt(keyboardKey);

                if (Enum.IsDefined(typeof(Key), savedValue))
                {
                    binding.keyboardKey = (Key)savedValue;
                }
            }

            if (PlayerPrefs.HasKey(gamepadKey))
            {
                int savedValue = PlayerPrefs.GetInt(gamepadKey);

                if (Enum.IsDefined(typeof(BattleGamepadControl), savedValue))
                {
                    binding.gamepadControl =
                        (BattleGamepadControl)savedValue;
                }
            }
        }
    }

    void ResetPhysicalState(bool blockUntilRelease)
    {
        foreach (KeyValuePair<BMButtonPrefab.Cell, LaneRuntimeState> pair in runtimeByCell)
        {
            LaneRuntimeState state = pair.Value;
            LaneBinding binding = state.Binding;

            if (binding == null)
            {
                continue;
            }

            bool keyboardPressed = ReadKeyboard(binding.keyboardKey);
            bool gamepadPressed = ReadGamepad(
                binding.gamepadControl,
                false);

            state.KeyboardPressed = keyboardPressed;
            state.GamepadPressed = gamepadPressed;
            state.CombinedPressed =
                blockUntilRelease &&
                (keyboardPressed || gamepadPressed);
        }
    }

    void BuildRuntimeState()
    {
        runtimeByCell.Clear();

        for (int i = 0; i < laneInputs.Count; i++)
        {
            LaneBinding binding = laneInputs[i];

            if (binding == null)
            {
                continue;
            }

            runtimeByCell[binding.cell] =
                new LaneRuntimeState
                {
                    Binding = binding
                };
        }
    }

    void ReleaseAllLanes(bool invokeReleaseEvents = true)
    {
        foreach (KeyValuePair<BMButtonPrefab.Cell, LaneRuntimeState> pair in runtimeByCell)
        {
            LaneRuntimeState state = pair.Value;

            if (!state.CombinedPressed)
            {
                continue;
            }

            state.CombinedPressed = false;

            if (invokeReleaseEvents)
            {
                LaneReleased?.Invoke(pair.Key);
            }
        }
    }

    void NormalizeLaneInputs()
    {
        Dictionary<BMButtonPrefab.Cell, LaneBinding> existingByCell =
            new Dictionary<BMButtonPrefab.Cell, LaneBinding>(6);

        if (laneInputs != null)
        {
            for (int i = 0; i < laneInputs.Count; i++)
            {
                LaneBinding binding = laneInputs[i];

                if (binding == null || existingByCell.ContainsKey(binding.cell))
                {
                    continue;
                }

                existingByCell.Add(binding.cell, binding);
            }
        }

        List<LaneBinding> normalized = new List<LaneBinding>(6);

        for (int laneIndex = 0; laneIndex < 6; laneIndex++)
        {
            BMButtonPrefab.Cell cell = (BMButtonPrefab.Cell)laneIndex;

            if (!existingByCell.TryGetValue(cell, out LaneBinding binding))
            {
                binding = new LaneBinding
                {
                    keyboardKey = Key.None,
                    gamepadControl = BattleGamepadControl.None,
                    displayColor = Color.white
                };
            }

            binding.cell = cell;
            normalized.Add(binding);
        }

        laneInputs = normalized;
    }

    void NormalizeIconLibraries()
    {
        if (keyboardIconBindings == null)
        {
            keyboardIconBindings = new List<KeyboardIconBinding>();
        }

        if (gamepadIconBindings == null)
        {
            gamepadIconBindings = new List<GamepadIconBinding>();
        }
    }

    void ClampThresholds()
    {
        triggerDownThreshold = Mathf.Clamp(triggerDownThreshold, 0.01f, 1f);
        triggerUpThreshold = Mathf.Clamp(
            triggerUpThreshold,
            0f,
            Mathf.Max(0f, triggerDownThreshold - 0.01f));
    }

    static string GetKeyboardDisplayLabel(Key key)
    {
        if (key == Key.None)
        {
            return string.Empty;
        }

        if (Keyboard.current != null)
        {
            return Keyboard.current[key].displayName;
        }

        return key.ToString()
            .Replace("Digit", string.Empty)
            .Replace("Numpad", "Num ");
    }

    static string GetGamepadDisplayLabel(BattleGamepadControl control)
    {
        if (!TryGetGamepadButton(control, out GamepadButton button))
        {
            return string.Empty;
        }

        if (Gamepad.current != null)
        {
            return Gamepad.current[button].displayName;
        }

        return control.ToString()
            .Replace("LeftTrigger", "LT")
            .Replace("RightTrigger", "RT")
            .Replace("LeftShoulder", "LB")
            .Replace("RightShoulder", "RB")
            .Replace("LeftStick", "L3")
            .Replace("RightStick", "R3")
            .Replace("Dpad", "D-Pad ");
    }

    void CacheBattleManagerMethods()
    {
        if (battleManager == null)
        {
            return;
        }

        const BindingFlags Flags =
            BindingFlags.Instance | BindingFlags.NonPublic;

        Type managerType = battleManager.GetType();

        resolvePressedLaneMethod = managerType.GetMethod(
            "ResolvePressedLaneQueue",
            Flags,
            null,
            new[]
            {
                typeof(BMButtonPrefab.Cell),
                typeof(string)
            },
            null);

        evaluateSpatialJudgmentMethod = managerType.GetMethod(
            "TryEvaluateSpatialJudgment",
            Flags);

        if (resolvePressedLaneMethod == null)
        {
            Debug.LogError(
                "BattleLaneInputRouter: metodo ResolvePressedLaneQueue " +
                "non trovato nel BattleManager corrente.");
        }
    }

    void ResolveLane(LaneBinding binding)
    {
        if (battleManager == null)
        {
            battleManager = FindObjectOfType<BattleManager>();
            CacheBattleManagerMethods();
        }

        if (battleManager == null || resolvePressedLaneMethod == null)
        {
            return;
        }

        string inputName = GetDisplayLabel(binding.cell);

        resolvePressedLaneMethod.Invoke(
            battleManager,
            new object[]
            {
                binding.cell,
                inputName
            });
    }

    void PresentPressJudgment(LaneBinding binding)
    {
        BattleButton note = BattleNoteRegistry.Peek(binding.cell);

        if (note == null)
        {
            visualFeedback?.ShowMiss(
                binding.cell,
                "MISS",
                binding.displayColor);

            return;
        }

        bool hasSpatialResult = TryEvaluateSpatialJudgment(
            note,
            out JudgmentResult result);

        if (!hasSpatialResult)
        {
            result = new JudgmentResult(
                JudgmentGrade.Perfect,
                0d);
        }

        visualFeedback?.ShowJudgment(
            binding.cell,
            result,
            binding.displayColor,
            false);

        note.PlayPressFeedback(result.Grade);
    }

    bool TryEvaluateSpatialJudgment(
        BattleButton note,
        out JudgmentResult result)
    {
        result = default;

        if (battleManager == null ||
            evaluateSpatialJudgmentMethod == null ||
            note == null)
        {
            return false;
        }

        object[] arguments =
        {
            note,
            default(JudgmentResult)
        };

        object invocationResult = evaluateSpatialJudgmentMethod.Invoke(
            battleManager,
            arguments);

        if (invocationResult is not bool hasResult || !hasResult)
        {
            return false;
        }

        result = (JudgmentResult)arguments[1];
        return true;
    }
}
