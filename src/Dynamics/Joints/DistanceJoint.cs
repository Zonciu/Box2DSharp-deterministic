using Box2DSharp.Common;

namespace Box2DSharp.Dynamics.Joints
{
    /// A distance joint constrains two points on two bodies to remain at a fixed
    /// distance from each other. You can view this as a massless, rigid rod.
    public class DistanceJoint : Joint
    {
        // Solver shared
        private readonly FVector2 _localAnchorA;

        private readonly FVector2 _localAnchorB;

        private FP _bias;

        private FP _gamma;

        private FP _impulse;

        // Solver temp
        private int _indexA;

        private int _indexB;

        private FP _invIa;

        private FP _invIb;

        private FP _invMassA;

        private FP _invMassB;

        private FVector2 _localCenterA;

        private FVector2 _localCenterB;

        private FP _mass;

        private FVector2 _rA;

        private FVector2 _rB;

        private FVector2 _u;

        /// The rest length
        private FP _length;

        private FP _minLength;

        private FP _maxLength;

        private FP _currentLength;

        private FP _lowerImpulse;

        private FP _upperImpulse;

        internal DistanceJoint(DistanceJointDef def)
            : base(def)
        {
            _localAnchorA = def.LocalAnchorA;
            _localAnchorB = def.LocalAnchorB;
            _length = FP.Max(def.Length, Settings.LinearSlop);
            _minLength = FP.Max(def.MinLength, Settings.LinearSlop);
            _maxLength = FP.Max(def.MaxLength, _minLength);
            Stiffness = def.Stiffness;
            Damping = def.Damping;
            _impulse = 0.0f;
            _gamma = 0.0f;
            _bias = 0.0f;
            _impulse = 0.0f;
            _lowerImpulse = 0.0f;
            _upperImpulse = 0.0f;
            _currentLength = 0.0f;
        }

        /// Set/get the linear stiffness in N/m
        public FP Stiffness { get; set; }

        /// Set/get linear damping in N*s/m
        public FP Damping { get; set; }

        public FP SoftMass { get; set; }

        public override FVector2 GetAnchorA()
        {
            return BodyA.GetWorldPoint(_localAnchorA);
        }

        public override FVector2 GetAnchorB()
        {
            return BodyB.GetWorldPoint(_localAnchorB);
        }

        /// Get the reaction force given the inverse time step.
        /// Unit is N.
        public override FVector2 GetReactionForce(FP inv_dt)
        {
            var F = inv_dt * (_impulse + _lowerImpulse - _upperImpulse) * _u;
            return F;
        }

        /// Get the reaction torque given the inverse time step.
        /// Unit is N*m. This is always zero for a distance joint.
        public override FP GetReactionTorque(FP inv_dt)
        {
            return 0.0f;
        }

        /// The local anchor point relative to bodyA's origin.
        public FVector2 GetLocalAnchorA()
        {
            return _localAnchorA;
        }

        /// The local anchor point relative to bodyB's origin.
        public FVector2 GetLocalAnchorB()
        {
            return _localAnchorB;
        }

        public FP SetLength(FP length)
        {
            _impulse = 0.0f;
            _length = FP.Max(Settings.LinearSlop, length);
            return _length;
        }

        public FP SetMinLength(FP minLength)
        {
            _lowerImpulse = 0.0f;
            _minLength = MathUtils.Clamp(minLength, Settings.LinearSlop, _maxLength);
            return _minLength;
        }

        public FP SetMaxLength(FP maxLength)
        {
            _upperImpulse = 0.0f;
            _maxLength = FP.Max(maxLength, _minLength);
            return _maxLength;
        }

        public FP GetCurrentLength()
        {
            var pA = BodyA.GetWorldPoint(_localAnchorA);
            var pB = BodyB.GetWorldPoint(_localAnchorB);
            var d = pB - pA;
            var length = d.Length();
            return length;
        }

        /// Dump joint to dmLog
        public override void Dump()
        {
            // Todo
        }

