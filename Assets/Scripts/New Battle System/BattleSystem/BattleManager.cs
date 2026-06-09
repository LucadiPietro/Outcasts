using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RhythmCombat.Domain.Geometry;
using RhythmCombat.Domain.Movement;
using RhythmCombat.Domain.Timing;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class BattleManager : MonoBehaviour
{
    [Serializable]
    public class Buttons
    {
        public BattleButton button;
        public InputAction buttonKeys;
    }

    public List<Player> players;
    public List<Enemy> enemies;

    public float timeBeforeStart = 1.5f;
    public static BattleManager Instance { get; private set; }

    public GameObject audioController;
    public bool playTimelineOnStart = true;

    public List<Buttons> buttons;

    private readonly Dictionary<BMButtonPrefab.Cell, Queue<BattleButton>> laneQueues =
        new Dictionary<BMButtonPrefab.Cell, Queue<BattleButton>>();

    private readonly Dictionary<BMButtonPrefab.Cell, int> lastInputFrameByCell =
        new Dictionary<BMButtonPrefab.Cell, int>();

    [Header("Input Behaviour")]
    [SerializeField] bool blockSameFrameDuplicateInput = true;
    [SerializeField] bool consumeNoteOnFailedSpatialInput = true;

    [Header("Trigger Edge Input")]
    [SerializeField] float triggerDownThreshold = 0.55f;
    [SerializeField] float triggerUpThreshold = 0.25f;

    private bool ltWasDown;
    private bool rtWasDown;

    public BattleUIManager battleUIManager;

    public int counter = 0;

    public float deadPlayerModificator = 1;
    public float deadEnemyModificator = 1;

    [SerializeField] bool enableKeyboardLaneFallback = true;
    [SerializeField] BattleInputDisplayMode inputDisplayMode = BattleInputDisplayMode.Keyboard;
    [SerializeField] Key inputDisplayModeSwitchKey = Key.F1;

    [Header("Spatial Judgment")]
    [SerializeField] bool enableSpatialJudgment = true;
    [SerializeField] float spatialApproachDurationSeconds = 2f;
    [SerializeField] float spatialLaneSpacing = 2f;
    [SerializeField] float spatialRowOffset = 1f;
    [SerializeField] float spatialToleranceRadius = 1f;
    [SerializeField] float spatialPerfectPercent = 0.1f;
    [SerializeField] float spatialGoodPercent = 0.25f;
    [SerializeField] float spatialBadPercent = 0.5f;
    [SerializeField] float spatialDespawnAfterHitSeconds = 1f;

    INoteJudgmentService noteJudgmentService;
    double currentChartTimeSeconds;

    public NoteMotionService SpatialMotionService { get; private set; }
    public double CurrentChartTimeSeconds => currentChartTimeSeconds;
    public BattleInputDisplayMode CurrentInputDisplayMode => inputDisplayMode;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private IEnumerator Start()
    {
        if (battleUIManager == null)
        {
            battleUIManager = GetComponent<BattleUIManager>();
        }

        if (battleUIManager == null)
        {
            battleUIManager = FindObjectOfType<BattleUIManager>();
        }

        if (battleUIManager != null)
        {
            battleUIManager.UpdateCounter(counter);
        }

        buttons = new List<Buttons>();
        InitializeLaneQueues();

        BuildSpatialJudgmentService(spatialApproachDurationSeconds);
        SetupInput();

        yield return new WaitForSeconds(timeBeforeStart);

        if (!playTimelineOnStart)
        {
            yield break;
        }

        if (audioController != null && audioController.TryGetComponent(out PlayableDirector playableDirector))
        {
            playableDirector.Play();
        }
        else
        {
            Debug.LogWarning("BattleManager: AudioController o PlayableDirector non configurato.");
        }
    }

    void InitializeLaneQueues()
    {
        laneQueues.Clear();

        laneQueues[BMButtonPrefab.Cell.Cell1] = new Queue<BattleButton>();
        laneQueues[BMButtonPrefab.Cell.Cell2] = new Queue<BattleButton>();
        laneQueues[BMButtonPrefab.Cell.Cell3] = new Queue<BattleButton>();
        laneQueues[BMButtonPrefab.Cell.Cell4] = new Queue<BattleButton>();
        laneQueues[BMButtonPrefab.Cell.Cell5] = new Queue<BattleButton>();
        laneQueues[BMButtonPrefab.Cell.Cell6] = new Queue<BattleButton>();
    }

    void EnsureLaneQueueExists(BMButtonPrefab.Cell cell)
    {
        if (!laneQueues.ContainsKey(cell))
        {
            laneQueues[cell] = new Queue<BattleButton>();
        }
    }

    public void ConfigureSpatialJudgment(
        float approachDurationSeconds,
        float toleranceRadius,
        float perfectPercent,
        float goodPercent,
        float badPercent,
        float despawnAfterHitSeconds)
    {
        spatialToleranceRadius = toleranceRadius;
        spatialPerfectPercent = perfectPercent;
        spatialGoodPercent = goodPercent;
        spatialBadPercent = badPercent;
        spatialDespawnAfterHitSeconds = despawnAfterHitSeconds;
        spatialApproachDurationSeconds = approachDurationSeconds;

        BuildSpatialJudgmentService(approachDurationSeconds);
    }

    public void SetChartTimeSeconds(double chartTimeSeconds)
    {
        currentChartTimeSeconds = chartTimeSeconds;
    }

    void SetupInput()
    {
        InputManager.Instance().SetAction(ActionKey.NORTH, delegate(InputAction.CallbackContext obj)
        {
            if (obj.canceled) return;
            OnMappedGamepadAction(Keys.Y);
        });

        InputManager.Instance().SetAction(ActionKey.SOUTH, delegate(InputAction.CallbackContext obj)
        {
            if (obj.canceled) return;
            OnMappedGamepadAction(Keys.A);
        });

        InputManager.Instance().SetAction(ActionKey.EAST, delegate(InputAction.CallbackContext obj)
        {
            if (obj.canceled) return;
            OnMappedGamepadAction(Keys.B);
        });

        InputManager.Instance().SetAction(ActionKey.WEST, delegate(InputAction.CallbackContext obj)
        {
            if (obj.canceled) return;
            OnMappedGamepadAction(Keys.X);
        });

        InputManager.Instance().SetAction(ActionKey.LT, delegate(InputAction.CallbackContext obj)
        {
            // Ignorato apposta: LT viene gestito manualmente in Update.
        });

        InputManager.Instance().SetAction(ActionKey.RT, delegate(InputAction.CallbackContext obj)
        {
            // Ignorato apposta: RT viene gestito manualmente in Update.
        });
    }

    private void Update()
    {
        HandleInputDisplayModeSwitch();

        HandleGamepadTriggersSingleInput();

        if (inputDisplayMode != BattleInputDisplayMode.Keyboard ||
            !enableKeyboardLaneFallback ||
            Keyboard.current == null ||
            buttons == null)
        {
            return;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            OnLanePressed(BMButtonPrefab.Cell.Cell1, "A");
        }

        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            OnLanePressed(BMButtonPrefab.Cell.Cell2, "S");
        }

        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            OnLanePressed(BMButtonPrefab.Cell.Cell3, "D");
        }

        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            OnLanePressed(BMButtonPrefab.Cell.Cell4, "J");
        }

        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            OnLanePressed(BMButtonPrefab.Cell.Cell5, "K");
        }

        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            OnLanePressed(BMButtonPrefab.Cell.Cell6, "L");
        }
    }

    void HandleGamepadTriggersSingleInput()
    {
        if (inputDisplayMode != BattleInputDisplayMode.Xbox)
        {
            return;
        }

        if (Gamepad.current == null)
        {
            ltWasDown = false;
            rtWasDown = false;
            return;
        }

        float ltValue = Gamepad.current.leftTrigger.ReadValue();
        float rtValue = Gamepad.current.rightTrigger.ReadValue();

        HandleTriggerEdge(ltValue, ref ltWasDown, Keys.LT);
        HandleTriggerEdge(rtValue, ref rtWasDown, Keys.RT);
    }

    void HandleTriggerEdge(float value, ref bool wasDown, Keys key)
    {
        if (wasDown && value <= triggerUpThreshold)
        {
            wasDown = false;
            return;
        }

        if (wasDown)
        {
            return;
        }

        if (value >= triggerDownThreshold)
        {
            wasDown = true;
            OnMappedGamepadAction(key, true);
        }
    }

    void OnMappedGamepadAction(Keys key, bool fromManualTriggerPolling = false)
    {
        if (inputDisplayMode != BattleInputDisplayMode.Xbox)
        {
            return;
        }

        if ((key == Keys.LT || key == Keys.RT) && !fromManualTriggerPolling)
        {
            Debug.Log("BattleManager: input " + key + " ignorato perché arrivato da InputManager.");
            return;
        }

        BMButtonPrefab.Cell? cell = GetCellFromXboxKey(key);

        if (!cell.HasValue)
        {
            ResolveEmptyLanePress(key.ToString());
            return;
        }

        ResolvePressedLaneQueue(cell.Value, key.ToString());
    }

    BMButtonPrefab.Cell? GetCellFromXboxKey(Keys key)
    {
        switch (key)
        {
            case Keys.Y:
                return BMButtonPrefab.Cell.Cell1;
            case Keys.X:
                return BMButtonPrefab.Cell.Cell2;
            case Keys.B:
                return BMButtonPrefab.Cell.Cell3;
            case Keys.A:
                return BMButtonPrefab.Cell.Cell4;
            case Keys.LT:
                return BMButtonPrefab.Cell.Cell5;
            case Keys.RT:
                return BMButtonPrefab.Cell.Cell6;
            default:
                return null;
        }
    }

    bool IsDuplicateLaneInput(BMButtonPrefab.Cell cell)
    {
        if (!blockSameFrameDuplicateInput)
        {
            return false;
        }

        int currentFrame = Time.frameCount;

        if (lastInputFrameByCell.TryGetValue(cell, out int lastFrame) && lastFrame == currentFrame)
        {
            return true;
        }

        lastInputFrameByCell[cell] = currentFrame;
        return false;
    }

    void HandleInputDisplayModeSwitch()
    {
        if (Keyboard.current == null || inputDisplayModeSwitchKey == Key.None)
        {
            return;
        }

        var switchKey = Keyboard.current[inputDisplayModeSwitchKey];

        if (switchKey != null && switchKey.wasPressedThisFrame)
        {
            ToggleInputDisplayMode();
        }
    }

    public void ToggleInputDisplayMode()
    {
        BattleInputDisplayMode nextMode = inputDisplayMode == BattleInputDisplayMode.Keyboard
            ? BattleInputDisplayMode.Xbox
            : BattleInputDisplayMode.Keyboard;

        SetInputDisplayMode(nextMode);
    }

    public void SetInputDisplayMode(BattleInputDisplayMode mode)
    {
        if (inputDisplayMode == mode)
        {
            return;
        }

        inputDisplayMode = mode;
        RefreshInputDisplayMode();
    }

    void RefreshInputDisplayMode()
    {
        BMBattleManager bmBattleManager = FindObjectOfType<BMBattleManager>();

        if (bmBattleManager != null)
        {
            bmBattleManager.RefreshActiveButtonDisplays();
        }

        RefreshSubscribedInputActions();

        string modeLabel = inputDisplayMode == BattleInputDisplayMode.Keyboard ? "KEYBOARD" : "XBOX";

        if (battleUIManager != null)
        {
            battleUIManager.ShowFeedback("INPUT " + modeLabel, Color.cyan);
        }

        Debug.Log("BattleManager: input mode impostato su " + modeLabel + ".");
    }

    void BuildSpatialJudgmentService(float approachDurationSeconds)
    {
        var layout = BattlefieldLayout.CreateLaneCenteredStandard(
            spatialLaneSpacing,
            spatialRowOffset,
            spatialToleranceRadius);

        var travelSettings = new NoteTravelSettings(
            approachDurationSeconds,
            spatialDespawnAfterHitSeconds);

        var config = new SpatialJudgmentConfig(
            spatialPerfectPercent,
            spatialGoodPercent,
            spatialBadPercent);

        var motionService = new NoteMotionService(layout, travelSettings);

        SpatialMotionService = motionService;
        noteJudgmentService = new SpatialJudgmentService(layout, motionService, config);
    }

    public void SubcribeButton(BattleButton battleButton)
    {
        if (battleButton == null)
        {
            return;
        }

        if (buttons == null)
        {
            buttons = new List<Buttons>();
        }

        EnsureLaneQueueExists(battleButton.cell);

        if (buttons.Any(b => b.button == battleButton))
        {
            return;
        }

        int laneIndex = GetLaneIndex(battleButton);
        battleButton.buttonAction = GetLaneButtonAction(laneIndex);

        InputAction inputAction = null;

        if (inputDisplayMode == BattleInputDisplayMode.Xbox)
        {
            if (BattleInputManager.Instance != null &&
                BattleInputManager.Instance.keyMap != null &&
                BattleInputManager.Instance.keyMap.TryGetValue(battleButton.buttonAction, out InputAction mappedAction))
            {
                inputAction = mappedAction;
            }
            else
            {
                Debug.LogWarning("BattleManager: BattleInputManager non configurato, registro il pulsante solo per input lane/tastiera.");
            }
        }

        Buttons newButton = new Buttons
        {
            button = battleButton,
            buttonKeys = inputAction
        };

        buttons.Add(newButton);
        laneQueues[battleButton.cell].Enqueue(battleButton);

        if (battleUIManager != null && !battleButton.hasSpatialJudgmentData)
        {
            battleUIManager.ShowFeedback("READY " + battleButton.cell, Color.yellow);
        }

        Debug.Log(
            $"BattleManager: pulsante registrato su {battleButton.cell}. " +
            $"Queue count: {laneQueues[battleButton.cell].Count}.");
    }

    public Keys GetLaneButtonAction(int laneIndex)
    {
        if (inputDisplayMode == BattleInputDisplayMode.Keyboard)
        {
            return Keys.NONE;
        }

        return GetXboxLaneKey(laneIndex);
    }

    public string GetLaneDisplayLabel(int laneIndex)
    {
        if (inputDisplayMode == BattleInputDisplayMode.Keyboard)
        {
            return GetKeyboardLaneLabel(laneIndex);
        }

        Keys key = GetXboxLaneKey(laneIndex);
        return key == Keys.NONE ? "?" : key.ToString();
    }

    void RefreshSubscribedInputActions()
    {
        if (buttons == null)
        {
            return;
        }

        for (int i = 0; i < buttons.Count; i++)
        {
            Buttons entry = buttons[i];

            if (entry == null || entry.button == null)
            {
                continue;
            }

            int laneIndex = GetLaneIndex(entry.button);
            entry.button.buttonAction = GetLaneButtonAction(laneIndex);
            entry.buttonKeys = null;

            if (inputDisplayMode == BattleInputDisplayMode.Xbox &&
                BattleInputManager.Instance != null &&
                BattleInputManager.Instance.keyMap != null &&
                BattleInputManager.Instance.keyMap.TryGetValue(entry.button.buttonAction, out InputAction mappedAction))
            {
                entry.buttonKeys = mappedAction;
            }
        }
    }

    static int GetLaneIndex(BattleButton button)
    {
        if (button == null)
        {
            return -1;
        }

        return button.laneIndex >= 0 ? button.laneIndex : (int)button.cell;
    }

    string GetKeyboardLaneLabel(int laneIndex)
    {
        switch (laneIndex)
        {
            case 0:
                return "A";
            case 1:
                return "S";
            case 2:
                return "D";
            case 3:
                return "J";
            case 4:
                return "K";
            case 5:
                return "L";
            default:
                return "?";
        }
    }

    Keys GetXboxLaneKey(int laneIndex)
    {
        switch (laneIndex)
        {
            case 0:
                return Keys.Y;
            case 1:
                return Keys.X;
            case 2:
                return Keys.B;
            case 3:
                return Keys.A;
            case 4:
                return Keys.LT;
            case 5:
                return Keys.RT;
            default:
                return Keys.NONE;
        }
    }

    public void Unsubscribe(BattleButton battleButton)
    {
        if (battleButton == null)
        {
            return;
        }

        if (buttons != null)
        {
            buttons.RemoveAll(b => b.button == battleButton);
        }

        RemoveButtonFromLaneQueue(battleButton);
    }

    void RemoveButtonFromLaneQueue(BattleButton battleButton)
    {
        if (battleButton == null)
        {
            return;
        }

        if (!laneQueues.TryGetValue(battleButton.cell, out Queue<BattleButton> queue))
        {
            return;
        }

        if (queue.Count == 0)
        {
            return;
        }

        Queue<BattleButton> rebuiltQueue = new Queue<BattleButton>();

        while (queue.Count > 0)
        {
            BattleButton queuedButton = queue.Dequeue();

            if (queuedButton != null &&
                queuedButton != battleButton &&
                !queuedButton.isResolved)
            {
                rebuiltQueue.Enqueue(queuedButton);
            }
        }

        laneQueues[battleButton.cell] = rebuiltQueue;
    }

    public void ResolveMissedButton(BattleButton battleButton)
    {
        ResolveMissedButton(battleButton, true);
    }

    public bool TryResolveSpatialMissedButton(BattleButton battleButton)
    {
        if (battleButton == null ||
            battleButton.isResolved ||
            !battleButton.hasSpatialJudgmentData ||
            battleButton.spatialNote == null ||
            noteJudgmentService == null)
        {
            return false;
        }

        if (!noteJudgmentService.IsMissed(battleButton.spatialNote, currentChartTimeSeconds))
        {
            return false;
        }

        ResolveMissedButton(battleButton, false);
        return true;
    }

    void ResolveMissedButton(BattleButton battleButton, bool fadeVisual)
    {
        if (battleButton == null || battleButton.isResolved)
        {
            return;
        }

        battleButton.MarkResolved();

        if (battleButton.cell is BMButtonPrefab.Cell.Cell4 or BMButtonPrefab.Cell.Cell5 or BMButtonPrefab.Cell.Cell6)
        {
            DefenceRoutine(battleButton.cell);
        }

        Unsubscribe(battleButton);
        ResetCounter();

        if (battleUIManager != null)
        {
            battleUIManager.ShowFeedback("MISS " + battleButton.cell, Color.red);
        }

        battleButton.ShakeMissAndDisappear();

        Debug.Log("BattleManager: miss risolto su " + battleButton.cell + ".");
    }

    void OnLanePressed(BMButtonPrefab.Cell cell)
    {
        OnLanePressed(cell, cell.ToString());
    }

    void OnLanePressed(BMButtonPrefab.Cell cell, string inputName)
    {
        ResolvePressedLaneQueue(cell, inputName);
    }

    void ResolvePressedLaneQueue(BMButtonPrefab.Cell cell, string inputName)
    {
        if (IsDuplicateLaneInput(cell))
        {
            Debug.Log($"BattleManager: input duplicato ignorato su {cell} con {inputName}.");
            return;
        }

        EnsureLaneQueueExists(cell);

        Queue<BattleButton> queue = laneQueues[cell];

        CleanResolvedButtonsFromQueue(queue);

        if (queue.Count == 0)
        {
            ResolveEmptyLanePress(inputName);
            return;
        }

        BattleButton button = queue.Peek();

        if (button == null || button.isResolved)
        {
            ResolveEmptyLanePress(inputName);
            return;
        }

        ResolveSinglePressedButton(button, inputName);
    }

    void CleanResolvedButtonsFromQueue(Queue<BattleButton> queue)
    {
        if (queue == null)
        {
            return;
        }

        while (queue.Count > 0)
        {
            BattleButton button = queue.Peek();

            if (button != null && !button.isResolved)
            {
                break;
            }

            queue.Dequeue();
        }
    }

    void ResolveEmptyLanePress(string inputName)
    {
        ResetCounter();

        if (battleUIManager != null)
        {
            battleUIManager.ShowFeedback("MISS " + inputName, Color.red);
        }

        Debug.Log($"BattleManager: input {inputName} su lane vuota. Counter reset.");
    }

    void ResolveSinglePressedButton(BattleButton button, string inputName)
    {
        if (button == null || button.isResolved)
        {
            ResolveEmptyLanePress(inputName);
            return;
        }

        JudgmentResult judgmentResult;
        bool hasSpatialResult = TryEvaluateSpatialJudgment(button, out judgmentResult);

        if (hasSpatialResult && !judgmentResult.IsHit)
        {
            ResetCounter();

            if (battleUIManager != null)
            {
                battleUIManager.ShowFeedback("MISS " + button.cell, Color.red);
            }

            Debug.Log(
                "BattleManager: spatial miss su " + button.cell +
                " delta=" + judgmentResult.DeltaSeconds.ToString("0.000") +
                " input=" + inputName + ".");

            if (consumeNoteOnFailedSpatialInput)
            {
                button.ShakeMissAndDisappear();
                Unsubscribe(button);
            }

            return;
        }

        counter++;

        if (battleUIManager != null)
        {
            battleUIManager.UpdateCounter(counter);
        }

        if (battleUIManager != null)
        {
            battleUIManager.ShowFeedback(
                GetFeedbackText(button, hasSpatialResult, judgmentResult),
                GetFeedbackColor(hasSpatialResult, judgmentResult));
        }

        Debug.Log(GetHitLog(button, inputName, counter, hasSpatialResult, judgmentResult));

        DamageRoutine(button.cell, GetDamageMultiplier(hasSpatialResult, judgmentResult));

        button.KillButton();
        Unsubscribe(button);
    }

    bool TryEvaluateSpatialJudgment(BattleButton battleButton, out JudgmentResult result)
    {
        result = default;

        if (!enableSpatialJudgment ||
            noteJudgmentService == null ||
            battleButton == null ||
            !battleButton.hasSpatialJudgmentData ||
            battleButton.spatialNote == null)
        {
            return false;
        }

        result = noteJudgmentService.Evaluate(battleButton.spatialNote, currentChartTimeSeconds);
        return true;
    }

    void ResetCounter()
    {
        counter = 0;

        if (battleUIManager != null)
        {
            battleUIManager.UpdateCounter(counter);
        }
    }

    string GetFeedbackText(BattleButton button, bool hasSpatialResult, JudgmentResult result)
    {
        if (!hasSpatialResult)
        {
            return "HIT " + button.cell;
        }

        return result.Grade.ToString().ToUpperInvariant() + " " + button.cell;
    }

    Color GetFeedbackColor(bool hasSpatialResult, JudgmentResult result)
    {
        if (!hasSpatialResult)
        {
            return Color.green;
        }

        switch (result.Grade)
        {
            case JudgmentGrade.Perfect:
                return new Color(0.2f, 1f, 0.85f);
            case JudgmentGrade.Good:
                return Color.green;
            case JudgmentGrade.Bad:
                return new Color(1f, 0.7f, 0.15f);
            default:
                return Color.red;
        }
    }

    float GetDamageMultiplier(bool hasSpatialResult, JudgmentResult result)
    {
        if (!hasSpatialResult)
        {
            return 1f;
        }

        switch (result.Grade)
        {
            case JudgmentGrade.Perfect:
                return 1f;
            case JudgmentGrade.Good:
                return 0.75f;
            case JudgmentGrade.Bad:
                return 0.4f;
            default:
                return 0f;
        }
    }

    string GetHitLog(
        BattleButton button,
        string inputName,
        int currentCounter,
        bool hasSpatialResult,
        JudgmentResult result)
    {
        if (!hasSpatialResult)
        {
            return $"BattleManager: preso pulsante {button.buttonAction} su {button.cell} con input {inputName}. Counter: {currentCounter}";
        }

        return "BattleManager: " + result.Grade +
               " su " + button.cell +
               " lane=" + button.laneIndex +
               " delta=" + result.DeltaSeconds.ToString("0.000") +
               " input=" + inputName +
               ". Counter: " + currentCounter;
    }

    public void DamageRoutine(BMButtonPrefab.Cell cell, float judgmentMultiplier = 1f)
    {
        Player playerToConsider = null;
        Enemy enemyToConsider = null;

        if (cell is BMButtonPrefab.Cell.Cell1 or BMButtonPrefab.Cell.Cell2 or BMButtonPrefab.Cell.Cell3)
        {
            switch (cell)
            {
                case BMButtonPrefab.Cell.Cell1:
                    foreach (var ene in enemies.Where(ene => ene.cell == BMButtonPrefab.Cell.Cell1))
                    {
                        enemyToConsider = ene;
                    }

                    foreach (var pla in players.Where(pla => pla.cell == BMButtonPrefab.Cell.Cell4))
                    {
                        playerToConsider = pla;
                    }

                    break;

                case BMButtonPrefab.Cell.Cell2:
                    foreach (var ene in enemies.Where(ene => ene.cell == BMButtonPrefab.Cell.Cell2))
                    {
                        enemyToConsider = ene;
                    }

                    foreach (var pla in players.Where(pla => pla.cell == BMButtonPrefab.Cell.Cell5))
                    {
                        playerToConsider = pla;
                    }

                    break;

                case BMButtonPrefab.Cell.Cell3:
                    foreach (var ene in enemies.Where(ene => ene.cell == BMButtonPrefab.Cell.Cell3))
                    {
                        enemyToConsider = ene;
                    }

                    foreach (var pla in players.Where(pla => pla.cell == BMButtonPrefab.Cell.Cell6))
                    {
                        playerToConsider = pla;
                    }

                    break;
            }

            Attack(playerToConsider, enemyToConsider, judgmentMultiplier);
        }
    }

    public void DefenceRoutine(BMButtonPrefab.Cell cell)
    {
        Player playerToConsider = null;
        Enemy enemyToConsider = null;

        if (cell is BMButtonPrefab.Cell.Cell4 or BMButtonPrefab.Cell.Cell5 or BMButtonPrefab.Cell.Cell6)
        {
            switch (cell)
            {
                case BMButtonPrefab.Cell.Cell4:
                    foreach (var ene in enemies.Where(ene => ene.cell == BMButtonPrefab.Cell.Cell1))
                    {
                        enemyToConsider = ene;
                    }

                    foreach (var pla in players.Where(pla => pla.cell == BMButtonPrefab.Cell.Cell4))
                    {
                        playerToConsider = pla;
                    }

                    break;

                case BMButtonPrefab.Cell.Cell5:
                    foreach (var ene in enemies.Where(ene => ene.cell == BMButtonPrefab.Cell.Cell2))
                    {
                        enemyToConsider = ene;
                    }

                    foreach (var pla in players.Where(pla => pla.cell == BMButtonPrefab.Cell.Cell5))
                    {
                        playerToConsider = pla;
                    }

                    break;

                case BMButtonPrefab.Cell.Cell6:
                    foreach (var ene in enemies.Where(ene => ene.cell == BMButtonPrefab.Cell.Cell3))
                    {
                        enemyToConsider = ene;
                    }

                    foreach (var pla in players.Where(pla => pla.cell == BMButtonPrefab.Cell.Cell6))
                    {
                        playerToConsider = pla;
                    }

                    break;
            }

            Defence(playerToConsider, enemyToConsider);
        }
    }

    private void Attack(Player player, Enemy enemy, float judgmentMultiplier)
    {
        if (player == null || enemy == null)
        {
            Debug.LogWarning("BattleManager: impossibile applicare danno, player o enemy non configurato.");
            return;
        }

        var enemiesToAttach = new List<Enemy>();

        if (enemy.actualHealth > 0)
        {
            enemiesToAttach.Add(enemy);
        }
        else
        {
            enemiesToAttach.AddRange(enemies.Where(ene => ene.actualHealth > 0));
        }

        if (enemiesToAttach.Count == 0)
        {
            return;
        }

        float constToUse = player.actualHealth > 0 ? 1 : deadPlayerModificator;

        foreach (var ene in enemiesToAttach)
        {
            float damageCalc =
                (((player.attack * player.attackBuff) - (enemy.defence * enemy.defenceBuff)) + player.damageConstant) *
                player.voteMultiplayer *
                counter *
                player.positionMultiplayer *
                judgmentMultiplier;

            float singleDamage = constToUse * damageCalc;
            float damage = singleDamage / enemiesToAttach.Count;

            ene.GetHit(damage);
        }
    }

    private void Defence(Player player, Enemy enemy)
    {
        if (player == null || enemy == null)
        {
            Debug.LogWarning("BattleManager: impossibile applicare difesa, player o enemy non configurato.");
            return;
        }

        var playersToAttach = new List<Player>();

        if (player.actualHealth > 0)
        {
            playersToAttach.Add(player);
        }
        else
        {
            playersToAttach.AddRange(players.Where(pla => pla.actualHealth > 0));
        }

        if (playersToAttach.Count == 0)
        {
            return;
        }

        float constToUse = enemy.actualHealth > 0 ? 1 : deadEnemyModificator;

        foreach (var pla in playersToAttach)
        {
            float damageCalc =
                (((enemy.attack * enemy.attackBuff) - (player.defence * player.defenceBuff)) + enemy.damageConstant) *
                player.positionMultiplayer;

            float singleDamage = constToUse * damageCalc;
            float damage = singleDamage / playersToAttach.Count;

            pla.GetHit(damage);
        }
    }
}