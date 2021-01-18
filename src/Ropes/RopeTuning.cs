using Box2DSharp.Common;

namespace Box2DSharp.Ropes
{
    ///
    public class RopeTuning
    {
        public RopeTuning()
        {
            StretchingModel = StretchingModel.PbdStretchingModel;
            BendingModel = BendingModel.PbdAngleBendingModel;
            Damping = 0.0f;
            StretchStiffness = 1.0f;
            BendStiffness = 0.5f;
            BendHertz = 1.0f;
            BendDamping = 0.0f;
            Isometric = false;
            FixedEffectiveMass = false;
            WarmStart = false;
        }

        public StretchingModel StretchingModel;

        public BendingModel BendingModel;

        public FP Damping;

        public FP StretchStiffness;

        public FP StretchHertz;

        public FP StretchDamping;

        public FP BendStiffness;

        public FP BendHertz;

        public FP BendDamping;

        public bool Isometric;

        public bool FixedEffectiveMass;

        public bool WarmStart;
    };
}