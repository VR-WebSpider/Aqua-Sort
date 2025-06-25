using UnityEngine;
using System.Collections.Generic;

public class BottleSpawner : MonoBehaviour
{
    public GameObject bottlePrefab;
    public int rows = 2;
    public int columns = 4;
    public float spacingX = 5f;
    public float spacingY = 3.5f;

    [Tooltip("Number of empty bottles to spawn randomly.")]
    public int numberOfEmptyBottles = 2;

    private void Start()
    {
        SpawnBottles();
    }

    private void SpawnBottles()
    {
        float totalWidth = (columns - 1) * spacingX;
        float totalHeight = (rows - 1) * spacingY;

        Vector2 centerOffset = new Vector2(-totalWidth / 2f, totalHeight / 2f);

        List<int> allIndices = new List<int>();
        for (int i = 0; i < rows * columns; i++)
            allIndices.Add(i);

        List<int> emptyBottleIndices = new List<int>();
        for (int i = 0; i < numberOfEmptyBottles; i++)
        {
            int randomIndex = Random.Range(0, allIndices.Count);
            emptyBottleIndices.Add(allIndices[randomIndex]);
            allIndices.RemoveAt(randomIndex);
        }

        int currentIndex = 0;

        for (int row = 0; row < rows; row++)
        {
            for (int col = 0; col < columns; col++)
            {
                Vector3 spawnPosition = new Vector3(
                    col * spacingX + centerOffset.x,
                    -row * spacingY + centerOffset.y,
                    0f
                );

                GameObject bottleObj = Instantiate(bottlePrefab, spawnPosition, Quaternion.identity);

                BottleInteraction interaction = bottleObj.GetComponent<BottleInteraction>();
                interaction.spawnPosition = spawnPosition;
                interaction.spawnIndex = currentIndex;

                LiquidBottle liquid = bottleObj.GetComponent<LiquidBottle>();
                bool shouldBeEmpty = emptyBottleIndices.Contains(currentIndex);
                liquid.InitializeFill(shouldBeEmpty);

                // Log bottle state after fill
                Debug.Log($"Bottle {currentIndex}: {(shouldBeEmpty ? "Empty" : "Filled")}");
                if (!shouldBeEmpty)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        float start = liquid.GetFillStart(i);
                        float end = liquid.GetFillEnd(i);
                        Color color = liquid.GetColor(i);

                        if (end > start)
                        {
                            string colorName = ColorUtility.ToHtmlStringRGB(color); // fallback
                            colorName = GetColorName(color);
                            Debug.Log($"  Segment {i + 1}: {colorName} from {start:F2} to {end:F2}");
                        }
                    }
                }

                currentIndex++;
            }
        }
    }

    // Optional: better name mapping for known colors
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
}
