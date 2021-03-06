﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

    public const int VISION_RADIUS = 2;
    public const int FOG_RADIUS = 4;
    public const int NIGHT_CYCLE = 5;

    public float levelStartDelay = 2f; 
    public float turnDelay;
    public static GameManager instance = null;
    [HideInInspector] public Vector3 playerPosition;
    [HideInInspector] public int playerFoodPoints;
    [HideInInspector] public bool enemiesMoving;
    [HideInInspector] public bool playerMoving;
    [HideInInspector] public bool playersTurn = true;

    private Text levelText;
    private GameObject levelImage;
    private BoardManager boardScript;
    private int level;
    private List<Enemy> enemies;
    private bool doingSetup;

    void Awake() {
        if (instance == null)
            instance = this;
        else if (instance != this)
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        enabled = true;
        level = 0;
        playerFoodPoints = 50;
        enemies = new List<Enemy>();
        boardScript = GetComponent<BoardManager>();
    }

    void InitGame() {
        doingSetup = true;

        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<Text>();
        levelText.text = "Day " + level;
        levelImage.SetActive(true);
        Invoke("HideLevelImage", levelStartDelay);
        
        enemies.Clear();
        boardScript.SetupScene(level);
    }

    private void HideLevelImage() {
        levelImage.SetActive(false);
        doingSetup = false;
    }

    public void GameOver(GameOverSource source) {
        if (source == GameOverSource.MOVE)
            levelText.text = "After " + level + " days, \n\nyou starved.";
        else if (source == GameOverSource.ENEMY)
            levelText.text = "After " + level + " days, \n\nyou were eaten alive.";
        levelImage.SetActive(true);
        enabled = false;
        Invoke("LoadTitle", 6f);
    }

    private void LoadTitle() {
        SoundManager.instance.musicSource.Play();
        SceneManager.LoadScene("TitleScene");
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (playersTurn || playerMoving || enemiesMoving || doingSetup)
            return;

        StartCoroutine(MoveEnemies());
    }

    public void AddEnemyToList(Enemy script) {
        enemies.Add(script);
    }

    IEnumerator MoveEnemies() {
        enemiesMoving = true;
        yield return new WaitForSeconds(turnDelay);

        if (enemies.Count > 0) {
            for (int i = 0; i < enemies.Count - 1; i++)
                enemies[i].SetNextEnemyToMove(enemies[i + 1]);
            enemies[0].MoveEnemy();
        }

        playersTurn = true;
        enemiesMoving = false;
    }

    //This is called each time a scene is loaded.
    void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode)
    {
        //Add one to our level number.
        level++;

        //Call InitGame to initialize our level.
        InitGame();
    }

    void OnEnable()
    {
        //Tell our ‘OnLevelFinishedLoading’ function to start listening for a scene change event as soon as this script is enabled.
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    void OnDisable()
    {
        //Tell our ‘OnLevelFinishedLoading’ function to stop listening for a scene change event as soon as this script is disabled.
        //Remember to always have an unsubscription for every delegate you subscribe to!
        SceneManager.sceneLoaded -= OnLevelFinishedLoading;
    }

    public void ApplyVision(Player player) {
        bool nigthTime = level % NIGHT_CYCLE == 0;
        int bonusVision = 0;
        if (!nigthTime)
            bonusVision += 1;
        boardScript.ApplyVision(nigthTime, player, VISION_RADIUS + bonusVision, FOG_RADIUS);
        if (nigthTime)
            boardScript.ApplyNight();
        else if (level % NIGHT_CYCLE == NIGHT_CYCLE - 1)
            boardScript.ApplyDusk();
        else if (level % NIGHT_CYCLE == 1)
            boardScript.ApplyDawn();
    }

    public List<Tile> GetBoardTiles() {
        return boardScript.boardTiles;
    }
}
