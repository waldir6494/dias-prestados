using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace DiasPrestados.Tests.Editor
{
    /// <summary>
    /// Física aislada de una plataforma. Los valores de PlatformEffector2D deben coincidir
    /// con ForestDemoSceneBuilder (surfaceArc 180, useOneWayGrouping true, sideArc 1).
    /// </summary>
    public class PlatformOneWayPhysicsTests
    {
        const float SurfaceArc = 180f;
        const float SideArc = 1f;
        const bool Grouping = true;
        const float BaseY = 400f;

        [Test]
        public void OneWayPlatform_LandsFromAbove_AtFivePoints()
        {
            WithProbe((physics, scene) =>
            {
                GameObject platform = CreatePlatform(scene, oneWay: true);
                try
                {
                    float[] xs = { -2f, -1f, 0f, 1f, 2f };
                    for (int i = 0; i < xs.Length; i++)
                    {
                        GameObject body = CreateBody(scene, new Vector2(xs[i], BaseY + 2.8f), Vector2.zero);
                        Rigidbody2D rb = body.GetComponent<Rigidbody2D>();
                        float maxY = rb.position.y;
                        Simulate(physics, rb, 2.2f, ref maxY);
                        float feet = rb.position.y - 0.85f;
                        Assert.Greater(feet, BaseY, $"Se cayó a través de la plataforma en x={xs[i]}.");
                        Assert.AreEqual(BaseY + 0.175f, feet, 0.28f, $"No aterrizó en la tapa en x={xs[i]}.");
                        UnityEngine.Object.DestroyImmediate(body);
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(platform);
                }
            });
        }

        [Test]
        public void OneWayPlatform_PassesFromBelow_AtFivePoints()
        {
            WithProbe((physics, scene) =>
            {
                GameObject platform = CreatePlatform(scene, oneWay: true);
                try
                {
                    float[] xs = { -2f, -1f, 0f, 1f, 2f };
                    for (int i = 0; i < xs.Length; i++)
                    {
                        GameObject body = CreateBody(scene, new Vector2(xs[i], BaseY - 2.2f), new Vector2(0f, 18f));
                        Rigidbody2D rb = body.GetComponent<Rigidbody2D>();
                        float maxY = rb.position.y;
                        Simulate(physics, rb, 1.6f, ref maxY);
                        Assert.Greater(maxY, BaseY + 0.6f, $"No atravesó la plataforma desde abajo en x={xs[i]} (maxY={maxY}).");
                        UnityEngine.Object.DestroyImmediate(body);
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(platform);
                }
            });
        }

        [Test]
        public void SolidObstacle_BlocksFromBelow_AtFivePoints()
        {
            WithProbe((physics, scene) =>
            {
                GameObject platform = CreatePlatform(scene, oneWay: false);
                try
                {
                    float[] xs = { -2f, -1f, 0f, 1f, 2f };
                    for (int i = 0; i < xs.Length; i++)
                    {
                        GameObject body = CreateBody(scene, new Vector2(xs[i], BaseY - 2.2f), new Vector2(0f, 18f));
                        Rigidbody2D rb = body.GetComponent<Rigidbody2D>();
                        float maxY = rb.position.y;
                        Simulate(physics, rb, 1.6f, ref maxY);
                        Assert.Less(maxY, BaseY + 0.2f, $"El sólido dejó pasar el cuerpo en x={xs[i]} (maxY={maxY}).");
                        UnityEngine.Object.DestroyImmediate(body);
                    }
                }
                finally
                {
                    UnityEngine.Object.DestroyImmediate(platform);
                }
            });
        }

        static void WithProbe(System.Action<PhysicsScene2D, Scene> body)
        {
            Scene previous = SceneManager.GetActiveScene();
            SimulationMode2D previousMode = Physics2D.simulationMode;
            var suspended = new System.Collections.Generic.List<Rigidbody2D>();
            var wasSimulated = new System.Collections.Generic.List<bool>();
            Scene probe = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Additive);
            try
            {
                SceneManager.SetActiveScene(probe);
                foreach (var rb in UnityEngine.Object.FindObjectsByType<Rigidbody2D>(FindObjectsInactive.Include))
                {
                    if (rb == null || rb.gameObject.scene == probe)
                    {
                        continue;
                    }

                    suspended.Add(rb);
                    wasSimulated.Add(rb.simulated);
                    rb.simulated = false;
                }

                Physics2D.simulationMode = SimulationMode2D.Script;
                body(probe.GetPhysicsScene2D(), probe);
            }
            finally
            {
                Physics2D.simulationMode = previousMode;
                for (int i = 0; i < suspended.Count; i++)
                {
                    if (suspended[i] != null)
                    {
                        suspended[i].simulated = wasSimulated[i];
                    }
                }

                if (probe.IsValid() && probe.isLoaded)
                {
                    EditorSceneManager.CloseScene(probe, true);
                }

                if (previous.IsValid() && previous.isLoaded)
                {
                    SceneManager.SetActiveScene(previous);
                }
            }
        }

        static GameObject CreatePlatform(Scene scene, bool oneWay)
        {
            var go = new GameObject(oneWay ? "ProbePlatform" : "ProbeSolid");
            SceneManager.MoveGameObjectToScene(go, scene);
            go.transform.position = new Vector3(0f, BaseY, 0f);
            var box = go.AddComponent<BoxCollider2D>();
            box.size = new Vector2(6f, 0.35f);
            if (!oneWay)
            {
                return go;
            }

            box.usedByEffector = true;
            var effector = go.AddComponent<PlatformEffector2D>();
            effector.useOneWay = true;
            effector.useOneWayGrouping = Grouping;
            effector.surfaceArc = SurfaceArc;
            effector.sideArc = SideArc;
            effector.rotationalOffset = 0f;
            effector.useSideFriction = false;
            effector.useSideBounce = false;
            effector.useColliderMask = false;
            return go;
        }

        static GameObject CreateBody(Scene scene, Vector2 position, Vector2 velocity)
        {
            var go = new GameObject("ProbeBody");
            SceneManager.MoveGameObjectToScene(go, scene);
            var rb = go.AddComponent<Rigidbody2D>();
            rb.gravityScale = 3f;
            rb.freezeRotation = true;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.interpolation = RigidbodyInterpolation2D.None;
            rb.position = position;
            rb.linearVelocity = velocity;
            var capsule = go.AddComponent<CapsuleCollider2D>();
            capsule.size = new Vector2(0.55f, 1.7f);
            capsule.direction = CapsuleDirection2D.Vertical;
            return go;
        }

        static void Simulate(PhysicsScene2D physics, Rigidbody2D tracked, float seconds, ref float maxY)
        {
            int steps = Mathf.CeilToInt(seconds / 0.02f);
            for (int i = 0; i < steps; i++)
            {
                physics.Simulate(0.02f);
                if (tracked.position.y > maxY)
                {
                    maxY = tracked.position.y;
                }
            }
        }
    }
}
