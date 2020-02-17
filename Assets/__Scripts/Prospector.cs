using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Prospector : MonoBehaviour
{
    public static Prospector S;
    [Header("Set in Inspector")]
    public TextAsset deckXML;   //xml文件
    public TextAsset layoutXML;   //xml文件
    public float xOffset = 3f;
    public float yOffset = -2.5f;
    public Vector3 layoutCenter;

    [Header("Set Dynamically")]
    public Deck deck;  //脚本类
    public Layout layout;  //脚本类
    public List<CardProspector> drawPile;  //脚本类组成的一个list
    public Transform layoutAnchor;
    public CardProspector target;
    public List<CardProspector> tableau;
    public List<CardProspector> discardPile;

    void Awake() {
        S = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        deck = GetComponent<Deck>();   //此脚本和Deck脚本同为MainCarema组件，此处调用Deck脚本
        deck.InitDeck(deckXML.text);   //调用方法初始化Deck
        Deck.Shuffle(ref deck.cards);

        // Card c;  //调用Card脚本类
        // for(int cNum=0; cNum<deck.cards.Count; cNum++) {  //把洗牌后的卡，放到新的位置
        //     c = deck.cards[cNum];
        //     c.transform.localPosition = new Vector3((cNum%13)*3, cNum/13*4, 0);
        // }

        layout = GetComponent<Layout>();
        layout.ReadLayout(layoutXML.text);

        //把deck.cards中的所有卡转换成CardProspector
        drawPile = ConvertListCardsToListCardProspectors(deck.cards);
        LayoutGame();
    }

    List<CardProspector> ConvertListCardsToListCardProspectors(List<Card> lCD) {
        List<CardProspector> lCP = new List<CardProspector>();
        CardProspector tCP;
        foreach(Card tCD in lCD) {
            tCP = tCD as CardProspector;   //把Card转换成CardProspector
            lCP.Add(tCP);
        }
        return(lCP);
    }

    CardProspector Draw() {
        CardProspector cd = drawPile[0];
        drawPile.RemoveAt(0);
        return(cd);  // 取牌堆上最上面的一张牌
    }

    //此函数是摆好桌面上的最初排列
    void LayoutGame() {
        //先创建一个新的GameObject，用来作为桌面tableau的锚定
        if(layoutAnchor == null) {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

        //建立起矿堆 tableau
        CardProspector cp;
        foreach(SlotDef tSD in layout.slotDefs) {
            cp = Draw();
            cp.faceUp = tSD.faceUp;
            cp.transform.parent = layoutAnchor;  //把deck.deckAnchor替换为layoutAnchor
            cp.transform.localPosition = new Vector3(
                layout.multiplier.x * tSD.x,
                layout.multiplier.y * tSD.y,
                -tSD.layerID);
            cp.layoutID = tSD.id;
            cp.slotDef = tSD;
            cp.state = eCardState.tableau;
            cp.SetSortingLayerName(tSD.layerName);
            tableau.Add(cp);
        }

        //找到tableau中哪些卡被哪些卡挡住
        foreach(CardProspector tCP in tableau) {
            foreach(int hid in tCP.slotDef.hiddenBy) {
                cp = FindCardByLayoutID(hid);
                tCP.hiddenBy.Add(cp);
            }
        }

        //抽取第一张target卡片
        MoveToTarget(Draw());

        //建立好初始的抽卡堆
        UpdateDrawPile();  
    }

    //把xml文件中的id装化成CardProspector中的id
    CardProspector FindCardByLayoutID(int layoutID) {
        foreach(CardProspector tCP in tableau) {
            if(tCP.layoutID == layoutID) {
                return(tCP);
            }
        }
        return(null);
    }

    //把矿中的卡翻面
    void SetTableauFaces() {
        foreach(CardProspector cd in tableau) {
            bool faceUp = true;
            foreach(CardProspector cover in cd.hiddenBy) {
                if(cover.state == eCardState.tableau) {
                    faceUp = false;
                }
            }
            cd.faceUp = faceUp;
        }
    }

    //把目前的target移动到废弃牌堆里
    void MoveToDiscard(CardProspector cd) {
        cd.state = eCardState.discard;
        discardPile.Add(cd);
        cd.transform.parent = layoutAnchor;
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID+0.5f
        );
        cd.faceUp = true;
        //set the depth sorting
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(-100 + discardPile.Count);
    }

    //从待抽取的卡堆中抽取一张新的target
    void MoveToTarget(CardProspector cd) {
        if(target != null) MoveToDiscard(target);
        target = cd;
        cd.state = eCardState.target;
        cd.transform.parent = layoutAnchor;   //先移动到世界坐标系的位置
        //把抽取的卡片移动到target的位置，注意是与parent: layerAnchor的相对位置
        cd.transform.localPosition = new Vector3(
            layout.multiplier.x * layout.discardPile.x,
            layout.multiplier.y * layout.discardPile.y,
            -layout.discardPile.layerID
        );
        cd.faceUp = true;
        cd.SetSortingLayerName(layout.discardPile.layerName);
        cd.SetSortOrder(0);
    }

    //重新安置待抽取的卡堆
    void UpdateDrawPile() {
        CardProspector cd;
        for(int i=0; i<drawPile.Count; i++) {
            cd = drawPile[i];
            cd.transform.parent = layoutAnchor;

            Vector2 dpStagger = layout.drawPile.stagger;
            cd.transform.localPosition = new Vector3(
                layout.multiplier.x * (layout.drawPile.x + i * dpStagger.x),
                layout.multiplier.y * (layout.drawPile.y + i * dpStagger.y),
                -layout.drawPile.layerID + 0.1f*i
            );
            cd.faceUp = false;
            cd.state = eCardState.drawpile;
            cd.SetSortingLayerName(layout.drawPile.layerName);
            cd.SetSortOrder(-10*i);
        }
    }

    public void CardClicked(CardProspector cd) {
        //根据卡的状态
        switch(cd.state) {
            case eCardState.target:
                break;
            case eCardState.drawpile:
                MoveToDiscard(target);
                MoveToTarget(Draw());
                UpdateDrawPile();
                ScoreManager.EVENT(eScoreEvent.draw);
                break;
            case eCardState.tableau:
                bool validMatch = true;
                if(!cd.faceUp) {
                    validMatch = false;
                }
                if(!AdjacentRank(cd, target)) {
                    validMatch = false;
                }
                if(!validMatch) return;

                //if valid
                tableau.Remove(cd);
                MoveToTarget(cd);
                SetTableauFaces();
                ScoreManager.EVENT(eScoreEvent.mine);
                break;
        }
        CheckForGameOver();
    }

    void CheckForGameOver() {
        if(tableau.Count == 0) {
            GameOver(true);
            return;
        }
        if(drawPile.Count > 0) return;
        foreach(CardProspector cd in tableau) {
            if(AdjacentRank(cd, target)) {
                return;
            }
        }
        GameOver(false);
    }

    void GameOver(bool won) {
        if(won) {
            print("Game Over. You won! :)");
            ScoreManager.EVENT(eScoreEvent.gameWin);
        } else {
            print("Game Over. You Lost. :(");
            ScoreManager.EVENT(eScoreEvent.gameLoss);
        }

        SceneManager.LoadScene("__Prospector_Scene_0");
    }
    
    public bool AdjacentRank(CardProspector c0, CardProspector c1) {
        if(!c0.faceUp || !c1.faceUp) return(false);
        if(Mathf.Abs(c0.rank - c1.rank) == 1) return(true);
        if(c0.rank == 1 && c1.rank == 13) return(true);
        if(c0.rank == 13 && c1.rank == 1) return(true);
        return(false);
    }
}
