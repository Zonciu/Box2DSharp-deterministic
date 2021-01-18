using Box2DSharp.Common;

namespace Box2DSharp.Ropes
{
    /// 
    public struct RopeDef
    {
        public FVector2 Position;

        public FVector2[] Vertices;

        public int Count;

        public FP[] Masses;

        public FVector2 Gravity;

        public RopeTuning Tuning;
    };
}