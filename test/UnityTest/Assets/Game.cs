using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Box2DSharp.Common;
using Box2DSharp.Testbed.Unity.Inspection;
using Testbed.Abstractions;
using Testbed.TestCases;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Camera = UnityEngine.Camera;
using Color = UnityEngine.Color;

namespace Box2DSharp.Testbed.Unity
{
    public class Game : MonoBehaviour
    {
        public TestBase Test { get; private set; }

        public FpsCounter FpsCounter = new FpsCounter();

        public UnityDrawer UnityDrawer;

        public DebugDrawer DebugDrawer;

        public UnityTestSettings Settings;

        public Camera MainCamera;

        public Vector3 Difference;

        public Vector3 Origin;

        public bool Drag;

        public UnityInput Input;

        public Dropdown TestMenu;

        public CustomFixedUpdate CustomFixedUpdate;

        private void Awake()
        {
            Settings = TestSettingHelper.Load();
            Global.Settings = Settings;
            Global.Camera.Width = Settings.WindowWidth;
            Global.Camera.Height = Settings.WindowHeight;
            Screen.SetResolution(Settings.WindowWidth, Settings.WindowHeight, Settings.FullScreenMode);

            var testBaseType = typeof(TestBase);
            var allTypes = this.GetType()
                               .Assembly.GetTypes();
            var testTypeArray = allTypes.Where(
                                             e => testBaseType.IsAssignableFrom(e)
                                               && !e.IsAbstract
                                               && e.GetCustomAttribute<TestCaseAttribute>() != null)
                                        .ToArray();
            var testTypes = new HashSet<Type>(testTypeArray);
            var inheritedTest = allTypes.Where(
                                             e => testBaseType.IsAssignableFrom(e)
                                               && e.GetCustomAttribute<TestInheritAttribute>() != null
                                               && e.GetCustomAttribute<TestCaseAttribute>() != null)
                                        .ToList();
            foreach (var type in inheritedTest)
            {
                testTypes.Remove(type.BaseType);
            }

            inheritedTest.ForEach(t => testTypes.Add(t));
            Global.SetupTestCases(testTypes.ToList());

            TestMenu.options.AddRange(Global.Tests.Select(e => new Dropdown.OptionData($"{e.Category}:{e.Name}")));
            TestMenu.onValueChanged.AddListener(SetTest);

            _screenWidth = Screen.width;
            _screenHeight = Screen.height;

            Input = new UnityInput();
            Global.Input = Input;

            UnityDrawer = UnityDrawer.GetDrawer();
            DebugDrawer = new DebugDrawer {Drawer = UnityDrawer, ShowUI = true};
            Global.DebugDrawer = DebugDrawer;

            Application.quitting += () => TestSettingHelper.Save(Settings);

            CustomFixedUpdate = new CustomFixedUpdate(
                (FP.One / 60).AsFloat,
                time =>
                {
                    CheckTestChange();
                    Tick();
                });

            MainCamera = Camera.main;

            _textStyle = new GUIStyle
            {
                fontSize = 16,
                alignment = TextAnchor.UpperLeft,

                //border = new RectOffset(5, 5, 5, 5),
                normal = {textColor = _textColor,},
            };
        }

        public void BackToInit()
        {
            SceneManager.LoadScene("Init");
        }

        private void Start()
        {
            Debug.Log($"Has keyboard: {Keyboard.current != null}");
            Debug.Log($"Has mouse: {Mouse.current != null}");
            Debug.Log($"Has touch: {Touchscreen.current != null}");

            //CurrentTestIndex = Mathf.Clamp(CurrentTestIndex, 0, Global.Tests.Count - 1);
            //if (CurrentTestIndex > Global.Tests.Count || CurrentTestIndex < 0)
            {
                CurrentTestIndex = Global.Tests.FindIndex(e => e.TestType == typeof(HelloWorld));
            }

            TestMenu.value = CurrentTestIndex;
            _testSelected = CurrentTestIndex;
            RestartTest();

            //FixedUpdater.Start();
        }

        private void Tick()
        {
            Test.Step();
            FpsCounter.Count();
        }

        public bool OverUI;

        private void Update()
        {
            OverUI = EventSystem.current.IsPointerOverGameObject();
            CheckScreenResize();
            CheckKeyboard();

            CheckZoom();
            CheckMouse();

            CheckTouchMoveScreen();
            CheckTouchZoom();

            CustomFixedUpdate.Update();
        }

