using UnityEngine;

public class SmoothInteractionCamera : MonoBehaviour
{
    public bool startFrozen = false; 

    public Transform initialAnchor;
    public Transform notebookAnchor;
    public Transform workingboardAnchor;

    public GameObject backButton;
    public GameObject exitButton;

    // movement
    public float moveSpeed = 3f;
    public float rotateSpeed = 3f;

    // mouse movement
    public float mouseOffsetRange = 15f;
    public float mouseSmoothTime = 2f;

    // breath feel
    public float defaultBreatheAmplitude = 0.05f;
    public float focusedBreatheAmplitude = 0.01f;
    public float breatheSpeed = 0.8f;

    [HideInInspector] public Transform targetAnchor;
    [HideInInspector] public bool isShowingDocument = false;

    private Vector2 currentMouseOffset;
    private float breatheTimer;
    private float currentBreatheAmplitude;
    private bool isFrozen = true;
    
    private bool hasTriggeredWorkingboardTutorial = false; 

    void Start()
    {
        if (initialAnchor != null) targetAnchor = initialAnchor;
        currentBreatheAmplitude = defaultBreatheAmplitude;
        UpdateUIButtonVisibility();
        
        SetCameraFrozen(startFrozen); 
    }

    public void SetCameraFrozen(bool state) => isFrozen = state;

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isFrozen)
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                if (hit.collider.gameObject.name == "Workingboard")
                {
                    if (!hasTriggeredWorkingboardTutorial)
                    {
                        TutorialManager tm = Object.FindFirstObjectByType<TutorialManager>();
                        tm?.ShowNextStep();
                        hasTriggeredWorkingboardTutorial = true;
                    }
                }
            }
        }

        if (targetAnchor == initialAnchor && !isFrozen)
        {
            Vector2 mousePos = new Vector2(Input.mousePosition.x / Screen.width, Input.mousePosition.y / Screen.height);
            Vector2 targetOffset = new Vector2((mousePos.x - 0.5f) * mouseOffsetRange, (mousePos.y - 0.5f) * mouseOffsetRange);
            currentMouseOffset = Vector2.Lerp(currentMouseOffset, targetOffset, Time.deltaTime * mouseSmoothTime);
            currentBreatheAmplitude = Mathf.Lerp(currentBreatheAmplitude, defaultBreatheAmplitude, Time.deltaTime * 5f);
        }
        else
        {
            currentBreatheAmplitude = Mathf.Lerp(currentBreatheAmplitude, focusedBreatheAmplitude, Time.deltaTime * 5f);
            currentMouseOffset = Vector2.Lerp(currentMouseOffset, Vector2.zero, Time.deltaTime * 5f);
        }
        breatheTimer += Time.deltaTime * breatheSpeed;
    }

    public void UpdateUIButtonVisibility()
    {
        if (isShowingDocument)
        {
            if (backButton != null) backButton.SetActive(false);
            if (exitButton != null) exitButton.SetActive(false);
            return;
        }
        bool isInitial = (targetAnchor == initialAnchor);
        if (backButton != null) backButton.SetActive(!isInitial);
        if (exitButton != null) exitButton.SetActive(isInitial);
    }

    void LateUpdate()
    {
        if (targetAnchor == null) return;
        float breatheOffset = Mathf.Sin(breatheTimer) * currentBreatheAmplitude;
        Vector3 finalPos = targetAnchor.position + (targetAnchor.up * breatheOffset);
        Quaternion finalRot = (targetAnchor == initialAnchor) ? targetAnchor.rotation * Quaternion.Euler(-currentMouseOffset.y, currentMouseOffset.x, 0) : targetAnchor.rotation;
        
        transform.position = Vector3.Lerp(transform.position, finalPos, Time.deltaTime * moveSpeed);
        transform.rotation = Quaternion.Slerp(transform.rotation, finalRot, Time.deltaTime * rotateSpeed);
        UpdateUIButtonVisibility();
    }

    public void BackToInitialView() { targetAnchor = initialAnchor; SetCameraFrozen(false); }
    public void ExitGame() => Application.Quit();
}