        internal override void InitVelocityConstraints(in SolverData data)
        {
            _indexA = BodyA.IslandIndex;
            _indexB = BodyB.IslandIndex;
            _localCenterA = BodyA.Sweep.LocalCenter;
            _localCenterB = BodyB.Sweep.LocalCenter;
            _invMassA = BodyA.InvMass;
            _invMassB = BodyB.InvMass;
            _invIa = BodyA.InverseInertia;
            _invIb = BodyB.InverseInertia;

            var cA = data.Positions[_indexA].Center;
            var aA = data.Positions[_indexA].Angle;
            var vA = data.Velocities[_indexA].V;
            var wA = data.Velocities[_indexA].W;

            var cB = data.Positions[_indexB].Center;
            var aB = data.Positions[_indexB].Angle;
            var vB = data.Velocities[_indexB].V;
            var wB = data.Velocities[_indexB].W;

            var qA = new Rotation(aA);
            var qB = new Rotation(aB);

            _rA = MathUtils.Mul(qA, _localAnchorA - _localCenterA);
            _rB = MathUtils.Mul(qB, _localAnchorB - _localCenterB);
            _u = cB + _rB - cA - _rA;

            // Handle singularity.
            _currentLength = _u.Length();
            if (_currentLength > Settings.LinearSlop)
            {
                _u *= 1.0f / _currentLength;
            }
            else
            {
                _u.Set(0.0f, 0.0f);
                _mass = 0.0f;
                _impulse = 0.0f;
                _lowerImpulse = 0.0f;
                _upperImpulse = 0.0f;
            }

            var crAu = MathUtils.Cross(_rA, _u);
            var crBu = MathUtils.Cross(_rB, _u);
            var invMass = _invMassA + _invIa * crAu * crAu + _invMassB + _invIb * crBu * crBu;
            _mass = invMass != FP.Zero ? FP.One / invMass : FP.Zero;
            if (Stiffness > 0.0f && _minLength < _maxLength)
            {
                // soft
                var C = _currentLength - _length;

                var d = Damping;
                var k = Stiffness;

                // magic formulas
                var h = data.Step.Dt;

                // gamma = 1 / (h * (d + h * k))
                // the extra factor of h in the denominator is since the lambda is an impulse, not a force
                _gamma = h * (d + h * k);
                _gamma = _gamma != FP.Zero ? FP.One / _gamma : FP.Zero;
                _bias = C * h * k * _gamma;

                invMass += _gamma;
                SoftMass = FP.Abs(invMass) > Settings.Epsilon ? FP.One / invMass : FP.Zero;
            }
            else
            {
                // rigid
                _gamma = 0.0f;
                _bias = 0.0f;
                _mass = invMass != FP.Zero ? FP.One / invMass : FP.Zero;
                SoftMass = _mass;
            }

            if (data.Step.WarmStarting)
            {
                // Scale the impulse to support a variable time step.
                _impulse *= data.Step.DtRatio;
                _lowerImpulse *= data.Step.DtRatio;
                _upperImpulse *= data.Step.DtRatio;

                var P = (_impulse + _lowerImpulse - _upperImpulse) * _u;
                vA -= _invMassA * P;
                wA -= _invIa * MathUtils.Cross(_rA, P);
                vB += _invMassB * P;
                wB += _invIb * MathUtils.Cross(_rB, P);
            }
            else
            {
                _impulse = 0.0f;
            }

            data.Velocities[_indexA].V = vA;
            data.Velocities[_indexA].W = wA;
            data.Velocities[_indexB].V = vB;
            data.Velocities[_indexB].W = wB;
        }

