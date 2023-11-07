using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UiManager : MonoBehaviour
{
    static UiManager instance;
    
    public static UiManager Instance
    {
        get => instance;
        private set => instance = value;
    }

    public List<TextMeshProUGUI> enemies;

    public List<TextMeshProUGUI> players;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 3; i++)
        {
            enemies[i].text = BattleManager.Instance.enemies[i].actualHealth + " / " +
                         BattleManager.Instance.enemies[i].totalHealth;
            players[i].text = BattleManager.Instance.players[i].actualHealth + " / " +
                              BattleManager.Instance.players[i].totalHealth;
        }
    }
}
