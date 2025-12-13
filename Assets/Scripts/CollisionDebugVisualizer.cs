using UnityEngine;

/// <summary>
/// Visualizes collision radii for all game objects with circles.
/// Helps debug collision detection issues.
/// </summary>
public class CollisionDebugVisualizer : MonoBehaviour
{
    [SerializeField] private bool showCollisionRadii = true;
    [SerializeField] private bool showInGameView = true; // Show circles in Game view too
    [SerializeField] private Color playerColor = Color.cyan;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private Color projectileColor = Color.yellow;
    [SerializeField] private Color orbiterColor = Color.magenta;
    
    private Transform playerTransform;
    private ProjectilePool projectilePool;
    private EnemyPool enemyPool;
    private OrbiterManager orbiterManager;
    
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
            Debug.Log($"[CollisionDebug] Player collision radius: 0.35f");
        }
        
        projectilePool = FindFirstObjectByType<ProjectilePool>();
        enemyPool = FindFirstObjectByType<EnemyPool>();
        orbiterManager = FindFirstObjectByType<OrbiterManager>();
        
        Debug.Log("[CollisionDebug] Collision radii:");
        Debug.Log("  - Player: 0.35f");
        Debug.Log("  - Enemy: 0.35f");
        Debug.Log("  - Projectile: 0.15f");
        Debug.Log("  - Orbiter: 0.35f");
    }
    
    private void OnDrawGizmos()
    {
        if (!showCollisionRadii) return;
        
        DrawCollisionCircles();
    }
    
    private void DrawCollisionCircles()
    {
        // Draw player collision radius
        if (playerTransform != null)
        {
            Gizmos.color = playerColor;
            Gizmos.DrawWireSphere(playerTransform.position, 0.35f);
        }
        
        // Draw enemy collision radii
        if (enemyPool != null)
        {
            Enemy[] enemies = enemyPool.GetComponentsInChildren<Enemy>();
            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy && enemy.IsActive)
                {
                    Gizmos.color = enemyColor;
                    Gizmos.DrawWireSphere(enemy.transform.position, 0.35f);
                }
            }
        }
        
        // Draw projectile collision radii
        if (projectilePool != null)
        {
            Projectile[] projectiles = projectilePool.GetComponentsInChildren<Projectile>();
            foreach (var projectile in projectiles)
            {
                if (projectile != null && projectile.IsActive)
                {
                    Gizmos.color = projectileColor;
                    Gizmos.DrawWireSphere(projectile.transform.position, 0.15f);
                }
            }
        }
        
        // Draw orbiter collision radii
        if (orbiterManager != null)
        {
            var orbiters = orbiterManager.GetActiveOrbiters();
            if (orbiters != null)
            {
                foreach (var orbiter in orbiters)
                {
                    if (orbiter != null && orbiter.IsActive)
                    {
                        Gizmos.color = orbiterColor;
                        Gizmos.DrawWireSphere(orbiter.transform.position, 0.35f);
                    }
                }
            }
        }
    }
    
    void OnGUI()
    {
        if (!showInGameView || !showCollisionRadii) return;

        Camera cam = Camera.main;
        if (cam == null) return;

        // Draw player hitbox circle
        if (playerTransform != null)
        {
            DrawCircleOnScreen(cam, playerTransform.position, 0.35f, playerColor);
        }

        // Draw enemy hitbox circles
        if (enemyPool != null)
        {
            Enemy[] enemies = enemyPool.GetComponentsInChildren<Enemy>();
            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy && enemy.IsActive)
                {
                    DrawCircleOnScreen(cam, enemy.transform.position, 0.35f, enemyColor);
                }
            }
        }

        // Draw projectile hitbox circles
        if (projectilePool != null)
        {
            Projectile[] projectiles = projectilePool.GetComponentsInChildren<Projectile>();
            foreach (var projectile in projectiles)
            {
                if (projectile != null && projectile.IsActive)
                {
                    DrawCircleOnScreen(cam, projectile.transform.position, 0.15f, projectileColor);
                }
            }
        }

        // Draw orbiter hitbox circles
        if (orbiterManager != null)
        {
            var orbiters = orbiterManager.GetActiveOrbiters();
            if (orbiters != null)
            {
                foreach (var orbiter in orbiters)
                {
                    if (orbiter != null && orbiter.IsActive)
                    {
                        DrawCircleOnScreen(cam, orbiter.transform.position, 0.35f, orbiterColor);
                    }
                }
            }
        }
    }

    private void DrawCircleOnScreen(Camera cam, Vector3 worldPos, float radius, Color color)
    {
        Vector3 screenPos = cam.WorldToScreenPoint(worldPos);
        if (screenPos.z < 0) return; // Behind camera

        // Convert world radius to screen pixels
        Vector3 worldRight = worldPos + Vector3.right * radius;
        Vector3 screenRight = cam.WorldToScreenPoint(worldRight);
        float screenRadius = Vector2.Distance(screenPos, screenRight);

        // Draw circle using line segments
        int segments = 32;
        Color guiColor = GUI.color;
        GUI.color = color;

        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * Mathf.PI * 2f;
            float angle2 = ((i + 1) / (float)segments) * Mathf.PI * 2f;

            Vector2 p1 = new Vector2(
                screenPos.x + Mathf.Cos(angle1) * screenRadius,
                Screen.height - (screenPos.y + Mathf.Sin(angle1) * screenRadius)
            );
            Vector2 p2 = new Vector2(
                screenPos.x + Mathf.Cos(angle2) * screenRadius,
                Screen.height - (screenPos.y + Mathf.Sin(angle2) * screenRadius)
            );

            DrawLine(p1, p2, color);
        }

        GUI.color = guiColor;
    }

    private void DrawLine(Vector2 p1, Vector2 p2, Color color)
    {
        // Draw a line using GUI texture
        Vector2 dir = p2 - p1;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        float distance = dir.magnitude;

        Matrix4x4 matrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, p1);

        GUI.color = color;
        GUI.DrawTexture(new Rect(p1.x, p1.y, distance, 2f), Texture2D.whiteTexture);

        GUI.matrix = matrix;
    }
}
