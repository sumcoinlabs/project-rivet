// Project Rivet — Sprint 2: original repeatable physics puzzle prototype.
// No licensed vehicle models, third-party gameplay source, ads, networking or analytics.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectRivet
{
    public sealed class FirstFlightGame : MonoBehaviour
    {
        private const int CourseCount = 3;
        private readonly List<GameObject> stage = new List<GameObject>();
        private readonly Dictionary<Color32, Material> materials = new Dictionary<Color32, Material>();
        private readonly string[] courseNames = { "FIRST CHARGE", "THE CANYON", "THE WIND TUNNEL" };
        private Camera cam;
        private WingoPhysics wingo;
        private int course;
        private int coins;
        private int availableCoins;
        private int hops;
        private bool won;
        private bool failed;
        private float elapsed;
                private GUIStyle titleStyle, labelStyle, hintStyle, buttonStyle, resultStyle;
        private float hudScale;
        private int lastWidth;
        private int lastHeight;
        private Vector3 cameraSmooth;
        
        public bool IsCompleted => won;
        public bool IsFinished => won || failed;
        public int Course => course;
        public WingoPhysics Wingo => wingo;

        private void Start()
        {
            Application.targetFrameRate = 60;
            Time.fixedDeltaTime = 1f / 60f;
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            ConfigureCamera();
            LoadCourse(0);
        }

        private void ConfigureCamera()
        {
            cam = Camera.main;
            if (cam == null)
            {
                GameObject obj = new GameObject("Main Camera");
                obj.tag = "MainCamera";
                cam = obj.AddComponent<Camera>();
            }
            cam.orthographic = true;
            cam.orthographicSize = 4.8f;
            cam.transform.position = new Vector3(0, .55f, -22f);
            cam.transform.rotation = Quaternion.identity;
            cam.backgroundColor = new Color(.46f, .74f, .93f);
            cam.clearFlags = CameraClearFlags.SolidColor;
            CameraReset();
            if (FindFirstObjectByType<Light>() == null)
            {
                GameObject sun = new GameObject("Sunlight");
                Light light = sun.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.7f;
                sun.transform.rotation = Quaternion.Euler(36f, -35f, 15f);
            }
        }

        private void CameraReset()
        {
            if (cam == null) return;
            UpdateCameraSize();
            cam.transform.position = new Vector3(Screen.width < Screen.height ? -1.7f : 0f, .45f, -22f);
            cameraSmooth = Vector3.zero;
        }

        private void UpdateCameraSize()
        {
            if (cam == null) return;
            cam.orthographicSize = Screen.height > Screen.width ? 5.1f : 4.4f;
        }

        private Material Mat(Color col)
        {
            Color32 key = col;
            Material m;
            if (materials.TryGetValue(key, out m)) return m;
            Shader s = Shader.Find("Universal Render Pipeline/Lit");
            if (s == null) s = Shader.Find("Universal Render Pipeline/Simple Lit");
            if (s == null) s = Shader.Find("Standard");
            m = new Material(s);
            m.color = col;
            if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", col);
            materials[key] = m;
            return m;
        }

        private GameObject Shape(string name, PrimitiveType primitive, Vector3 pos, Vector3 size, Color col, bool collision = true)
        {
            GameObject obj = GameObject.CreatePrimitive(primitive);
            obj.name = name;
            obj.transform.position = pos;
            obj.transform.localScale = size;
            obj.GetComponent<Renderer>().sharedMaterial = Mat(col);
            Collider collider = obj.GetComponent<Collider>();
            if (!collision && collider != null) Destroy(collider);
            stage.Add(obj);
            return obj;
        }

        private GameObject Part(Transform parent, string name, PrimitiveType primitive, Vector3 localPos, Vector3 size, Color color)
        {
            GameObject obj = GameObject.CreatePrimitive(primitive);
            obj.name = name;
            obj.transform.SetParent(parent, false);
            obj.transform.localPosition = localPos;
            obj.transform.localScale = size;
            Collider collider = obj.GetComponent<Collider>();
            if (collider != null) Destroy(collider);
            obj.GetComponent<Renderer>().sharedMaterial = Mat(color);
            return obj;
        }

        private static readonly Color Navy = new Color(.09f, .17f, .32f);
        private static readonly Color Steel = new Color(.24f, .39f, .55f);
        private static readonly Color Teal = new Color(.02f, .86f, .85f);
        private static readonly Color Gold = new Color(1f, .77f, .18f);
        private static readonly Color Cream = new Color(.98f, .96f, .87f);

        public void LoadCourse(int next)
        {
            for (int i = stage.Count - 1; i >= 0; --i) if (stage[i] != null) Destroy(stage[i]);
            stage.Clear();
            course = Mathf.Clamp(next, 0, CourseCount - 1);
            won = false;
            failed = false;
            coins = 0;
            availableCoins = 0;
            hops = 0;
            elapsed = 0f;
                        wingo = null;
            CameraReset();
            PopulateBackdrop();
            if (course == 0) BuildOne();
            else if (course == 1) BuildTwo();
            else BuildThree();
        }

        private void PopulateBackdrop()
        {
            Color distant = course == 2 ? new Color(.37f, .42f, .67f) : new Color(.35f, .62f, .76f);
            Color mid = course == 2 ? new Color(.33f, .31f, .59f) : new Color(.22f, .55f, .64f);
            for (int i = 0; i < 9; ++i)
            {
                float x = -8f + i * 2.2f;
                float high = (i % 3) * .37f;
                Shape("Distant hillside", PrimitiveType.Sphere, new Vector3(x, -3.5f + high, 9f),
                    new Vector3(4.5f, 4.4f + high, 1.1f), distant, false);
                Shape("Near hillside", PrimitiveType.Sphere, new Vector3(x + .9f, -4.45f, 6f),
                    new Vector3(4.0f, 3.6f, 1f), mid, false);
            }
            for (int i = 0; i < 5; i++)
            {
                float x = -7f + i * 3.6f;
                float y = 3.1f + (i % 2) * .65f;
                GameObject cloud = Shape("Soft cloud", PrimitiveType.Sphere,
                    new Vector3(x, y, 8f), new Vector3(1.9f, .38f, .5f), Cream, false);
                cloud.AddComponent<CloudFloat>().Set(i * .65f);
            }
            Shape("Low fog band", PrimitiveType.Cube, new Vector3(0, -4.7f, 7.8f),
                new Vector3(27f, .6f, .3f), new Color(.19f, .48f, .65f), false);
        }

        private void Platform(string name, float x, float y, float width)
        {
            Shape(name + " base", PrimitiveType.Cube, new Vector3(x, y - .19f, 0f),
                new Vector3(width, .58f, 1.35f), Navy);
            Shape(name + " trim", PrimitiveType.Cube, new Vector3(x, y + .10f, -.05f),
                new Vector3(width, .12f, 1.48f), new Color(.28f, .58f, .72f));
            Shape(name + " surface", PrimitiveType.Cube, new Vector3(x, y + .19f, -.05f),
                new Vector3(width, .10f, 1.43f), new Color(.70f, .86f, .86f));
        }

        private void BuildOne()
        {
            Platform("Charging boulevard", -.05f, -2.05f, 9.5f);
            AddSpring(new Vector3(-.25f, -1.75f, -.55f), new Vector3(0f, 6.5f, 0));
            AddCoin(-.55f, -.05f); AddCoin(.67f, .73f); AddCoin(1.92f, .35f);
            AddCharger(new Vector3(3.30f, -1.54f, 0));
            AddWingo(new Vector3(-3.45f, -1.36f, 0));
        }

        private void BuildTwo()
        {
            Platform("Canyon launch", -3.0f, -2.05f, 3.5f);
            Platform("Canyon finish", 2.95f, -2.05f, 3.65f);
            AddSpring(new Vector3(-1.65f, -1.72f, 0), new Vector3(2.8f, 7.2f, 0));
            AddCoin(-.55f, .28f); AddCoin(.50f, 1.12f); AddCoin(1.66f, .38f);
            AddBumper(new Vector3(.15f, -1.23f, 0), .55f);
            AddCharger(new Vector3(3.35f, -1.54f, 0));
            AddWingo(new Vector3(-3.65f, -1.36f, 0));
        }

        private void BuildThree()
        {
            Platform("Factory floor", -3f, -2.05f, 3.75f);
            Platform("Factory arrival", 3.0f, -.96f, 3.3f);
            AddFan(new Vector3(-.12f, -.65f, 0), new Vector3(2.2f, 3.6f, 0));
            AddBumper(new Vector3(1.05f, .13f, 0), .68f);
            GameObject moving = Shape("Moving bridge", PrimitiveType.Cube,
                new Vector3(.20f, -1.48f, .0f), new Vector3(1.35f, .20f, 1.25f), Teal);
            moving.AddComponent<MovingBridge>().Configure(.48f, 1.25f);
            AddCoin(-.90f, .10f); AddCoin(.62f, 1.43f); AddCoin(2.38f, 1.05f);
            AddCharger(new Vector3(3.43f, -.47f, 0));
            AddWingo(new Vector3(-3.68f, -1.36f, 0));
        }

        private void AddWingo(Vector3 pos)
        {
            GameObject root = new GameObject("Wingo — original prototype SUV");
            root.transform.position = pos;
            stage.Add(root);
            Rigidbody rb = root.AddComponent<Rigidbody>();
            rb.mass = 1.3f;
            rb.linearDamping = .22f;
            rb.angularDamping = 2.8f;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            BoxCollider collider = root.AddComponent<BoxCollider>();
            collider.center = new Vector3(0f, .02f, 0f);
            collider.size = new Vector3(1.24f, .54f, .58f);
            PhysicsMaterial rubber = new PhysicsMaterial("Rivet tire grip");
            rubber.dynamicFriction = .60f;
            rubber.staticFriction = .73f;
            rubber.bounciness = .06f;
            collider.material = rubber;

            // Rounded, original futuristic SUV. No Tesla trademarks, CAD, designs or logos.
            Part(root.transform, "Rounded pearlescent body", PrimitiveType.Capsule,
                new Vector3(0f, .06f, 0f), new Vector3(.53f, 1.1f, .65f), Cream)
                .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Part(root.transform, "Blue lower skirt", PrimitiveType.Cube,
                new Vector3(0f, -.13f, -.02f), new Vector3(1.08f, .12f, .67f), Navy);
            Part(root.transform, "Curved glass roof", PrimitiveType.Capsule,
                new Vector3(-.10f, .32f, .05f), new Vector3(.31f, .74f, .49f), Navy)
                .transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
            Part(root.transform, "Windshield shine", PrimitiveType.Cube,
                new Vector3(.13f, .39f, -.23f), new Vector3(.17f, .13f, .05f), new Color(.44f, .83f, 1f));
            // Playful but not preschool-style expressive headlights.
            Part(root.transform, "Front left lamp", PrimitiveType.Sphere,
                new Vector3(.51f, .07f, -.33f), new Vector3(.17f, .09f, .05f), Teal);
            Part(root.transform, "Rear lamp", PrimitiveType.Sphere,
                new Vector3(-.52f, .05f, -.33f), new Vector3(.08f, .12f, .045f), new Color(1f, .34f, .32f));
            for (int side = -1; side <= 1; side += 2)
            {
                for (int axle = -1; axle <= 1; axle += 2)
                {
                    float wheelX = axle * .40f;
                    float wheelZ = side * .33f;
                    Part(root.transform, "Tire", PrimitiveType.Cylinder,
                        new Vector3(wheelX, -.21f, wheelZ), new Vector3(.21f, .075f, .21f), Navy)
                        .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                    Part(root.transform, "Wheel hub", PrimitiveType.Cylinder,
                        new Vector3(wheelX, -.21f, wheelZ + side * .068f), new Vector3(.092f, .076f, .092f), Steel)
                        .transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
                }
            }
            GameObject frontPivot = new GameObject("Falcon-door front pivot");
            frontPivot.transform.SetParent(root.transform, false);
            frontPivot.transform.localPosition = new Vector3(.17f, .48f, -.13f);
            Part(frontPivot.transform, "Front lifting panel", PrimitiveType.Cube,
                new Vector3(.17f, .07f, 0), new Vector3(.47f, .10f, .43f), Cream);
            GameObject backPivot = new GameObject("Falcon-door rear pivot");
            backPivot.transform.SetParent(root.transform, false);
            backPivot.transform.localPosition = new Vector3(-.20f, .48f, .13f);
            Part(backPivot.transform, "Rear lifting panel", PrimitiveType.Cube,
                new Vector3(-.17f, .07f, 0), new Vector3(.47f, .10f, .43f), Cream);

            TrailRenderer trail = root.AddComponent<TrailRenderer>();
            trail.time = .28f;
            trail.startWidth = .16f;
            trail.endWidth = 0f;
            trail.material = Mat(new Color(.65f, .98f, 1f));
            trail.emitting = false;
            wingo = root.AddComponent<WingoPhysics>();
            wingo.Initialize(this, rb, frontPivot.transform, backPivot.transform, trail);
        }

        private void AddCoin(float x, float y)
        {
            GameObject coin = Shape("Lightning coin", PrimitiveType.Cylinder, new Vector3(x, y, -.15f),
                new Vector3(.20f, .06f, .20f), Gold);
            coin.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            coin.GetComponent<Collider>().isTrigger = true;
            coin.AddComponent<CoinTrigger>().Initialize(this, x, y);
            availableCoins++;
        }

        private void AddCharger(Vector3 pos)
        {
            Shape("Charger docking pad", PrimitiveType.Cube, pos + new Vector3(-.22f, -.17f, 0),
                new Vector3(1.18f, .16f, .91f), Navy);
            Shape("Charging blue strip", PrimitiveType.Cube, pos + new Vector3(-.22f, -.07f, -.24f),
                new Vector3(.97f, .05f, .25f), Teal, false);
            Shape("Charge pedestal", PrimitiveType.Capsule, pos + new Vector3(.39f, .47f, .05f),
                new Vector3(.38f, .87f, .32f), Cream, false);
            Shape("Bright charger face", PrimitiveType.Cube, pos + new Vector3(.38f, .47f, -.14f),
                new Vector3(.18f, .40f, .07f), Teal, false);
            GameObject ring = Shape("Charger aura", PrimitiveType.Sphere,
                pos + new Vector3(-.12f, .53f, .28f), new Vector3(1.25f, 1.32f, .09f),
                new Color(.33f, .95f, .85f), false);
            ring.GetComponent<Renderer>().enabled = false; // Collider below is independent of visuals.
            GameObject target = new GameObject("Charger docking zone");
            target.transform.position = pos + new Vector3(-.28f, .40f, 0);
            BoxCollider box = target.AddComponent<BoxCollider>();
            box.size = new Vector3(1.16f, 1.17f, 1.05f);
            box.isTrigger = true;
            target.AddComponent<ChargerTrigger>().Initialize(this);
            stage.Add(target);
        }

        private void AddSpring(Vector3 pos, Vector3 push)
        {
            GameObject spring = Shape("Red spring bumper", PrimitiveType.Cube, pos,
                new Vector3(.56f, .18f, .70f), new Color(.98f, .29f, .38f));
            spring.AddComponent<SpringPad>().Configure(push);
            Shape("Spring foundation", PrimitiveType.Cylinder, pos + new Vector3(0, -.15f, .05f),
                new Vector3(.28f, .12f, .27f), Navy, false);
        }

        private void AddBumper(Vector3 pos, float size)
        {
            GameObject bumper = Shape("Orange pinball bumper", PrimitiveType.Sphere, pos,
                new Vector3(size, size, size), new Color(1f, .48f, .22f));
            bumper.AddComponent<SpringPad>().Configure(new Vector3(2.6f, 4.7f, 0));
            PhysicsMaterial spring = new PhysicsMaterial("Bouncy bumper") { bounciness = .8f, bounceCombine = PhysicsMaterialCombine.Maximum };
            bumper.GetComponent<Collider>().material = spring;
        }

        private void AddFan(Vector3 pos, Vector3 wind)
        {
            GameObject fan = Shape("Wind turbine", PrimitiveType.Cylinder, pos,
                new Vector3(.40f, .25f, .4f), Navy, false);
            fan.transform.rotation = Quaternion.Euler(90, 0, 0);
            GameObject blades = Shape("Spinning wind blades", PrimitiveType.Cube, pos + new Vector3(0, 0, -.3f),
                new Vector3(.12f, 1.07f, .10f), Teal, false);
            blades.AddComponent<SpinBlade>();
            GameObject trigger = new GameObject("Wind column trigger");
            trigger.transform.position = pos + new Vector3(.35f, 1.13f, 0);
            BoxCollider hit = trigger.AddComponent<BoxCollider>();
            hit.isTrigger = true;
            hit.size = new Vector3(1.8f, 2.8f, 1f);
            trigger.AddComponent<LiftFanTrigger>().Configure(wind);
            stage.Add(trigger);
        }

        public void OnHop() { hops++; }
        public void CollectCoin(GameObject coin)
        {
            if (won || failed || !coin.activeSelf) return;
            coins++;
            if (wingo != null) wingo.RestoreBattery(.12f);
            coin.SetActive(false);
        }
        public void ChargeComplete()
        {
            if (won || failed || wingo == null || !wingo.HasLaunched) return;
            won = true;
                        wingo.Finish();
            int best = PlayerPrefs.GetInt("RivetBestCoins" + course, 0);
            PlayerPrefs.SetInt("RivetBestCoins" + course, Mathf.Max(best, coins));
            PlayerPrefs.SetInt("RivetUnlocked", Mathf.Max(PlayerPrefs.GetInt("RivetUnlocked", 1), course + 2));
            PlayerPrefs.Save();
            Celebrate(wingo.transform.position);
        }
        public void Fail()
        {
            if (won || failed || wingo == null) return;
            failed = true;
                        wingo.Fail();
        }

        private void Celebrate(Vector3 at)
        {
            Color[] colors = { Gold, Teal, Cream, new Color(1f, .48f, .64f) };
            for (int i = 0; i < 20; i++)
            {
                GameObject piece = Shape("Confetti", PrimitiveType.Cube,
                    at + new Vector3(UnityEngine.Random.Range(-.32f, .32f), UnityEngine.Random.Range(-.2f, .4f), -.3f),
                    new Vector3(.09f, .13f, .05f), colors[i % colors.Length], false);
                piece.AddComponent<ConfettiFly>().Set(new Vector3(UnityEngine.Random.Range(-2.9f, 2.9f),
                    UnityEngine.Random.Range(1f, 3.6f), 0f));
            }
        }

        private void Update()
        {
            if (Screen.width != lastWidth || Screen.height != lastHeight)
            {
                lastWidth = Screen.width; lastHeight = Screen.height;
                UpdateCameraSize();
            }
            if (!won && !failed && wingo != null)
            {
                if (wingo.HasLaunched) elapsed += Time.deltaTime;
                if (wingo.transform.position.y < -4.1f || wingo.transform.position.x > 6f ||
                    wingo.transform.position.x < -5.5f || (wingo.HasLaunched && elapsed > 75f)) Fail();
            }
            if (Keyboard.current != null)
            {
                if (Keyboard.current.rKey.wasPressedThisFrame) LoadCourse(course);
                if (Keyboard.current.nKey.wasPressedThisFrame) LoadCourse((course + 1) % CourseCount);
            }
        }

        private void LateUpdate()
        {
            if (cam == null || wingo == null) return;
            float aspect = (float)Mathf.Max(Screen.width, 1) / Mathf.Max(Screen.height, 1);
            float followX = aspect < .9f ? Mathf.Clamp(wingo.transform.position.x + .85f, -2.6f, 2.9f) : 0f;
            float followY = aspect < .9f ? Mathf.Clamp(wingo.transform.position.y * .22f + .25f, -.1f, 1.0f) : .45f;
            Vector3 target = new Vector3(followX, followY, -22f);
            cam.transform.position = Vector3.SmoothDamp(cam.transform.position, target, ref cameraSmooth, .18f);
        }

        private static void Fill(Rect rect, Color color)
        {
            Color old = GUI.color;
            GUI.color = color;
            GUI.DrawTexture(rect, Texture2D.whiteTexture);
            GUI.color = old;
        }

        private void Styles()
        {
            if (titleStyle != null) return;
            titleStyle = new GUIStyle(GUI.skin.label) { fontSize = 24, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            titleStyle.normal.textColor = Color.white;
            labelStyle = new GUIStyle(GUI.skin.label) { fontSize = 16, fontStyle = FontStyle.Bold };
            labelStyle.normal.textColor = Cream;
            hintStyle = new GUIStyle(GUI.skin.label) { fontSize = 15, alignment = TextAnchor.MiddleCenter, wordWrap = true };
            hintStyle.normal.textColor = Color.white;
            buttonStyle = new GUIStyle(GUI.skin.button) { fontSize = 17, fontStyle = FontStyle.Bold };
            resultStyle = new GUIStyle(GUI.skin.label) { fontSize = 31, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleCenter };
            resultStyle.normal.textColor = Color.white;
        }

        private void OnGUI()
        {
            Styles();
            hudScale = Mathf.Max(1f, Mathf.Min(Screen.width / 640f, Screen.height / 500f));
            Matrix4x4 old = GUI.matrix;
            GUI.matrix = Matrix4x4.Scale(new Vector3(hudScale, hudScale, 1));
            float w = Screen.width / hudScale, h = Screen.height / hudScale;
            Fill(new Rect(0, 0, w, 98f), new Color(.05f, .13f, .26f, .84f));
            GUI.Label(new Rect(16, 8, w - 32, 37), "FLAP N CHARGE  /  " + (course + 1) + ": " + courseNames[course], titleStyle);
            GUI.Label(new Rect(16, 51, w - 32, 28), "COINS  " + coins + "/" + availableCoins + "     FLIGHT  " +
                (wingo != null ? Mathf.RoundToInt(wingo.Battery * 100f).ToString() : "--") + "%     HOPS  " + hops, labelStyle);
            if (wingo != null)
            {
                Fill(new Rect(16, 85, Mathf.Max(10, w - 32), 5f), new Color(.20f, .28f, .42f));
                Fill(new Rect(16, 85, Mathf.Max(10, w - 32) * wingo.Battery, 5f), Teal);
            }
            if (won || failed)
            {
                float panelWidth = Mathf.Min(w - 40, 400f);
                float x = (w - panelWidth) / 2f;
                float y = Mathf.Max(125f, (h - 180f) / 2f);
                Fill(new Rect(x, y, panelWidth, 175f), new Color(.04f, .14f, .29f, .91f));
                GUI.Label(new Rect(x + 9f, y + 11f, panelWidth - 18f, 48f),
                    won ? "CHARGE COMPLETE!" : "ALMOST! TRY AGAIN", resultStyle);
                GUI.Label(new Rect(x + 15f, y + 64f, panelWidth - 30f, 28f),
                    won ? ("Collected " + coins + "/" + availableCoins + " coins  •  " + hops + " hops") : "Every launch is another chance.", hintStyle);
                if (GUI.Button(new Rect(x + 22f, y + 111f, panelWidth - 44f, 48f),
                    won ? (course == 2 ? "REPLAY COURSE 1" : "NEXT COURSE") : "RETRY", buttonStyle))
                    LoadCourse(won ? ((course + 1) % CourseCount) : course);
            }
            else if (wingo != null)
            {
                Fill(new Rect(10, h - 83f, w - 20f, 73f), new Color(.05f, .13f, .26f, .74f));
                GUI.Label(new Rect(18, h - 75f, w - 36f, 48f),
                    wingo.IsAiming ? "RELEASE TO FLING  •  TRAJECTORY PREVIEW" :
                    (!wingo.HasLaunched ? "DRAG UP + RIGHT TO AIM, RELEASE TO FLIP  •  SPACE = QUICK LAUNCH" :
                    "DRAG AGAIN TO HOP  •  HOLD SPACE OR CLICK TO GLIDE"), hintStyle);
            }
            if (!won && !failed && GUI.Button(new Rect(w - 102f, 111f, 88f, 39f), "RETRY", buttonStyle))
                LoadCourse(course);
            GUI.matrix = old;
        }

        private void OnDestroy()
        {
            foreach (Material m in materials.Values) if (m != null) Destroy(m);
            materials.Clear();
        }
    }

    public sealed class WingoPhysics : MonoBehaviour
    {
        private FirstFlightGame game;
        private Rigidbody body;
        private Transform leftDoor, rightDoor;
        private TrailRenderer trail;
        private LineRenderer aimLine;
        private bool aiming, launched, gliding, won, failed;
        private Vector2 startPointer, currentPointer;
        private float battery = 1f;
        private float lastLaunch = -5f;
        private const float LaunchCooldown = .36f;
        public float Battery => battery;
        public bool HasFailed => failed;
        public bool HasLaunched => launched;
        public bool IsAiming => aiming;

        public void Initialize(FirstFlightGame director, Rigidbody rb, Transform first, Transform second, TrailRenderer trailRenderer)
        {
            game = director; body = rb; leftDoor = first; rightDoor = second; trail = trailRenderer;
            body.isKinematic = true;
            GameObject arc = new GameObject("Visible launch trajectory");
            arc.transform.SetParent(transform, false);
            aimLine = arc.AddComponent<LineRenderer>();
            aimLine.useWorldSpace = true;
            aimLine.positionCount = 0;
            aimLine.startWidth = .105f; aimLine.endWidth = .025f;
            Shader lineShader = Shader.Find("Sprites/Default");
            if (lineShader == null) lineShader = Shader.Find("Universal Render Pipeline/Unlit");
            aimLine.material = new Material(lineShader);
            aimLine.startColor = new Color(.99f, .85f, .28f, .95f);
            aimLine.endColor = new Color(.99f, .85f, .28f, .08f);
        }

        private static Vector2 ThrowVelocity(Vector2 drag)
        {
            float magnitude = Mathf.Clamp01(drag.magnitude / Mathf.Max(105f, Screen.height * .21f));
            Vector2 direction = drag.sqrMagnitude > 25f ? drag.normalized : new Vector2(.76f, .65f);
            direction.x = Mathf.Clamp(direction.x, .15f, 1f);
            direction.y = Mathf.Clamp(direction.y, .18f, .98f);
            direction.Normalize();
            return direction * Mathf.Lerp(5.0f, 7.65f, magnitude);
        }

        private void Launch(Vector2 drag)
        {
            if (Time.time - lastLaunch < LaunchCooldown) return;
            Vector2 shot = ThrowVelocity(drag);
            if (body.isKinematic) body.isKinematic = false;
            Vector3 v = body.linearVelocity;
            body.linearVelocity = new Vector3(Mathf.Clamp(v.x * .15f + shot.x, -1f, 7f),
                Mathf.Clamp(v.y * .15f + shot.y, 0, 8f), 0f);
            body.angularVelocity = new Vector3(0, 0, -1.0f);
            lastLaunch = Time.time;
            launched = true;
            trail.emitting = true;
            game.OnHop();
        }

        private void Update()
        {
            if (failed || won) return;
            bool down = false, held = false, up = false;
            Vector2 screen = Vector2.zero;
            if (Touchscreen.current != null && (Touchscreen.current.primaryTouch.press.isPressed || Touchscreen.current.primaryTouch.press.wasReleasedThisFrame))
            {
                var touch = Touchscreen.current.primaryTouch;
                down = touch.press.wasPressedThisFrame;
                held = touch.press.isPressed;
                up = touch.press.wasReleasedThisFrame;
                screen = touch.position.ReadValue();
            }
            else if (Mouse.current != null)
            {
                down = Mouse.current.leftButton.wasPressedThisFrame;
                held = Mouse.current.leftButton.isPressed;
                up = Mouse.current.leftButton.wasReleasedThisFrame;
                screen = Mouse.current.position.ReadValue();
            }
            // Don't mistake controls at the top/bottom of the Game view for a launch gesture.
            bool inPlayfield = screen.y > Screen.height * .15f && screen.y < Screen.height * .82f;
            if (down && inPlayfield)
            {
                aiming = true;
                startPointer = screen;
                currentPointer = screen;
            }
            if (aiming && held) currentPointer = screen;
            if (up && aiming)
            {
                aiming = false;
                aimLine.positionCount = 0;
                Vector2 drag = screen - startPointer;
                if (drag.magnitude >= 26f) Launch(drag);
            }
            if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
            {
                if (!launched) Launch(new Vector2(170f, 144f));
                else if (body.linearVelocity.magnitude < 1.7f) Launch(new Vector2(150f, 130f));
            }
            gliding = launched && ((held && (!aiming || (currentPointer - startPointer).magnitude < 22f)) ||
                (Keyboard.current != null && Keyboard.current.spaceKey.isPressed));
            if (aiming) Preview(currentPointer - startPointer);
            if (leftDoor != null && rightDoor != null)
            {
                float flap = gliding && battery > 0 ? 1f : 0f;
                float bob = flap * (46f + 10f * Mathf.Sin(Time.time * 10f));
                leftDoor.localRotation = Quaternion.Lerp(leftDoor.localRotation, Quaternion.Euler(0, 0, bob), Time.deltaTime * 12f);
                rightDoor.localRotation = Quaternion.Lerp(rightDoor.localRotation, Quaternion.Euler(0, 0, -bob), Time.deltaTime * 12f);
            }
        }

        private void Preview(Vector2 drag)
        {
            if (aimLine == null) return;
            if (drag.magnitude < 12f) { aimLine.positionCount = 0; return; }
            const int n = 17;
            aimLine.positionCount = n;
            Vector2 velocity = ThrowVelocity(drag);
            Vector3 start = transform.position + Vector3.forward * (-.5f);
            for (int i = 0; i < n; i++)
            {
                float t = i * .10f;
                Vector3 point = start + new Vector3(velocity.x * t, velocity.y * t + .5f * Physics.gravity.y * t * t, 0);
                aimLine.SetPosition(i, point);
            }
        }

        private void FixedUpdate()
        {
            if (!launched || won || failed || body.isKinematic) return;
            Vector3 speed = body.linearVelocity;
            if (gliding && battery > 0f)
            {
                body.AddForce(new Vector3(.65f, 6.8f, 0), ForceMode.Acceleration);
                if (speed.y < -2.1f) body.linearVelocity = new Vector3(speed.x, -2.1f, 0);
                battery = Mathf.Max(0, battery - Time.fixedDeltaTime * .23f);
            }
            else if (Mathf.Abs(speed.y) < .15f && Mathf.Abs(speed.x) < .25f)
                battery = Mathf.Min(1f, battery + Time.fixedDeltaTime * .15f);
            // Damp uncontrolled spins without preventing the satisfying flip on launch.
            float angle = Mathf.DeltaAngle(0f, transform.eulerAngles.z);
            body.AddTorque(new Vector3(0f, 0f, -angle * .016f - body.angularVelocity.z * .8f), ForceMode.Acceleration);
            speed = body.linearVelocity;
            if (Mathf.Abs(speed.x) > 7.2f) body.linearVelocity = new Vector3(Mathf.Clamp(speed.x, -7.2f, 7.2f), speed.y, 0);
        }
        public void RestoreBattery(float bonus) { battery = Mathf.Clamp01(battery + bonus); }
        public void Finish()
        {
            won = true;
            gliding = false;
            if (trail != null) trail.emitting = false;
            if (aimLine != null) aimLine.positionCount = 0;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
        }
        public void Fail()
        {
            failed = true;
            gliding = false;
            if (trail != null) trail.emitting = false;
            if (aimLine != null) aimLine.positionCount = 0;
            body.linearVelocity = Vector3.zero;
            body.angularVelocity = Vector3.zero;
            body.isKinematic = true;
        }
        private void OnDestroy()
        {
            if (aimLine != null) Destroy(aimLine.material);
        }
    }

    public sealed class ChargerTrigger : MonoBehaviour
    {
        private FirstFlightGame game;
        public void Initialize(FirstFlightGame director) { game = director; }
        private void OnTriggerEnter(Collider other)
        {
            if (game != null && other.GetComponentInParent<WingoPhysics>() != null) game.ChargeComplete();
        }
    }

    public sealed class CoinTrigger : MonoBehaviour
    {
        private FirstFlightGame game;
        private Vector3 start;
        public void Initialize(FirstFlightGame director, float x, float y)
        { game = director; start = new Vector3(x, y, -.15f); }
        private void Update()
        {
            transform.Rotate(0, 110f * Time.deltaTime, 0, Space.World);
            transform.position = start + new Vector3(0, .075f * Mathf.Sin(Time.time * 3f + start.x), 0);
        }
        private void OnTriggerEnter(Collider other)
        {
            if (game != null && other.GetComponentInParent<WingoPhysics>() != null) game.CollectCoin(gameObject);
        }
    }

    public sealed class SpringPad : MonoBehaviour
    {
        private Vector3 impulse = new Vector3(2f, 7f, 0f);
        private float lastHit;
        public void Configure(Vector3 push) { impulse = push; }
        private void OnCollisionEnter(Collision collision)
        {
            if (Time.time - lastHit < .3f || collision.collider.GetComponentInParent<WingoPhysics>() == null) return;
            lastHit = Time.time;
            if (collision.rigidbody != null)
            {
                Vector3 v = collision.rigidbody.linearVelocity;
                collision.rigidbody.linearVelocity = new Vector3(v.x * .30f, Mathf.Max(v.y, 0f), 0);
                collision.rigidbody.AddForce(impulse, ForceMode.VelocityChange);
            }
        }
    }

    public sealed class LiftFanTrigger : MonoBehaviour
    {
        private Vector3 wind;
        public void Configure(Vector3 windForce) { wind = windForce; }
        private void OnTriggerStay(Collider other)
        {
            WingoPhysics car = other.GetComponentInParent<WingoPhysics>();
            if (car != null)
            {
                Rigidbody body = car.GetComponent<Rigidbody>();
                if (body != null && !body.isKinematic) body.AddForce(wind, ForceMode.Acceleration);
            }
        }
    }

    public sealed class SpinBlade : MonoBehaviour
    {
        private void Update() { transform.Rotate(0, 0, Time.deltaTime * 220f); }
    }

    public sealed class MovingBridge : MonoBehaviour
    {
        private Vector3 origin;
        private float speed, range;
        public void Configure(float horizontalRange, float movementSpeed)
        { origin = transform.position; range = horizontalRange; speed = movementSpeed; }
        private void Update()
        { transform.position = origin + Vector3.right * Mathf.Sin(Time.time * speed) * range; }
    }

    public sealed class CloudFloat : MonoBehaviour
    {
        private Vector3 initial;
        private float phase;
        public void Set(float offset) { phase = offset; initial = transform.position; }
        private void Update()
        { transform.position = initial + new Vector3(.08f * Mathf.Sin(Time.time * .35f + phase), 0, 0); }
    }

    public sealed class ConfettiFly : MonoBehaviour
    {
        private Vector3 velocity;
        private float created;
        public void Set(Vector3 direction) { velocity = direction; created = Time.time; }
        private void Update()
        {
            velocity += Physics.gravity * Time.deltaTime * .38f;
            transform.position += velocity * Time.deltaTime;
            transform.Rotate(70f * Time.deltaTime, 90f * Time.deltaTime, 110f * Time.deltaTime);
            if (Time.time - created > 2.4f) Destroy(gameObject);
        }
    }
}
