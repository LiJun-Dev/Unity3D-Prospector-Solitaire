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
        //先创建一个新的GameObject，用来作为桌面table的锚定
        if(layoutAnchor == null) {
            GameObject tGO = new GameObject("_LayoutAnchor");
            layoutAnchor = tGO.transform;
            layoutAnchor.transform.position = layoutCenter;
        }

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
    }
}
