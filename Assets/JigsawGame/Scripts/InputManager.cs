using UnityEngine;
using UnityEngine.Rendering;

public class InputManager : MonoBehaviour
{
    enum State { None, PuzzlePiece, Camera, Crop }
    private State state;

    [Header(" Elements ")]
    [SerializeField] private PuzzleController controller;
    [SerializeField] private CameraController cameraController;
    [SerializeField] private PhotoCropUI photoCropUI;

    [Header(" Double Touch ")]
    private Vector2 touch0ClickedPos, touch1ClickedPos;
    private Vector2 initialCameraDelta;

    private void Start()
    {
        state = State.None;
    }
    void Update()
    {
        ManageInput();
    }

    private void ManageInput()
    {
        // If menu UI is open, block gameplay inputs
        if (UIManager.Instance != null && UIManager.Instance.IsMenuOpen)
            return;

        // Eğer crop UI aktifse, öncelik kırpma kontrolünde
        if (photoCropUI != null && photoCropUI.gameObject.activeInHierarchy)
        {
            ManageCropInput();
            return;
        }

        if (Input.touchCount == 1)
            ManageSingleInput();
        else if (Input.touchCount == 2)
            ManageDoubleInput();
    }

    private void ManageCropInput()
    {
        // Touch: pan + pinch
        if (Input.touchCount > 0)
        {
            if (Input.touchCount == 1)
            {
                Vector2 touchPos = Input.touches[0].position;
                TouchPhase touchPhase = Input.touches[0].phase;
                switch (touchPhase)
                {
                    case TouchPhase.Began:
                        photoCropUI.BeginCropDrag(touchPos);
                        state = State.Crop;
                        break;
                    case TouchPhase.Moved:
                    case TouchPhase.Stationary:
                        if (state == State.Crop)
                            photoCropUI.DragCrop(touchPos);
                        break;
                    case TouchPhase.Ended:
                    case TouchPhase.Canceled:
                        if (state == State.Crop)
                            photoCropUI.EndCropDrag();
                        state = State.None;
                        break;
                }
                return;
            }
            else if (Input.touchCount == 2)
            {
                Touch t0 = Input.touches[0];
                Touch t1 = Input.touches[1];
                if (t0.phase == TouchPhase.Began || t1.phase == TouchPhase.Began)
                {
                    photoCropUI.BeginCropPinch(t0.position, t1.position);
                }
                else
                {
                    photoCropUI.UpdateCropPinch(t0.position, t1.position);
                }

                if (t0.phase == TouchPhase.Ended || t0.phase == TouchPhase.Canceled || t1.phase == TouchPhase.Ended || t1.phase == TouchPhase.Canceled)
                {
                    photoCropUI.EndCropPinch();
                }
                return;
            }
        }

        // Mouse: pan + wheel zoom
        if (Input.GetMouseButtonDown(0))
        {
            photoCropUI.BeginCropDrag(Input.mousePosition);
            state = State.Crop;
        }
        else if (Input.GetMouseButton(0))
        {
            if (state == State.Crop)
                photoCropUI.DragCrop(Input.mousePosition);
        }
        else if (Input.GetMouseButtonUp(0))
        {
            if (state == State.Crop)
                photoCropUI.EndCropDrag();
            state = State.None;
        }

        float scroll = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scroll) > 0.0001f)
        {
            photoCropUI.ApplyMouseWheelZoom(scroll, Input.mousePosition);
        }
    }

    private void ManageSingleInput()
    {
        Vector2 touchPos = Input.touches[0].position;
        Vector3 worldTouchPos = Camera.main.ScreenToWorldPoint(touchPos);
        worldTouchPos.z = 0;
        TouchPhase touchPhase = Input.touches[0].phase;

        switch (touchPhase)
        {
            case TouchPhase.Began:
                if (controller.SingleTouchBeganCallback(worldTouchPos))
                {
                    state = State.PuzzlePiece;
                }
                else
                {
                    cameraController.SingleTouchBeganCallback(touchPos);
                    state = State.Camera;
                }
                    break;

            case TouchPhase.Moved:
                if (state == State.PuzzlePiece)
                    controller.SingleTouchDrag(worldTouchPos);
                else if (state == State.Camera)
                    cameraController.SingleTouchDrag(touchPos);
                    break;

            case TouchPhase.Stationary:
                if (state == State.PuzzlePiece)
                    controller.SingleTouchDrag(worldTouchPos);
                else if (state == State.Camera)
                    cameraController.SingleTouchDrag(touchPos);
                break;

            case TouchPhase.Ended:
                if(state == State.PuzzlePiece)
                    controller.SingleTouchEnded(); 
                break;

            default: break;
        }
    }

    private void ManageDoubleInput()
    {
        switch (state)
        {
            case State.None:
                ManageCameraDoubleInput();
                break;

            case State.Camera:
                ManageCameraDoubleInput();
                break;

            case State.PuzzlePiece:
                ManagePieceDoubleInput();
                break;
        }
    }

    private void ManageCameraDoubleInput()
    {
        Touch[] touches = Input.touches;

        if (touches[0].phase == TouchPhase.Began)
            touch0ClickedPos = touches[0].position;

        if (touches[1].phase == TouchPhase.Began)
        {
            touch0ClickedPos = touches[0].position;
            touch1ClickedPos = touches[1].position;

            initialCameraDelta = touch1ClickedPos - touch0ClickedPos;

            cameraController.DoubleTouchBeganCallback(touch0ClickedPos, touch1ClickedPos);
            return;
        }

        Vector2 currentDelta = touches[1].position - touches[0].position;
        float distanceDelta = (currentDelta.magnitude - initialCameraDelta.magnitude);

        cameraController.DoubleTouchDrag(distanceDelta);

        foreach (Touch touch in touches)
        {
            if(touch.phase == TouchPhase.Ended)
                state = State.None;
        }
    }

    private void ManagePieceDoubleInput() 
    {
        Touch[] touches = Input.touches;

        if (touches[1].phase == TouchPhase.Began)
        {
            touch1ClickedPos = touches[1].position;
            controller.StartRotatingPiece();
        }

        float xDelta = (touches[1].position.x - touch1ClickedPos.x) / Screen.width;
        controller.RotatePiece(xDelta);

    }
}

