using UnityEngine;

public class DistanceDetection : MonoBehaviour
{

    [Header("Detection Setting")]
    public float maxDrawDistance = 0.05f;
    public float detectionRadius = 0.01f;
    public float surfaceOffset = 0.001f;
    public LayerMask drawableLayers = ~0;

    public DrawingManager drawingManager;

    [SerializeField]
    private Transform tip;
    private LineRenderer currentLine;
    private int lastKnownPointCount = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

        if (drawingManager == null)
        {

            Debug.Log("DrawingManager not found");

        }

        tip = drawingManager.tip;

    }

    // Update is called once per frame
    void Update()
    {

    }

    void LateUpdate()
    {
        // Find objects "DrawnLine"
        GameObject drawnLineObj = GameObject.Find("DrawnLine");

            if (drawnLineObj != null)
            {
                LineRenderer line = drawnLineObj.GetComponent<LineRenderer>();

                if (line != null && line.positionCount > 0)
                {
                    
                    if (currentLine != line)
                    {
                        currentLine = line;
                        lastKnownPointCount = 0;
                    }


                    if (line.positionCount > lastKnownPointCount)
                    {
                        for (int i = lastKnownPointCount; i < line.positionCount; i++)
                        {
                            Vector3 pointPosition = line.GetPosition(i);
                            Vector3 surfacePoint;
                            Vector3 surfaceNormal;

                            if (TryGetSurfacePointNear(pointPosition, out surfacePoint, out surfaceNormal))
                            {
                                line.SetPosition(i, surfacePoint);
                            }
                        }

                        lastKnownPointCount = line.positionCount;
                    }
                }
            }
            else
            {
                currentLine = null;
                lastKnownPointCount = 0;
            }
        }

    private bool TryGetSurfacePointNear(Vector3 position, out Vector3 surfacePoint, out Vector3 surfaceNormal)
    {
        surfacePoint = position;
        surfaceNormal = Vector3.up;

        Vector3 tipPosition = tip.position;
        Vector3 tipForward = tip.forward;

        // First try spherecast in the forward direction from actual tip position
        RaycastHit hit;
        if (Physics.SphereCast(tipPosition, detectionRadius, tipForward, out hit, maxDrawDistance, drawableLayers))
        {
            surfacePoint = hit.point + (hit.normal * surfaceOffset);
            surfaceNormal = hit.normal;
            return true;
        }

        // If spherecast misses, try finding nearby surfaces with overlap sphere
        Collider[] nearbyColliders = Physics.OverlapSphere(tipPosition, maxDrawDistance, drawableLayers);

        if (nearbyColliders.Length > 0)
        {
            // Find the closest point among all nearby colliders
            float closestDistance = float.MaxValue;
            Vector3 closestPoint = Vector3.zero;
            Collider closestCollider = null;

            foreach (Collider col in nearbyColliders)
            {
                Vector3 point = col.ClosestPoint(tipPosition);
                float distance = Vector3.Distance(tipPosition, point);

                if (distance < closestDistance && distance <= maxDrawDistance)
                {
                    closestDistance = distance;
                    closestPoint = point;
                    closestCollider = col;
                }
            }

            if (closestCollider != null)
            {
                // Calculate surface normal by raycasting from tip to the closest point
                Vector3 directionToSurface = (closestPoint - tipPosition).normalized;
                if (Physics.Raycast(tipPosition, directionToSurface, out hit, maxDrawDistance, drawableLayers))
                {
                    surfaceNormal = hit.normal;
                }
                else
                {
                    // Fallback: approximate normal as direction from surface to tip
                    surfaceNormal = (tipPosition - closestPoint).normalized;
                }

                surfacePoint = closestPoint + (surfaceNormal * surfaceOffset);
                return true;
            }
        }

        return false;
    }
}