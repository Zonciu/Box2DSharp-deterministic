using Testbed.TestCases;
using UnityEngine;
using UnityEngine.UI;

namespace Box2DSharp.Testbed.Unity.Tests
{
    [TestInherit]
    public class CompoundShapesRender : CompoundShapes
    {
        private GameObject CanvasObj;

        private Canvas Canvas;

        protected override void OnGUI()
        { }

        /// <inheritdoc />
        protected override void OnRender()
        {
            // ImGui.SetNextWindowPos(new Vector2(10.0f, 100.0f));
            // ImGui.SetNextWindowSize(new Vector2(200.0f, 100.0f));
            // ImGui.Begin("Controls", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
            //
            // if (ImGui.Button("Spawn"))
            // {
            //     Spawn();
            // }
            //
            // ImGui.End();
            base.OnRender();
        }
    }
}