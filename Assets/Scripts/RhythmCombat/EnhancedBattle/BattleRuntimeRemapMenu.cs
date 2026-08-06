using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.UI;

[DefaultExecutionOrder(-300)]
[AddComponentMenu("Rhythm Combat/Battle Runtime Remap Menu")]
public class BattleRuntimeRemapMenu : MonoBehaviour
{
    enum CaptureMode
    {
        None,
        Keyboard,
        Gamepad
    }

    static readonly BattleGamepadControl[] CapturableGamepadControls =
    {
        BattleGamepadControl.South,
        BattleGamepadControl.North,
        BattleGamepadControl.West,
        BattleGamepadControl.East,
        BattleGamepadControl.LeftShoulder,
        BattleGamepadControl.RightShoulder,
        BattleGamepadControl.LeftTrigger,
        BattleGamepadControl.RightTrigger,
        BattleGamepadControl.DpadUp,
        BattleGamepadControl.DpadDown,
        BattleGamepadControl.DpadLeft,
        BattleGamepadControl.DpadRight,
        BattleGamepadControl.LeftStick,
        BattleGamepadControl.RightStick,
        BattleGamepadControl.Select
    };

    [Header("Scene References")]
    [SerializeField] BattleLaneInputRouter inputRouter;
    [SerializeField] BMBattleManager buttonManager;
    [SerializeField] BattleRhythmChartRunner chartRunner;

    [Header("Menu UI - prebuilt in Battle.unity")]
    [SerializeField] CanvasGroup menuCanvasGroup;
    [SerializeField] List<Button> keyboardButtons = new List<Button>(6);
    [SerializeField] List<Button> gamepadButtons = new List<Button>(6);
    [SerializeField] List<TextMeshProUGUI> keyboardLabels = new List<TextMeshProUGUI>(6);
    [SerializeField] List<TextMeshProUGUI> gamepadLabels = new List<TextMeshProUGUI>(6);
    [SerializeField] List<Image> keyboardIcons = new List<Image>(6);
    [SerializeField] List<Image> gamepadIcons = new List<Image>(6);
    [SerializeField] TextMeshProUGUI statusLabel;

    [Header("Selection Colors")]
    [SerializeField] Color normalButtonColor = new Color(0.12f, 0.14f, 0.18f, 0.98f);
    [SerializeField] Color selectedButtonColor = new Color(0.25f, 0.55f, 0.95f, 1f);
    [SerializeField] Color captureButtonColor = new Color(0.95f, 0.68f, 0.18f, 1f);

    bool menuOpen;
    CaptureMode captureMode;
    int selectedLane;
    int selectedColumn;
    float previousTimeScale = 1f;
    bool chartWasPausedBeforeMenu;
    bool inputWasSuppressedBeforeMenu;

    public bool IsOpen => menuOpen;

    void Awake()
    {
        ResolveReferences();
        RegisterButtonCallbacks();
        SetMenuVisible(false);
        RefreshAllLabels();
    }

    void OnDisable()
    {
        if (!menuOpen)
        {
            return;
        }

        menuOpen = false;
        RestoreGameplayState();
        SetMenuVisible(false);
    }

    void OnDestroy()
    {
        RemoveButtonCallbacks();
    }

    void Update()
    {
        if (captureMode != CaptureMode.None)
        {
            ProcessCapture();
            return;
        }

        if (WasMenuTogglePressed())
        {
            SetOpen(!menuOpen);
            return;
        }

        if (!menuOpen)
        {
            return;
        }

        ProcessMenuNavigation();
    }

