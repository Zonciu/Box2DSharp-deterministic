using Testbed.TestCases;

namespace Box2DSharp.Testbed.Unity.Tests
{
    [TestInherit]
    public class PrismaticJointTestRender : PrismaticJointTest
    {
        /// <inheritdoc />
        protected override void OnRender()
        {
            // ImGui.SetNextWindowPos(new Vector2(10.0f, 100.0f));
            // ImGui.SetNextWindowSize(new Vector2(200.0f, 100.0f));
            // ImGui.Begin("Joint Controls", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
            //
            // if (ImGui.Checkbox("Limit", ref EnableLimit))
            // {
            //     Joint.EnableLimit(EnableLimit);
            // }
            //
            // if (ImGui.Checkbox("Motor", ref EnableMotor))
            // {
            //     Joint.EnableMotor(EnableMotor);
            // }
            //
            // var speed = (float)MotorSpeed;
            // if (ImGui.SliderFloat("Speed", ref speed, -100.0f, 100.0f, "%.0f"))
            // {
            //     MotorSpeed = speed;
            //     Joint.SetMotorSpeed(MotorSpeed);
            // }
            //
            // ImGui.End();
            base.OnRender();
        }
    }
}