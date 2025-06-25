using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LiquidBottle : MonoBehaviour
{
    [Header("Reference to the liquid material")]
    public SpriteRenderer liquidRenderer;
    public Color[] possibleColors;

    private List<Color> fillColors = new List<Color>();
    private List<float> fillStarts = new List<float>();
    private List<float> fillEnds = new List<float>();
    private Material liquidMaterial;

    private void Awake()
    {
        liquidMaterial = Instantiate(liquidRenderer.sharedMaterial);
        liquidRenderer.material = liquidMaterial;
    }

    public void InitializeFill(bool empty, List<Color> presetColors = null)
    {
        fillColors.Clear();
        fillStarts.Clear();
        fillEnds.Clear();

        if (empty)
        {
            for (int i = 0; i < 4; i++)
            {
                fillColors.Add(Color.clear);
                fillStarts.Add(0f);
                fillEnds.Add(0f);
            }
        }
        else
        {
            List<Color> colors = presetColors ?? GenerateRandomStack();

            float fillHeight = 0f;
            for (int i = 0; i < colors.Count; i++)
            {
                fillColors.Add(colors[i]);
                fillStarts.Add(fillHeight);
                fillHeight += 0.25f;
                fillEnds.Add(fillHeight);
            }

            // Fill remaining slots with transparent
            for (int i = colors.Count; i < 4; i++)
            {
                fillColors.Add(Color.clear);
                fillStarts.Add(0f);
                fillEnds.Add(0f);
            }
        }

        Debug.Log($"[LiquidBottle] Bottle {(empty ? "is EMPTY" : "Initialized with FILL")}");
        ApplyFillToMaterial();
    }

    private List<Color> GenerateRandomStack()
    {
        List<Color> stack = new List<Color>();
        for (int i = 0; i < 4; i++)
        {
            stack.Add(possibleColors[Random.Range(0, possibleColors.Length)]);
        }
        return stack;
    }

    public Color GetTopColor()
    {
        for (int i = fillColors.Count - 1; i >= 0; i--)
        {
            if (fillEnds[i] > fillStarts[i])
                return fillColors[i];
        }
        return Color.clear;
    }

    public bool IsEmpty()
    {
        return fillEnds.TrueForAll(end => end == 0f);
    }

    public bool IsFull()
    {
        return fillEnds.Count(c => c > 0f) >= 4;
    }

    public bool CanPourInto(LiquidBottle target)
    {
        if (target.IsFull()) return false;
        Color myTop = GetTopColor();
        Color theirTop = target.GetTopColor();

        return target.IsEmpty() || myTop == theirTop;
    }

    public void PourTopInto(LiquidBottle target)
    {
        for (int i = fillColors.Count - 1; i >= 0; i--)
        {
            if (fillEnds[i] > fillStarts[i])
            {
                Color topColor = fillColors[i];
                float volume = fillEnds[i] - fillStarts[i];

                // Clear my top segment
                fillStarts[i] = 0f;
                fillEnds[i] = 0f;
                fillColors[i] = Color.clear;

                // Add to target
                for (int j = 0; j < 4; j++)
                {
                    if (target.fillEnds[j] == 0f)
                    {
                        target.fillColors[j] = topColor;
                        target.fillStarts[j] = j * 0.25f;
                        target.fillEnds[j] = target.fillStarts[j] + volume;
                        break;
                    }
                }

                break;
            }
        }

        ApplyFillToMaterial();
        target.ApplyFillToMaterial();
    }

    public float GetFillStart(int index) => fillStarts[index];
    public float GetFillEnd(int index) => fillEnds[index];
    public Color GetColor(int index) => fillColors[index];

    public string GetTopSegmentColorName()
    {
        return GetColorName(GetTopColor());
    }

    private string GetColorName(Color color)
    {
        if (color == Color.red) return "Red";
        if (color == Color.green) return "Green";
        if (color == Color.blue) return "Blue";
        if (color == Color.yellow) return "Yellow";
        if (color == Color.cyan) return "Cyan";
        if (color == Color.magenta) return "Magenta";
        if (color == Color.white) return "White";
        if (color == Color.black) return "Black";
        if (color == new Color(1f, 0.5f, 0f)) return "Orange";
        return $"Unknown ({ColorUtility.ToHtmlStringRGB(color)})";
    }

    private void ApplyFillToMaterial()
    {
        for (int i = 0; i < 4; i++)
        {
            liquidMaterial.SetColor($"_Color{i + 1}", fillColors[i]);
            liquidMaterial.SetFloat($"_FillStart{i + 1}", fillStarts[i]);
            liquidMaterial.SetFloat($"_FillEnd{i + 1}", fillEnds[i]);
        }
    }

    public Material GetLiquidMaterial()
    {
        return liquidMaterial;
    }

}
