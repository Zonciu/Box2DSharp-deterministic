﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Box2DSharp.Common;
using ImGuiNET;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
using OpenTK.Windowing.Common;
using OpenTK.Windowing.Desktop;
using OpenTK.Windowing.GraphicsLibraryFramework;
using Testbed.Abstractions;
using Testbed.Gui;
using Testbed.Render;
using Testbed.TestCases;
using TKKeyModifiers = OpenTK.Windowing.GraphicsLibraryFramework.KeyModifiers;
using TKMouseButton = OpenTK.Windowing.GraphicsLibraryFramework.MouseButton;

namespace Testbed
{
    public class Game : GameWindow
    {
        private ImGuiController _controller;

        public FpsCounter FpsCounter = new FpsCounter();

        public TestBase Test { get; private set; }

        public readonly DebugDrawer DebugDrawer;

        public readonly Input Input;

        private bool _stopped;

        private string _environment;

        /// <inheritdoc />
        public Game(GameWindowSettings gameWindowSettings, NativeWindowSettings nativeWindowSettings)
            : base(gameWindowSettings, nativeWindowSettings)
        {
            _environment = System.Environment.Version.ToString();
            Input = new Input(this);
            Global.Input = Input;

            DebugDrawer = new DebugDrawer();
            Global.DebugDrawer = DebugDrawer;
        }

        /// <inheritdoc />
        protected override void OnLoad()
        {
            Title = $"Box2DSharp Testbed - Runtime Version: {_environment}";
            var testBaseType = typeof(TestBase);
            var testTypes = typeof(HelloWorld).Assembly.GetTypes()
                                              .Where(e => testBaseType.IsAssignableFrom(e) && !e.IsAbstract && e.GetCustomAttribute<TestCaseAttribute>() != null)
                                              .ToHashSet();
            var inheritedTest = this.GetType()
                                    .Assembly.GetTypes()
                                    .Where(
                                         e => testBaseType.IsAssignableFrom(e)
                                           && e.GetCustomAttribute<TestInheritAttribute>() != null
                                           && e.GetCustomAttribute<TestCaseAttribute>() != null)
                                    .ToList();
            foreach (var type in inheritedTest)
            {
                testTypes.Remove(type.BaseType);
            }

            var typeList = new List<Type>(testTypes.Count + inheritedTest.Count);
            typeList.AddRange(testTypes);
            typeList.AddRange(inheritedTest);
            Global.SetupTestCases(typeList);

            GL.ClearColor(0.2f, 0.2f, 0.2f, 1.0f);
            _controller = new ImGuiController(Size.X, Size.Y);
            DebugDrawer.Create();

            _currentTestIndex = Math.Clamp(_currentTestIndex, 0, Global.Tests.Count - 1);
            _testSelected = _currentTestIndex;
            LoadTest(_testSelected);
            base.OnLoad();
        }

        /// <inheritdoc />
        protected override void OnUnload()
        {
            _stopped = true;
            base.OnUnload();
        }

        /// <inheritdoc />
        protected override void OnClosed()
        {
            Test?.Dispose();
            Test = null;
            DebugDrawer.Destroy();
            TestSettingHelper.Save(Global.Settings);
            _controller.Dispose();
            _controller = null;
            base.OnClosed();
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            if (_stopped)
            {
                return;
            }

            var input = KeyboardState;
            if (input.IsKeyDown(Keys.Escape))
            {
                Close();
            }

            CheckTestChange();
            Test.Step();
            FpsCounter.Count();
            base.OnUpdateFrame(e);
        }

        #region Render

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            _controller.Update(this, (float)e.Time);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            UpdateText();
            UpdateUI();
            Test.Render();
            Test.DrawGUI();
            if (DebugDrawer.ShowUI)
            {
                DebugDrawer.DrawString(5, Global.Camera.Height - 60, $"steps: {Test.StepCount}");
                DebugDrawer.DrawString(5, Global.Camera.Height - 40, $"{FpsCounter.Ms:0.0} ms");
                DebugDrawer.DrawString(5, Global.Camera.Height - 20, $"{FpsCounter.Fps:F1} fps");
            }

            DebugDrawer.Flush();

