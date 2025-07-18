using UnityEngine;

public class DragCharacter : MonoBehaviour
{
    // Dragging state and references
    private Vector3 startPosition;
    private Quaternion startRotation;
    private Vector3 offset;
    private Camera cam;
    private Rigidbody rb;
    private bool isDragging = false;
    private static DragCharacter selectedCharacter = null;
    public static DragCharacter SelectedCharacter => selectedCharacter; // Allows CameraController to check selection
    private Plane groundPlane;

    private Outline outline; // Reference to Quick Outline component
    private Vector3 originalPosition;
    private Bounds stageBounds;

    private Transform parentObject; // New: Reference to parent group
    private CutsceneManager cutsceneManager;

    void Start()
    {
        // Initialize references and setup
        cutsceneManager = FindObjectOfType<CutsceneManager>();
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        groundPlane = new Plane(Vector3.up, Vector3.zero);
        originalPosition = transform.position;

        // Setup outline effect
        outline = GetComponent<Outline>();
        if (outline != null)
        {
            outline.enabled = false;
        }

        // Find and validate stage boundaries
        GameObject stage = GameObject.FindWithTag("Stage");
        if (stage != null)
        {
            BoxCollider stageCollider = stage.GetComponent<BoxCollider>();
            if (stageCollider != null)
            {
                stageBounds = stageCollider.bounds;
            }
            else
            {
                Debug.LogError("Stage does not have a BoxCollider!");
            }
        }
        else
        {
            Debug.LogError("No GameObject with the 'Stage' tag found!");
        }

        // Cache parent transform for group movement
        parentObject = transform.parent != null ? transform.parent : transform;
    }

