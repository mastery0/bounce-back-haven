
using System.Collections.Generic;
using UnityEngine;

public class PlatformGenerator : MonoBehaviour
{
    [Header("Platform Prefabs")]
    public List<GameObject> platformPrefabs;
    public Transform startPlatform;
    public Transform generatedParent;

    [Header("Generation")]
    public int platformsToGenerate = 30;
    public int maxAttempts = 50;

    [Header("Player Physics")]
    public Rigidbody2D playerRb;
    public Collider2D playerCollider;

    [Tooltip("Same value assigned to player Y velocity when jumping")]
    public float jumpVelocity = 12f;

    [Tooltip("Maximum horizontal velocity while in air")]
    public float maxAirSpeed = 7f;

    [Header("Double Jump")]
    public bool allowDoubleJump = false;

    [Header("Platform Spacing")]
    public float minVerticalGap = 0.5f;
    public float maxVerticalGap = 2.5f;
    public float maxHorizontalOffset = 5f;
    public float minHorizontalOffset = 5f;

    [Header("Simulation")]
    public int simulationFrames = 180;
    public int maxDoubleJumpFrame = 100;

    [Header("Clearance")]
    public float minPlayerClearance = 1f;
    public float platformSeparation = 0.5f;

    private Bounds currentBounds;

    private float Gravity => -Physics2D.gravity.y * playerRb.gravityScale;

    private float PlayerHalfWidth => playerCollider.bounds.extents.x;

    private float PlayerHalfHeight => playerCollider.bounds.extents.y;

    void Start()
    {
        if (!ValidateSetup())
            return;

        Collider2D startCollider = startPlatform.GetComponentInChildren<Collider2D>();

        if (startCollider == null)
        {
            Debug.LogError("Start platform needs a Collider2D!");
            return;
        }

        currentBounds = startCollider.bounds;

        GeneratePlatforms();
    }

    bool ValidateSetup() //check for compilation errors
    {
        if (platformPrefabs == null || platformPrefabs.Count == 0)
        {
            Debug.LogError("No platform prefabs assigned!");
            return false;
        }

        if (startPlatform == null ||
            playerRb == null ||
            playerCollider == null)
        {
            Debug.LogError("Missing required references!");
            return false;
        }

        if (jumpVelocity <= 0 ||
            maxAirSpeed <= 0 ||
            Gravity <= 0)
        {
            Debug.LogError("Invalid player physics values!");
            return false;
        }

        return true;
    }

    void GeneratePlatforms()
    {
        for (int i = 0; i < platformsToGenerate; i++)
        {
            if (!GenerateNextPlatform())
            {
                Debug.LogWarning("Generation stopped at platform " + i);
                break;
            }
        }
    }

    bool GenerateNextPlatform()
    {
        // Random generation attempts
        for (int i = 0; i < maxAttempts; i++)
        {
            GameObject prefab = platformPrefabs[Random.Range(0, platformPrefabs.Count)];

            if (prefab == null) continue;

            float gap = Random.Range(minVerticalGap,maxVerticalGap);

            float targetX = currentBounds.center.x + Random.Range(minHorizontalOffset,maxHorizontalOffset);
            targetX = Random.Range(-1,1) > 0 ? targetX : targetX*(-1);

            if (TrySpawn(prefab, targetX, gap)) return true;
        }

        // Fallback: try a safe platform directly above
        // the current platform.

        foreach (GameObject prefab in platformPrefabs)
        {
            if (prefab == null) continue;

            if (TrySpawn(prefab,currentBounds.center.x,minVerticalGap)) return true;
        }

        return false;
    }

    bool TrySpawn(GameObject prefab,float targetX,float verticalGap)
    {
        float targetBottom = currentBounds.max.y + verticalGap;

        GameObject platform = Instantiate(prefab,new Vector3(targetX, targetBottom, 0),Quaternion.identity,generatedParent);

        Collider2D col = platform.GetComponentInChildren<Collider2D>();

        if (col == null || col.isTrigger)
        {
            Destroy(platform);
            return false;
        }

        // Align collider center X and bottom Y.
        Bounds bounds = col.bounds;

        Vector3 correction = new Vector3(targetX - bounds.center.x,targetBottom - bounds.min.y,0);

        platform.transform.position += correction;

        bounds = col.bounds;

        // Check actual jump trajectory.
        if (!HasEnoughClearance(currentBounds, bounds))
        {
            Destroy(platform);
            return false;
        }
        if (!CanReachPlatform(currentBounds, bounds))
        {
            Destroy(platform);
            return false;
        }
        // Accept platform.
        currentBounds = bounds;

        return true;
    }

    bool CanReachPlatform(Bounds source,Bounds target)
    {
        float halfWidth = PlayerHalfWidth;
        float halfHeight = PlayerHalfHeight;

        // Possible starting positions on source platform.
        float left = source.min.x + halfWidth;
        float right = source.max.x - halfWidth;

        if (left > right)
        {
            left = source.center.x;
            right = source.center.x;
        }

        float[] startPositions ={left,source.center.x,right};

        // Try ordinary jump first.
        // If enabled, also test double jump at different times.
        int lastDoubleFrame = allowDoubleJump ? maxDoubleJumpFrame : -1;

        for (int i = 0; i < startPositions.Length; i++)
        {
            for (int doubleFrame = -1;doubleFrame <= lastDoubleFrame;doubleFrame++)
            {
                if (SimulateJump(startPositions[i],source.max.y,target,doubleFrame))
                {
                    return true;
                }
            }
        }

        return false;
    }

    bool SimulateJump(float startX,float sourceTop,Bounds target,int doubleJumpFrame)
    {
        float dt = Time.fixedDeltaTime;

        float x = startX;
        float y = sourceTop + PlayerHalfHeight;

        float velocityY = jumpVelocity;

        float targetX = target.center.x;

        for (int frame = 0;frame < simulationFrames; frame++)
        {
            // Optional second jump.
            if (frame == doubleJumpFrame)
            {
                velocityY = jumpVelocity;
            }

            // Horizontal air control.
            float difference = targetX - x;

            float horizontalStep = Mathf.Clamp( difference,-maxAirSpeed * dt,maxAirSpeed * dt);

            x += horizontalStep;

            // Vertical physics.
            float previousBottom = y - PlayerHalfHeight;

            velocityY -= Gravity * dt;
            y += velocityY * dt;

            float currentBottom = y - PlayerHalfHeight;

            // Landing must happen while descending.
            if (velocityY < 0 &&previousBottom >= target.max.y &&currentBottom <= target.max.y)
            {
                bool horizontalOverlap =x + PlayerHalfWidth >= target.min.x && x - PlayerHalfWidth <= target.max.x;

                return horizontalOverlap;
            }

            // Stop simulation if player falls too far.
           // if (currentBottom < source.min.y - 10f)
            //    break;
        }

        return false;
    }
    bool HasEnoughClearance(Bounds source, Bounds target)
{
    float requiredHeight = PlayerHalfHeight * 2f + minPlayerClearance;

    float horizontalOverlap = Mathf.Min(source.max.x,target.max.x) - Mathf.Max(source.min.x,target.min.x);

    float verticalDistance = target.min.y - source.max.y;

    // Check vertical clearance when platforms overlap horizontally
    if (horizontalOverlap > 0 && verticalDistance < requiredHeight)
    {
        return false;
    }

    // Minimum distance between platform surfaces
    if (verticalDistance < platformSeparation && horizontalOverlap > 0)
    {
        return false;
    }

    return true;
}
}
