﻿using Testbed.TestCases;
using UnityEngine;

namespace Box2DSharp.Testbed.Unity.Tests
{
    [TestInherit]
    public class RensorsTestRender : Sensors
    {
        /// <inheritdoc />
        protected override void OnRender()
        {
            // ImGui.SetNextWindowPos(new Vector2(10.0f, 100.0f));
            // ImGui.SetNextWindowSize(new Vector2(200.0f, 60.0f));
            // ImGui.Begin("Sensor Controls", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
            // var force = (float)_force;
            // ImGui.SliderFloat("Force", ref force, 0.0f, 2000.0f, "%.0f");
            // _force = force;
            // ImGui.End();
            base.OnRender();
        }
    }
}