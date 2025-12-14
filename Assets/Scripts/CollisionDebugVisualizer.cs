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
        }
        
        projectilePool = GameServices.ProjectilePool;
        enemyPool = GameServices.EnemyPool;
        orbiterManager = GameServices.OrbiterManager;
        
        CreateLineMaterial();
        
        DebugLog.Info("[CollisionDebug] Press 0 (zero) to toggle collision visualization");
    }
    
    private void Update()
    {
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
            DebugLog.Info($"[CollisionDebug] Entities to draw: Player={playerCount}, Enemies={enemyCount}, Projectiles={projectileCount}");
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
        if (!showCollisionRadii || lineMaterial == null) return;
        
        // Get the main camera
        Camera cam = Camera.main;
        if (cam == null) return;
        
        int entityCount = 0;
        if (playerTransform != null) entityCount++;
        if (enemyPool != null) entityCount += enemyPool.GetActiveEnemies().Count;
        if (projectilePool != null) entityCount += projectilePool.GetActiveProjectiles().Count;
        
        if (entityCount == 0) return;
        
        lineMaterial.SetPass(0);
        GL.PushMatrix();
        GL.LoadProjectionMatrix(cam.projectionMatrix);
        GL.modelview = cam.worldToCameraMatrix;
        
        GL.Begin(GL.LINES);
        
        // Draw player collision circle
        if (playerTransform != null)
        {
            DrawCircleGL(playerTransform.position, 0.35f, playerColor);
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
                    }
                }
            }
        }
        
        GL.End();
        GL.PopMatrix();
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
