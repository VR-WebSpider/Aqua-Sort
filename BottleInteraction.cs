using System.Collections;
using UnityEngine;

public class BottleInteraction : MonoBehaviour
{
    public static BottleInteraction SelectedBottle;

    [Header("References")]
    public Transform bodyTransform;

    [Header("Pour Settings")]
    public float pourDuration = 0.4f;

    [HideInInspector] public Vector3 spawnPosition;
    [HideInInspector] public int spawnIndex;

    private Vector3 originalScale;
    private Vector3 originalPosition;
    private Quaternion originalRotation;

    private LiquidBottle liquidBottle;
    private bool isAnimating = false;

    private void Start()
    {
        originalScale = transform.localScale;
        originalPosition = transform.position;
        originalRotation = transform.rotation;
        liquidBottle = GetComponent<LiquidBottle>();
    }

    private void Update()
    {
        bool inputBegan = false;
        Vector2 inputPos = Vector2.zero;

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            inputBegan = true;
            inputPos = Input.GetTouch(0).position;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            inputBegan = true;
            inputPos = Input.mousePosition;
        }

        if (inputBegan)
        {
            Ray ray = Camera.main.ScreenPointToRay(inputPos);
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction);

            if (hit.collider != null && hit.collider.gameObject == gameObject)
            {
                HandleBottleSelection();
            }
        }
    }

    private void HandleBottleSelection()
    {
        if (isAnimating) return;

        if (SelectedBottle == null)
        {
            SelectedBottle = this;
            Highlight(true);
            Debug.Log($"Selected Bottle {spawnIndex} [{liquidBottle.GetTopSegmentColorName()}]");
        }
        else if (SelectedBottle == this)
        {
            Highlight(false);
            SelectedBottle = null;
            Debug.Log("Deselected current bottle");
        }
        else
        {
            LiquidBottle source = SelectedBottle.liquidBottle;
            LiquidBottle target = this.liquidBottle;

            Debug.Log($"Attempting to pour from Bottle {SelectedBottle.spawnIndex} to Bottle {spawnIndex}");

            if (source.CanPourInto(target))
            {
                SelectedBottle.Highlight(false);
                StartCoroutine(SelectedBottle.MoveToPourAbove(this, () =>
                {
                    source.PourTopInto(target);
                }));
            }
            else
            {
                Debug.LogWarning("Invalid pour: Colors don't match or target is full");
                SelectedBottle.Highlight(false);
                SelectedBottle = null;
            }
        }
    }

    public void Highlight(bool enable)
    {
        transform.localScale = enable ? originalScale * 1.1f : originalScale;
    }

    public void Deselect()
    {
        Highlight(false);
    }

    public IEnumerator MoveToPourAbove(BottleInteraction targetBottle, System.Action onPourComplete)
    {
        isAnimating = true;

        Vector3 targetPos = CalculatePourPosition(this, targetBottle);
        Quaternion targetRot = CalculatePourRotation(this, targetBottle);

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;

        float elapsed = 0f;
        while (elapsed < pourDuration)
        {
            float t = elapsed / pourDuration;
            transform.position = Vector3.Lerp(startPos, targetPos, t);
            transform.rotation = Quaternion.Lerp(startRot, targetRot, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = targetPos;
        transform.rotation = targetRot;

        yield return new WaitForSeconds(0.4f); // Simulate pour delay

        onPourComplete?.Invoke();

        // Return to original position
        elapsed = 0f;
        while (elapsed < pourDuration)
        {
            float t = elapsed / pourDuration;
            transform.position = Vector3.Lerp(targetPos, originalPosition, t);
            transform.rotation = Quaternion.Lerp(targetRot, originalRotation, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition;
        transform.rotation = originalRotation;

        isAnimating = false;
        SelectedBottle = null;
    }

    private Vector3 CalculatePourPosition(BottleInteraction source, BottleInteraction target)
    {
        Vector3 offset = new Vector3(-1f, 1.1f, 0f); // default offset
        float xDir = (target.spawnPosition.x >= source.spawnPosition.x) ? 1f : -1f;

        bool isVertical = Mathf.Approximately(source.spawnPosition.x, target.spawnPosition.x);

        if (isVertical)
        {
            offset.x = source.spawnPosition.x < 0f ? 1f : -1f;
        }
        else
        {
            offset.x *= xDir;
        }

        return target.spawnPosition + offset;
    }

    private Quaternion CalculatePourRotation(BottleInteraction source, BottleInteraction target)
    {
        Vector3 sourcePos = source.spawnPosition;
        Vector3 targetPos = target.spawnPosition;

        bool isVertical = Mathf.Approximately(sourcePos.x, targetPos.x);

        float zAngle;
        if (isVertical)
        {
            zAngle = sourcePos.x < 0f ? 85f : -85f;
        }
        else
        {
            zAngle = targetPos.x > sourcePos.x ? -85f : 85f;
        }

        return Quaternion.Euler(0f, 0f, zAngle);
    }
}
