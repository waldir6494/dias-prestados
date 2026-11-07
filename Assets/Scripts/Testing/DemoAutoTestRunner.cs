#if UNITY_EDITOR
using System.Collections;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using DiasPrestados.Companions;
using DiasPrestados.Interaction;
namespace DiasPrestados.Testing
{
    public class DemoAutoTestRunner : MonoBehaviour
    {
        public const string OutputFolder = "Temp/DemoAutotest";
        const float CliffX = 118f;
        const float RavineLeft = 82f;
        const float RavineRight = 85.4f;

        readonly StringBuilder report = new StringBuilder();
        Keyboard keyboard;
        PlayerController2D player;
        CompanionFollower brother;
        InteractionSensor sensor;
        Rigidbody2D playerBody;
        bool right;
        bool jump;
        bool interact;
        float t0;

        void Start()
        {
            InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
            keyboard = InputSystem.AddDevice<Keyboard>("AutoTestKeyboard");
            StartCoroutine(Main());
        }

        void Log(string line)
        {
            string stamped = $"[{Time.time - t0,6:0.0}s] {line}";
            report.AppendLine(stamped);
            Debug.Log("[Autotest] " + stamped);
        }

        void PushKeys()
        {
            var state = new KeyboardState();
            if (right)
            {
                state.Press(Key.D);
            }

            if (jump)
            {
                state.Press(Key.UpArrow);
            }

            if (interact)
            {
                state.Press(Key.E);
            }

            InputSystem.QueueStateEvent(keyboard, state);
            if (!interact)
            {
                // Con E pulsada se deja que el update dinámico procese el evento, para que WasPressedThisFrame lo vea.
                InputSystem.Update();
            }
        }

        void SetKeys(bool r, bool j, bool e)
        {
            right = r;
            jump = j;
            interact = e;
            PushKeys();
        }

        IEnumerator Tap(bool r, float seconds)
        {
            SetKeys(r, true, false);
            yield return new WaitForSeconds(seconds);
            SetKeys(r, false, false);
        }

        IEnumerator Shot(string name)
        {
            yield return new WaitForEndOfFrame();
            Camera cam = Camera.main;
            var rt = new RenderTexture(1600, 900, 24);
            RenderTexture prevTarget = cam.targetTexture;
            cam.targetTexture = rt;
            cam.Render();
            cam.targetTexture = prevTarget;
            RenderTexture prev = RenderTexture.active;
            RenderTexture.active = rt;
            var tex = new Texture2D(1600, 900, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(0, 0, 1600, 900), 0, 0);
            tex.Apply();
            RenderTexture.active = prev;
            File.WriteAllBytes(Path.Combine(OutputFolder, name + ".png"), tex.EncodeToPNG());
            Destroy(tex);
            rt.Release();
            Destroy(rt);
        }

        bool WallAhead()
        {
            Vector2 o = playerBody.position + new Vector2(0.45f, -0.5f);
            RaycastHit2D h = Physics2D.Raycast(o, Vector2.right, 0.5f, ~(1 << brother.gameObject.layer));
            return h.collider != null && !h.collider.isTrigger && h.collider.transform != player.transform;
        }

        bool GapAhead()
        {
            Vector2 o = playerBody.position + new Vector2(1.1f, -0.9f);
            RaycastHit2D h = Physics2D.Raycast(o, Vector2.down, 1.6f, ~(1 << brother.gameObject.layer));
            return h.collider == null;
        }