        private GUIStyle _textStyle;

        private Color _textColor = new Color(0.9f, 0.6f, 0.6f, 1f);

        private void OnGUI()
        {
            GUI.backgroundColor = default;
            GUI.color = _textColor;
            var (category, testName, _) = Global.Tests[Global.Settings.TestIndex];
            Test.DrawTitle($"{category} : {testName}");
            DebugDrawer.DrawString(5, Global.Camera.Height - 70, $"steps: {Test.StepCount}");
            DebugDrawer.DrawString(5, Global.Camera.Height - 50, $"{FpsCounter.Ms:0.0} ms");
            DebugDrawer.DrawString(5, Global.Camera.Height - 30, $"{FpsCounter.Fps:F1} fps");
            Test.DrawGUI();
            while (DebugDrawer.Texts.TryDequeue(out var text))
            {
                var size = new Vector2(Global.Camera.Width, Global.Camera.Height);
                GUI.Box(new Rect(text.Position.ToUVector2(), size), text.Text, _textStyle);
            }
        }

        private void OnPreRender()
        {
            Test.Render();
        }

        #region Test Control

        public void TogglePause()
        {
            Global.Settings.Pause = !Global.Settings.Pause;
        }

        public void SingleStep()
        {
            Global.Settings.SingleStep = true;
        }

        public void RestartTest()
        {
            LoadTest(CurrentTestIndex);
        }

        private int _testSelected;

        public static int CurrentTestIndex
        {
            get => Global.Settings.TestIndex;
            set => Global.Settings.TestIndex = value;
        }

        public void SetTest(int index)
        {
            _testSelected = index;
        }

        private void CheckTestChange()
        {
            if (CurrentTestIndex != _testSelected)
            {
                CurrentTestIndex = _testSelected;
                LoadTest(_testSelected);
            }
        }

        public void LoadTest(int index)
        {
            Test?.Dispose();
            var (category, testName, testType) = Global.Tests[index];
            Debug.Log($"Run {category}:{testName}");
            Test = (TestBase)Activator.CreateInstance(testType);
            if (Test != null)
            {
                Test.Input = Global.Input;
                Test.Drawer = Global.DebugDrawer;
                Test.TestSettings = Global.Settings;
                Test.World.Drawer = Global.DebugDrawer;
                Test.TextIncrement = 20;
            }
        }

        #endregion

        #region KeyboardControl

