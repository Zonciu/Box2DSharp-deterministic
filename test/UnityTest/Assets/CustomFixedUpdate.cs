using UnityEngine;

namespace Box2DSharp.Testbed.Unity
{
    public class CustomFixedUpdate
    {
        public delegate void OnFixedUpdateCallback(float aDeltaTime);

        private float m_FixedTimeStep;

        private float m_Timer = 0;

        private OnFixedUpdateCallback m_Callback;

        public float MaxAllowedTimeStep { get; set; } = 0f;

        public float DeltaTime
        {
            get => m_FixedTimeStep;
            set => m_FixedTimeStep = Mathf.Max(value, 0.000001f); // max rate: 1000000
        }

        public float UpdateRate
        {
            get => 1.0f / DeltaTime;
            set => DeltaTime = 1.0f / value;
        }

        public CustomFixedUpdate(float aTimeStep, OnFixedUpdateCallback aCallback, float aMaxAllowedTimestep = 0f)
        {
            if (aTimeStep <= 0f)
            {
                throw new System.ArgumentException("TimeStep needs to be greater than 0");
            }

            m_Callback = aCallback ?? throw new System.ArgumentException("CustomFixedUpdate needs a valid callback");
            DeltaTime = aTimeStep;
            MaxAllowedTimeStep = aMaxAllowedTimestep;
        }

        public CustomFixedUpdate(OnFixedUpdateCallback aCallback)
            : this(0.01f, aCallback, 0f)
        { }

        public CustomFixedUpdate(OnFixedUpdateCallback aCallback, float aFPS, float aMaxAllowedTimestep = 0f)
            : this(1f / aFPS, aCallback, aMaxAllowedTimestep)
        { }

        public void Update(float aDeltaTime)
        {
            m_Timer -= aDeltaTime;
            if (MaxAllowedTimeStep > 0)
            {
                var timeout = Time.realtimeSinceStartup + MaxAllowedTimeStep;
                while (m_Timer < 0f && Time.realtimeSinceStartup < timeout)
                {
                    m_Callback(m_FixedTimeStep);
                    m_Timer += m_FixedTimeStep;
                }
            }
            else
            {
                while (m_Timer < 0f)
                {
                    m_Callback(m_FixedTimeStep);
                    m_Timer += m_FixedTimeStep;
                }
            }
        }

        public void Update()
        {
            Update(Time.deltaTime);
        }
    }
}