        IEnumerator Main()
        {
            t0 = Time.time;
            yield return null;
            player = FindFirstObjectByType<PlayerController2D>();
            brother = FindFirstObjectByType<CompanionFollower>();
            sensor = FindFirstObjectByType<InteractionSensor>();
            playerBody = player.GetComponent<Rigidbody2D>();
            bool ok = true;

            Log($"Inicio. Jugador {playerBody.position}, hermano {brother.transform.position}");
            yield return new WaitForSeconds(1.5f);
            yield return Shot("01_inicio");

            // Movimiento
            float x0 = playerBody.position.x;
            SetKeys(true, false, false);
            yield return new WaitForSeconds(2f);
            float dx = playerBody.position.x - x0;
            Log($"Caminar 2s a la derecha: avance {dx:0.0}u (esperado ~12-14). Hermano en x={brother.transform.position.x:0.0}");
            ok &= Check(dx > 8f, "El jugador se mueve hacia la derecha");
            yield return Shot("02_caminando");

            // Salto
            SetKeys(true, false, false);
            yield return new WaitForSeconds(0.1f);
            float y0 = playerBody.position.y;
            float maxY = y0;
            SetKeys(true, true, false);
            float tEnd = Time.time + 0.9f;
            bool shot = false;
            while (Time.time < tEnd)
            {
                maxY = Mathf.Max(maxY, playerBody.position.y);
                if (!shot && playerBody.position.y > y0 + 1.5f)
                {
                    shot = true;
                    yield return Shot("03_salto");
                }

                yield return null;
            }

            SetKeys(true, false, false);
            Log($"Salto: altura máxima {maxY - y0:0.00}u (esperado ~2.5-2.9)");
            ok &= Check(maxY - y0 > 1.8f, "El salto alcanza altura suficiente");

            // Recorrido con piloto automático
            float tLimit = Time.time + 150f;
            bool boostDone = false;
            bool askShot = false;
            bool ravineShot = false;
            float lastJump = -10f;
            float lastLog = Time.time;
            float stuckSince = -1f;
            float lastX = playerBody.position.x;
            bool reachedPlateau = false;
            float minBrotherDistAfterRavine = 0f;

            while (Time.time < tLimit)
            {
                float px = playerBody.position.x;
                bool grounded = player.IsGrounded;

                if (!reachedPlateau && px > CliffX + 1f && playerBody.position.y > 2.3f)
                {
                    reachedPlateau = true;
                    Log($"Jugador sobre el risco. Hermano: modo {brother.Mode}, pos {brother.transform.position}");
                }

                // El piloto espera en la meseta del risco hasta ayudar al pequeño.
                if (reachedPlateau && !boostDone)
                {
                    SetKeys(false, false, false);
                    if (Time.time - lastLog > 2f)
                    {
                        lastLog = Time.time;
                        Log($"Espera: jugador ({playerBody.position.x:0.0},{playerBody.position.y:0.00}) hermano {brother.transform.position} modo {brother.Mode} suelo {brother.IsGrounded} vel {brother.Velocity}");
                    }

                    if (!askShot && brother.IsWaitingForHelp)
                    {
                        askShot = true;
                        yield return new WaitForSeconds(0.6f);
                        Log($"El hermano pide ayuda (modo {brother.Mode}); prompt: {(sensor.Current != null ? "visible" : "NO visible")}");
                        yield return Shot("05_pide_ayuda");
                    }

                    if (brother.IsWaitingForHelp && sensor.Current != null)
                    {
                        Log("Pulsando E para ayudar.");
                        SetKeys(false, false, true);
                        yield return null;
                        yield return null;
                        SetKeys(false, false, false);
                        yield return new WaitForSeconds(0.45f);
                        yield return Shot("06_impulso");
                        yield return new WaitForSeconds(1.6f);
                        Log($"Tras el impulso: hermano en {brother.transform.position}, modo {brother.Mode}");
                        ok &= Check(brother.transform.position.y > 2.4f, "El hermano pequeño llega al risco tras la ayuda");
                        yield return Shot("07_animo");
                        boostDone = true;
                    }
                    else if (Time.time > tLimit - 100f && !askShot)
                    {
                        ok &= Check(false, "El hermano nunca pidió ayuda");
                        break;
                    }

                    yield return null;
                    continue;
                }

                // Avance con salto automático ante obstáculos o huecos
                bool wantJump = grounded && Time.time - lastJump > 0.5f && (WallAhead() || GapAhead());
                if (wantJump)
                {
                    lastJump = Time.time;
                    if (!ravineShot && px > RavineLeft - 3f && px < RavineRight)
                    {
                        ravineShot = true;
                    }

                    SetKeys(true, true, false);
                    float end = Time.time + 0.6f;
                    while (Time.time < end)
                    {
                        yield return null;
                    }

                    SetKeys(true, false, false);
                }
                else
                {
                    SetKeys(true, false, false);
                }

                if (Time.time - lastLog > 8f)
                {
                    lastLog = Time.time;
                    Log($"x={px:0.0} y={playerBody.position.y:0.0} | hermano x={brother.transform.position.x:0.0} modo={brother.Mode}");
                }

                if (Mathf.Abs(px - lastX) < 0.05f)
                {
                    stuckSince = stuckSince < 0f ? Time.time : stuckSince;
                    if (Time.time - stuckSince > 6f && !reachedPlateau)
                    {
                        Log($"ATASCADO el jugador en x={px:0.0} y={playerBody.position.y:0.0}");
                        ok = false;
                        yield return Shot("99_atascado");
                        break;
                    }
                }
                else
                {
                    stuckSince = -1f;
                    lastX = px;
                }

                if (px > 189f)
                {
                    Log($"Final alcanzado en x={px:0.0}. Hermano en x={brother.transform.position.x:0.0}");
                    minBrotherDistAfterRavine = Mathf.Abs(px - brother.transform.position.x);
                    yield return new WaitForSeconds(1.0f);
                    yield return Shot("08_final");
                    break;
                }

                yield return null;
            }

            SetKeys(false, false, false);
            ok &= Check(playerBody.position.x > 189f, "El jugador completa el recorrido");
            ok &= Check(boostDone, "Se completó la interacción de ayuda");
            ok &= Check(minBrotherDistAfterRavine < 12f, "El hermano pequeño sigue cerca al final");
            Log(ok ? "RESULTADO: TODO CORRECTO" : "RESULTADO: HAY FALLOS (ver arriba)");
            File.WriteAllText(Path.Combine(OutputFolder, "report.txt"), report.ToString());
            InputSystem.RemoveDevice(keyboard);
            EditorApplication.isPlaying = false;
        }

        bool Check(bool condition, string description)
        {
            Log((condition ? "OK   " : "FALLO") + " - " + description);
            return condition;
        }
    }
}
#endif
