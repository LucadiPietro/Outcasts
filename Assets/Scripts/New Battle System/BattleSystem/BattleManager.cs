using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public List<Buttons> buttons;

    public BattleUIManager battleUIManager;

    public int counter = 0;

    public float deadPlayerModificator = 1;
    
    public float deadEnemyModificator = 1;

    [SerializeField] bool enableKeyboardLaneFallback = true;

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
        if (audioController != null && audioController.TryGetComponent(out PlayableDirector playableDirector))
        {
            playableDirector.Play();
        }
        else
        {
            Debug.LogWarning("BattleManager: AudioController o PlayableDirector non configurato.");
        }
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
        int count = list.Count();

        if (count > 0)
        {
            counter++;
            if (battleUIManager != null)
            {
                battleUIManager.UpdateCounter(counter);
            }
        }

        else
        {
            counter = 0;
            if (battleUIManager != null)
            {
                battleUIManager.UpdateCounter(counter);
            }
        }

        if (list.Any())
        {
            var ele = list.Last();

            if (battleUIManager != null)
            {
                battleUIManager.ShowFeedback("HIT " + ele.button.cell, Color.green);
            }

            Debug.Log($"BattleManager: preso pulsante {ele.button.buttonAction} su {ele.button.cell} con input {inputName}. Counter: {counter}");
            DamageRoutine(ele.button.cell);
            ele.button.KillButton();
            Unsubscribe(ele.button);
        }
        else
        {
            if (battleUIManager != null)
            {
                battleUIManager.ShowFeedback("MISS " + inputName, Color.red);
            }

            Debug.Log($"BattleManager: input {inputName} fuori finestra. Counter reset.");
        }
    }

    public void DamageRoutine(BMButtonPrefab.Cell cell)
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

            Attack(playerToConsider, enemyToConsider);
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

    private void Attack(Player player, Enemy enemy)
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
                player.voteMultiplayer * counter * player.positionMultiplayer;

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