    public void SetOpen(bool open)
    {
        if (menuOpen == open)
        {
            return;
        }

        menuOpen = open;
        captureMode = CaptureMode.None;

        if (menuOpen)
        {
            ResolveReferences();
            previousTimeScale = Time.timeScale;
            chartWasPausedBeforeMenu =
                chartRunner != null && chartRunner.IsPaused;
            inputWasSuppressedBeforeMenu =
                inputRouter != null && inputRouter.IsInputSuppressed;

            Time.timeScale = 0f;
            inputRouter?.SetInputSuppressed(true);
            chartRunner?.SetPaused(true);
            RefreshAllLabels();
            SetStatus("Seleziona una lane. Invio/A per rimappare, Cancella/X per rimuovere.");
        }
        else
        {
            RestoreGameplayState();
        }

        SetMenuVisible(menuOpen);
        RefreshSelectionVisuals();
    }

#if UNITY_EDITOR
    public void ConfigureEditor(
        BattleLaneInputRouter router,
        BMBattleManager manager,
        BattleRhythmChartRunner runner,
        CanvasGroup canvasGroup,
        List<Button> newKeyboardButtons,
        List<Button> newGamepadButtons,
        List<TextMeshProUGUI> newKeyboardLabels,
        List<TextMeshProUGUI> newGamepadLabels,
        List<Image> newKeyboardIcons,
        List<Image> newGamepadIcons,
        TextMeshProUGUI newStatusLabel)
    {
        inputRouter = router;
        buttonManager = manager;
        chartRunner = runner;
        menuCanvasGroup = canvasGroup;
        keyboardButtons = newKeyboardButtons ?? new List<Button>(6);
        gamepadButtons = newGamepadButtons ?? new List<Button>(6);
        keyboardLabels = newKeyboardLabels ?? new List<TextMeshProUGUI>(6);
        gamepadLabels = newGamepadLabels ?? new List<TextMeshProUGUI>(6);
        keyboardIcons = newKeyboardIcons ?? new List<Image>(6);
        gamepadIcons = newGamepadIcons ?? new List<Image>(6);
        statusLabel = newStatusLabel;
    }
#endif

    void ResolveReferences()
    {
        if (inputRouter == null)
        {
            inputRouter = BattleLaneInputRouter.Instance != null
                ? BattleLaneInputRouter.Instance
                : FindObjectOfType<BattleLaneInputRouter>();
        }

        if (buttonManager == null)
        {
            buttonManager = FindObjectOfType<BMBattleManager>();
        }

        if (chartRunner == null)
        {
            chartRunner = FindObjectOfType<BattleRhythmChartRunner>();
        }
    }

    void RestoreGameplayState()
    {
        captureMode = CaptureMode.None;
        chartRunner?.SetPaused(chartWasPausedBeforeMenu);
        inputRouter?.SetInputSuppressed(inputWasSuppressedBeforeMenu);
        Time.timeScale = previousTimeScale;
    }

    void SetMenuVisible(bool visible)
    {
        if (menuCanvasGroup == null)
        {
            return;
        }

        menuCanvasGroup.alpha = visible ? 1f : 0f;
        menuCanvasGroup.interactable = visible;
        menuCanvasGroup.blocksRaycasts = visible;
    }

    void ProcessMenuNavigation()
    {
        if (WasMoveUpPressed())
        {
            selectedLane = (selectedLane + 5) % 6;
            RefreshSelectionVisuals();
        }
        else if (WasMoveDownPressed())
        {
            selectedLane = (selectedLane + 1) % 6;
            RefreshSelectionVisuals();
        }

        if (WasMoveLeftPressed())
        {
            selectedColumn = 0;
            RefreshSelectionVisuals();
        }
        else if (WasMoveRightPressed())
        {
            selectedColumn = 1;
            RefreshSelectionVisuals();
        }

        if (WasConfirmPressed())
        {
            BeginCapture(
                selectedLane,
                selectedColumn == 0
                    ? CaptureMode.Keyboard
                    : CaptureMode.Gamepad);
            return;
        }

        if (WasClearPressed())
        {
            ClearSelectedBinding();
            return;
        }

        if (WasCancelPressed())
        {
            SetOpen(false);
        }
    }

    void ProcessCapture()
    {
        if (captureMode == CaptureMode.Keyboard)
        {
            CaptureKeyboardInput();
        }
        else if (captureMode == CaptureMode.Gamepad)
        {
            CaptureGamepadInput();
        }
    }

