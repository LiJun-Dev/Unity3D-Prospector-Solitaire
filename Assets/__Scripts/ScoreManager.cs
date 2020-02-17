using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum eScoreEvent {
    draw,
    mine,
    mineGold,
    gameWin,
    gameLoss
}

public class ScoreManager : MonoBehaviour
{
    private static ScoreManager S;
    public static int SCORE_FROM_PREV_ROUND = 0;
    public static int HIGH_SCORE = 0;

    [Header("Set Dynamically")]
    public int chain = 0;
    public int scoreRun = 0;
    public int score = 0;

    void Awake()
    {
        if(S == null) {
            S = this;
        } else {
            Debug.LogError("ERROR: ScoreManager.Awake(): S is already set!");
        }
        //检测目前的最高分
        if(PlayerPrefs.HasKey("ProspectorHighScore")) {
            HIGH_SCORE = PlayerPrefs.GetInt("ProspectorHighScore");
        }

        score += SCORE_FROM_PREV_ROUND;
        SCORE_FROM_PREV_ROUND = 0;
    }

    public static void EVENT(eScoreEvent evt) {
        //try catch stops an error from breaking your program
        try{
            S.Event(evt);
        } catch (System.NullReferenceException nre) {
            Debug.LogError("ScoreManager:EVENT() called while S=null. \n" + nre);
        }
    }

    void Event(eScoreEvent evt) {
        switch (evt) {
            case eScoreEvent.draw:
            case eScoreEvent.gameWin:
            case eScoreEvent.gameLoss:
                chain = 0;
                score += scoreRun;
                scoreRun = 0;
                break;
            case eScoreEvent.mine:
                chain++;                      // 1,2,3...每在tableau中抓一张牌，就累加一次
                scoreRun += chain;
                break;
            // case eScoreEvent.mineGold: 
        }
        switch (evt) {
            case eScoreEvent.gameWin:
                SCORE_FROM_PREV_ROUND = score;
                print("You won this round! Round score: " + score);
                break;
            case eScoreEvent.gameLoss:
                if(HIGH_SCORE < score) {
                    print("You got the high score! High score: " + score);
                } else {
                    print("Your final score for the game was: " + score);
                }
                break;
            default:
                print("score: "+score+" scoreRun: "+scoreRun+" chain: "+chain);
                break;
        }
    }

    public static int CHAIN {get {return S.chain;}}
    public static int SCORE {get {return S.score;}}
    public static int SCORE_RUN {get {return S.scoreRun;}}
}
