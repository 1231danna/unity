using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    Camera cam;
    
    [SerializeField]
    Tilemap boundaryTilemap;
    TilemapRenderer tilemapRenderer; 

    [Header("Movement")]
    public float moveSpeed = 20f;
    public float smoothTime = 0.15f;
    
    Vector3 currentVelocity;
    Vector3 targetPosition;

    [Header("Zoom")]
    public float zoomSpeed = 10f;
    public float minZoom = 3f;
    public float maxZoom = 12f;
    
    float targetZoom;
    float minX, maxX, minY, maxY;

    public static CameraController instance;

    void Awake()
    {
        instance = this;
    }

    void Start()
    {
        cam = GetComponent<Camera>();
        targetPosition = transform.position;
        targetZoom = cam.orthographicSize;

        if (boundaryTilemap != null)
        {
            
            boundaryTilemap.CompressBounds();
         
            tilemapRenderer = boundaryTilemap.GetComponent<TilemapRenderer>();
        }

        UpdateCameraBoundaries();
    }

    void LateUpdate()
    {
       
        if (TurnManager.instance != null && TurnManager.instance.currentState == BattleState.PlayerTurn)
        {
            HandleZoom();
            HandleMove();
        }

 
        UpdateCameraBoundaries();

        // Target Position
        targetPosition.x = Mathf.Clamp(targetPosition.x, minX, maxX);
        targetPosition.y = Mathf.Clamp(targetPosition.y, minY, maxY);

        // smooth move
        Vector3 finalTarget = new Vector3(targetPosition.x, targetPosition.y, -10f);
        transform.position = Vector3.SmoothDamp(transform.position, finalTarget, ref currentVelocity, smoothTime);

    
        Vector3 clampedPos = transform.position;
        clampedPos.x = Mathf.Clamp(clampedPos.x, minX, maxX);
        clampedPos.y = Mathf.Clamp(clampedPos.y, minY, maxY);
        transform.position = clampedPos;
    }

    void HandleMove()
    {
        float x = Input.GetAxisRaw("Horizontal");
        float y = Input.GetAxisRaw("Vertical");
        Vector3 move = new Vector3(x, y, 0).normalized * moveSpeed * Time.deltaTime;
        targetPosition += move;
        

    }

    void HandleZoom()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        targetZoom -= scroll * zoomSpeed;
        targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);

        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, targetZoom, Time.deltaTime * 10f);
    }

    void UpdateCameraBoundaries()
    {
        if (tilemapRenderer == null) return;
        
    
        Bounds bounds = tilemapRenderer.bounds;

        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        minX = bounds.min.x + camWidth;
        maxX = bounds.max.x - camWidth;
        minY = bounds.min.y + camHeight;
        maxY = bounds.max.y - camHeight;

        // lock to center if < cam
        if (minX > maxX) minX = maxX = bounds.center.x;
        if (minY > maxY) minY = maxY = bounds.center.y;
    }

    public void FocusOn(Vector3 worldPos)
    {
        targetPosition = new Vector3(worldPos.x, worldPos.y, transform.position.z);
    }
}