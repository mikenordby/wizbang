using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target; // The player to follow
    [SerializeField] private float smoothSpeed = 0.125f; // Optional: smooth following
    [SerializeField] private bool smoothFollow = false; // Toggle smooth following
    
    private Vector3 offset;
    
private void Start()
    {
        // Calculate initial offset (should be directly above for top-down)
        if (target == null)
        {
            // Auto-find player if not assigned
            GameObject playerObj = GameObject.Find("Player");
            if (playerObj != null)
            {
                target = playerObj.transform;
            }
        }
        
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }
    
    private void LateUpdate()
    {
        if (target == null) return;
        
        Vector3 desiredPosition = target.position + offset;
        
        if (smoothFollow)
        {
            // Smooth camera following
            Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
            transform.position = smoothedPosition;
        }
        else
        {
            // Direct camera following (locked to player)
            transform.position = desiredPosition;
        }
    }
    
    // Method to set the target at runtime if needed
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            offset = transform.position - target.position;
        }
    }
}