    void CaptureKeyboardInput()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            SetStatus("Nessuna tastiera rilevata. Esc annulla.");
            return;
        }

        if (keyboard.escapeKey.wasPressedThisFrame)
        {
            CancelCapture();
            return;
        }

        if (keyboard.backspaceKey.wasPressedThisFrame ||
            keyboard.deleteKey.wasPressedThisFrame)
        {
            ApplyKeyboardBinding(Key.None);
            return;
        }

        for (int i = 0; i < keyboard.allKeys.Count; i++)
        {
            var keyControl = keyboard.allKeys[i];

            if (keyControl == null ||
                !keyControl.wasPressedThisFrame ||
                keyControl.keyCode == Key.Escape)
            {
                continue;
            }

            ApplyKeyboardBinding(keyControl.keyCode);
            return;
        }
    }

    void CaptureGamepadInput()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CancelCapture();
            return;
        }

        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Gamepad gamepad = Gamepad.all[i];

            if (gamepad == null)
            {
                continue;
            }

            if (gamepad.startButton.wasPressedThisFrame)
            {
                CancelCapture();
                return;
            }

            for (int controlIndex = 0;
                 controlIndex < CapturableGamepadControls.Length;
                 controlIndex++)
            {
                BattleGamepadControl control =
                    CapturableGamepadControls[controlIndex];

                if (!BattleLaneInputRouter.TryGetGamepadButton(
                        control,
                        out GamepadButton button))
                {
                    continue;
                }

                if (gamepad[button].wasPressedThisFrame)
                {
                    ApplyGamepadBinding(control);
                    return;
                }
            }
        }

        SetStatus("Premi un controllo del gamepad. Start annulla.");
    }

    void BeginCapture(int lane, CaptureMode mode)
    {
        if (lane < 0 || lane >= 6 || mode == CaptureMode.None)
        {
            return;
        }

        selectedLane = lane;
        selectedColumn = mode == CaptureMode.Keyboard ? 0 : 1;
        captureMode = mode;

        SetStatus(
            mode == CaptureMode.Keyboard
                ? "Premi il nuovo tasto tastiera. Esc annulla, Backspace rimuove."
                : "Premi il nuovo controllo gamepad. Start annulla.");

        RefreshSelectionVisuals();
    }

    void CancelCapture()
    {
        captureMode = CaptureMode.None;
        SetStatus("Rimappatura annullata.");
        RefreshSelectionVisuals();
    }

    void ApplyKeyboardBinding(Key key)
    {
        inputRouter?.SetKeyboardKey(
            (BMButtonPrefab.Cell)selectedLane,
            key,
            true);

        FinishBindingChange();
    }

    void ApplyGamepadBinding(BattleGamepadControl control)
    {
        inputRouter?.SetGamepadControl(
            (BMButtonPrefab.Cell)selectedLane,
            control,
            true);

        FinishBindingChange();
    }

    void ClearSelectedBinding()
    {
        if (selectedColumn == 0)
        {
            inputRouter?.SetKeyboardKey(
                (BMButtonPrefab.Cell)selectedLane,
                Key.None,
                true);
        }
        else
        {
            inputRouter?.SetGamepadControl(
                (BMButtonPrefab.Cell)selectedLane,
                BattleGamepadControl.None,
                true);
        }

        FinishBindingChange();
    }

    void FinishBindingChange()
    {
        captureMode = CaptureMode.None;
        buttonManager?.RefreshActiveButtonDisplays();
        RefreshAllLabels();
        SetStatus("Comando aggiornato e salvato.");
        RefreshSelectionVisuals();
    }

    void RefreshAllLabels()
    {
        if (inputRouter == null)
        {
            return;
        }

        for (int lane = 0; lane < 6; lane++)
        {
            BMButtonPrefab.Cell cell = (BMButtonPrefab.Cell)lane;

            RefreshBindingVisual(
                keyboardLabels,
                keyboardIcons,
                lane,
                inputRouter.GetKeyboardBindingLabel(cell),
                inputRouter.GetKeyboardBindingSprite(cell));

            RefreshBindingVisual(
                gamepadLabels,
                gamepadIcons,
                lane,
                inputRouter.GetGamepadBindingLabel(cell),
                inputRouter.GetGamepadBindingSprite(cell));
        }
    }

    static void RefreshBindingVisual(
        List<TextMeshProUGUI> labels,
        List<Image> icons,
        int index,
        string labelText,
        Sprite sprite)
    {
        if (index >= 0 && index < labels.Count && labels[index] != null)
        {
            labels[index].text = labelText;
        }

        if (index < 0 || index >= icons.Count || icons[index] == null)
        {
            return;
        }

        Image icon = icons[index];
        icon.sprite = sprite;
        icon.enabled = sprite != null;
        icon.preserveAspect = true;
    }

    void RefreshSelectionVisuals()
    {
        for (int lane = 0; lane < 6; lane++)
        {
            SetButtonColor(
                keyboardButtons,
                lane,
                ResolveButtonColor(lane, 0));

            SetButtonColor(
                gamepadButtons,
                lane,
                ResolveButtonColor(lane, 1));
        }
    }

    Color ResolveButtonColor(int lane, int column)
    {
        if (!menuOpen || lane != selectedLane || column != selectedColumn)
        {
            return normalButtonColor;
        }

        return captureMode != CaptureMode.None
            ? captureButtonColor
            : selectedButtonColor;
    }

    static void SetButtonColor(
        List<Button> buttons,
        int index,
        Color color)
    {
        if (index < 0 || index >= buttons.Count || buttons[index] == null)
        {
            return;
        }

        Graphic graphic = buttons[index].targetGraphic;

        if (graphic != null)
        {
            graphic.color = color;
        }
    }

    void SetStatus(string message)
    {
        if (statusLabel != null)
        {
            statusLabel.text = message;
        }
    }

    void RegisterButtonCallbacks()
    {
        for (int lane = 0; lane < 6; lane++)
        {
            int capturedLane = lane;

            if (lane < keyboardButtons.Count && keyboardButtons[lane] != null)
            {
                keyboardButtons[lane].onClick.AddListener(
                    () => BeginCapture(capturedLane, CaptureMode.Keyboard));
            }

            if (lane < gamepadButtons.Count && gamepadButtons[lane] != null)
            {
                gamepadButtons[lane].onClick.AddListener(
                    () => BeginCapture(capturedLane, CaptureMode.Gamepad));
            }
        }
    }

    void RemoveButtonCallbacks()
    {
        for (int lane = 0; lane < keyboardButtons.Count; lane++)
        {
            Button button = keyboardButtons[lane];

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }

        for (int lane = 0; lane < gamepadButtons.Count; lane++)
        {
            Button button = gamepadButtons[lane];

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
            }
        }
    }

    static bool WasMenuTogglePressed()
    {
        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            return true;
        }

        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Gamepad gamepad = Gamepad.all[i];

            if (gamepad != null && gamepad.startButton.wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
    }

    static bool WasMoveUpPressed()
    {
        return KeyboardPressed(Key.UpArrow, Key.W) ||
               GamepadPressed(GamepadButton.DpadUp);
    }

    static bool WasMoveDownPressed()
    {
        return KeyboardPressed(Key.DownArrow, Key.S) ||
               GamepadPressed(GamepadButton.DpadDown);
    }

    static bool WasMoveLeftPressed()
    {
        return KeyboardPressed(Key.LeftArrow, Key.A) ||
               GamepadPressed(GamepadButton.DpadLeft);
    }

    static bool WasMoveRightPressed()
    {
        return KeyboardPressed(Key.RightArrow, Key.D) ||
               GamepadPressed(GamepadButton.DpadRight);
    }

    static bool WasConfirmPressed()
    {
        return KeyboardPressed(Key.Enter, Key.NumpadEnter, Key.Space) ||
               GamepadPressed(GamepadButton.South);
    }

    static bool WasClearPressed()
    {
        return KeyboardPressed(Key.Backspace, Key.Delete) ||
               GamepadPressed(GamepadButton.West);
    }

    static bool WasCancelPressed()
    {
        return GamepadPressed(GamepadButton.East);
    }

    static bool KeyboardPressed(params Key[] keys)
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            return false;
        }

        for (int i = 0; i < keys.Length; i++)
        {
            if (keyboard[keys[i]].wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
    }

    static bool GamepadPressed(GamepadButton button)
    {
        for (int i = 0; i < Gamepad.all.Count; i++)
        {
            Gamepad gamepad = Gamepad.all[i];

            if (gamepad != null && gamepad[button].wasPressedThisFrame)
            {
                return true;
            }
        }

        return false;
    }
}
