using UnityEngine;

public class ComputerFocusClick : MonoBehaviour
{
    [Header("What you click")]
    public Collider computerCollider;

    [Header("Where camera moves to")]
    public Transform focusPoint; // position + rotation target

    [Header("How fast it moves")]
    public float moveTime = 0.6f;

    [Header("Optional: UI that appears when focused")]
    public GameObject computerUIScreen;

    private Vector3 startPos;
    private Quaternion startRot;
    private bool focused;
    private bool moving;

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;

        if (computerUIScreen != null)
            computerUIScreen.SetActive(false);
    }

    void Update()
    {
        if (moving) return;

        // Listen for a mouse click
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 200f))
            {
                // If we hit the computer
                if (hit.collider == computerCollider)
                {
                    if (focused)
                    {
                        // Already focused? Move back to start
                        StartCoroutine(MoveCamera(startPos, startRot, false));
                    }
                    else
                    {
                        // Not focused? Move to the focus point
                        StartCoroutine(MoveCamera(focusPoint.position, focusPoint.rotation, true));
                    }
                }
            }
        }
    }

    System.Collections.IEnumerator MoveCamera(Vector3 targetPos, Quaternion targetRot, bool toFocused)
    {
        moving = true;

        // If backing out, hide UI immediately so it doesn't block the view/click
        if (!toFocused && computerUIScreen != null)
            computerUIScreen.SetActive(false);

        Vector3 p0 = transform.position;
        Quaternion r0 = transform.rotation;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / moveTime;
            transform.position = Vector3.Lerp(p0, targetPos, t);
            transform.rotation = Quaternion.Slerp(r0, targetRot, t);
            yield return null;
        }

        focused = toFocused;
        moving = false;

        // If moving TO focus, show UI after movement completes
        if (focused && computerUIScreen != null)
            computerUIScreen.SetActive(true);
    }
}