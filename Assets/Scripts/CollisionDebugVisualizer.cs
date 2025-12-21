using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

/// <summary>
/// Visualizes collision radii for all game objects with circles.
/// Helps debug collision detection issues.
/// </summary>
public class CollisionDebugVisualizer : MonoBehaviour
{
    [SerializeField] private bool showCollisionRadii = true;
    [SerializeField] private Color playerColor = Color.cyan;
    [SerializeField] private Color enemyColor = Color.red;
    [SerializeField] private Color projectileColor = Color.yellow;
    [SerializeField] private Color orbiterColor = Color.magenta;
    
    private Material lineMaterial;
    
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
            DebugLog.Info($"[CollisionDebug] Found player at position: {playerTransform.position}");
        }
        else
        {
            DebugLog.Error("[CollisionDebug] FAILED to find player with tag 'Player'!");
        }
        
        // DON'T get pools here - they may not be initialized yet
        // We'll get them lazily in Update/OnRenderObject
        
        CreateLineMaterial();
        
        // ENABLE HITBOXES AUTOMATICALLY ON START
        showCollisionRadii = true;
        DebugLog.Verbose("[CollisionDebug] Hitbox visualization enabled (press 0 to toggle)");
    }
    
    private void Update()
    {
        // Lazy initialize pools if null (they may not be ready at Start time)
        if (projectilePool == null)
        {
            projectilePool = GameServices.ProjectilePool;
        }
        if (enemyPool == null)
        {
            enemyPool = GameServices.EnemyPool;
        }
        if (orbiterManager == null)
        {
            orbiterManager = GameServices.OrbiterManager;
            if (orbiterManager != null)
            {
                DebugLog.Info("[CollisionDebug] ✓ OrbiterManager found and initialized!");
            }
        }
        
        // Toggle with 0 key
        if (Keyboard.current != null && Keyboard.current.digit0Key.wasPressedThisFrame)
        {
            showCollisionRadii = !showCollisionRadii;
            string status = showCollisionRadii ? "ON" : "OFF";
            DebugLog.Info($"[CollisionDebug] Collision visualization: {status}");
            DebugLog.Info($"[CollisionDebug] Attached to: {gameObject.name}, Camera.main: {Camera.main != null}, Material: {lineMaterial != null}");
            
            // Count entities for debugging
            int playerCount = playerTransform != null ? 1 : 0;
            int enemyCount = enemyPool != null ? enemyPool.GetActiveEnemies().Count : 0;
            int projectileCount = projectilePool != null ? projectilePool.GetActiveProjectiles().Count : 0;
            int orbiterCount = orbiterManager != null ? orbiterManager.GetActiveOrbiters().Count : 0;
            DebugLog.Info($"[CollisionDebug] Entities to draw: Player={playerCount}, Enemies={enemyCount}, Projectiles={projectileCount}, Orbiters={orbiterCount}");
        }
        
        // Draw hitboxes using Debug.DrawLine (visible in Game view with Gizmos enabled)
        if (!showCollisionRadii) return;
        
        int circlesDrawn = 0;
        
        // Draw player collision circle
        if (playerTransform != null)
        {
            DrawDebugCircle(playerTransform.position, 0.35f, playerColor);
            circlesDrawn++;
        }
        
        // Draw enemy collision circles
        if (enemyPool != null)
        {
            List<Enemy> enemies = enemyPool.GetActiveEnemies();
            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy && enemy.IsActive)
                {
                    DrawDebugCircle(enemy.transform.position, 0.35f, enemyColor);
                    circlesDrawn++;
                }
            }
        }
        
        // Draw projectile collision circles
        if (projectilePool != null)
        {
            List<Projectile> projectiles = projectilePool.GetActiveProjectiles();
            foreach (var projectile in projectiles)
            {
                if (projectile != null && projectile.IsActive)
                {
                    DrawDebugCircle(projectile.transform.position, 0.15f, projectileColor);
                    circlesDrawn++;
                }
            }
        }
        
        // Draw orbiter collision circles
        if (orbiterManager != null)
        {
            var orbiters = orbiterManager.GetActiveOrbiters();
            if (orbiters != null)
            {
                foreach (var orbiter in orbiters)
                {
                    if (orbiter != null && orbiter.IsActive)
                    {
                        DrawDebugCircle(orbiter.transform.position, 0.35f, orbiterColor);
                        circlesDrawn++;
                    }
                }
            }
        }
        
        // Log once every 5 seconds for debugging (reduced spam)
        if (Time.frameCount % 300 == 0 && circlesDrawn > 0)
        {
            DebugLog.Info($"[CollisionDebug] Drawing {circlesDrawn} hitbox circles");
        }
    }
    
    /// <summary>
    /// Draw a circle using Debug.DrawLine (visible in Game view with Gizmos enabled)
    /// </summary>
    private void DrawDebugCircle(Vector3 center, float radius, Color color)
    {
        int segments = 32;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * Mathf.PI * 2f;
            float angle2 = ((i + 1) / (float)segments) * Mathf.PI * 2f;
            
            Vector3 p1 = center + new Vector3(Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius, 0f);
            Vector3 p2 = center + new Vector3(Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius, 0f);
            
            Debug.DrawLine(p1, p2, color);
        }
    }
    
    private void CreateLineMaterial()
    {
        if (lineMaterial == null)
        {
            // Create a simple unlit material for GL.Lines
            Shader shader = Shader.Find("Hidden/Internal-Colored");
            lineMaterial = new Material(shader);
            lineMaterial.hideFlags = HideFlags.HideAndDontSave;
            lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
            lineMaterial.SetInt("_ZWrite", 0);
            lineMaterial.SetInt("_ZTest", (int)UnityEngine.Rendering.CompareFunction.Always);
            DebugLog.Info("[CollisionDebug] Created line material");
        }
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
            List<Enemy> enemies = enemyPool.GetActiveEnemies();
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
            List<Projectile> projectiles = projectilePool.GetActiveProjectiles();
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
    
    private void OnRenderObject()
    {
        // Always show hitboxes during gameplay (not just when paused)
        if (!showCollisionRadii)
        {
            return;
        }
        
        if (lineMaterial == null)
        {
            DebugLog.Warning("[CollisionDebug.OnRenderObject] lineMaterial is NULL!");
            return;
        }
        
        // Get the main camera
        Camera cam = Camera.main;
        if (cam == null)
        {
            DebugLog.Warning("[CollisionDebug.OnRenderObject] Camera.main is NULL!");
            return;
        }
        
        // Count entities (for frame 0 diagnostic only)
        if (Time.frameCount == 1)
        {
            int playerCount = playerTransform != null ? 1 : 0;
            int enemyCount = enemyPool != null ? enemyPool.GetActiveEnemies().Count : 0;
            int projectileCount = projectilePool != null ? projectilePool.GetActiveProjectiles().Count : 0;
            int orbiterCount = orbiterManager != null ? orbiterManager.GetActiveOrbiters().Count : 0;
            DebugLog.Info($"[CollisionDebug.OnRenderObject] Frame 1: Player={playerCount}, Enemies={enemyCount}, Projectiles={projectileCount}, Orbiters={orbiterCount}");
        }
        
        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.LoadProjectionMatrix(cam.projectionMatrix);
        GL.modelview = cam.worldToCameraMatrix;
        
        GL.Begin(GL.LINES);
        
        int circlesDrawn = 0;
        
        // Draw player collision circle
        if (playerTransform != null)
        {
            DrawCircleGL(playerTransform.position, 0.35f, playerColor);
            circlesDrawn++;
        }
        
        // Draw enemy collision circles
        if (enemyPool != null)
        {
            List<Enemy> enemies = enemyPool.GetActiveEnemies();
            foreach (var enemy in enemies)
            {
                if (enemy != null && enemy.gameObject.activeInHierarchy && enemy.IsActive)
                {
                    DrawCircleGL(enemy.transform.position, 0.35f, enemyColor);
                    circlesDrawn++;
                }
            }
        }
        
        // Draw projectile collision circles
        if (projectilePool != null)
        {
            List<Projectile> projectiles = projectilePool.GetActiveProjectiles();
            foreach (var projectile in projectiles)
            {
                if (projectile != null && projectile.IsActive)
                {
                    DrawCircleGL(projectile.transform.position, 0.15f, projectileColor);
                    circlesDrawn++;
                }
            }
        }
        
        // Draw orbiter collision circles
        if (orbiterManager != null)
        {
            var orbiters = orbiterManager.GetActiveOrbiters();
            if (orbiters != null)
            {
                foreach (var orbiter in orbiters)
                {
                    if (orbiter != null && orbiter.IsActive)
                    {
                        DrawCircleGL(orbiter.transform.position, 0.35f, orbiterColor);
                        circlesDrawn++;
                    }
                }
            }
        }
        
        GL.End();
        GL.PopMatrix();
        
        // Log once per second for debugging
        if (Time.frameCount % 60 == 0 && circlesDrawn > 0)
        {
            DebugLog.Info($"[CollisionDebug.OnRenderObject] Drew {circlesDrawn} hitbox circles this frame");
        }
    }
    
    private void DrawCircleGL(Vector3 center, float radius, Color color)
    {
        GL.Color(color);
        
        int segments = 32;
        for (int i = 0; i < segments; i++)
        {
            float angle1 = (i / (float)segments) * Mathf.PI * 2f;
            float angle2 = ((i + 1) / (float)segments) * Mathf.PI * 2f;
            
            Vector3 p1 = center + new Vector3(Mathf.Cos(angle1) * radius, Mathf.Sin(angle1) * radius, 0f);
            Vector3 p2 = center + new Vector3(Mathf.Cos(angle2) * radius, Mathf.Sin(angle2) * radius, 0f);
            
            GL.Vertex3(p1.x, p1.y, p1.z);
            GL.Vertex3(p2.x, p2.y, p2.z);
        }
    }
}
