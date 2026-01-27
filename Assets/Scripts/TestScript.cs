using UnityEngine;

public class TestScript : MonoBehaviour
{
    private Camera mainCamera;
    public Camera uiCamera;

    public RectTransform testImage;

    void Awake()
    {
        mainCamera = Camera.main;
        Debug.Log(mainCamera.name);

    }

    [ContextMenu("Test")]
    public void Test()
    {

        mainCamera = Camera.main;

        Vector3 posViewPort = mainCamera.WorldToViewportPoint(transform.position);
        Debug.Log("Position in Main Camera Viewport: " + posViewPort);

        Vector2 screenPoint = mainCamera.WorldToScreenPoint(transform.position);
        Debug.Log("Position on Main Camera Screen: " + screenPoint);

        Vector3 posViewPortUI = uiCamera.WorldToViewportPoint(transform.position);
        Debug.Log("Position in UI Camera Viewport: " + posViewPortUI);

        Vector2 screenPointUI = uiCamera.WorldToScreenPoint(transform.position);
        Debug.Log("Position on UI Screen: " + screenPointUI);

        // RectTransformUtility.ScreenPointToLocalPointInRectangle(
        //     testImage.parent as RectTransform,
        //     screenPoint,
        //     uiCamera,
        //     out Vector2 localPoint
        // );

        // Debug.Log("Local Point in UI Image: " + localPoint);
        // testImage.anchoredPosition = localPoint;

        Vector2 anchoredPos = testImage.anchoredPosition;
        Debug.Log("Anchored Position in UI Image: " + anchoredPos);

        Canvas canvas = testImage.GetComponentInParent<Canvas>();
        Debug.Log("Canvas name: " + canvas.name);
        Vector3 worldPos = testImage.transform.TransformPoint(anchoredPos);
        Debug.Log("World Position from Anchored Position: " + worldPos);

        transform.position = worldPos;


    }
}