        public void CheckKeyboard()
        {
            if (Keyboard.current == null)
            {
                return;
            }

            if (Input.IsKeyDown(KeyCodes.LeftArrow))
            {
                if (Input.KeyModifiers.HasFlag(KeyModifiers.Ctrl))
                {
                    Test.ShiftOrigin(new FVector2(2.0f, 0.0f));
                }
                else
                {
                    Global.Camera.Center.X -= 0.5f;
                }
            }
            else if (Input.IsKeyDown(KeyCodes.RightArrow))
            {
                if (Input.KeyModifiers.HasFlag(KeyModifiers.Ctrl))
                {
                    var newOrigin = new FVector2(-2.0f, 0.0f);
                    Test.ShiftOrigin(newOrigin);
                }
                else
                {
                    Global.Camera.Center.X += 0.5f;
                }
            }
            else if (Input.IsKeyDown(KeyCodes.UpArrow))
            {
                if (Input.KeyModifiers.HasFlag(KeyModifiers.Ctrl))
                {
                    var newOrigin = new FVector2(0.0f, -2.0f);
                    Test.ShiftOrigin(newOrigin);
                }
                else
                {
                    Global.Camera.Center.Y += 0.5f;
                }
            }
            else if (Input.IsKeyDown(KeyCodes.DownArrow))
            {
                if (Input.KeyModifiers.HasFlag(KeyModifiers.Ctrl))
                {
                    var newOrigin = new FVector2(0.0f, 2.0f);
                    Test.ShiftOrigin(newOrigin);
                }
                else
                {
                    Global.Camera.Center.Y -= 0.5f;
                }
            }
            else if (Input.IsKeyDown(KeyCodes.Home))
            {
                // Reset view
                Global.Camera.Zoom = 1.0f;
                Global.Camera.Center.Set(0.0f, 20.0f);
            }
            else if (Input.IsKeyDown(KeyCodes.Z))
            {
                // Zoom out
                Global.Camera.Zoom = Math.Min(1.1f * Global.Camera.Zoom, 20.0f);
            }
            else if (Input.IsKeyDown(KeyCodes.X))
            {
                // Zoom in
                Global.Camera.Zoom = Math.Max(0.9f * Global.Camera.Zoom, 0.02f);
            }
            else if (Input.IsKeyDown(KeyCodes.R))
            {
                // Reset test
                RestartTest();
            }
            else if (Input.IsKeyDown(KeyCodes.Space))
            {
                // Launch a bomb.
                Test?.LaunchBomb();
            }
            else if (Input.IsKeyDown(KeyCodes.O))
            {
                SingleStep();
            }
            else if (Input.IsKeyDown(KeyCodes.P))
            {
                TogglePause();
            }
            else if (Input.IsKeyDown(KeyCodes.LeftBracket))
            {
                // Switch to previous test
                --_testSelected;
                if (_testSelected < 0)
                {
                    _testSelected = Global.Tests.Count - 1;
                }
            }
            else if (Input.IsKeyDown(KeyCodes.RightBracket))
            {
                // Switch to next test
                ++_testSelected;
                if (_testSelected == Global.Tests.Count)
                {
                    _testSelected = 0;
                }
            }
            else if (Input.IsKeyDown(KeyCodes.Tab))
            {
                DebugDrawer.ShowUI = !DebugDrawer.ShowUI;
            }
            else if (Input.IsKeyDown(KeyCodes.Escape))
            {
                BackToInit();
            }
            else
            {
                foreach (var map in UnityInput.KeyCodeMap)
                {
                    if (Input.IsKeyDown(map.Key))
                    {
                        Test?.OnKeyDown(new KeyInputEventArgs(map.Key, Input.KeyModifiers, false));
                    }
                }
            }

            foreach (var map in UnityInput.KeyCodeMap)
            {
                if (Input.IsKeyUp(map.Key))
                {
                    Test?.OnKeyUp(new KeyInputEventArgs(map.Key, Input.KeyModifiers, false));
                }
            }
        }

        #endregion

        #region MouseControl

        private void CheckMouse()
        {
            if (Mouse.current == null)
            {
                return;
            }

            var mousePosition = Mouse.current.position.ReadValue();
            if (!OverUI)
            {
                if (Input.IsMouseDown(MouseButton.Left))
                {
                    var pw = MainCamera.ScreenToWorldPoint(mousePosition).ToFVector2();
                    if (Input.KeyModifiers.HasFlag(KeyModifiers.Shift))
                    {
                        // Mouse left drag
                        Test.ShiftMouseDown(pw);
                    }
                    else
                    {
                        Test.MouseDown(pw);
                    }
                }

                // Mouse right move camera
                if (Input.IsMouseDown(MouseButton.Right))
                {
                    Drag = true;
                    Origin = MainCamera.ScreenToWorldPoint(mousePosition);
                }
            }

            if (Input.IsMouseUp(MouseButton.Left))
            {
                Test.MouseUp(MainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue()).ToFVector2());
            }

            if (Input.IsMousePressed(MouseButton.Left))
            {
                var pw = MainCamera.ScreenToWorldPoint(new Vector2(mousePosition.x, mousePosition.y)).ToFVector2();
                Test.MouseMove(pw);
            }

            if (Input.IsMouseUp(MouseButton.Right))
            {
                Drag = false;
            }

            if (Input.IsMousePressed(MouseButton.Right))
            {
                var delta = Mouse.current.delta.ReadValue();
                Global.Camera.Center.X -= delta.x * 0.05f * Global.Camera.Zoom;
                Global.Camera.Center.Y += delta.y * 0.05f * Global.Camera.Zoom;
            }

            if (Drag)
            {
                var cameraTransform = MainCamera.transform;
                Difference = MainCamera.ScreenToWorldPoint(mousePosition) - cameraTransform.position;
                cameraTransform.position = Origin - Difference;
            }
        }

        #endregion

        #region Touch Control

