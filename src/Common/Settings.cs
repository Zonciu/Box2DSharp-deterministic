namespace Box2DSharp.Common
{
    public static class Settings
    {
        public static FP MaxFloat = FP.MaxValue;

        public static FP MinFloat = FP.MinValue;

        public static FP Epsilon = FP.Epsilon;

        public static FP Pi = FP.Pi;

        public static FP LengthUnitsPerMeter = FP.One;

        // @file
        // Global tuning constants based on meters-kilograms-seconds (MKS) units.

        // Collision

        /// The maximum number of contact points between two convex shapes. Do
        /// not change this value.
        public const int MaxManifoldPoints = 2;

        /// The maximum number of vertices on a convex polygon. You cannot increase
        /// this too much because b2BlockAllocator has a maximum object size.
        public const int MaxPolygonVertices = 8;

        /// This is used to fatten AABBs in the dynamic tree. This allows proxies
        /// to move by a small amount without triggering a tree adjustment.
        /// This is in meters.
        public static FP AABBExtension = 0.1f * LengthUnitsPerMeter;

        /// This is used to fatten AABBs in the dynamic tree. This is used to predict
        /// the future position based on the current displacement.
        /// This is a dimensionless multiplier.
        public const int AABBMultiplier = 4;

        /// A small length used as a collision and constraint tolerance. Usually it is
        /// chosen to be numerically significant, but visually insignificant.
        public static FP LinearSlop = 0.005f * LengthUnitsPerMeter;

        /// A small angle used as a collision and constraint tolerance. Usually it is
        /// chosen to be numerically significant, but visually insignificant.
        public static FP AngularSlop = FP.PiTimes2 / 180;

        /// The radius of the polygon/edge shape skin. This should not be modified. Making
        /// this smaller means polygons will have an insufficient buffer for continuous collision.
        /// Making it larger may create artifacts for vertex collision.
        public static FP PolygonRadius = 2 * LinearSlop;

        /// Maximum number of sub-steps per contact in continuous physics simulation.
        public const int MaxSubSteps = 8;

        // Dynamics

        /// Maximum number of contacts to be handled to solve a TOI impact.
        public const int MaxToiContacts = 32;

        /// The maximum linear position correction used when solving constraints. This helps to
        /// prevent overshoot.
        public static FP MaxLinearCorrection = 0.2f * LengthUnitsPerMeter;

        /// The maximum angular position correction used when solving constraints. This helps to
        /// prevent overshoot.
        public static FP MaxAngularCorrection = 8 * Pi / 180;

        /// The maximum linear velocity of a body. This limit is very large and is used
        /// to prevent numerical problems. You shouldn't need to adjust this.
        public static FP MaxTranslation = 2 * LengthUnitsPerMeter;

        public static FP MaxTranslationSquared = MaxTranslation * MaxTranslation;

        /// The maximum angular velocity of a body. This limit is very large and is used
        /// to prevent numerical problems. You shouldn't need to adjust this.
        public static FP MaxRotation = 0.5 * Pi;

        public static FP MaxRotationSquared = MaxRotation * MaxRotation;

        /// This scale factor controls how fast overlap is resolved. Ideally this would be 1 so
        /// that overlap is removed in one time step. However using values close to 1 often lead
        /// to overshoot.
        public static FP Baumgarte = 0.2;

        public static FP ToiBaumgarte = 0.75;

        // Sleep

        /// The time that a body must be still before it will go to sleep.
        public static FP TimeToSleep = 0.5;

        /// A body cannot sleep if its linear velocity is above this tolerance.
        public static FP LinearSleepTolerance = 0.01 * LengthUnitsPerMeter;

        /// A body cannot sleep if its angular velocity is above this tolerance.
        public static FP AngularSleepTolerance = FP.PiTimes2 / 180;
    }
}