﻿using Testbed.TestCases;
using UnityEngine;

namespace Box2DSharp.Testbed.Unity.Tests
{
    [TestInherit]
    public class EdgeTestRender : EdgeTest
    {
        /// <inheritdoc />
        protected override void OnRender()
        {
            // ImGui.SetNextWindowPos(new Vector2(10.0f, 100.0f));
            // ImGui.SetNextWindowSize(new Vector2(200.0f, 100.0f));
            // ImGui.Begin("Custom Controls", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
            //
            // if (ImGui.RadioButton("Boxes", Boxes))
            // {
            //     CreateBoxes();
            //     Boxes = true;
            // }
            //
            // if (ImGui.RadioButton("Circles", Boxes == false))
            // {
            //     CreateCircles();
            //     Boxes = false;
            // }
            //
            // ImGui.End();
            base.OnRender();
        }
    }
}