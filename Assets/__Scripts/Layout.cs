using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SlotDef {
    public float x;
    public float y;
    public bool faceUp = false;
    public string layerName = "Default";
    public int layerID = 0;
    public int id;
    public List<int> hiddenBy = new List<int>();
    public string type = "slot";
    public Vector2 stagger;     //未抓取排队的每张牌的错位
}

public class Layout : MonoBehaviour
{
    public PT_XMLReader xmlr;
    public PT_XMLHashtable xml;
    public Vector2 multiplier;   //offset of table
    public List<SlotDef> slotDefs;   //row0 - row3 所有的slotDefs
    public SlotDef drawPile;
    public SlotDef discardPile;
    public string[] sortingLayerNames = new string[] {"Row0", "Row1", "Row2", "Row3", "Draw", "Discard"};

    //读取LayoutXML.xml中的纸牌排列信息
    public void ReadLayout(string xmlTest) 
    {
        xmlr = new PT_XMLReader();    //调用PT_XMLReader脚本
        xmlr.Parse(xmlTest);
        xml = xmlr.xml["xml"][0];

        //读取缩放系数multiplier
        multiplier.x = float.Parse(xml["multiplier"][0].att("x"));
        multiplier.y = float.Parse(xml["multiplier"][0].att("y"));

        //读取slot中的信息
        SlotDef tSD;
        PT_XMLHashList slotsX = xml["slot"];
        for(int i=0; i<slotsX.Count; i++) {
            tSD = new SlotDef();
            if(slotsX[i].HasAtt("type")) {   //说明是两种牌堆之一
                tSD.type = slotsX[i].att("type");
            } else {           //说明是row1-row3
                tSD.type = "slot";
            }
            tSD.x = float.Parse(slotsX[i].att("x"));
            tSD.y = float.Parse(slotsX[i].att("y"));
            tSD.layerID = int.Parse(slotsX[i].att("layer"));
            //把layerID转换成text layerName
            tSD.layerName = sortingLayerNames[tSD.layerID];

            switch(tSD.type) {
                case "slot": 
                    tSD.faceUp = (slotsX[i].att("faceup") == "1");   //bool
                    tSD.id = int.Parse(slotsX[i].att("id"));
                    if(slotsX[i].HasAtt("hiddenby")) {      //对于不是在最上面的那几排
                        string[] hiding = slotsX[i].att("hiddenby").Split(',');
                        foreach(string s in hiding) {
                            tSD.hiddenBy.Add(int.Parse(s));
                        }
                    }
                    slotDefs.Add(tSD);   //加入了row0-row3中的一张牌
                    break;

                case "drawpile":
                    tSD.stagger.x = float.Parse(slotsX[i].att("xstagger"));
                    drawPile = tSD;
                    break;
                case "discardpile":
                    discardPile = tSD;
                    break;
            }
        }
    }
}
