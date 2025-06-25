using UnityEngine;

[ExecuteAlways]
public class LiquidWobble : MonoBehaviour
{
    [Range(0.1f, 20f)]
    public float wobbleSpeed = 5f;

    private Material liquidMaterial;
    private float smoothedAngle = 0f;

    void Start()
    {
        LiquidBottle liquidBottle = GetComponent<LiquidBottle>();
        if (liquidBottle != null)
        {
            liquidMaterial = liquidBottle.GetLiquidMaterial();  // Get shared material instance
        }

        if (liquidMaterial == null)
        {
            Debug.LogWarning($"[LiquidWobble] Material not found on: {gameObject.name}");
        }
    }

    void Update()
    {
        if (liquidMaterial == null) return;

        float targetAngle = transform.eulerAngles.z * Mathf.Deg2Rad;
        smoothedAngle = Mathf.LerpAngle(smoothedAngle, targetAngle, Time.deltaTime * wobbleSpeed);
        liquidMaterial.SetFloat("_BottleAngle", smoothedAngle);
    }
}
