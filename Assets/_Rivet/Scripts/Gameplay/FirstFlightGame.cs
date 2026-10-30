// Project Rivet — Sprint 3. Original code and art. Unity 6.3, Input System, 3D physics.
// A bounded, step-based puzzle. No Tesla trademarks, analytics, advertising or online services.
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace ProjectRivet
{
    public sealed class FirstFlightGame : MonoBehaviour
    {
        public const int SolidLayer = 8; // Internal physics-only layer; no TagManager dependency.
        private const int LevelCount = 3;
        private const float LeftEdge = -6.05f;
        private const float RightEdge = 6.05f;
        private const float BottomEdge = -3.48f;

        private readonly List<GameObject> objects = new List<GameObject>();
        private readonly List<Material> runtimeMaterials = new List<Material>();
        private readonly Dictionary<string, Sprite> spriteCache = new Dictionary<string, Sprite>();
        private readonly List<StepTrigger> steps = new List<StepTrigger>();
        private WingoController wingo;
        private GameObject gate;
        private Camera sceneCamera;
        private LineRenderer aimLine;
        private GUIStyle headingStyle, labelStyle, hintStyle, buttonStyle, centeredStyle, stepStyle;
        private Texture2D whitePixel;
        private int levelIndex;
        private int activatedSteps;
        private int coins;
        private int availableCoins;
        private bool won;
        private bool failed;
        private float noticeUntil;
        private string notice = "";
        private static readonly Color Ice = new Color(.72f, .96f, 1f);
        private static readonly Color Mint = new Color(.30f, 1f, .77f);
        private static readonly Color Orange = new Color(1f, .69f, .31f);

        public bool IsActive => !failed && !won;
        public bool IsWon => won;
        public float FloorY => BottomEdge;

        private void Start()
        {
            Application.targetFrameRate = 60;
            Time.fixedDeltaTime = 1f / 60f;
            sceneCamera = Camera.main;
            if (sceneCamera == null)
            {
                GameObject cam = new GameObject("Main Camera");
                cam.tag = "MainCamera";
                sceneCamera = cam.AddComponent<Camera>();
            }
            sceneCamera.clearFlags = CameraClearFlags.SolidColor;
            sceneCamera.backgroundColor = new Color(.09f,.14f,.3f);
            sceneCamera.orthographic = true;
            sceneCamera.transform.position = new Vector3(0,0,-20);
            sceneCamera.transform.rotation = Quaternion.identity;
            ConfigureCamera();
            LoadLevel(0);
        }

        private void ConfigureCamera()
        {
            if (sceneCamera == null) return;
            float aspect = Mathf.Max(.1f, (float)Screen.width / Mathf.Max(1, Screen.height));
            sceneCamera.orthographicSize = Mathf.Max(3.95f, 6.6f / aspect);
        }

        private Sprite Artwork(string name)
        {
            if(spriteCache.TryGetValue(name,out Sprite existing)) return existing;
            Sprite sprite=Resources.Load<Sprite>("RivetArt/"+name);
            if(sprite==null)
                Debug.LogError("Project Rivet sprite missing. Reimport RivetArt as Sprite via the Sprint 3 installer: " +name);
            spriteCache[name]=sprite;
            return sprite;
        }

        private GameObject Visual(string label, string art, Vector3 position, float width, float height,
            int order = 0, Transform parent = null)
        {
            GameObject go = new GameObject(label);
            if (parent != null) go.transform.SetParent(parent, false);
            go.transform.position = parent == null ? position : parent.TransformPoint(position);
            SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
            Sprite sprite = Artwork(art);
            renderer.sprite = sprite;
            renderer.sortingOrder = order;
            if (sprite != null)
            {
                Vector2 size = sprite.bounds.size;
                go.transform.localScale = new Vector3(width / Mathf.Max(.001f, size.x),
                    height / Mathf.Max(.001f, size.y), 1f);
            }
            if (parent == null) objects.Add(go);
            return go;
        }

        private static GameObject InvisibleCollider(string name, Vector3 position, Vector3 size,
            Transform parent = null)
        {
            GameObject result = new GameObject(name);
            if (parent != null) result.transform.SetParent(parent, false);
            result.transform.position = parent == null ? position : parent.TransformPoint(position);
            BoxCollider box = result.AddComponent<BoxCollider>();
            box.size = size;
            return result;
        }

        private GameObject Platform(string name, float x, float y, float width, bool checkpoint, int index)
        {
            Visual(name + " artwork", "platform", new Vector3(x,y,0), width,.40f,1);
            GameObject solid = InvisibleCollider(name, new Vector3(x,y,0),
                new Vector3(width-.08f,.30f,.8f));
            solid.layer = SolidLayer;
            objects.Add(solid);
            if(checkpoint) steps.Add(AddStep(x,y+.43f,index));
            return solid;
        }

        private StepTrigger AddStep(float x,float y,int index)
        {
            GameObject step = InvisibleCollider("Checkpoint " + (index+1),
                new Vector3(x,y,0),new Vector3(.96f,.55f,1f));
            objects.Add(step);
            step.GetComponent<BoxCollider>().isTrigger=true;
            GameObject visual = Visual("Checkpoint light","relay_switch",Vector3.zero,.48f,.32f,13,step.transform);
            StepTrigger component=step.AddComponent<StepTrigger>();
            component.Setup(this,index,visual.GetComponent<SpriteRenderer>());
            return component;
        }

        private void AddCoin(float x,float y)
        {
            GameObject pickup = InvisibleCollider("Energy coin",new Vector3(x,y,0),new Vector3(.44f,.44f,.8f));
            objects.Add(pickup);
            pickup.GetComponent<BoxCollider>().isTrigger=true;
            Visual("Spinning coin", "energy_coin",Vector3.zero,.36f,.36f,17,pickup.transform);
            pickup.AddComponent<EnergyPickup>().Setup(this);
            availableCoins++;
        }

        private void AddSpring(float x,float y)
        {
            GameObject solid=InvisibleCollider("Jump spring",new Vector3(x,y,0),
                new Vector3(.79f,.18f,.85f));
            objects.Add(solid);
            solid.layer=SolidLayer;
            Visual("Spring artwork","spring",Vector3.zero,.84f,.36f,5,solid.transform);
            solid.AddComponent<SpringObstacle>();
        }

        private void AddBumper(float x,float y)
        {
            GameObject solid=new GameObject("Bounce bumper");
            solid.transform.position=new Vector3(x,y,0);
            objects.Add(solid);
            solid.layer=SolidLayer;
            SphereCollider sphere=solid.AddComponent<SphereCollider>();
            sphere.radius=.30f;
            Visual("Bumper artwork","bumper",Vector3.zero,.6f,.6f,6,solid.transform);
            solid.AddComponent<BounceObstacle>();
        }

        private void AddGoal(float x,float platformTop)
        {
            Visual("Charging bay stripe","platform",new Vector3(x,platformTop-.035f,0),1.1f,.19f,3);
            Visual("Charging cabinet","charger",new Vector3(x+.34f,platformTop+.55f,.02f),
                .67f,1.13f,8);
            GameObject zone=InvisibleCollider("Charge success trigger",
                new Vector3(x-.08f,platformTop+.48f,0),new Vector3(1.25f,.88f,1f));
            objects.Add(zone);
            zone.GetComponent<BoxCollider>().isTrigger=true;
            zone.AddComponent<ChargerZone>().Setup(this);
        }

        private void AddGate(float x,float y)
        {
            gate=InvisibleCollider("Locked energy gate",new Vector3(x,y,0),
                new Vector3(.17f,1.4f,.75f));
            gate.layer=SolidLayer;
            objects.Add(gate);
            Visual("Gate artwork","energy_gate",Vector3.zero,.19f,1.52f,12,gate.transform);
        }

        private void AddWingo(Vector3 at)
        {
            GameObject car = new GameObject("Wingo — original cartoon robot EV");
            car.transform.position = at;
            objects.Add(car);
            car.layer = 0;
            Rigidbody rb = car.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.useGravity = true;
            rb.linearDamping = .14f;
            rb.angularDamping = 3.3f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.constraints = RigidbodyConstraints.FreezePositionZ |
                             RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY;
            BoxCollider box = car.AddComponent<BoxCollider>();
            box.size = new Vector3(1.12f,.48f,.62f);
            box.center = new Vector3(0,-.055f,0);
            PhysicsMaterial friction = new PhysicsMaterial("Wingo grip");
            friction.staticFriction = .8f;
            friction.dynamicFriction = .72f;
            friction.bounciness = .04f;
            box.material = friction;
            // Fully original vector automobile illustration, not a primitive cube.
            Visual("Gloss body", "wingo_car", new Vector3(0,.035f,0),1.42f,.65f, 20,car.transform);
            Transform left = new GameObject("Falcon wing left hinge").transform;
            left.SetParent(car.transform,false);
            left.localPosition = new Vector3(-.27f,.30f,0);
            Visual("Left gullwing", "wing_panel", new Vector3(.18f,.045f,0),.52f,.23f,22,left);
            Transform right = new GameObject("Falcon wing right hinge").transform;
            right.SetParent(car.transform,false);
            right.localPosition = new Vector3(.15f,.34f,0);
            Visual("Right gullwing", "wing_panel", new Vector3(.16f,.045f,0),.49f,.23f,21,right);
            wingo = car.AddComponent<WingoController>();
            wingo.Setup(this, rb, left, right, sceneCamera);
        }

        private void AddBoundaries()
        {
            for (int i=0;i<3;i++)
            {
                Vector3 pos; Vector3 size;
                if (i==0) {pos = new Vector3(LeftEdge-.15f,0,0);size = new Vector3(.3f,8f,1f);}
                else if (i==1) {pos = new Vector3(RightEdge+.15f,0,0);size = new Vector3(.3f,8f,1f);}
                else {pos = new Vector3(0,BottomEdge-.45f,0);size = new Vector3(13f,.1f,1f);}
                GameObject bound = InvisibleCollider("Arena safety wall",pos,size);
                bound.layer = SolidLayer;
                objects.Add(bound);
            }
        }

        private void LoadLevel(int next)
        {
            for (int i=objects.Count-1;i>=0;i--) if(objects[i]!=null) Destroy(objects[i]);
            objects.Clear();
            steps.Clear();
            gate = null;
            wingo = null;
            aimLine = null;
            coins=0;
            availableCoins=0;
            activatedSteps=0;
            failed=false;
            won=false;
            levelIndex = Mathf.Clamp(next,0,LevelCount-1);
            ConfigureCamera();
            GameObject backdrop = Visual("Volt City original background", "garage_backdrop",
                new Vector3(0,0,2f),14.7f,8.25f,-100);
            AddBoundaries();

            if (levelIndex == 0)
            {
                // Five physical ledges. No one-shot flying across the whole screen.
                Platform("01 Departure bay",-4.8f,-2.24f,2.1f,false,-1);
                Platform("02 First ledge",-2.4f,-1.52f,1.50f,true,0);
                Platform("03 Middle ledge",-.20f,-.80f,1.52f,true,1);
                Platform("04 Relay ledge",2.04f,-.12f,1.48f,true,2);
                Platform("05 Charger dock",4.48f,.36f,2.18f,false,-1);
                AddCoin(-3.38f,-1.15f);
                AddCoin(-1.30f,-.52f);
                AddCoin(1.08f,.35f);
                AddGoal(4.48f,.51f);
                AddGate(3.63f,1.10f);
                AddWingo(new Vector3(-4.92f,-1.78f,0));
                Notice("Reach pads 1 > 2 > 3, then charge!",4f);
            }
            else if (levelIndex == 1)
            {
                Platform("01 Start",-4.8f,-2.30f,2.12f,false,-1);
                Platform("02 Spring landing",-2.37f,-1.55f,1.42f,true,0);
                Platform("03 Bumper bay",-.20f,-.93f,1.58f,true,1);
                Platform("04 Upper ledge",2.00f,-.20f,1.58f,true,2);
                Platform("05 Charger dock",4.49f,.32f,2.10f,false,-1);
                AddSpring(-4.1f,-1.99f);
                AddBumper(.90f,-.17f);
                AddCoin(-3.10f,-.92f);AddCoin(-1.3f,-.30f);AddCoin(2.95f,.77f);
                AddGoal(4.49f,.47f);
                AddGate(3.65f,1.08f);
                AddWingo(new Vector3(-5.15f,-1.84f,0));
                Notice("Use the spring, hit all three checkpoints",4f);
            }
            else
            {
                Platform("01 Start",-4.80f,-2.25f,2.1f,false,-1);
                Platform("02 High landing",-2.45f,-1.28f,1.52f,true,0);
                Platform("03 Switchback",-.12f,-1.77f,1.55f,true,1);
                Platform("04 Final climb",2.10f,-.53f,1.5f,true,2);
                Platform("05 Charger dock",4.51f,.31f,2.15f,false,-1);
                AddSpring(-.10f,-1.45f);
                AddBumper(.88f,-.25f);
                AddCoin(-3.40f,-.76f);AddCoin(.25f,-.60f);AddCoin(2.90f,.47f);
                AddGoal(4.51f,.46f);
                AddGate(3.68f,1.05f);
                AddWingo(new Vector3(-4.95f,-1.79f,0));
                Notice("The route doubles back. Plan each hop.",4f);
            }
            CreateAimLine();
        }

        private void CreateAimLine()
        {
            GameObject holder = new GameObject("Launch trajectory guide");
            objects.Add(holder);
            aimLine = holder.AddComponent<LineRenderer>();
            aimLine.positionCount = 26;
            aimLine.useWorldSpace = true;
            aimLine.widthMultiplier = .05f;
            aimLine.numCapVertices = 4;
            Shader shader = Shader.Find("Sprites/Default");
            if (shader != null)
            {
                Material mat = new Material(shader);
                runtimeMaterials.Add(mat);
                aimLine.material = mat;
            }
            aimLine.startColor = new Color(.53f,1f,.91f,.95f);
            aimLine.endColor = new Color(.53f,1f,.91f,.05f);
            aimLine.sortingOrder = 45;
        }

        public void Aim(Vector3 position,Vector2 velocity,bool visible)
        {
            if(aimLine==null)return;
            aimLine.enabled=visible && IsActive;
            if(!aimLine.enabled)return;
            for(int i=0;i<aimLine.positionCount;i++)
            {
                float t=i*.052f;
                Vector3 point=position+new Vector3(velocity.x*t,velocity.y*t-.5f*9.81f*t*t,-.08f);
                aimLine.SetPosition(i,point);
            }
        }

        public void ActivateStep(int index)
        {
            if (!IsActive || index<0 || index>=3)return;
            if (index<activatedSteps)return;
            if (index>activatedSteps)
            {
                Notice("Checkpoint " + (activatedSteps+1) + " comes first",1.5f);
                return;
            }
            activatedSteps++;
            if(index<steps.Count)steps[index].SetActivated();
            Notice("PAD " + activatedSteps + "/3 ACTIVATED",1.5f);
            if(activatedSteps==3)
            {
                if(gate!=null) gate.SetActive(false);
                Notice("CHARGER UNLOCKED — LAND AT THE DOCK!",3f);
            }
        }

        public void Collect(GameObject pickup)
        {
            if (!IsActive) return;
            coins++;
            pickup.SetActive(false);
            Notice("+1 ENERGY COIN",.8f);
        }

        public void AttemptFinish()
        {
            if(!IsActive || wingo==null)return;
            if(activatedSteps<3)
            {
                Notice("COMPLETE ALL THREE PADS FIRST",1.7f);
                return;
            }
            won=true;
            wingo.Freeze();
            PlayerPrefs.SetInt("RivetUnlockedLevel",Mathf.Max(PlayerPrefs.GetInt("RivetUnlockedLevel",0),
                Mathf.Min(levelIndex+1,LevelCount-1)));
            PlayerPrefs.SetInt("RivetBestCoins"+levelIndex,
                Mathf.Max(PlayerPrefs.GetInt("RivetBestCoins"+levelIndex,0),coins));
            PlayerPrefs.Save();
            Notice("CHARGE COMPLETE!",8f);
        }

        public void Fail()
        {
            if(!IsActive)return;
            failed=true;
            if(wingo!=null)wingo.Freeze();
            Notice("OOPS! TRY THE NEXT HOP DIFFERENTLY",10f);
        }

        public void Notice(string s,float duration)
        {
            notice=s;
            noticeUntil=Time.unscaledTime+duration;
        }

        private void Update()
        {
            if (Keyboard.current != null)
            {
                if (Keyboard.current.rKey.wasPressedThisFrame)LoadLevel(levelIndex);
                if (Keyboard.current.nKey.wasPressedThisFrame)LoadLevel((levelIndex+1)%LevelCount);
            }
            if(wingo!=null && IsActive && (wingo.transform.position.y<BottomEdge ||
                wingo.transform.position.x<LeftEdge-.15f || wingo.transform.position.x>RightEdge+.15f))
                Fail();
            // Keep fixed game board framed even after maximizing Game tab.
            if (sceneCamera!=null && Time.frameCount%30==0)ConfigureCamera();
        }

        private void RectFill(Rect r,Color color)
        {
            if(whitePixel==null)
            {
                whitePixel=new Texture2D(1,1,TextureFormat.RGBA32,false);
                whitePixel.SetPixel(0,0,Color.white);
                whitePixel.Apply();
            }
            Color prior=GUI.color;
            GUI.color=color;
            GUI.DrawTexture(r,whitePixel);
            GUI.color=prior;
        }

        private void InitStyles()
        {
            if(headingStyle!=null)return;
            headingStyle=new GUIStyle(GUI.skin.label)
            {fontSize=24,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleLeft};
            headingStyle.normal.textColor=Color.white;
            labelStyle=new GUIStyle(GUI.skin.label)
            {fontSize=15,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleLeft};
            labelStyle.normal.textColor=Ice;
            centeredStyle=new GUIStyle(headingStyle){alignment=TextAnchor.MiddleCenter,fontSize=30};
            hintStyle=new GUIStyle(labelStyle){alignment=TextAnchor.MiddleCenter,fontSize=15};
            buttonStyle=new GUIStyle(GUI.skin.button){fontSize=19,fontStyle=FontStyle.Bold};
            stepStyle=new GUIStyle(headingStyle){alignment=TextAnchor.MiddleCenter,fontSize=17};
        }

        private void OnGUI()
        {
            InitStyles();
            float scale=Mathf.Max(.72f,Mathf.Min(1.2f,Screen.height/800f));
            GUI.matrix=Matrix4x4.Scale(new Vector3(scale,scale,1));
            float width=Screen.width/scale;
            float height=Screen.height/scale;
            RectFill(new Rect(12,12,width-24,85),new Color(.032f,.083f,.16f,.90f));
            GUI.Label(new Rect(28,17,width-60,34),"FLAP N CHARGE  /  " + (levelIndex+1) + ": " +
                (levelIndex==0?"RELAY RUN":levelIndex==1?"SPRING CIRCUIT":"SKYLINE SWITCHBACK"),headingStyle);
            string state=activatedSteps>=3?"CHARGER OPEN":"PADS "+activatedSteps+" / 3";
            GUI.Label(new Rect(28,54,width-52,32),state+"     •     COINS "+coins+"/"+availableCoins+
                "     •     WING ENERGY "+(wingo==null?"--":Mathf.RoundToInt(wingo.Energy*100f)+"%"),labelStyle);
            RectFill(new Rect(27,89,(width-54)* (wingo==null?1f:wingo.Energy),4),Mint);

            // World-space checkpoint indicators projected into the camera.
            if(sceneCamera!=null)
            {
                for(int i=0;i<steps.Count;i++)
                {
                    if(steps[i]==null)continue;
                    Vector3 pixel=sceneCamera.WorldToScreenPoint(steps[i].transform.position+Vector3.up*.34f);
                    float x=pixel.x/scale;
                    float y=(Screen.height-pixel.y)/scale;
                    RectFill(new Rect(x-19,y-20,38,30),new Color(.05f,.12f,.2f,.85f));
                    GUI.Label(new Rect(x-18,y-20,36,30),(i+1).ToString(),stepStyle);
                }
            }
            if (Time.unscaledTime<noticeUntil)
            {
                RectFill(new Rect(width*.21f,105,width*.58f,44),new Color(.03f,.10f,.19f,.87f));
                GUI.Label(new Rect(width*.21f+8,108,width*.58f-16,38),notice,hintStyle);
            }
            if(won||failed)
            {
                RectFill(new Rect(width*.20f,height*.29f,width*.60f,200),new Color(.025f,.08f,.16f,.95f));
                GUI.Label(new Rect(width*.20f,height*.30f,width*.60f,53),
                    won?"CHARGE COMPLETE!":"ONE MORE TRY!",centeredStyle);
                GUI.Label(new Rect(width*.21f,height*.38f,width*.58f,37),
                    won?"Your robot made it to the charger.":"Plan another route across the pads.",hintStyle);
                if(GUI.Button(new Rect(width*.5f-128,height*.44f,256,54),
                    won?"NEXT LEVEL":"RETRY LEVEL",buttonStyle))
                    LoadLevel(won?(levelIndex+1)%LevelCount:levelIndex);
            }
            else
            {
                RectFill(new Rect(12,height-50,width-24,38),new Color(.032f,.083f,.16f,.83f));
                GUI.Label(new Rect(18,height-47,width-36,30),
                    "← → MOVE / STEER     ↑ ↓ AIM     SPACE HOP / HOLD TO GLIDE     R RETRY     N NEXT     •     MOUSE: DRAG TO AIM",hintStyle);
                if(GUI.Button(new Rect(width-120,108,103,38),"RETRY",buttonStyle)) LoadLevel(levelIndex);
            }
            GUI.matrix=Matrix4x4.identity;
        }

        private void OnDestroy()
        {
            foreach(Material mat in runtimeMaterials)if(mat!=null)Destroy(mat);
            if(whitePixel!=null)Destroy(whitePixel);
        }
    }

    public sealed class WingoController : MonoBehaviour
    {
        private FirstFlightGame director;
        private Rigidbody body;
        private Transform wingLeft,wingRight;
        private Camera view;
        private bool pointerAiming;
        private Vector2 pointerStart;
        private float aimDegrees=58f;
        private int facing=1;
        private float lastLaunch=-2f;
        private bool frozen;
        private float energy=1f;
        private const float LaunchSpeed=5.88f;
        private const float LaunchDelay=.35f;
        private const float GroundSpeed=2.0f;
        public float Energy=>energy;

        public void Setup(FirstFlightGame game,Rigidbody rigid,Transform left,Transform right,Camera camera)
        {
            director=game;body=rigid;wingLeft=left;wingRight=right;view=camera;
        }

        private bool IsGrounded()
        {
            // Two rays hit only generated platform/obstacle layer; cannot hit the car itself.
            for(int i=0;i<3;i++)
            {
                float x=(i-1)*.37f;
                Vector3 origin=transform.position+new Vector3(x,-.17f,0);
                if(Physics.Raycast(origin,Vector3.down,out RaycastHit hit,.30f,1 << FirstFlightGame.SolidLayer)
                    && hit.normal.y>.48f) return true;
            }
            return false;
        }

        private void Update()
        {
            if(frozen||director==null||!director.IsActive)return;
            Keyboard kb=Keyboard.current;
            float aimInput=0f;
            if(kb!=null)
            {
                if(kb.upArrowKey.isPressed)aimInput++;
                if(kb.downArrowKey.isPressed)aimInput--;
            }
            aimDegrees=Mathf.Clamp(aimDegrees+aimInput*57f*Time.deltaTime,38f,80f);
            bool grounded=IsGrounded();
            bool canHop=grounded && Time.time-lastLaunch>LaunchDelay;
            // Mouse/trackpad aims a deliberate impulse. Touch follows the same behavior.
            bool down=false,up=false,held=false;
            Vector2 pointer=Vector2.zero;
            if(Touchscreen.current!=null && Touchscreen.current.primaryTouch.press.isPressed)
            {
                var touch=Touchscreen.current.primaryTouch;
                pointer=touch.position.ReadValue();
                down=touch.press.wasPressedThisFrame;
                held=true;
            }
            else if(Touchscreen.current!=null && Touchscreen.current.primaryTouch.press.wasReleasedThisFrame)
            {
                up=true;pointer=Touchscreen.current.primaryTouch.position.ReadValue();
            }
            else if(Mouse.current!=null)
            {
                down=Mouse.current.leftButton.wasPressedThisFrame;
                up=Mouse.current.leftButton.wasReleasedThisFrame;
                held=Mouse.current.leftButton.isPressed;
                pointer=Mouse.current.position.ReadValue();
            }
            if(down&&canHop&&pointer.y<Screen.height*.84f)
            {
                pointerStart=pointer;
                pointerAiming=true;
            }
            if(pointerAiming&&held)
            {
                Vector2 v=pointer-pointerStart;
                if(v.sqrMagnitude>200f)
                {
                    aimDegrees=Mathf.Clamp(Mathf.Atan2(Mathf.Abs(v.y),Mathf.Max(1f,Mathf.Abs(v.x)))
                        *Mathf.Rad2Deg,38f,80f);
                    if(Mathf.Abs(v.x)>25f)facing=v.x>0?1:-1;
                }
            }
            if(up&&pointerAiming)
            {
                Vector2 delta=pointer-pointerStart;
                pointerAiming=false;
                if(delta.magnitude>24f && canHop)
                    Hop();
            }
            if(kb!=null && kb.spaceKey.wasPressedThisFrame && canHop)Hop();
            bool flap=(kb!=null&&kb.spaceKey.isPressed)||(held&&!pointerAiming);
            bool airborne=!IsGrounded();
            if(airborne&&flap&&Time.time-lastLaunch>.12f&&energy>0)
            {
                // Lift is weaker than gravity: cannot replace stepping platforms with free flight.
                float beat=Mathf.Sin(Time.time*18f)*9f;
                wingLeft.localRotation=Quaternion.Slerp(wingLeft.localRotation,
                    Quaternion.Euler(0,0,65f+beat),Time.deltaTime*18f);
                wingRight.localRotation=Quaternion.Slerp(wingRight.localRotation,
                    Quaternion.Euler(0,0,-65f-beat),Time.deltaTime*18f);
            }
            else
            {
                wingLeft.localRotation=Quaternion.Slerp(wingLeft.localRotation,
                    Quaternion.Euler(0,0,12f),Time.deltaTime*8f);
                wingRight.localRotation=Quaternion.Slerp(wingRight.localRotation,
                    Quaternion.Euler(0,0,-12f),Time.deltaTime*8f);
            }
            Vector2 vAim=LaunchVector;
            director.Aim(transform.position+new Vector3(0,.2f,0),vAim,grounded && canHop);
        }

        private Vector2 LaunchVector
        {
            get
            {
                float radians=aimDegrees*Mathf.Deg2Rad;
                return new Vector2(Mathf.Cos(radians)*facing,Mathf.Sin(radians))*LaunchSpeed;
            }
        }

        private void Hop()
        {
            if(!IsGrounded()||Time.time-lastLaunch<LaunchDelay)return;
            lastLaunch=Time.time;
            // Set a stable launch impulse irrespective of accumulated horizontal velocity.
            Vector2 shot=LaunchVector;
            body.linearVelocity=new Vector3(shot.x,shot.y,0);
            body.angularVelocity=new Vector3(0,0,-facing*.9f);
            energy=Mathf.Min(1f,energy+.24f);
        }

        private void FixedUpdate()
        {
            if(frozen||director==null||!director.IsActive)return;
            Keyboard kb=Keyboard.current;
            int direction=0;
            if(kb!=null)
            {
                if(kb.leftArrowKey.isPressed)direction--;
                if(kb.rightArrowKey.isPressed)direction++;
            }
            bool grounded=IsGrounded();
            Vector3 velocity=body.linearVelocity;
            if(grounded && Time.time-lastLaunch>.20f)
            {
                if(direction!=0)facing=direction;
                float target=direction*GroundSpeed;
                velocity.x=Mathf.MoveTowards(velocity.x,target,10.0f*Time.fixedDeltaTime);
                body.angularVelocity=new Vector3(0,0,-Mathf.DeltaAngle(0,transform.eulerAngles.z)*.07f);
                energy=Mathf.Min(1f,energy+Time.fixedDeltaTime*.30f);
            }
            else if(!grounded)
            {
                velocity.x=Mathf.Clamp(velocity.x+direction*1.8f*Time.fixedDeltaTime,-4.8f,4.8f);
                bool flap=(kb!=null&&kb.spaceKey.isPressed)||
                    (Mouse.current!=null&&Mouse.current.leftButton.isPressed&&!pointerAiming)||
                    (Touchscreen.current!=null&&Touchscreen.current.primaryTouch.press.isPressed&&!pointerAiming);
                if(flap&&energy>0&&Time.time-lastLaunch>.12f)
                {
                    velocity.y=Mathf.Min(velocity.y+3.3f*Time.fixedDeltaTime,4.9f);
                    energy=Mathf.Max(0,energy-Time.fixedDeltaTime*.23f);
                }
                body.angularVelocity=new Vector3(0,0,Mathf.Clamp(body.angularVelocity.z,-2.4f,2.4f));
            }
            body.linearVelocity=velocity;
        }

        public void Freeze()
        {
            frozen=true;
            if(body==null)return;
            body.linearVelocity=Vector3.zero;
            body.angularVelocity=Vector3.zero;
            body.isKinematic=true;
        }
    }

    public sealed class StepTrigger : MonoBehaviour
    {
        private FirstFlightGame game;
        private SpriteRenderer art;
        private int number;
        private bool active;
        public void Setup(FirstFlightGame owner,int index,SpriteRenderer visual)
        {game=owner;number=index;art=visual;}
        private void OnTriggerEnter(Collider other)
        {
            if(active||other.GetComponentInParent<WingoController>()==null)return;
            if(game!=null)game.ActivateStep(number);
        }
        private void OnTriggerStay(Collider other)
        {
            if(active||other.GetComponentInParent<WingoController>()==null)return;
            if(game!=null)game.ActivateStep(number);
        }
        public void SetActivated()
        {active=true;if(art!=null)art.color=new Color(.38f,1f,.74f);}
    }

    public sealed class EnergyPickup : MonoBehaviour
    {
        private FirstFlightGame game;
        private float startY;
        public void Setup(FirstFlightGame owner){game=owner;startY=transform.position.y;}
        private void Update()
        {transform.position=new Vector3(transform.position.x,startY+Mathf.Sin(Time.time*4f)*.09f,transform.position.z);}
        private void OnTriggerEnter(Collider other)
        {if(other.GetComponentInParent<WingoController>()!=null&&game!=null)game.Collect(gameObject);}
    }

    public sealed class ChargerZone : MonoBehaviour
    {
        private FirstFlightGame game;
        public void Setup(FirstFlightGame owner){game=owner;}
        private void OnTriggerEnter(Collider other)
        {if(other.GetComponentInParent<WingoController>()!=null&&game!=null)game.AttemptFinish();}
        private void OnTriggerStay(Collider other)
        {if(other.GetComponentInParent<WingoController>()!=null&&game!=null)game.AttemptFinish();}
    }

    public sealed class SpringObstacle : MonoBehaviour
    {
        private float lastBounce=-2f;
        private void OnCollisionEnter(Collision collision)
        {
            Rigidbody rb=collision.rigidbody;
            if(rb==null||rb.GetComponent<WingoController>()==null||Time.time-lastBounce<.3f)return;
            lastBounce=Time.time;
            Vector3 v=rb.linearVelocity;
            rb.linearVelocity=new Vector3(Mathf.Max(3.0f,v.x),Mathf.Max(6.0f,v.y),0);
        }
    }

    public sealed class BounceObstacle : MonoBehaviour
    {
        private float lastBounce=-2f;
        private void OnCollisionEnter(Collision collision)
        {
            Rigidbody rb=collision.rigidbody;
            if(rb==null||rb.GetComponent<WingoController>()==null||Time.time-lastBounce<.25f)return;
            lastBounce=Time.time;
            Vector3 outward=(collision.transform.position-transform.position).normalized;
            rb.AddForce((outward+Vector3.up*.35f)*2.4f,ForceMode.Impulse);
        }
    }
}
