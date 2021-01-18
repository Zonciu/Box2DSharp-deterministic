using Box2DSharp.Dynamics.Joints;
using Testbed.TestCases;
using UnityEngine;

namespace Box2DSharp.Testbed.Unity.Tests
{
    [TestInherit]
    public class DistanceJointTestRender : DistanceJointTest
    {
        /// <inheritdoc />
        public DistanceJointTestRender()
        {
            
        }

        /// <inheritdoc />
        protected override void OnRender()
        {
            // ImGui.SetNextWindowPos(new Vector2(10.0f, 100.0f));
            // ImGui.SetNextWindowSize(new Vector2(260.0f, 150.0f));
            // ImGui.Begin("Joint Controls", ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize);
            //
            // float len = (float)m_length;
            // if (ImGui.SliderFloat("Length", ref len, 0.0f, 20.0f, "%.0f"))
            // {
            //     m_length = m_joint.SetLength(len);
            // }
            //
            // float minlen = (float)m_minLength;
            // if (ImGui.SliderFloat("Min Length", ref minlen, 0.0f, 20.0f, "%.0f"))
            // {
            //     m_minLength = m_joint.SetMinLength(minlen);
            // }
            //
            // float maxLen = (float)m_maxLength;
            // if (ImGui.SliderFloat("Max Length", ref maxLen, 0.0f, 20.0f, "%.0f"))
            // {
            //     m_maxLength = m_joint.SetMaxLength(maxLen);
            // }
            //
            // float hz = (float)m_hertz;
            // if (ImGui.SliderFloat("Hertz", ref hz, 0.0f, 10.0f, "%.1f"))
            // {
            //     m_hertz = hz;
            //     JointUtils.LinearStiffness(out var stiffness, out var damping, m_hertz, m_dampingRatio, m_joint.BodyA, m_joint.BodyB);
            //     m_joint.Stiffness = stiffness;
            //     m_joint.Damping = damping;
            // }
            //
            // float ratio = (float)m_dampingRatio;
            // if (ImGui.SliderFloat("Damping Ratio", ref ratio, 0.0f, 2.0f, "%.1f"))
            // {
            //     m_dampingRatio = ratio;
            //     JointUtils.LinearStiffness(out var stiffness, out var damping, m_hertz, m_dampingRatio, m_joint.BodyA, m_joint.BodyB);
            //     m_joint.Stiffness = stiffness;
            //     m_joint.Damping = damping;
            // }
            //
            // ImGui.End();
            base.OnRender();
        }
    }
}