using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BarUI : MonoBehaviour {

    [Header("Set in Inpector")]
    public string barType = "Bar Type";
    public Color barColor = Color.red;
    public float valueMax = 100f;
    public float curBar = 0f;
    public Image barIn;
    public Text barText;


    [Header("Set Dynamically")]
    public float maxWidth;
    public float u = -1;
    private void Awake()
    {
        maxWidth = barIn.rectTransform.rect.width;
        barText.text = barType + " " + RoundFirstPoint(valueMax) + " / " + RoundFirstPoint(valueMax);
        barIn.color = barColor;
    }

    private void FixedUpdate()
    {
        if (u == -1)
        {
            return;
        }

        Vector2 tempRect = barIn.GetComponent<RectTransform>().sizeDelta;
        tempRect.x = Mathf.Lerp(tempRect.x, maxWidth * u, 0.1f);
        barIn.GetComponent<RectTransform>().sizeDelta = tempRect;
    }

    public void SetBarCurValue(float barValue)
    {
        u = RoundFirstPoint(barValue) / RoundFirstPoint(valueMax);
        curBar = RoundFirstPoint(barValue);
        barText.text = barType + " " + curBar.ToString("F1") + " / " + RoundFirstPoint(valueMax).ToString("F1");
    }

    public void SetInit(float barValueMax)
    {
        valueMax = RoundFirstPoint(barValueMax);
        curBar = valueMax;
        barText.text = barType + " " + curBar.ToString("F1") + " / " + valueMax.ToString("F1");
    }

    public void SetBarMax(float barValueMax)
    {
        valueMax = RoundFirstPoint(barValueMax);
        barText.text = barType + " " + curBar.ToString("F1") + " / " + valueMax.ToString("F1");
    }

    public float RoundFirstPoint(float value)
    {
        return Mathf.Round(value * 10) / 10;
    }
}