        private void CheckTouchMoveScreen()
        {
            if (Touchscreen.current == null)
            {
                return;
            }

            var touch = Touchscreen.current;
            if (touch.primaryTouch.isInProgress && !touch.touches[1].isInProgress)
            {
                var touchPosition = touch.primaryTouch.position.ReadValue();
                var delta = touch.primaryTouch.delta.ReadValue();
                Global.Camera.Center.X -= delta.x * 0.05f * Global.Camera.Zoom;
                Global.Camera.Center.Y += delta.y * 0.05f * Global.Camera.Zoom;

                Difference = MainCamera.ScreenToWorldPoint(touchPosition)
                           - MainCamera.transform.position;
                if (Drag == false)
                {
                    Drag = true;
                    Origin = MainCamera.ScreenToWorldPoint(touchPosition);
                }

                MainCamera.transform.position = Origin - Difference;
            }
            else
            {
                Drag = false;
            }
        }

        private float _touchDelta;

        private void CheckTouchZoom()
        {
            if (Touchscreen.current == null)
            {
                return;
            }

            var touch = Touchscreen.current;
            if (touch.touches[0].isInProgress && touch.touches[1].isInProgress)
            {
                var distance = Vector2.Distance(touch.touches[0].position.ReadValue(), touch.touches[1].position.ReadValue()); //两指之间的距离

                //Zoom out
                if (_touchDelta > distance)
                {
                    if (MainCamera.orthographicSize > 1)
                    {
                        MainCamera.orthographicSize += 0.5f;
                    }
                    else
                    {
                        MainCamera.orthographicSize += 0.05f;
                    }

                    Scroll = new Vector2(0, distance);
                    ScrollCallback(Scroll.x, Scroll.y);
                }

                //Zoom in
                else if (_touchDelta < distance)
                {
                    if (MainCamera.orthographicSize > 1)
                    {
                        MainCamera.orthographicSize -= 0.5f;
                    }
                    else if (MainCamera.orthographicSize > 0.1f)
                    {
                        MainCamera.orthographicSize -= 0.05f;
                    }

                    Scroll = new Vector2(0, distance);
                    ScrollCallback(Scroll.x, Scroll.y);
                }

                _touchDelta = distance;
            }
        }

        #endregion

        #region View Control

        public Vector2 Scroll;

        /// <summary>
        /// Mouse wheel zoom
        /// </summary>
        private void CheckZoom()
        {
            if (Mouse.current == null)
            {
                return;
            }

            if (OverUI)
            {
                return;
            }

            var scroll = Mouse.current.scroll.ReadValue();

            //Zoom out
            if (scroll.y < 0)
            {
                if (MainCamera.orthographicSize > 1)
                {
                    MainCamera.orthographicSize += 1f;
                }
                else
                {
                    MainCamera.orthographicSize += 0.1f;
                }

                Scroll = scroll;
                ScrollCallback(Scroll.x, Scroll.y);
            }

            //Zoom in
            else if (scroll.y > 0)
            {
                if (MainCamera.orthographicSize > 1)
                {
                    MainCamera.orthographicSize -= 1f;
                }
                else if (MainCamera.orthographicSize > 0.2f)
                {
                    MainCamera.orthographicSize -= 0.1f;
                }

                Scroll = scroll;
                ScrollCallback(Scroll.x, Scroll.y);
            }
        }

        private int _screenWidth;

        private int _screenHeight;

        private FullScreenMode _mode;

        private void CheckScreenResize()
        {
            var w = Screen.width;
            var h = Screen.height;
            var mode = Screen.fullScreenMode;
            if (_screenWidth != w || _screenHeight != h || _mode != mode)
            {
                _screenWidth = w;
                _screenHeight = h;
                _mode = mode;
                GL.Viewport(new Rect(0, 0, w, h));
                ResizeWindowCallback(w, h, mode);
            }
        }

        public void ResizeWindowCallback(int width, int height, FullScreenMode fullScreenMode)
        {
            Global.Camera.Width = width;
            Global.Camera.Height = height;
            Settings.WindowWidth = width;
            Settings.WindowHeight = height;
            Settings.FullScreenMode = fullScreenMode;
        }

        public void ScrollCallback(double dx, double dy)
        {
            if (dy > 0)
            {
                Global.Camera.Zoom /= 1.1f;
            }
            else
            {
                Global.Camera.Zoom *= 1.1f;
            }
        }

        #endregion
    }
}