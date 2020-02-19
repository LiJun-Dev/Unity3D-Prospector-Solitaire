using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    public static Scoreboard S;

    [Header("Set in Inspector")]
    public GameObject prefabFloatingScore;

    [Header("Set Dynamically")]
    [SerializeField] private int _score = 0;
    [SerializeField] private string _scoreString;

    private Transform canvasTrans;

    public int score {
        get {
            return(_score);
        }
        set {
            _score = value;
            scoreString = _score.ToString("N0");
        }
    }

    //这个属性用来设置Text.text的值
    public string scoreString {
        get {
            return(_scoreString);
        }
        set {
            _scoreString = value;
            GetComponent<Text>().text = _scoreString;
        }
    }

    void Awake()
    {
        if(S == null) {
            S = this;
        } else {
            Debug.LogError("ERROR: Scoreboard.Awake(): S is already set!");
        }
        canvasTrans = transform.parent;
    }

    //when called by SendMessage, this adds the fs.score to this.score
    public void FSCallback(FloatingScore fs) {
        score += fs.score;
    }

    //
    public FloatingScore CreateFloatingScore(int amt, List<Vector2> pts) {
        GameObject go = Instantiate<GameObject>(prefabFloatingScore);   //得到预设
        // GameObject go = Instantiate(prefabFloatingScore) as gameObject; 
        go.transform.SetParent(canvasTrans);
        FloatingScore fs = go.GetComponent<FloatingScore>();   //得到预设的脚本组件中的类
        fs.score = amt;
        fs.reportFinishTo = this.gameObject;
        fs.Init(pts);
        return(fs);
    }
}
