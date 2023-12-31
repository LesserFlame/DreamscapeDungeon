using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class BattleManager : Singleton<BattleManager>
{
    [Header("Music")]
    //[SerializeField] AudioClip music;
    [SerializeField] MusicPlayer musicPlayer;

    [Header("Actors")]
    [SerializeField] public PlayerActor player;
    [SerializeField] public List<EnemyActor> enemies;
    [SerializeField] public List<EnemyActor> activeEnemies;
    public List<BattleActor> actorOrder = new List<BattleActor>();

    [Header("UI")]
    [SerializeField] private BattleUIManager ui;
    

    

    //private int enemyCount = 0;

    public enum BattleState
    { 
        BATTLE_INACTIVE,
        BATTLE_START,
        BATTLE_STOP, 
        BATTLE_WON,
        BATTLE_LOST,
        BATTLE_TURNORDER,
        BATTLE_NEXTTURN,
        BATTLE_WAIT,
        PLAYER_DECISION,
        PLAYER_ACTION,
        ENEMY_DECISION,
        ENEMY_ACTION
    }

    BattleState state = BattleState.BATTLE_INACTIVE;
    BattleState nextState = BattleState.BATTLE_INACTIVE;
    float stateTimer = 0;
    bool victory = false;
    bool active = false;

    void Start()
    {
        //if (musicPlayer != null)
        //{
        //    musicSource = new AudioSource();
        //}
        ui.HideAll();
    }

    void Update()
    {
        //if (player.HP <= 0 && active) state = BattleState.BATTLE_STOP;
        switch (state)
        {
            case BattleState.BATTLE_INACTIVE:
                {
                    //nothing
                    if (ui.active) ui.HideAll();
                }
                break;
            case BattleState.BATTLE_START:
                if (musicPlayer != null)
                {
                    Invoke("PlayBattleMusic", 1.01f);
                }
                ui.HideAll();
                ui.detectInput = false;
                state = BattleState.BATTLE_WAIT;
                nextState = BattleState.BATTLE_TURNORDER;
                stateTimer = 2;
                //state = BattleState.PLAYER_DECISION;
                break;

            case BattleState.PLAYER_DECISION:
                //wait for user input
                ui.detectInput = true;
                ui.active = true;
                if (player.HP <= 0) { state = BattleState.BATTLE_STOP; }
                break;

            case BattleState.PLAYER_ACTION:
                //wait for player action
                ui.HideAll();
                ui.detectInput = false;
                if (stateTimer <= 0)
                {
                    OnPlayerAction();
                    actorOrder.RemoveAt(0);
                }
                break;
            case BattleState.ENEMY_DECISION:
                //wait for enemy to choose action
                if (stateTimer <= 0) OnEnemyDecide();
                break;

            case BattleState.ENEMY_ACTION:
                //wait for enemy action
                //Debug.Log(state.ToString());
                //if (stateTimer <= 0)
                {
                    OnEnemyAction();
                    if (actorOrder.Count > 0) actorOrder.RemoveAt(0); 
                    state = BattleState.BATTLE_NEXTTURN;
                }
                break;
            case BattleState.BATTLE_TURNORDER:
                OnCalculateTurns();
                break;
            case BattleState.BATTLE_NEXTTURN:
                if (stateTimer <= 0 )
                {
                    if (player.HP <= 0) state = BattleState.BATTLE_STOP;
                    if(actorOrder.Count <= 0) 
                    {
                        state = BattleState.BATTLE_TURNORDER;
                        break;
                    }
                    if (actorOrder[0].CompareTag("Player"))
                    {
                        ui.OnShowButtons();
                        ui.OnSelectButton(0);
                        ui.OnPlaySound(3);
                        state = BattleState.PLAYER_DECISION;
                    }
                    else state = BattleState.ENEMY_DECISION;
                }
                break;
            case BattleState.BATTLE_WAIT:
                if (stateTimer <= 0) state = nextState;
                break;
            case BattleState.BATTLE_STOP:
                //decide win / loss
                //active = false;
                //Invoke("ResetBattle", 3);
                actorOrder.Clear();
                ui.HideAll();
                if (stateTimer <= 0 && victory) state = BattleState.BATTLE_WON;
                if (stateTimer <= 0 && !victory) state = BattleState.BATTLE_LOST;
                break;

            case BattleState.BATTLE_WON:
                //give player xp / gold, send to overworld position
                GameManager.Instance.BattleTransition();
                //GameManager.Instance.TransitionScreen();
                state = BattleState.BATTLE_INACTIVE;
                break;

            case BattleState.BATTLE_LOST:
                //game over, send back to home
                state = BattleState.BATTLE_INACTIVE;
                //GameManager.Instance.TransitionScreen();
                //GameManager.Instance.BattleTransition();
                //SceneManager.LoadScene("House");
                LoadingScreen.Instance.LoadScene(1);
                Invoke("ResetBattle", 3);
                break;
            default:
                break;
        }
        if (stateTimer > 0) stateTimer -= Time.deltaTime;
        //Debug.Log(state);
    }
    public void OnStartBattle(List<EnemyData> enemiesData) 
    {
        active = true;
        activeEnemies.Clear();
        int tempEnemy = 0;
        foreach (var enemy in enemiesData) 
        {
            if (tempEnemy < 3) enemies[tempEnemy].InitializeEnemy(enemy);
            activeEnemies.Add(enemies[tempEnemy]);
            tempEnemy++;
        }
        for (int i = tempEnemy; i < enemies.Count; i++) enemies[i].gameObject.SetActive(false);

        player.OnInitialize(FindFirstObjectByType<PlayerController>());
        ui.OnShowButtons();
        ui.OnSliderChanged(0, player.HP, player.maxHP, false);
        ui.OnSliderChanged(1, player.MP, player.maxMP, false);
        ui.OnSelectButton(0);
        ui.OnInitialize();
        state = BattleState.BATTLE_START;

        player.actions = GameManager.Instance.player.actions;
        ui.options[2] = activeEnemies.Count;

        ui.ActivateFireLight(true);
    }
    public void OnPlayerDecide(float wait)
    {
        state = BattleState.PLAYER_ACTION;
        stateTimer = wait;
        //Debug.Log(state);
    }
    public void OnPlayerAction()
    {
        if (active)
        {
            state = BattleState.BATTLE_NEXTTURN;
            stateTimer = 1;
        }
        else state = BattleState.BATTLE_STOP;
        //Debug.Log(state);
    }
    public void OnEnemyDecide()
    {
        state = BattleState.ENEMY_ACTION;
        stateTimer = 1;
        if (!active) state = BattleState.BATTLE_STOP;
        //Debug.Log(state);
    }
    public void OnEnemyAction()
    {
        //ui.OnShowButtons(true);
        //ui.OnSelectButton(0);
        if (actorOrder.Count > 0) actorOrder[0].OnDecide();
        //actorOrder.RemoveAt(0);
        stateTimer = 1;
        state = BattleState.BATTLE_NEXTTURN;
    }
    public void OnEnemyDeath()
    {
        ui.options[2] = activeEnemies.Count;
        if (activeEnemies.Count <= 0)
        {
            MusicManager.Instance.FadeSong();
            active = false;
            state = BattleState.BATTLE_STOP;
            stateTimer = 3;
            victory = true;
        }
        //else
        {//actorOrder.Remove();//OnCalculateTurns();
            foreach (var actor in actorOrder)
            {
                if (!actor.isActiveAndEnabled)
                {
                    actorOrder.Remove(actor);
                    break;
                }
            }
        }
    }
    public void OnPlayerDeath()
    {
        MusicManager.Instance.FadeSong();
        active = false;
        ui.HideAll();
        stateTimer = 4;
        state = BattleState.BATTLE_STOP;
        victory = false;
        player.initialized = false;
        //GameManager.Instance.TransitionScreen();

        ui.ActivateFireLight(false);
    }    
    public void OnPlayerFlee()
    {
        MusicManager.Instance.FadeSong(1);
        active = false;
        victory = false;
        //Invoke("ResetBattle", 2);
        state = BattleState.BATTLE_INACTIVE;
        GameManager.Instance.BattleTransition();
        //GameManager.Instance.TransitionScreen();
    }
    //public void OnNextTurn()
    //{
        
    //}
    //public void OnNextTurn()
    //{
        
    //}
    public void OnCalculateTurns()
    {
        if (active)
        {
            actorOrder.Clear();
            foreach (var actor in activeEnemies)
            {
                actorOrder.Add(actor);
                actor.GenerateTempSpeed();
            }
            player.GenerateTempSpeed();
            actorOrder.Add(player);
            actorOrder = actorOrder.OrderByDescending(actor => actor.tempSpeed).ToList();
            //foreach (var actor in actorOrder)
            //{
            //    Debug.Log(actor.name + ": " + actor.tempSpeed);
            //}
            if (actorOrder[0].CompareTag("Player"))
            {
                ui.OnShowButtons(true);
                ui.OnSelectButton(0);
                ui.OnPlaySound(3);
                state = BattleState.PLAYER_DECISION;
            }
            else state = BattleState.ENEMY_DECISION;
        }
        else state = BattleState.BATTLE_STOP;
    }
    public void ResetBattle()
    {
        //Debug.Log("Reset");
        active = false;
        activeEnemies.Clear();
        foreach (var enemy in enemies) enemy.gameObject.SetActive(false);
        ui.detectInput = false;
        GameManager.Instance.inBattle = false;
        //player.initialized = false;
    }

    private void PlayBattleMusic()
    {
        musicPlayer.PlaySong();
    }
}