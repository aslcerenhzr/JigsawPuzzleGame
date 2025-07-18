using UnityEngine;
using UnityEngine.Rendering;

public class InputManager : MonoBehaviour
{
    enum State { None, PuzzlePiece, Camera }
    private State state;

    [Header(" Elements ")]
    [SerializeField] private PuzzleController controller;
    [SerializeField] private CameraController cameraController;

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
        if (Input.touchCount == 1)
            ManageSingleInput();
        else if (Input.touchCount == 2)
            ManageDoubleInput();

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
}

