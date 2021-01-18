using Box2DSharp.Common;

namespace Box2DSharp.Dynamics
{
    /// This is an internal structure.
    public struct TimeStep
    {
        public FP Dt; // time step

        public FP InvDt; // inverse time step (0 if dt == 0).

        public FP DtRatio; // dt * inv_dt0

        public int VelocityIterations;

        public int PositionIterations;

        public bool WarmStarting;
    }
}