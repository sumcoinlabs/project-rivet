// Project Rivet — original first-flight prototype. Unity 6.3 / Input System / 3D physics.
// Greybox gameplay only: no branded vehicle likenesses, advertising or network services.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectRivet
{
    public sealed class FirstFlightGame : MonoBehaviour
    {
        private const int CourseCount = 3;
        private const float WorldBottom = -4.8f;
        private readonly List<GameObject> spawned = new List<GameObject>();
        private WingoPhysics wingo;
        private Camera gameCamera;
        private int course;
        private int collected;
        private int totalCollected;
        private int availableCoins;
        private bool completed;
        private float showHintUntil;
        private GUIStyle headline;
        private GUIStyle small;
        private GUIStyle bigButton;
        private bool showTouchHint;

        public bool IsCompleted => completed;
        public int Course => course;

        private void Start()
        {
            Application.targetFrameRate = 60;
            Time.fixedDeltaTime = 1f / 60f;
            ConfigureCamera();
            LoadCourse(0);
        }

        private void ConfigureCamera()
        {
            gameCamera = Camera.main;
            if (gameCamera == null)
            {
                GameObject go = new GameObject("Main Camera");
                go.tag = "MainCamera";
                gameCamera = go.AddComponent<Camera>();
            }
            gameCamera.orthographic = true;
            gameCamera.orthographicSize = 5.4f;
            gameCamera.transform.position = new Vector3(0f, 2.0f, -18f);
            gameCamera.transform.rotation = Quaternion.identity;
            gameCamera.backgroundColor = new Color(0.49f, 0.76f, 0.96f);
            gameCamera.clearFlags = CameraClearFlags.SolidColor;

            if (FindFirstObjectByType<Light>() == null)
            {
                GameObject lightObject = new GameObject("Sunlight");
                Light sun = lightObject.AddComponent<Light>();
                sun.type = LightType.Directional;
                sun.intensity = 1.6f;
                lightObject.transform.rotation = Quaternion.Euler(40f, -30f, 10f);
            }
        }

        private static Material MakeMaterial(Color color)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Lit");
            if (shader == null) shader = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (shader == null) shader = Shader.Find("Standard");
            Material material = new Material(shader);
            material.color = color;
            return material;
        }

        private GameObject Shape(string objectName, PrimitiveType primitive, Vector3 position,
            Vector3 scale, Color tint, bool solid = true)
        {
            GameObject go = GameObject.CreatePrimitive(primitive);
            go.name = objectName;
            go.transform.position = position;
            go.transform.localScale = scale;
            Collider collider = go.GetComponent<Collider>();
            if (!solid && collider != null) Destroy(collider);
            Renderer renderer = go.GetComponent<Renderer>();
            renderer.sharedMaterial = MakeMaterial(tint);
            spawned.Add(go);
            return go;
        }

        private void LoadCourse(int index)
        {
            for (int i = spawned.Count - 1; i >= 0; --i)
                if (spawned[i] != null) Destroy(spawned[i]);
            spawned.Clear();
            wingo = null;
            completed = false;
            collected = 0;
            availableCoins = 0;
            course = Mathf.Clamp(index, 0, CourseCount - 1);
            showHintUntil = Time.unscaledTime + 9f;
            showTouchHint = true;

            // Simple, intentionally unbranded greybox art. Side-view game physics in XY.
            Shape("Backdrop cloud A", PrimitiveType.Sphere, new Vector3(-2.3f, 4.5f, 5f),
                new Vector3(1.8f, 0.35f, 0.3f), new Color(0.94f, 0.98f, 1f), false);
            Shape("Backdrop cloud B", PrimitiveType.Sphere, new Vector3(1.9f, 3.8f, 5f),
                new Vector3(1.35f, 0.3f, 0.3f), Color.white, false);

            Color ground = new Color(0.26f, 0.39f, 0.55f);
            if (course == 0)
            {
                Shape("Course 1 floor", PrimitiveType.Cube, new Vector3(0, -1.73f, 0),
                    new Vector3(6.3f, 0.35f, 1.2f), ground);
                Shape("Hop obstacle", PrimitiveType.Cube, new Vector3(0.1f, -1.25f, 0),
                    new Vector3(0.55f, 0.6f, 0.9f), new Color(0.97f, 0.58f, 0.37f));
                AddCoin(-0.30f, 0.05f);
                AddCoin(0.45f, 0.65f);
                AddCoin(1.18f, 0.15f);
                AddCharger(new Vector3(2.30f, -1.12f, 0));
                AddWingo(new Vector3(-2.2f, -1.05f, 0));
            }
            else if (course == 1)
            {
                Shape("Left cliff", PrimitiveType.Cube, new Vector3(-2f, -1.75f, 0),
                    new Vector3(2.7f, 0.35f, 1.2f), ground);
                Shape("Right cliff", PrimitiveType.Cube, new Vector3(2.0f, -1.75f, 0),
                    new Vector3(2.6f, 0.35f, 1.2f), ground);
                AddSpring(new Vector3(-1.04f, -1.43f, 0));
                AddCoin(-0.10f, 0.65f);
                AddCoin(0.55f, 1.24f);
                AddCoin(1.20f, 0.65f);
                AddCharger(new Vector3(2.23f, -1.12f, 0));
                AddWingo(new Vector3(-2.36f, -1.04f, 0));
            }
            else
            {
                Shape("Takeoff ground", PrimitiveType.Cube, new Vector3(-1.93f, -1.76f, 0),
                    new Vector3(2.85f, 0.35f, 1.2f), ground);
                Shape("Landing platform", PrimitiveType.Cube, new Vector3(2.09f, 0.12f, 0),
                    new Vector3(2.1f, 0.3f, 1.1f), ground);
                GameObject ramp = Shape("Angled ramp", PrimitiveType.Cube, new Vector3(-0.63f, -1.16f, 0),
                    new Vector3(1.38f, 0.2f, 1.1f), new Color(0.95f, 0.60f, 0.32f));
                ramp.transform.rotation = Quaternion.Euler(0, 0, 25);
                AddBumper(new Vector3(0.47f, 0.02f, 0));
                AddCoin(-0.2f, 0.92f);
                AddCoin(0.65f, 1.69f);
                AddCoin(1.48f, 1.89f);
                AddCharger(new Vector3(2.24f, 0.70f, 0));
                AddWingo(new Vector3(-2.35f, -1.04f, 0));
            }
        }

        private void AddWingo(Vector3 position)
        {
            GameObject root = new GameObject("Wingo — prototype (original placeholder)");
            root.transform.position = position;
            spawned.Add(root);
            Rigidbody body = root.AddComponent<Rigidbody>();
            body.mass = 1.2f;
            body.linearDamping = 0.30f;
            body.angularDamping = 0.85f;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.constraints = RigidbodyConstraints.FreezePositionZ |
                               RigidbodyConstraints.FreezeRotationX |
                               RigidbodyConstraints.FreezeRotationY;

            BoxCollider hitbox = root.AddComponent<BoxCollider>();
            hitbox.size = new Vector3(1.07f, 0.46f, 0.49f);
            hitbox.center = new Vector3(0, 0.11f, 0);
            PhysicsMaterial bounce = new PhysicsMaterial("Wingo soft bounce");
            bounce.bounciness = 0.18f;
            bounce.dynamicFriction = 0.55f;
            bounce.staticFriction = 0.55f;
            bounce.bounceCombine = PhysicsMaterialCombine.Maximum;
            hitbox.material = bounce;

            GameObject bodyArt = ChildShape(root.transform, "Bright body", PrimitiveType.Cube,
                new Vector3(0, 0.12f, 0), new Vector3(1.10f, 0.41f, 0.50f),
                new Color(0.13f, 0.73f, 0.94f));
            ChildShape(root.transform, "Windshield / expressive face", PrimitiveType.Cube,
                new Vector3(-0.08f, 0.41f, 0), new Vector3(0.69f, 0.26f, 0.43f),
                new Color(0.06f, 0.20f, 0.34f));
            ChildShape(root.transform, "Left headlight / eye", PrimitiveType.Sphere,
                new Vector3(0.37f, 0.33f, -0.24f), new Vector3(0.12f, 0.10f, 0.055f),
                Color.white);
            ChildShape(root.transform, "Right wheel", PrimitiveType.Cylinder,
                new Vector3(0.33f, -0.12f, -0.04f), new Vector3(0.20f, 0.14f, 0.20f),
                new Color(0.08f, 0.10f, 0.15f)).transform.localRotation = Quaternion.Euler(90, 0, 0);
            ChildShape(root.transform, "Left wheel", PrimitiveType.Cylinder,
                new Vector3(-0.34f, -0.12f, -0.04f), new Vector3(0.20f, 0.14f, 0.20f),
                new Color(0.08f, 0.10f, 0.15f)).transform.localRotation = Quaternion.Euler(90, 0, 0);

            GameObject frontWing = ChildShape(root.transform, "Front wing-door", PrimitiveType.Cube,
                new Vector3(-0.24f, 0.63f, -0.17f), new Vector3(0.43f, 0.08f, 0.29f),
                new Color(0.79f, 0.95f, 1.00f));
            GameObject rearWing = ChildShape(root.transform, "Rear wing-door", PrimitiveType.Cube,
                new Vector3(0.26f, 0.63f, 0.17f), new Vector3(0.43f, 0.08f, 0.29f),
                new Color(0.79f, 0.95f, 1.00f));

            wingo = root.AddComponent<WingoPhysics>();
            wingo.Initialize(this, body, frontWing.transform, rearWing.transform);
        }

        private GameObject ChildShape(Transform parent, string name, PrimitiveType type,
            Vector3 localPosition, Vector3 localScale, Color tint)
        {
            GameObject go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = localPosition;
            go.transform.localScale = localScale;
            Collider collider = go.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            go.GetComponent<Renderer>().sharedMaterial = MakeMaterial(tint);
            return go;
        }

        private void AddCoin(float x, float y)
        {
            GameObject coin = Shape("Energy coin", PrimitiveType.Sphere, new Vector3(x, y, 0),
                new Vector3(0.29f, 0.29f, 0.12f), new Color(1f, 0.78f, 0.16f));
            Collider trigger = coin.GetComponent<Collider>();
            trigger.isTrigger = true;
            CoinTrigger pickup = coin.AddComponent<CoinTrigger>();
            pickup.Initialize(this);
            availableCoins++;
        }

        private void AddCharger(Vector3 position)
        {
            Shape("Charging station base", PrimitiveType.Cube, position + new Vector3(0.07f, -0.24f, 0),
                new Vector3(0.85f, 0.22f, 0.85f), new Color(0.15f, 0.27f, 0.44f));
            Shape("Charger pedestal", PrimitiveType.Cube, position + new Vector3(0.35f, 0.18f, 0.06f),
                new Vector3(0.21f, 0.71f, 0.28f), new Color(0.95f, 0.99f, 1.00f), false);
            Shape("Charger blue indicator", PrimitiveType.Cube, position + new Vector3(0.35f, 0.30f, -0.12f),
                new Vector3(0.12f, 0.18f, 0.05f), new Color(0.04f, 0.92f, 0.75f), false);
            GameObject finish = Shape("Charging target trigger", PrimitiveType.Cube,
                position + new Vector3(-0.12f, 0.31f, 0),
                new Vector3(0.9f, 0.88f, 0.77f), new Color(0.25f, 1f, 0.6f), false);
            BoxCollider trigger = finish.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            finish.GetComponent<Renderer>().enabled = false;
            finish.AddComponent<ChargerTrigger>().Initialize(this);
        }

        private void AddSpring(Vector3 position)
        {
            GameObject spring = Shape("Jump spring", PrimitiveType.Cube, position,
                new Vector3(0.49f, 0.18f, 0.70f), new Color(0.95f, 0.27f, 0.50f));
            spring.AddComponent<SpringPad>();
        }

        private void AddBumper(Vector3 position)
        {
            GameObject bumper = Shape("Bouncy bumper", PrimitiveType.Sphere, position,
                new Vector3(0.60f, 0.60f, 0.70f), new Color(0.98f, 0.36f, 0.34f));
            SphereCollider collider = bumper.GetComponent<SphereCollider>();
            PhysicsMaterial mat = new PhysicsMaterial("Bumper bouncy");
            mat.bounciness = 0.95f;
            mat.bounceCombine = PhysicsMaterialCombine.Maximum;
            collider.material = mat;
        }

        public void CollectCoin(GameObject coin)
        {
            if (completed) return;
            collected++;
            totalCollected++;
            coin.SetActive(false);
        }

        public void ChargeComplete()
        {
            if (completed || wingo == null) return;
            completed = true;
            wingo.Finish();
        }

        public void Fail()
        {
            if (completed || wingo == null) return;
            showTouchHint = false;
            wingo.Fail();
        }

        private void Update()
        {
            if (wingo != null && !completed && wingo.transform.position.y < WorldBottom)
                Fail();
            if (Keyboard.current != null)
            {
                if (Keyboard.current.rKey.wasPressedThisFrame) LoadCourse(course);
                if (Keyboard.current.nKey.wasPressedThisFrame) LoadCourse((course + 1) % CourseCount);
            }
        }

        private void OnGUI()
        {
            if (headline == null)
            {
                headline = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold };
                headline.normal.textColor = new Color(0.07f, 0.17f, 0.27f);
                small = new GUIStyle(GUI.skin.label) { fontSize = 18, wordWrap = true };
                small.normal.textColor = new Color(0.08f, 0.18f, 0.28f);
                bigButton = new GUIStyle(GUI.skin.button) { fontSize = 20, fontStyle = FontStyle.Bold };
            }
            float width = Screen.width;
            float scale = Mathf.Max(1f, Screen.height / 800f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1));
            float layoutWidth = width / scale;
            float y = 12;
            GUI.Label(new Rect(15, y, layoutWidth - 30, 36), "FIRST FLIGHT  •  COURSE " + (course + 1) + "/3", headline);
            y += 40;
            GUI.Label(new Rect(15, y, layoutWidth - 30, 32),
                "⚡ Coins: " + collected + "/" + availableCoins + "  |  Battery: " + (wingo == null ? "--" : Mathf.RoundToInt(wingo.Battery * 100f) + "%"), small);
            if (completed)
            {
                GUI.Label(new Rect(15, 95, layoutWidth - 30, 42), "CHARGE COMPLETE!", headline);
                if (GUI.Button(new Rect(layoutWidth / 2 - 100, 155, 200, 58),
                    course == 2 ? "PLAY AGAIN" : "NEXT COURSE", bigButton))
                    LoadCourse((course + 1) % CourseCount);
            }
            else if (wingo != null && wingo.HasFailed)
            {
                GUI.Label(new Rect(15, 100, layoutWidth - 30, 44), "OOPS! TRY AGAIN", headline);
                if (GUI.Button(new Rect(layoutWidth / 2 - 100, 157, 200, 58), "RETRY", bigButton))
                    LoadCourse(course);
            }
            else if (showTouchHint && Time.unscaledTime < showHintUntil)
            {
                GUI.Label(new Rect(15, Screen.height / scale - 135, layoutWidth - 30, 95),
                    "Swipe UP + RIGHT, release to launch.\nThen HOLD to flap/glide!  (Space/R/N on Mac)", small);
            }
            if (GUI.Button(new Rect(layoutWidth - 110, Screen.height / scale - 72, 94, 50), "RETRY", bigButton))
                LoadCourse(course);
            GUI.matrix = Matrix4x4.identity;
        }
    }

    public sealed class WingoPhysics : MonoBehaviour
    {
        private FirstFlightGame game;
        private Rigidbody body;
        private Transform leftWing;
        private Transform rightWing;
        private Vector2 gestureStart;
        private bool tracking;
        private bool launched;
        private bool wingHeld;
        private bool finished;
        private bool failed;
        private float battery = 1f;
        public float Battery => battery;
        public bool HasFailed => failed;

        public void Initialize(FirstFlightGame director, Rigidbody physicsBody, Transform firstWing, Transform secondWing)
        {
            game = director;
            body = physicsBody;
            leftWing = firstWing;
            rightWing = secondWing;
            body.isKinematic = true;
        }

        private void Update()
        {
            if (finished || failed) return;
            bool down = false, held = false, up = false;
            Vector2 screen = default;
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                var touch = Touchscreen.current.primaryTouch;
                down = touch.press.wasPressedThisFrame;
                held = touch.press.isPressed;
                up = touch.press.wasReleasedThisFrame;
                screen = touch.position.ReadValue();
            }
            else if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
            {
                up = true;
                screen = Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else if (Mouse.current != null)
            {
                down = Mouse.current.leftButton.wasPressedThisFrame;
                held = Mouse.current.leftButton.isPressed;
                up = Mouse.current.leftButton.wasReleasedThisFrame;
                screen = Mouse.current.position.ReadValue();
            }

            if (!launched)
            {
                if (down && screen.y < Screen.height * 0.83f)
                {
                    tracking = true;
                    gestureStart = screen;
                }
                if (up && tracking)
                {
                    tracking = false;
                    Vector2 drag = screen - gestureStart;
                    if (drag.magnitude > 24f)
                    {
                        Vector2 normalized = Vector2.ClampMagnitude(drag / Mathf.Max(220f, Screen.height * 0.40f), 1f);
                        normalized.y = Mathf.Max(0.20f, normalized.y);
                        body.isKinematic = false;
                        body.AddForce(new Vector3(normalized.x, normalized.y, 0) * 12.8f, ForceMode.Impulse);
                        launched = true;
                    }
                }
                if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
                {
                    body.isKinematic = false;
                    body.AddForce(new Vector3(6.0f, 5.3f, 0), ForceMode.Impulse);
                    launched = true;
                }
            }
            else
            {
                wingHeld = held || (Keyboard.current != null && Keyboard.current.spaceKey.isPressed);
            }
            float deploy = wingHeld && battery > 0.002f ? 1f : 0f;
            // Animate a comic wingbeat while the ability is active; physics stays on Rigidbody.
            float wingbeatDegrees = deploy * (28f + 15f * Mathf.Sin(Time.time * 14f));
            leftWing.localRotation = Quaternion.Slerp(leftWing.localRotation,
                Quaternion.Euler(0, 0, -wingbeatDegrees), Time.deltaTime * 10f);
            rightWing.localRotation = Quaternion.Slerp(rightWing.localRotation,
                Quaternion.Euler(0, 0, wingbeatDegrees), Time.deltaTime * 10f);
        }

        private void FixedUpdate()
        {
            if (!launched || finished || failed || body.isKinematic) return;
            if (wingHeld && battery > 0f)
            {
                float gravityCompensation = 8.0f;
                float forwardAssist = 2.1f;
                body.AddForce(new Vector3(forwardAssist, gravityCompensation, 0), ForceMode.Acceleration);
                Vector3 current = body.linearVelocity;
                if (current.y < -2.5f) body.linearVelocity = new Vector3(current.x, -2.5f, 0);
                battery = Mathf.Max(0, battery - Time.fixedDeltaTime * 0.19f);
            }
            if (Mathf.Abs(transform.position.x) > 5.0f) game.Fail();
        }

        public void Finish()
        {
            finished = true;
            wingHeld = false;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
            battery = 1f;
        }

        public void Fail()
        {
            failed = true;
            wingHeld = false;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
        }
    }

    public sealed class ChargerTrigger : MonoBehaviour
    {
        private FirstFlightGame director;
        public void Initialize(FirstFlightGame game) => director = game;
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<WingoPhysics>() != null && director != null)
                director.ChargeComplete();
        }
    }

    public sealed class CoinTrigger : MonoBehaviour
    {
        private FirstFlightGame director;
        public void Initialize(FirstFlightGame game) => director = game;
        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponentInParent<WingoPhysics>() != null && director != null)
                director.CollectCoin(gameObject);
        }
        private void Update() => transform.Rotate(0f, 130f * Time.deltaTime, 0f);
    }

    public sealed class SpringPad : MonoBehaviour
    {
        private void OnCollisionEnter(Collision collision)
        {
            if (collision.collider.GetComponentInParent<WingoPhysics>() == null) return;
            Rigidbody body = collision.rigidbody;
            if (body == null) return;
            body.AddForce(new Vector3(1.7f, 8.4f, 0), ForceMode.Impulse);
        }
    }
}
