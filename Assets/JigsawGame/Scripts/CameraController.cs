using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header(" Movement ")]
    private Vector3 cameraStartMovePos;
    private Vector2 touch0ClickedPos;
    [SerializeField] private float moveSpeed;
    private Vector3 zoomInitialPos;
    private Vector3 zoomCenter;
    private float clickedOrthoSize;
    [SerializeField] private float zoomMultiplier;
    [SerializeField] private Vector2 minMaxOrthoSize;
    [SerializeField] private float zoomSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SingleTouchBeganCallback(Vector2 screenPos)
    {
        cameraStartMovePos = transform.position;
        touch0ClickedPos = screenPos;
    }

    public void SingleTouchDrag(Vector2 screenPos)
    {
        Vector3 moveDelta = (screenPos - touch0ClickedPos) / Screen.width;
        Vector3 targetPos = cameraStartMovePos - (moveDelta * moveSpeed);

        transform.position = targetPos;
    }

    public void DoubleTouchBeganCallback(Vector2 touch0Pos, Vector2 touch1Pos)
    {
        clickedOrthoSize = Camera.main.orthographicSize;
        this.touch0ClickedPos = touch0Pos;

        zoomInitialPos = transform.position;

        zoomCenter = (Camera.main.ScreenToWorldPoint(touch0Pos) + Camera.main.ScreenToWorldPoint(touch1Pos)) / 2;
        zoomCenter.z = -10;
    }

    public void DoubleTouchDrag(float distanceDelta)
    {
        SetOrthoSize(distanceDelta);
        MoveTowardsZoomCenter();
    }

    private void SetOrthoSize(float distanceDelta)
    {
        float targetOrthoSize = clickedOrthoSize - distanceDelta * zoomMultiplier;
        targetOrthoSize = Mathf.Clamp(targetOrthoSize, minMaxOrthoSize.x, minMaxOrthoSize.y);

        Camera.main.orthographicSize = Mathf.Lerp(Camera.main.orthographicSize, targetOrthoSize, Time.deltaTime * 60 * .3f);

    }

    private void MoveTowardsZoomCenter()
    {
        float percent = Mathf.InverseLerp(minMaxOrthoSize.x, minMaxOrthoSize.y, clickedOrthoSize - Camera.main.orthographicSize);
        percent *= zoomSpeed;
        Vector3 targetPos = Vector3.Lerp(zoomInitialPos, zoomCenter, percent);
        transform.position = targetPos;
        cameraStartMovePos = transform.position;
    }
}
