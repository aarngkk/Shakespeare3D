using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class DraggableWithOutline : MonoBehaviour
{
    private Camera cam;
    private Plane groundPlane = new Plane(Vector3.up, Vector3.zero);
    private Rigidbody rb;
    private Vector3 offset;
    private bool isDragging = false;
    private Outline outline;
    private static DraggableWithOutline selected;

    private Bounds stageBounds;

    void Start()
    {
        cam = Camera.main;
        rb = GetComponent<Rigidbody>();
        outline = GetComponent<Outline>();

        if (outline) outline.enabled = false;

        // Setup movement boundary from object with tag "Stage"
        GameObject stage = GameObject.FindWithTag("Stage");
        if (stage && stage.TryGetComponent(out BoxCollider box))
        {
            stageBounds = box.bounds;
        }
        else
        {
            Debug.LogWarning("No BoxCollider found on Stage object!");
        }
    }

    void OnMouseDown()
    {
        if (selected != null && selected != this)
            selected.Deselect();

        selected = this;
        if (outline) outline.enabled = true;

        if (GetMouseWorldPosition(out Vector3 worldPos))
        {
            offset = transform.position - worldPos;
            isDragging = true;
        }
    }

    void OnMouseDrag()
    {
        if (!isDragging) return;

        if (GetMouseWorldPosition(out Vector3 worldPos))
        {
            Vector3 target = worldPos + offset;
            target.y = transform.position.y; // Lock to horizontal plane

            // Clamp to stage bounds
            target.x = Mathf.Clamp(target.x, stageBounds.min.x, stageBounds.max.x);
            target.z = Mathf.Clamp(target.z, stageBounds.min.z, stageBounds.max.z);

            rb.MovePosition(target);
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
    }

    void Update()
    {
        // Deselect if user clicks on empty space
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = cam.ScreenPointToRay(Input.mousePosition);
            if (!Physics.Raycast(ray, out RaycastHit hit) || hit.transform != transform)
                Deselect();
        }
        if (Input.GetKeyDown("space") && selected == this)
        {
            this.transform.rotation *= Quaternion.Euler(0, 90, 0);
        }
    }

    void Deselect()
    {
        if (outline) outline.enabled = false;
        if (selected == this) selected = null;
    }

    bool GetMouseWorldPosition(out Vector3 worldPosition)
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
}