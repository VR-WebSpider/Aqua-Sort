using UnityEngine;

public class OutsideClickDeselect : MonoBehaviour
{
    void Update()
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

            if (hit.collider == null || hit.collider.GetComponent<BottleInteraction>() == null)
            {
                if (BottleInteraction.SelectedBottle != null)
                {
                    BottleInteraction.SelectedBottle.Deselect();
                    BottleInteraction.SelectedBottle = null;
                }
            }
        }
    }
}
