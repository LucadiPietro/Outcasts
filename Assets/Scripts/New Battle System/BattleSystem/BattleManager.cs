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

    public BattleUIManager battleUIManager;

    public int counter = 0;

    public float deadPlayerModificator = 1;
    
    public float deadEnemyModificator = 1;

    [SerializeField] bool enableKeyboardLaneFallback = true;
    [SerializeField] bool enableSpatialJudgment = true;
    [SerializeField] float spatialLaneSpacing = 2f;
    [SerializeField] float spatialRowOffset = 1f;
    [SerializeField] float spatialToleranceRadius = 1f;
    [SerializeField] float spatialPerfectPercent = 0.1f;
    [SerializeField] float spatialGoodPercent = 0.25f;
    [SerializeField] float spatialBadPercent = 0.5f;
    [SerializeField] float spatialDespawnAfterHitSeconds = 1f;

    INoteJudgmentService noteJudgmentService;
    double currentChartTimeSeconds;

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
        battleUIManager = GetComponent<BattleUIManager>();
        if (battleUIManager != null)
        {
            battleUIManager.UpdateCounter(counter);
        }

        buttons = new List<Buttons>();
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
            InputAction buttonAction = new InputAction();
            buttonAction = BattleInputManager.Instance.keyMap[Keys.Y];
            OnButtonPressed(buttonAction);
        });
        InputManager.Instance().SetAction(ActionKey.SOUTH, delegate(InputAction.CallbackContext obj)
        {
            InputAction buttonAction = new InputAction();
            buttonAction = BattleInputManager.Instance.keyMap[Keys.A];
            OnButtonPressed(buttonAction);
        });
        InputManager.Instance().SetAction(ActionKey.EAST, delegate(InputAction.CallbackContext obj)
        {
            InputAction buttonAction = new InputAction();
            buttonAction = BattleInputManager.Instance.keyMap[Keys.B];
            OnButtonPressed(buttonAction);
        });
        InputManager.Instance().SetAction(ActionKey.WEST, delegate(InputAction.CallbackContext obj)
        {
            InputAction buttonAction = new InputAction();
            buttonAction = BattleInputManager.Instance.keyMap[Keys.X];
            OnButtonPressed(buttonAction);
        });
        InputManager.Instance().SetAction(ActionKey.LT, delegate(InputAction.CallbackContext obj)
        {
            InputAction buttonAction = new InputAction();
            buttonAction = BattleInputManager.Instance.keyMap[Keys.LT];
            OnButtonPressed(buttonAction);
        });
        InputManager.Instance().SetAction(ActionKey.RT, delegate(InputAction.CallbackContext obj)
        {
            InputAction buttonAction = new InputAction();
            buttonAction = BattleInputManager.Instance.keyMap[Keys.RT];
            OnButtonPressed(buttonAction);
        });
    }

    private void Update()
    {
        if (!enableKeyboardLaneFallback || Keyboard.current == null || buttons == null)
        {
            return;
        }

        if (Keyboard.current.aKey.wasPressedThisFrame)
        {
            OnLanePressed(BMButtonPrefab.Cell.Cell1);
        }

        if (Keyboard.current.sKey.wasPressedThisFrame)
        {
            OnLanePressed(BMButtonPrefab.Cell.Cell2);
        }

        if (Keyboard.current.dKey.wasPressedThisFrame)
        {
            OnLanePressed(BMButtonPrefab.Cell.Cell3);
        }

        if (Keyboard.current.jKey.wasPressedThisFrame)
        {
            OnLanePressed(BMButtonPrefab.Cell.Cell4);
        }

        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            OnLanePressed(BMButtonPrefab.Cell.Cell5);
        }

        if (Keyboard.current.lKey.wasPressedThisFrame)
        {
            OnLanePressed(BMButtonPrefab.Cell.Cell6);
        }
    }

    void BuildSpatialJudgmentService(float approachDurationSeconds)
    {
        var layout = BattlefieldLayout.CreateStandard(spatialLaneSpacing, spatialRowOffset, spatialToleranceRadius);
        var travelSettings = new NoteTravelSettings(approachDurationSeconds, spatialDespawnAfterHitSeconds);
        var config = new SpatialJudgmentConfig(spatialPerfectPercent, spatialGoodPercent, spatialBadPercent);
        var motionService = new NoteMotionService(layout, travelSettings);
        noteJudgmentService = new SpatialJudgmentService(layout, motionService, config);
    }

    public void SubcribeButton(BattleButton battleButton)
    {
        Buttons newButton = new Buttons
        {
            button = battleButton,
            buttonKeys = BattleInputManager.Instance.keyMap[battleButton.buttonAction]
        };

        if (buttons.Contains(newButton)) return;

        buttons.Add(newButton);
        if (battleUIManager != null)
        {
            battleUIManager.ShowFeedback("READY " + battleButton.cell, Color.yellow);
        }

        Debug.Log($"BattleManager: pulsante in finestra {battleButton.buttonAction} su {battleButton.cell}.");
    }

    public void Unsubscribe(BattleButton battleButton)
    {
        if (buttons.Any(b => b.button == battleButton))
        {
            buttons.Remove(buttons.Last(b => b.button == battleButton));
        }
    }

    void OnButtonPressed(InputAction context)
    {
        var list = buttons.Where(c => c.buttonKeys == context).ToList();
        ResolvePressedButtons(list, context != null ? context.name : "unknown");
    }

    void OnLanePressed(BMButtonPrefab.Cell cell)
    {
        var list = buttons.Where(c => c.button != null && c.button.cell == cell).ToList();
        ResolvePressedButtons(list, cell.ToString());
    }

    void ResolvePressedButtons(List<Buttons> list, string inputName)
    {
        if (list.Any())
        {
            var ele = list.Last();
            JudgmentResult judgmentResult;
            bool hasSpatialResult = TryEvaluateSpatialJudgment(ele.button, out judgmentResult);

            if (hasSpatialResult && !judgmentResult.IsHit)
            {
                ResetCounter();
                if (battleUIManager != null)
                {
                    battleUIManager.ShowFeedback("MISS " + ele.button.cell, Color.red);
                }

                Debug.Log(
                    "BattleManager: spatial miss su " + ele.button.cell +
                    " delta=" + judgmentResult.DeltaSeconds.ToString("0.000") +
                    " input=" + inputName + ".");
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
                    GetFeedbackText(ele.button, hasSpatialResult, judgmentResult),
                    GetFeedbackColor(hasSpatialResult, judgmentResult));
            }

            Debug.Log(GetHitLog(ele.button, inputName, counter, hasSpatialResult, judgmentResult));
            DamageRoutine(ele.button.cell, GetDamageMultiplier(hasSpatialResult, judgmentResult));
            ele.button.KillButton();
            Unsubscribe(ele.button);
        }
        else
        {
            ResetCounter();
            if (battleUIManager != null)
            {
                battleUIManager.ShowFeedback("MISS " + inputName, Color.red);
            }

            Debug.Log($"BattleManager: input {inputName} fuori finestra. Counter reset.");
        }
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

    string GetHitLog(BattleButton button, string inputName, int currentCounter, bool hasSpatialResult, JudgmentResult result)
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
        
        float constToUse = player.actualHealth > 0 ? 1 : deadPlayerModificator;

        foreach (var ene in enemiesToAttach)
        {
            float damageCalc =
                (((player.attack * player.attackBuff) - (enemy.defence * enemy.defenceBuff)) + player.damageConstant) *
                player.voteMultiplayer * counter * player.positionMultiplayer * judgmentMultiplier;

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
