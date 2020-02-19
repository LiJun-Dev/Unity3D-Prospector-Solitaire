using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum eFSState {
    idle,
    pre,
    active,
    post
}

//建立一个贝塞尔曲线运动轨迹来设置FloatScore在游戏界面中的移动
public class FloatingScore : MonoBehaviour  
{
    [Header("Set Dynamically")]
    public eFSState state = eFSState.idle;

    [SerializeField]
    protected int _score = 0;
    public string scoreString;

    //该属性用来获取或者设定_score的值
    public int score {
        get {
            return(_score);
        }
        set {
            _score = value;
            scoreString = _score.ToString("N0");
            GetComponent<Text>().text = scoreString;
        }
    }

    public List<Vector2> bezierPts;
    public List<float> fontSizes;
    public float timeStart = 1f;
    public float timeDuration = 1f;
    public string easingCurve = Easing.InOut;  //Using Easing in Utils.py
    public GameObject reportFinishTo = null;
    public RectTransform rectTrans;    //this.gameObject的其中一个component
    public Text txt;    //this.gameObject的其中一个component

    //初始化贝塞尔曲线的两个点、开始、持续时间
    public void Init(List<Vector2> ePts, float eTimeS = 0, float eTimeD = 1) 
    {
        rectTrans = GetComponent<RectTransform>();
        rectTrans.anchoredPosition = Vector2.zero;
        txt = GetComponent<Text>();
        bezierPts = new List<Vector2>(ePts);

        if(ePts.Count == 1) {
            transform.position = ePts[0];   //如果输入的List只有一个点，把this.gameObject的位置设置成该点的位置
            return;
        }

        if(eTimeS == 0) eTimeS = Time.time;
        timeStart = eTimeS;
        timeDuration = eTimeD;
        state = eFSState.pre;  //准备移动的阶段
    }

    public void FSCallback(FloatingScore fs) {
        //当这个回传函数被SendMessage调用，score属性被重新设置
        score += fs.score;
    }

    // Update is called once per frame
    void Update()
    {
        if(state == eFSState.idle) return;
        //设置贝塞尔函数的插值系数，和时间相关
        float u = (Time.time - timeStart) / timeDuration;
        //使用Easing中的方法来Curve u的值
        float uC = Easing.Ease(u, easingCurve);
        if(u < 0) {
            //说明还没到timeState的时间
            state = eFSState.pre;
            txt.enabled = false;     //把显示txt的HighScore先隐藏
        } else {
            if(u > 1) {
                uC = 1;
                state = eFSState.post;
                if(reportFinishTo != null) {
                    // 如果存在一个回传函数，用SendMessage来调用FSCallback方法
                    reportFinishTo.SendMessage("FSCallback", this);
                    Destroy(gameObject);
                } else {
                    //如果没有可回传的
                    state = eFSState.idle;
                }
            } else {
                // u在0到1之间
                state = eFSState.active;
                txt.enabled = true;
            }
            Vector2 pos = Utils.Bezier(uC, bezierPts);
            rectTrans.anchorMin = rectTrans.anchorMax = pos;
            if(fontSizes != null && fontSizes.Count > 0) {
                //如果fontSizes有值，则把GUIText的fontSize设置成按时间变化
                int size = Mathf.RoundToInt(Utils.Bezier(uC, fontSizes));
                GetComponent<Text>().fontSize = size;
            }
        }
    }
}