        internal override void SolveVelocityConstraints(in SolverData data)
        {
            var vA = data.Velocities[_indexA].V;
            var wA = data.Velocities[_indexA].W;
            var vB = data.Velocities[_indexB].V;
            var wB = data.Velocities[_indexB].W;
            if (_minLength < _maxLength)
            {
                if (Stiffness > 0.0f)
                {
                    // Cdot = dot(u, v + cross(w, r))
                    var vpA = vA + MathUtils.Cross(wA, _rA);
                    var vpB = vB + MathUtils.Cross(wB, _rB);
                    var Cdot = FVector2.Dot(_u, vpB - vpA);

                    var impulse = -SoftMass * (Cdot + _bias + _gamma * _impulse);
                    _impulse += impulse;

                    var P = impulse * _u;
                    vA -= _invMassA * P;
                    wA -= _invIa * MathUtils.Cross(_rA, P);
                    vB += _invMassB * P;
                    wB += _invIb * MathUtils.Cross(_rB, P);
                }

                // lower
                {
                    var C = _currentLength - _minLength;
                    var bias = FP.Max(0.0f, C) * data.Step.InvDt;

                    var vpA = vA + MathUtils.Cross(wA, _rA);
                    var vpB = vB + MathUtils.Cross(wB, _rB);
                    var Cdot = FVector2.Dot(_u, vpB - vpA);

                    var impulse = -_mass * (Cdot + bias);
                    var oldImpulse = _lowerImpulse;
                    _lowerImpulse = FP.Max(0.0f, _lowerImpulse + impulse);
                    impulse = _lowerImpulse - oldImpulse;
                    var P = impulse * _u;

                    vA -= _invMassA * P;
                    wA -= _invIa * MathUtils.Cross(_rA, P);
                    vB += _invMassB * P;
                    wB += _invIb * MathUtils.Cross(_rB, P);
                }

                // upper
                {
                    var C = _maxLength - _currentLength;
                    var bias = FP.Max(0.0f, C).SafeMul(data.Step.InvDt);

                    var vpA = vA + MathUtils.Cross(wA, _rA);
                    var vpB = vB + MathUtils.Cross(wB, _rB);
                    var Cdot = FVector2.Dot(_u, vpA - vpB);

                    var impulse = -_mass.SafeMul(Cdot.SafeAdd(bias));
                    var oldImpulse = _upperImpulse;
                    _upperImpulse = FP.Max(0.0f, _upperImpulse + impulse);
                    impulse = _upperImpulse - oldImpulse;
                    var P = -impulse * _u;

                    vA -= _invMassA * P;
                    wA -= _invIa * MathUtils.Cross(_rA, P);
                    vB += _invMassB * P;
                    wB += _invIb * MathUtils.Cross(_rB, P);
                }
            }
            else
            {
                // Equal limits

                // Cdot = dot(u, v + cross(w, r))
                var vpA = vA + MathUtils.Cross(wA, _rA);
                var vpB = vB + MathUtils.Cross(wB, _rB);
                var Cdot = FVector2.Dot(_u, vpB - vpA);

                var impulse = -_mass * Cdot;
                _impulse += impulse;

                var P = impulse * _u;
                vA -= _invMassA * P;
                wA -= _invIa * MathUtils.Cross(_rA, P);
                vB += _invMassB * P;
                wB += _invIb * MathUtils.Cross(_rB, P);
            }

            data.Velocities[_indexA].V = vA;
            data.Velocities[_indexA].W = wA;
            data.Velocities[_indexB].V = vB;
            data.Velocities[_indexB].W = wB;
        }

        internal override bool SolvePositionConstraints(in SolverData data)
        {
            var cA = data.Positions[_indexA].Center;
            var aA = data.Positions[_indexA].Angle;
            var cB = data.Positions[_indexB].Center;
            var aB = data.Positions[_indexB].Angle;

            var qA = new Rotation(aA);
            var qB = new Rotation(aB);

            var rA = MathUtils.Mul(qA, _localAnchorA - _localCenterA);
            var rB = MathUtils.Mul(qB, _localAnchorB - _localCenterB);
            var u = cB + rB - cA - rA;

            var length = u.Normalize();
            FP C = FP.Zero;
            if (FP.Abs(_minLength - _maxLength) < Settings.Epsilon)
            {
                C = length - _minLength;
            }
            else if (length < _minLength)
            {
                C = length - _minLength;
            }
            else if (_maxLength < length)
            {
                C = length - _maxLength;
            }
            else
            {
                return true;
            }

            var impulse = -_mass * C;
            var P = impulse * u;

            cA -= _invMassA * P;
            aA -= _invIa * MathUtils.Cross(rA, P);
            cB += _invMassB * P;
            aB += _invIb * MathUtils.Cross(rB, P);

            data.Positions[_indexA].Center = cA;
            data.Positions[_indexA].Angle = aA;
            data.Positions[_indexB].Center = cB;
            data.Positions[_indexB].Angle = aB;

            return FP.Abs(C) < Settings.LinearSlop;
        }

        /// <inheritdoc />
        public override void Draw(IDrawer drawer)
        {
            var xfA = BodyA.GetTransform();
            var xfB = BodyB.GetTransform();
            var pA = MathUtils.Mul(xfA, _localAnchorA);
            var pB = MathUtils.Mul(xfB, _localAnchorB);

            var axis = pB - pA;
            axis.Normalize();

            var c1 = Color.FromArgb(0.7f, 0.7f, 0.7f);
            var c2 = Color.FromArgb(0.3f, 0.9f, 0.3f);
            var c3 = Color.FromArgb(0.9f, 0.3f, 0.3f);
            var c4 = Color.FromArgb(0.4f, 0.4f, 0.4f);

            drawer.DrawSegment(pA, pB, c4);

            var pRest = pA + _length * axis;
            drawer.DrawPoint(pRest, 8.0f, c1);

            if (FP.Abs(_minLength - _maxLength) > Settings.Epsilon)
            {
                if (_minLength > Settings.LinearSlop)
                {
                    var pMin = pA + _minLength * axis;
                    drawer.DrawPoint(pMin, 4.0f, c2);
                }

                if (_maxLength < Settings.MaxFloat)
                {
                    var pMax = pA + _maxLength * axis;
                    drawer.DrawPoint(pMax, 4.0f, c3);
                }
            }
        }
    }
}