    void Update()
    {
        // Handle mouse click to potentially deselect character
        if (Input.GetMouseButtonDown(0))
        {
            if (!IsDraggingAllowed())
            {
                return;
            }

            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit) || hit.transform != transform)
            {
                DeselectCharacter();
            }
        }
    }

    // Called when mouse button is pressed on this object
    void OnMouseDown()
    {
        if (Input.GetMouseButton(0))
        {
            SelectCharacter();
            startPosition = parentObject.position;
            startRotation = parentObject.rotation;

            // Calculate drag offset if valid mouse position
            if (GetMouseWorldPosition(out Vector3 worldPosition))
            {
                offset = parentObject.position - worldPosition;
                isDragging = true;
            }
        }
    }

    // Called while mouse is held down and moving
    void OnMouseDrag()
    {
        if (!IsDraggingAllowed()) return;

        if (isDragging && selectedCharacter == this && GetMouseWorldPosition(out Vector3 worldPosition))
        {
            // Calculate target position with bounds clamping
            Vector3 targetPosition = worldPosition + offset;
            targetPosition.y = parentObject.position.y;

            // Restrict movement to stage bounds
            targetPosition.x = Mathf.Clamp(targetPosition.x, stageBounds.min.x, stageBounds.max.x);
            targetPosition.z = Mathf.Clamp(targetPosition.z, stageBounds.min.z, stageBounds.max.z);

            rb.MovePosition(targetPosition);
        }
    }

    // Called when mouse button is released
    void OnMouseUp()
    {
        isDragging = false;

        CheckSnapPoint();
    }

    // Checks for nearby snap zones and handles snapping logic
    void CheckSnapPoint()
    {
        Collider[] colliders = Physics.OverlapSphere(parentObject.position, 0.5f);

        // Get references to the Outline components
        Outline bedOutline = GameObject.FindWithTag("Bed")?.GetComponent<Outline>();
        Outline dresserOutline = GameObject.FindWithTag("Dresser")?.GetComponent<Outline>();
        Outline carpetOutline = GameObject.FindWithTag("CarpetZone")?.GetComponent<Outline>();

        bool snappedToBed = false;
        bool snappedToDresser = false;
        bool snappedToCarpet = false;

        // Handle snapping logic
        foreach (Collider col in colliders)
        {
            if (col.CompareTag("BedZone"))
            {
                Debug.Log("Snapped to Bed");
                parentObject.position = new Vector3(-4.0f, -1.5f, -4.1f);
                parentObject.rotation = Quaternion.Euler(0, 0, 0);
                FindObjectOfType<PlaySceneButton>().SetCurrentSnapPoint("Bed");
                snappedToBed = true;

                if (cutsceneManager != null && cutsceneManager.IsCutscene6Finished() && !cutsceneManager.IsCutscene7Started())
                {
                    cutsceneManager.ShowScript7ChoicePopup("Bed");
                }
            }
            else if (col.CompareTag("DresserZone"))
            {
                Debug.Log("Snapped to Dresser");
                parentObject.position = new Vector3(4.7f, -1.5f, -3.85f);
                parentObject.rotation = Quaternion.Euler(0, 0, 0);
                FindObjectOfType<PlaySceneButton>().SetCurrentSnapPoint("Dresser");
                snappedToDresser = true;
            }
            else if (col.CompareTag("CarpetZone"))
            {
                Debug.Log("Snapped to Carpet");
                parentObject.position = new Vector3(1.1f, -1.5f, -2.5f);
                parentObject.rotation = Quaternion.Euler(0, 0, 0);
                FindObjectOfType<PlaySceneButton>().SetCurrentSnapPoint("Carpet");
                snappedToCarpet = true;

                if (cutsceneManager != null && cutsceneManager.IsCutscene6Finished() && !cutsceneManager.IsCutscene7Started())
                {
                    cutsceneManager.ShowScript7ChoicePopup("Carpet");
                }
            }
        }

        // Hide popup if not snapped to valid zones
        if (!snappedToBed && !snappedToCarpet)
        {
            if (cutsceneManager != null)
            {
                cutsceneManager.HideScript7ChoicePopup();
            }
        }

        // Skip outline updates if we shouldn't be showing them
        if (TutorialManager.tutorialActive || cutsceneManager == null ||
            cutsceneManager.IsCutscene7Started() ||
            (cutsceneManager.IsCutscene1Started() && !cutsceneManager.IsCutscene6Finished()))
        {
            // Ensure all outlines are disabled in these cases
            if (bedOutline != null) bedOutline.enabled = false;
            if (dresserOutline != null) dresserOutline.enabled = false;
            if (carpetOutline != null) carpetOutline.enabled = false;
            return;
        }

        // Handle outline states based on game state
        if (cutsceneManager.IsCutscene6Finished())
        {
            // Post-cutscene6 state - only bed and carpet matter
            if (dresserOutline != null) dresserOutline.enabled = false;

            if (snappedToBed)
            {
                if (bedOutline != null)
                {
                    bedOutline.enabled = true;
                    bedOutline.OutlineColor = Color.yellow;
                }
                if (carpetOutline != null) carpetOutline.enabled = false;
            }
            else if (snappedToCarpet)
            {
                if (carpetOutline != null)
                {
                    carpetOutline.enabled = true;
                    carpetOutline.OutlineColor = Color.yellow;
                }
                if (bedOutline != null) bedOutline.enabled = false;
            }
            else
            {
                // Not snapped - show white outlines
                if (bedOutline != null)
                {
                    bedOutline.enabled = true;
                    bedOutline.OutlineColor = Color.white;
                }
                if (carpetOutline != null)
                {
                    carpetOutline.enabled = true;
                    carpetOutline.OutlineColor = Color.white;
                }
            }
        }
        else
        {
            // Pre-cutscene6 state - all outlines possible
            if (snappedToBed)
            {
                if (bedOutline != null)
                {
                    bedOutline.enabled = true;
                    bedOutline.OutlineColor = Color.yellow;
                }
                if (dresserOutline != null) dresserOutline.enabled = false;
                if (carpetOutline != null) carpetOutline.enabled = false;
            }
            else if (snappedToDresser)
            {
                if (dresserOutline != null)
                {
                    dresserOutline.enabled = true;
                    dresserOutline.OutlineColor = Color.yellow;
                }
                if (bedOutline != null) bedOutline.enabled = false;
                if (carpetOutline != null) carpetOutline.enabled = false;
            }
            else if (snappedToCarpet)
            {
                if (carpetOutline != null)
                {
                    carpetOutline.enabled = true;
                    carpetOutline.OutlineColor = Color.yellow;
                }
                if (bedOutline != null) bedOutline.enabled = false;
                if (dresserOutline != null) dresserOutline.enabled = false;
            }
            else
            {
                // Not snapped - show all white outlines
                if (bedOutline != null)
                {
                    bedOutline.enabled = true;
                    bedOutline.OutlineColor = Color.white;
                }
                if (dresserOutline != null)
                {
                    dresserOutline.enabled = true;
                    dresserOutline.OutlineColor = Color.white;
                }
                if (carpetOutline != null)
                {
                    carpetOutline.enabled = true;
                    carpetOutline.OutlineColor = Color.white;
                }
                FindObjectOfType<PlaySceneButton>().SetCurrentSnapPoint("");
            }
        }
    }

    // Calculates mouse position in world space
    private bool GetMouseWorldPosition(out Vector3 worldPosition)
    {
        Ray ray = cam.ScreenPointToRay(Input.mousePosition);
        if (groundPlane.Raycast(ray, out float distance))
        {
            worldPosition = ray.GetPoint(distance);
            return true;
        }
        worldPosition = Vector3.zero;
        return false;
    }

    // Selects character
    private void SelectCharacter()
    {
        if (selectedCharacter != null)
        {
            selectedCharacter.DeselectCharacter();
        }

        if (!TutorialManager.tutorialActive)
        {
            selectedCharacter = this;

            if (outline != null)
            {
                outline.enabled = true;
            }
        }
    }

    // Deselects character
    private void DeselectCharacter()
    {
        if (selectedCharacter == this)
        {
            if (outline != null)
            {
                outline.enabled = false;
            }

            selectedCharacter = null;
        }
    }

    // Resets character to original position
    public void ResetPosition()
    {
        parentObject.position = originalPosition;
    }

    // Clears all snap point states
    public void ClearSnapPoints()
    {
        // Reset the characters' positions to their original positions
        parentObject.position = originalPosition;

        // Clear any snap-related state
        PlaySceneButton playSceneButton = FindObjectOfType<PlaySceneButton>();
        if (playSceneButton != null)
        {
            playSceneButton.SetCurrentSnapPoint("");
        }

        // Disable outlines for bed and dresser
        Outline bedOutline = GameObject.FindWithTag("Bed")?.GetComponent<Outline>();
        Outline dresserOutline = GameObject.FindWithTag("Dresser")?.GetComponent<Outline>();

        if (bedOutline != null)
        {
            bedOutline.enabled = false;
        }

        if (dresserOutline != null)
        {
            dresserOutline.enabled = false;
        }
    }

    // Determines if dragging is allowed based on cutscene state
    private bool IsDraggingAllowed()
    {
        if (cutsceneManager == null)
            cutsceneManager = FindObjectOfType<CutsceneManager>();

        bool allowDragging = (!TutorialManager.tutorialActive && !cutsceneManager.IsCutscene1Started() ||
                            (cutsceneManager.IsCutscene6Finished() && !cutsceneManager.IsCutscene7Started()));

        Debug.Log($"Dragging allowed: {allowDragging}");
        return allowDragging;
    }
}