            _controller.Render();

            Util.CheckGLError("End of frame");
            SwapBuffers();
            base.OnRenderFrame(e);
        }

        private void UpdateText()
        {
            if (DebugDrawer.ShowUI)
            {
                ImGui.SetNextWindowPos(new System.Numerics.Vector2(0.0f, 0.0f));
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(Global.Camera.Width, Global.Camera.Height));
                ImGui.SetNextWindowBgAlpha(0);
                ImGui.Begin("Overlay", ImGuiWindowFlags.NoTitleBar | ImGuiWindowFlags.NoInputs | ImGuiWindowFlags.AlwaysAutoResize | ImGuiWindowFlags.NoScrollbar);
                ImGui.End();
                var (category, name, _) = Global.Tests[_currentTestIndex];
                Test?.DrawTitle($"{category} : {name}");
            }
        }

        public void UpdateUI()
        {
            const int MenuWidth = 180;
            if (DebugDrawer.ShowUI)
            {
                ImGui.SetNextWindowPos(new System.Numerics.Vector2((float)Global.Camera.Width - MenuWidth - 10, 10));
                ImGui.SetNextWindowSize(new System.Numerics.Vector2(MenuWidth, (float)Global.Camera.Height - 20));

                ImGui.Begin("Tools", ref DebugDrawer.ShowUI, ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize | ImGuiWindowFlags.NoCollapse);

                if (ImGui.BeginTabBar("ControlTabs", ImGuiTabBarFlags.None))
                {
                    if (ImGui.BeginTabItem("Controls"))
                    {
                        ImGui.SliderInt("Vel Iters", ref Global.Settings.VelocityIterations, 0, 50);
                        ImGui.SliderInt("Pos Iters", ref Global.Settings.PositionIterations, 0, 50);
                        ImGui.SliderFloat("Hertz", ref Global.Settings.Hertz, 5.0f, 120.0f, "%.0f hz");

                        ImGui.Separator();

                        ImGui.Checkbox("Sleep", ref Global.Settings.EnableSleep);
                        ImGui.Checkbox("Warm Starting", ref Global.Settings.EnableWarmStarting);
                        ImGui.Checkbox("Time of Impact", ref Global.Settings.EnableContinuous);
                        ImGui.Checkbox("Sub-Stepping", ref Global.Settings.EnableSubStepping);

                        ImGui.Separator();

                        ImGui.Checkbox("Shapes", ref Global.Settings.DrawShapes);
                        ImGui.Checkbox("Joints", ref Global.Settings.DrawJoints);
                        ImGui.Checkbox("AABBs", ref Global.Settings.DrawAABBs);
                        ImGui.Checkbox("Contact Points", ref Global.Settings.DrawContactPoints);
                        ImGui.Checkbox("Contact Normals", ref Global.Settings.DrawContactNormals);
                        ImGui.Checkbox("Contact Impulses", ref Global.Settings.DrawContactImpulse);
                        ImGui.Checkbox("Friction Impulses", ref Global.Settings.DrawFrictionImpulse);
                        ImGui.Checkbox("Center of Masses", ref Global.Settings.DrawCOMs);
                        ImGui.Checkbox("Statistics", ref Global.Settings.DrawStats);
                        ImGui.Checkbox("Profile", ref Global.Settings.DrawProfile);

                        var buttonSz = new System.Numerics.Vector2(-1, 0);
                        if (ImGui.Button("Pause (P)", buttonSz))
                        {
                            Global.Settings.Pause = !Global.Settings.Pause;
                        }

                        if (ImGui.Button("Single Step (O)", buttonSz))
                        {
                            Global.Settings.SingleStep = !Global.Settings.SingleStep;
                        }

                        if (ImGui.Button("Restart (R)", buttonSz))
                        {
                            RestartTest();
                        }

                        if (ImGui.Button("Quit", buttonSz))
                        {
                            Close();
                        }

                        ImGui.EndTabItem();
                    }

                    var leafNodeFlags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick;
                    leafNodeFlags |= ImGuiTreeNodeFlags.Leaf | ImGuiTreeNodeFlags.NoTreePushOnOpen;

                    const ImGuiTreeNodeFlags NodeFlags = ImGuiTreeNodeFlags.OpenOnArrow | ImGuiTreeNodeFlags.OpenOnDoubleClick;

                    if (ImGui.BeginTabItem("Tests"))
                    {
                        var categoryIndex = 0;
                        var category = Global.Tests[categoryIndex].Category;
                        var i = 0;
                        while (i < Global.Tests.Count)
                        {
                            var categorySelected = string.CompareOrdinal(category, Global.Tests[_currentTestIndex].Category) == 0;
                            var nodeSelectionFlags = categorySelected ? ImGuiTreeNodeFlags.Selected : 0;
                            var nodeOpen = ImGui.TreeNodeEx(category, NodeFlags | nodeSelectionFlags);

                            if (nodeOpen)
                            {
                                while (i < Global.Tests.Count && string.CompareOrdinal(category, Global.Tests[i].Category) == 0)
                                {
                                    ImGuiTreeNodeFlags selectionFlags = 0;
                                    if (_currentTestIndex == i)
                                    {
                                        selectionFlags = ImGuiTreeNodeFlags.Selected;
                                    }

                                    ImGui.TreeNodeEx((IntPtr)i, leafNodeFlags | selectionFlags, Global.Tests[i].Name);
                                    if (ImGui.IsItemClicked())
                                    {
                                        SetTest(i);
                                    }

                                    ++i;
                                }

                                ImGui.TreePop();
                            }
                            else
                            {
                                while (i < Global.Tests.Count && string.CompareOrdinal(category, Global.Tests[i].Category) == 0)
                                {
                                    ++i;
                                }
                            }

                            if (i < Global.Tests.Count)
                            {
                                category = Global.Tests[i].Category;
                                categoryIndex = i;
                            }
                        }

                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();
                }

                ImGui.End();
            }
        }

        #endregion

        #region KeyboardControl

        /// <inheritdoc />
        protected override void OnKeyDown(KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
            case Keys.Left:
                if (e.Control)
                {
                    Test.ShiftOrigin(new FVector2(2.0f, 0.0f));
                }
                else
                {
                    Global.Camera.Center.X -= 0.5f;
                }

                break;
            case Keys.Right:
                if (e.Control)
                {
                    var newOrigin = new FVector2(-2.0f, 0.0f);
                    Test.ShiftOrigin(newOrigin);
                }
                else
                {
                    Global.Camera.Center.X += 0.5f;
                }

                break;
            case Keys.Up:
                if (e.Control)
                {
                    var newOrigin = new FVector2(0.0f, -2.0f);
                    Test.ShiftOrigin(newOrigin);
                }
                else
                {
                    Global.Camera.Center.Y += 0.5f;
                }

                break;
            case Keys.Down:
                if (e.Control)
                {
                    var newOrigin = new FVector2(0.0f, 2.0f);
                    Test.ShiftOrigin(newOrigin);
                }
                else
                {
                    Global.Camera.Center.Y -= 0.5f;
                }

                break;
            case Keys.Home:
                // Reset view
                Global.Camera.ResetView();
                break;
            case Keys.Z:
                // Zoom out
                Global.Camera.Zoom = Math.Min(1.1f * Global.Camera.Zoom, 20.0f);
                break;
            case Keys.X:
                // Zoom in
                Global.Camera.Zoom = Math.Max(0.9f * Global.Camera.Zoom, 0.02f);
                break;
            case Keys.R:
                // Reset test
                RestartTest();
                break;
            case Keys.Space:
                // Launch a bomb.
                Test?.LaunchBomb();
                break;
            case Keys.O:
                Global.Settings.SingleStep = true;
                break;

            case Keys.P:
                Global.Settings.Pause = !Global.Settings.Pause;
                break;

            case Keys.LeftBracket:
                // Switch to previous test
                --_testSelected;
                if (_testSelected < 0)
                {
                    _testSelected = Global.Tests.Count - 1;
                }

                break;

            case Keys.RightBracket:
                // Switch to next test
                ++_testSelected;
                if (_testSelected == Global.Tests.Count)
                {
                    _testSelected = 0;
                }

                break;

            case Keys.Tab:
                DebugDrawer.ShowUI = !DebugDrawer.ShowUI;
                break;
            default:
                Test?.OnKeyDown(new KeyInputEventArgs(Input.GetKeyCode(e.Key), Input.GetKeyModifiers(e.Modifiers), e.IsRepeat));
                break;
            }

            base.OnKeyDown(e);
        }

        /// <inheritdoc />
        protected override void OnKeyUp(KeyboardKeyEventArgs e)
        {
            Test.OnKeyUp(new KeyInputEventArgs(Input.GetKeyCode(e.Key), Input.GetKeyModifiers(e.Modifiers), e.IsRepeat));
            base.OnKeyUp(e);
        }

        #endregion

        #region MouseControl

        /// <inheritdoc />
        protected override void OnMouseDown(MouseButtonEventArgs e)
        {
            if (e.Button == TKMouseButton.Left)
            {
                var pw = Global.Camera.ConvertScreenToWorld(new System.Numerics.Vector2(MousePosition.X, MousePosition.Y)).ToFVector2();

                if (e.Modifiers == TKKeyModifiers.Shift)
                {
                    Test.ShiftMouseDown(pw);
                }
                else
                {
                    Test.MouseDown(pw);
                }
            }

            base.OnMouseDown(e);
        }

        /// <inheritdoc />
        protected override void OnMouseUp(MouseButtonEventArgs e)
        {
            if (e.Button == TKMouseButton.Left)
            {
                var pw = Global.Camera.ConvertScreenToWorld(new System.Numerics.Vector2(MousePosition.X, MousePosition.Y)).ToFVector2();
                Test.MouseUp(pw);
            }

            base.OnMouseUp(e);
        }

        /// <inheritdoc />
        protected override void OnMouseMove(MouseMoveEventArgs e)
        {
            if (IsMouseButtonDown(TKMouseButton.Left))
            {
                var pw = Global.Camera.ConvertScreenToWorld(new System.Numerics.Vector2(MousePosition.X, MousePosition.Y)).ToFVector2();
                Test.MouseMove(pw);
            }

            if (IsMouseButtonDown(TKMouseButton.Right))
            {
                Global.Camera.Center.X -= e.DeltaX * 0.05f * Global.Camera.Zoom;
                Global.Camera.Center.Y += e.DeltaY * 0.05f * Global.Camera.Zoom;
            }

            base.OnMouseMove(e);
        }

        #endregion

        #region Test Control

        public void RestartTest()
        {
            LoadTest(_currentTestIndex);
        }

        private int _testSelected;

        private static int _currentTestIndex
        {
            get => Global.Settings.TestIndex;
            set => Global.Settings.TestIndex = value;
        }

        private void SetTest(int index)
        {
            _testSelected = index;
        }

        private void CheckTestChange()
        {
            if (_currentTestIndex != _testSelected)
            {
                _currentTestIndex = _testSelected;
                LoadTest(_testSelected);
            }
        }

        private void LoadTest(int index)
        {
            Test?.Dispose();
            Test = (TestBase)Activator.CreateInstance(Global.Tests[index].TestType);
            if (Test != null)
            {
                Test.Input = Global.Input;
                Test.Drawer = Global.DebugDrawer;
                Test.TestSettings = Global.Settings;
                Test.World.Drawer = Global.DebugDrawer;
            }
        }

        #endregion

        #region View Control

        public Vector2 Scroll;

        /// <inheritdoc />
        protected override void OnMouseWheel(MouseWheelEventArgs e)
        {
            Scroll = MouseState.ScrollDelta;
            ScrollCallback(Scroll.X, Scroll.Y);
        }

        /// <inheritdoc />
        protected override void OnResize(ResizeEventArgs e)
        {
            GL.Viewport(0, 0, e.Width, e.Height);
            _controller.WindowResized(e.Width, e.Height);
            ResizeWindowCallback(e.Width, e.Height);
            base.OnResize(e);
        }

        public static void ResizeWindowCallback(int width, int height)
        {
            Global.Camera.Width = width;
            Global.Camera.Height = height;
            Global.Settings.WindowWidth = width;
            Global.Settings.WindowHeight = height;
        }

        public static void ScrollCallback(double dx, double dy)
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