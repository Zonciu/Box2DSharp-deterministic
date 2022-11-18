using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Box2DSharp.Collision.Shapes;
using Box2DSharp.Common;
using Box2DSharp.Dynamics.Contacts;
using Box2DSharp.Dynamics.Joints;

namespace Box2DSharp.Dynamics
{
    /// The body type.
    /// static: zero mass, zero velocity, may be manually moved
    /// kinematic: zero mass, non-zero velocity set by user, moved by solver
    /// dynamic: positive mass, non-zero velocity determined by forces, moved by solver
    public enum BodyType
    {
        StaticBody = 0,

        KinematicBody = 1,

        DynamicBody = 2
    }

    /// A body definition holds all the data needed to construct a rigid body.
    /// You can safely re-use body definitions. Shapes are added to a body after construction.
    public struct BodyDef
    {
        private bool? _enabled;

        /// Does this body start out enabled?
        public bool Enabled
        {
            get => _enabled ?? true;
            set => _enabled = value;
        }

        private bool? _allowSleep;

        /// Set this flag to false if this body should never fall asleep. Note that
        /// this increases CPU usage.
        public bool AllowSleep
        {
            get => _allowSleep ?? true;
            set => _allowSleep = value;
        }

        /// The world angle of the body in radians.
        public FP Angle;

        /// Angular damping is use to reduce the angular velocity. The damping parameter
        /// can be larger than 1.0f but the damping effect becomes sensitive to the
        /// time step when the damping parameter is large.
        /// Units are 1/time
        public FP AngularDamping;

        /// The angular velocity of the body.
        public FP AngularVelocity;

        private bool? _awake;

        /// Is this body initially awake or sleeping?
        public bool Awake
        {
            get => _awake ?? true;
            set => _awake = value;
        }

        /// The body type: static, kinematic, or dynamic.
        /// Note: if a dynamic body would have zero mass, the mass is set to one.
        public BodyType BodyType;

        /// Is this a fast moving body that should be prevented from tunneling through
        /// other moving bodies? Note that all bodies are prevented from tunneling through
        /// kinematic and static bodies. This setting is only considered on dynamic bodies.
        /// @warning You should use this flag sparingly since it increases processing time.
        public bool Bullet;

        /// Should this body be prevented from rotating? Useful for characters.
        public bool FixedRotation;

        private FP? _gravityScale;

        /// Scale the gravity applied to this body.
        public FP GravityScale
        {
            get => _gravityScale ?? 1;
            set => _gravityScale = value;
        }

        /// Linear damping is use to reduce the linear velocity. The damping parameter
        /// can be larger than 1.0f but the damping effect becomes sensitive to the
        /// time step when the damping parameter is large.
        /// Units are 1/time
        public FP LinearDamping;

        /// The linear velocity of the body's origin in world co-ordinates.
        public FVector2 LinearVelocity;

        /// The world position of the body. Avoid creating bodies at the origin
        /// since this can lead to many overlapping shapes.
        public FVector2 Position;

        /// Use this to store application specific body data.
        public object UserData;
    }

    /// A rigid body. These are created via b2World::CreateBody.
    public class Body : IDisposable
    {
        /// <summary>
        /// 接触边缘列表
        /// </summary>
        internal readonly LinkedList<ContactEdge> ContactEdges;

        /// <summary>
        /// 夹具列表
        /// </summary>
        public IReadOnlyList<Fixture> FixtureList => Fixtures;

        /// <summary>
        /// 夹具列表
        /// </summary>
        internal readonly List<Fixture> Fixtures;

        /// <summary>
        /// 关节边缘列表
        /// </summary>
        internal readonly LinkedList<JointEdge> JointEdges;

        /// <summary>
        /// Get/Set the angular damping of the body.
        /// 角阻尼
        /// </summary>
        private FP _angularDamping;

        /// <summary>
        /// 质心的转动惯量
        /// </summary>
        private FP _inertia;

        /// <summary>
        /// 线性阻尼
        /// </summary>
        private FP _linearDamping;

        /// <summary>
        /// Get the total mass of the body.
        /// @return the mass, usually in kilograms (kg).
        /// 质量
        /// </summary>
        private FP _mass;

        /// <summary>
        /// 物体类型
        /// </summary>
        private BodyType _type;

        /// <summary>
        /// 所属世界
        /// </summary>
        internal World _world;

        /// <summary>
        /// 物体标志
        /// </summary>
        internal BodyFlags Flags;

        /// <summary>
        /// 受力
        /// </summary>
        internal FVector2 Force;

        /// <summary>
        /// 重力系数
        /// </summary>
        internal FP GravityScale;

        /// <summary>
        /// 质心的转动惯量倒数
        /// </summary>
        internal FP InverseInertia;

        /// <summary>
        /// 质量倒数
        /// </summary>
        internal FP InvMass;

        /// <summary>
        /// 岛屿索引
        /// </summary>
        internal int IslandIndex;

        /// <summary>
        /// 链表节点物体
        /// </summary>
        internal LinkedListNode<Body> Node;

        /// <summary>
        /// 扫描
        /// </summary>
        internal Sweep Sweep; // the swept motion for CCD

        /// <summary>
        /// 扭矩
        /// </summary>
        internal FP Torque;

        /// <summary>
        /// 物体位置
        /// </summary>
        internal Transform Transform; // the body origin transform

        internal Body(in BodyDef def, World world)
        {
            Debug.Assert(def.AngularDamping >= 0.0f);
            Debug.Assert(def.LinearDamping >= 0.0f);

            Flags = 0;

            if (def.Bullet)
            {
                Flags |= BodyFlags.IsBullet;
            }

            if (def.FixedRotation)
            {
                Flags |= BodyFlags.FixedRotation;
            }

            if (def.AllowSleep)
            {
                Flags |= BodyFlags.AutoSleep;
            }

            if (def.Awake && def.BodyType != BodyType.StaticBody)
            {
                Flags |= BodyFlags.IsAwake;
            }

            if (def.Enabled)
            {
                Flags |= BodyFlags.IsEnabled;
            }

            _world = world;

            Transform.Position = def.Position;
            Transform.Rotation.Set(def.Angle);

            Sweep = new Sweep
            {
                LocalCenter = FVector2.Zero,
                C0 = Transform.Position,
                C = Transform.Position,
                A0 = def.Angle,
                A = def.Angle,
                Alpha0 = 0.0f
            };

            JointEdges = new LinkedList<JointEdge>();
            ContactEdges = new LinkedList<ContactEdge>();
            Fixtures = new List<Fixture>();
            Node = null;

            LinearVelocity = def.LinearVelocity;
            AngularVelocity = def.AngularVelocity;

            _linearDamping = def.LinearDamping;
            AngularDamping = def.AngularDamping;
            GravityScale = def.GravityScale;

            Force.SetZero();
            Torque = 0.0f;

            SleepTime = 0.0f;

            _type = def.BodyType;

            _mass = 0.0f;
            InvMass = 0.0f;

            _inertia = 0.0f;
            InverseInertia = 0.0f;

            UserData = def.UserData;
        }

        public FP AngularDamping
        {
            get => _angularDamping;
            set => _angularDamping = value;
        }

        /// <summary>
        /// Get/Set the angular velocity.
        /// the new angular velocity in radians/second.
        /// 角速度
        /// </summary>
        public FP AngularVelocity { get; internal set; }

        /// Get the rotational inertia of the body about the local origin.
        /// @return the rotational inertia, usually in kg-m^2.
        public FP Inertia => _inertia + _mass * FVector2.Dot(Sweep.LocalCenter, Sweep.LocalCenter);

        /// Get/Set the linear damping of the body.
        public FP LinearDamping
        {
            get => _linearDamping;
            set => _linearDamping = value;
        }

        /// <summary>
        /// 线速度
        /// </summary>
        /// Set the linear velocity of the center of mass.
        /// @param v the new linear velocity of the center of mass.
        /// Get the linear velocity of the center of mass.
        /// @return the linear velocity of the center of mass.
        public FVector2 LinearVelocity { get; internal set; }

        public FP Mass => _mass;

        /// <summary>
        /// 休眠时间
        /// </summary>
        internal FP SleepTime { get; set; }

        /// Set the type of this body. This may alter the mass and velocity.
        public BodyType BodyType
        {
            get => _type;
            set
            {
                Debug.Assert(_world.IsLocked == false);
                if (_world.IsLocked)
                {
                    return;
                }

                if (_type == value)
                {
                    return;
                }

                _type = value;

                ResetMassData();

                if (_type == BodyType.StaticBody)
                {
                    LinearVelocity = FVector2.Zero;
                    AngularVelocity = 0.0f;
                    Sweep.A0 = Sweep.A;
                    Sweep.C0 = Sweep.C;
                    UnsetFlag(BodyFlags.IsAwake);
                    SynchronizeFixtures();
                }

                IsAwake = true;

                Force.SetZero();
                Torque = 0.0f;

                // Delete the attached contacts.
                // 删除所有接触点

                var node = ContactEdges.First;
                while (node != null)
                {
                    var c = node.Value;
                    node = node.Next;
                    _world.ContactManager.Destroy(c.Contact);
                }

                ContactEdges.Clear();

                // Touch the proxies so that new contacts will be created (when appropriate)
                var broadPhase = _world.ContactManager.BroadPhase;
                foreach (var f in Fixtures)
                {
                    var proxyCount = f.ProxyCount;
                    for (var i = 0; i < proxyCount; ++i)
                    {
                        broadPhase.TouchProxy(f.Proxies[i].ProxyId);
                    }
                }
            }
        }

        /// Should this body be treated like a bullet for continuous collision detection?
        /// Is this body treated like a bullet for continuous collision detection?
        public bool IsBullet
        {
            get => Flags.HasSetFlag(BodyFlags.IsBullet);
            set
            {
                if (value)
                {
                    Flags |= BodyFlags.IsBullet;
                }
                else
                {
                    Flags &= ~BodyFlags.IsBullet;
                }
            }
        }

        /// You can disable sleeping on this body. If you disable sleeping, the
        /// body will be woken.
        /// Is this body allowed to sleep
        public bool IsSleepingAllowed
        {
            get => Flags.HasSetFlag(BodyFlags.AutoSleep);
            set
            {
                if (value)
                {
                    Flags |= BodyFlags.AutoSleep;
                }
                else
                {
                    Flags &= ~BodyFlags.AutoSleep;
                    IsAwake = true;
                }
            }
        }

        /// <summary>
        /// Set the sleep state of the body. A sleeping body has very
        /// low CPU cost.
        /// @param flag set to true to wake the body, false to put it to sleep.
        /// Get the sleeping state of this body.
        /// @return true if the body is awake.
        /// </summary>
        public bool IsAwake
        {
            get => Flags.HasSetFlag(BodyFlags.IsAwake);
            set
            {
                if (BodyType == BodyType.StaticBody)
                {
                    return;
                }

                if (value)
                {
                    Flags |= BodyFlags.IsAwake;
                    SleepTime = 0.0f;
                }
                else
                {
                    Flags &= ~BodyFlags.IsAwake;
                    SleepTime = 0.0f;
                    LinearVelocity = FVector2.Zero;
                    AngularVelocity = 0.0f;
                    Force.SetZero();
                    Torque = 0.0f;
                }
            }
        }

        /// <summary>
        /// Set the active state of the body. An inactive body is not
        /// simulated and cannot be collided with or woken up.
        /// If you pass a flag of true, all fixtures will be added to the
        /// broad-phase.
        /// If you pass a flag of false, all fixtures will be removed from
        /// the broad-phase and all contacts will be destroyed.
        /// Fixtures and joints are otherwise unaffected. You may continue
        /// to create/destroy fixtures and joints on inactive bodies.
        /// Fixtures on an inactive body are implicitly inactive and will
        /// not participate in collisions, ray-casts, or queries.
        /// Joints connected to an inactive body are implicitly inactive.
        /// An inactive body is still owned by a b2World object and remains
        /// in the body list.
        /// Get the active state of the body.
        /// </summary>
        public bool IsEnabled

        {
            get => Flags.HasSetFlag(BodyFlags.IsEnabled);
            set
            {
                Debug.Assert(_world.IsLocked == false);

                if (value == IsEnabled)
                {
                    return;
                }

                if (value)
                {
                    Flags |= BodyFlags.IsEnabled;

                    // Create all proxies.
                    // 激活时创建粗检测代理
                    var broadPhase = _world.ContactManager.BroadPhase;
                    foreach (var f in Fixtures)
                    {
                        f.CreateProxies(broadPhase, Transform);
                    }

                    // Contacts are created at the beginning of the next
                    World.HasNewContacts = true;
                }
                else
                {
                    Flags &= ~BodyFlags.IsEnabled;

                    // Destroy all proxies.
                    // 休眠时销毁粗检测代理
                    var broadPhase = _world.ContactManager.BroadPhase;
                    foreach (var f in Fixtures)
                    {
                        f.DestroyProxies(broadPhase);
                    }

                    // Destroy the attached contacts.
                    // 销毁接触点
                    var node = ContactEdges.First;
                    while (node != null)
                    {
                        var c = node.Value;
                        node = node.Next;
                        _world.ContactManager.Destroy(c.Contact);
                    }

                    ContactEdges.Clear();
                }
            }
        }

        /// Set this body to have fixed rotation. This causes the mass
        /// to be reset.
        public bool IsFixedRotation
        {
            get => Flags.HasSetFlag(BodyFlags.FixedRotation);
            set
            {
                // 物体已经有固定旋转,不需要设置
                if (Flags.HasSetFlag(BodyFlags.FixedRotation) && value)
                {
                    return;
                }

                if (value)
                {
                    Flags |= BodyFlags.FixedRotation;
                }
                else
                {
                    Flags &= ~BodyFlags.FixedRotation;
                }

                AngularVelocity = 0.0f;

                ResetMassData();
            }
        }

        /// <summary>
        /// Get/Set the user data pointer that was provided in the body definition.
        /// 用户信息
        /// </summary>
        public object UserData { get; set; }

        /// Get the parent world of this body.
        public World World => _world;

        /// <inheritdoc />
        public void Dispose()
        {
            _world = null;
            Debug.Assert(ContactEdges.Count == 0, "ContactEdges.Count == 0");
            Debug.Assert(JointEdges.Count == 0, "JointEdges.Count == 0");
            ContactEdges?.Clear();
            JointEdges?.Clear();
            Fixtures?.Clear();
            GC.SuppressFinalize(this);
        }

        public void SetAngularVelocity(FP value)
        {
            if (_type == BodyType.StaticBody) // 静态物体无角速度
            {
                return;
            }

            if (value * value > 0.0f)
            {
                IsAwake = true;
            }

            AngularVelocity = value;
        }

        public void SetLinearVelocity(in FVector2 value)
        {
            if (_type == BodyType.StaticBody) // 静态物体无加速度
            {
                return;
            }

            if (FVector2.Dot(value, value) > 0.0f) // 点积大于0时唤醒本物体
            {
                IsAwake = true;
            }

            LinearVelocity = value;
        }

        /// <summary>
        /// Creates a fixture and attach it to this body. Use this function if you need
        /// to set some fixture parameters, like friction. Otherwise you can create the
        /// fixture directly from a shape.
        /// If the density is non-zero, this function automatically updates the mass of the body.
        /// Contacts are not created until the next time step.
        /// @param def the fixture definition.
        /// @warning This function is locked during callbacks.
        /// 创建夹具
        /// </summary>
        /// <param name="def"></param>
        /// <returns></returns>
        public Fixture CreateFixture(FixtureDef def)
        {
            Debug.Assert(_world.IsLocked == false);
            if (_world.IsLocked)
            {
                return null;
            }

            var fixture = Fixture.Create(this, def);

            if (Flags.HasSetFlag(BodyFlags.IsEnabled))
            {
                var broadPhase = _world.ContactManager.BroadPhase;
                fixture.CreateProxies(broadPhase, Transform);
            }

            fixture.Body = this;
            Fixtures.Add(fixture);

            // Adjust mass properties if needed.
            if (fixture.Density > 0.0f)
            {
                ResetMassData();
            }

            // Let the world know we have a new fixture. This will cause new contacts
            // to be created at the beginning of the next time step.
            // 通知世界存在新增夹具,在下一个时间步中将自动创建新夹具的接触点
            _world.HasNewContacts = true;

            return fixture;
        }

        /// Creates a fixture from a shape and attach it to this body.
        /// This is a convenience function. Use b2FixtureDef if you need to set parameters
        /// like friction, restitution, user data, or filtering.
        /// If the density is non-zero, this function automatically updates the mass of the body.
        /// @param shape the shape to be cloned.
        /// @param density the shape density (set to zero for static bodies).
        /// @warning This function is locked during callbacks.
        /// 创建夹具
        public Fixture CreateFixture(Shape shape, FP density)
        {
            var def = new FixtureDef
            {
                Shape = shape,
                Density = density
            };

            return CreateFixture(def);
        }

        /// Destroy a fixture. This removes the fixture from the broad-phase and
        /// destroys all contacts associated with this fixture. This will
        /// automatically adjust the mass of the body if the body is dynamic and the
        /// fixture has positive density.
        /// All fixtures attached to a body are implicitly destroyed when the body is destroyed.
        /// @param fixture the fixture to be removed.
        /// @warning This function is locked during callbacks.
        /// 删除夹具
        public void DestroyFixture(Fixture fixture)
        {
            if (fixture == default)
            {
                return;
            }

            // 世界锁定时不能删除夹具
            Debug.Assert(_world.IsLocked == false);
            if (_world.IsLocked)
            {
                return;
            }

            // 断言夹具所属物体
            Debug.Assert(fixture.Body == this);

            // Remove the fixture from this body's singly linked list.
            Debug.Assert(Fixtures.Count > 0);

            // You tried to remove a shape that is not attached to this body.
            // 确定该夹具存在于物体的夹具列表中
            Debug.Assert(Fixtures.Any(e => e == fixture));
            var density = fixture.Density;

            // Destroy any contacts associated with the fixture.
            // 销毁关联在夹具上的接触点
            var node = ContactEdges.First;
            while (node != null)
            {
                var contactEdge = node.Value;
                node = node.Next;
                if (contactEdge.Contact.FixtureA == fixture || contactEdge.Contact.FixtureB == fixture)
                {
                    // This destroys the contact and removes it from
                    // this body's contact list.
                    _world.ContactManager.Destroy(contactEdge.Contact);
                }
            }

            // 如果物体处于活跃状态,销毁夹具的粗检测代理对象
            if (Flags.HasSetFlag(BodyFlags.IsEnabled))
            {
                var broadPhase = _world.ContactManager.BroadPhase;
                fixture.DestroyProxies(broadPhase);
            }

            Fixtures.Remove(fixture);
            fixture.Body = null;
            Fixture.Destroy(fixture);

            // Reset the mass data.
            // 夹具销毁后重新计算物体质量
            if (density > FP.Zero)
            {
                ResetMassData();
            }
        }

        /// Set the position of the body's origin and rotation.
        /// Manipulating a body's transform may cause non-physical behavior.
        /// Note: contacts are updated on the next call to b2World::Step.
        /// @param position the world position of the body's local origin.
        /// @param angle the world rotation in radians.
        public void SetTransform(in FVector2 position, FP angle)
        {
            Debug.Assert(_world.IsLocked == false);
            if (_world.IsLocked)
            {
                return;
            }

            Transform.Rotation.Set(angle);
            Transform.Position = position;

            Sweep.C = MathUtils.Mul(Transform, Sweep.LocalCenter);
            Sweep.A = angle;

            Sweep.C0 = Sweep.C;
            Sweep.A0 = angle;

            var broadPhase = _world.ContactManager.BroadPhase;
            foreach (var f in Fixtures)
            {
                f.Synchronize(broadPhase, Transform, Transform);
            }

            // Check for new contacts the next step
            World.HasNewContacts = true;
        }

        /// Get the body transform for the body's origin.
        /// @return the world transform of the body's origin.
        public Transform GetTransform()
        {
            return Transform;
        }

        /// Get the world body origin position.
        /// @return the world position of the body's origin.
        public FVector2 GetPosition()
        {
            return Transform.Position;
        }

        /// Get the angle in radians.
        /// @return the current world rotation angle in radians.
        public FP GetAngle()
        {
            return Sweep.A;
        }

        /// Get the world position of the center of mass.
        public FVector2 GetWorldCenter()
        {
            return Sweep.C;
        }

        /// Get the local position of the center of mass.
        public FVector2 GetLocalCenter()
        {
            return Sweep.LocalCenter;
        }

        /// <summary>
        /// Apply a force at a world point. If the force is not
        /// applied at the center of mass, it will generate a torque and
        /// affect the angular velocity. This wakes up the body.
        /// @param force the world force vector, usually in Newtons (N).
        /// @param point the world position of the point of application.
        /// @param wake also wake up the body
        /// 在指定位置施加作用力
        /// </summary>
        /// <param name="force"></param>
        /// <param name="point"></param>
        /// <param name="wake"></param>
        public void ApplyForce(in FVector2 force, in FVector2 point, bool wake)
        {
            if (_type != BodyType.DynamicBody)
            {
                return;
            }

            if (wake && !Flags.HasSetFlag(BodyFlags.IsAwake))
            {
                IsAwake = true;
            }

            // Don't accumulate a force if the body is sleeping.
            if (Flags.HasSetFlag(BodyFlags.IsAwake))
            {
                Force += force;
                Torque += MathUtils.Cross(point - Sweep.C, force);
            }
        }

        /// <summary>
        /// Apply a force to the center of mass. This wakes up the body.
        /// @param force the world force vector, usually in Newtons (N).
        /// @param wake also wake up the body
        /// 在质心施加作用力
        /// </summary>
        /// <param name="force"></param>
        /// <param name="wake"></param>
        public void ApplyForceToCenter(in FVector2 force, bool wake)
        {
            if (_type != BodyType.DynamicBody)
            {
                return;
            }

            if (wake && !Flags.HasSetFlag(BodyFlags.IsAwake))
            {
                IsAwake = true;
            }

            // Don't accumulate a force if the body is sleeping
            if (Flags.HasSetFlag(BodyFlags.IsAwake))
            {
                Force += force;
            }
        }

        /// <summary>
        /// Apply a torque. This affects the angular velocity
        /// without affecting the linear velocity of the center of mass.
        /// @param torque about the z-axis (out of the screen), usually in N-m.
        /// @param wake also wake up the body
        /// 施加扭矩
        /// </summary>
        /// <param name="torque"></param>
        /// <param name="wake"></param>
        public void ApplyTorque(FP torque, bool wake)
        {
            if (_type != BodyType.DynamicBody)
            {
                return;
            }

            if (wake && !Flags.HasSetFlag(BodyFlags.IsAwake))
            {
                IsAwake = true;
            }

            // Don't accumulate a force if the body is sleeping
            if (Flags.HasSetFlag(BodyFlags.IsAwake))
            {
                Torque += torque;
            }
        }

        /// <summary>
        /// Apply an impulse at a point. This immediately modifies the velocity.
        /// It also modifies the angular velocity if the point of application
        /// is not at the center of mass. This wakes up the body.
        /// @param impulse the world impulse vector, usually in N-seconds or kg-m/s.
        /// @param point the world position of the point of application.
        /// @param wake also wake up the body
        /// 在物体指定位置施加线性冲量
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="point"></param>
        /// <param name="wake"></param>
        public void ApplyLinearImpulse(in FVector2 impulse, in FVector2 point, bool wake)
        {
            if (_type != BodyType.DynamicBody)
            {
                return;
            }

            if (wake && !Flags.HasSetFlag(BodyFlags.IsAwake))
            {
                IsAwake = true;
            }

            // Don't accumulate velocity if the body is sleeping
            if (Flags.HasSetFlag(BodyFlags.IsAwake))
            {
                LinearVelocity += InvMass * impulse;
                AngularVelocity += InverseInertia * MathUtils.Cross(point - Sweep.C, impulse);
            }
        }

        /// <summary>
        /// Apply an impulse to the center of mass. This immediately modifies the velocity.
        /// @param impulse the world impulse vector, usually in N-seconds or kg-m/s.
        /// @param wake also wake up the body
        /// 在质心施加线性冲量
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="wake"></param>
        public void ApplyLinearImpulseToCenter(in FVector2 impulse, bool wake)
        {
            if (_type != BodyType.DynamicBody)
            {
                return;
            }

            if (wake && !Flags.HasSetFlag(BodyFlags.IsAwake))
            {
                IsAwake = true;
            }

            // Don't accumulate velocity if the body is sleeping
            if (Flags.HasSetFlag(BodyFlags.IsAwake))
            {
                LinearVelocity += InvMass * impulse;
            }
        }

        /// <summary>
        /// Apply an angular impulse.
        /// @param impulse the angular impulse in units of kg*m*m/s
        /// @param wake also wake up the body
        /// 施加角冲量
        /// </summary>
        /// <param name="impulse"></param>
        /// <param name="wake"></param>
        public void ApplyAngularImpulse(FP impulse, bool wake)
        {
            if (_type != BodyType.DynamicBody)
            {
                return;
            }

            if (wake && !Flags.HasSetFlag(BodyFlags.IsAwake))
            {
                IsAwake = true;
            }

            // Don't accumulate velocity if the body is sleeping
            if ((Flags & BodyFlags.IsAwake) != 0)
            {
                AngularVelocity += InverseInertia * impulse;
            }
        }

        /// Get the mass data of the body.
        /// @return a struct containing the mass, inertia and center of the body.
        public MassData GetMassData()
        {
            return new MassData
            {
                Mass = _mass,
                RotationInertia = _inertia + _mass * FVector2.Dot(Sweep.LocalCenter, Sweep.LocalCenter),
                Center = Sweep.LocalCenter
            };
        }

        /// Set the mass properties to override the mass properties of the fixtures.
        /// Note that this changes the center of mass position.
        /// Note that creating or destroying fixtures can also alter the mass.
        /// This function has no effect if the body isn't dynamic.
        /// @param massData the mass properties.
        public void SetMassData(in MassData massData)
        {
            Debug.Assert(_world.IsLocked == false);
            if (_world.IsLocked)
            {
                return;
            }

            if (_type != BodyType.DynamicBody)
            {
                return;
            }

            InvMass = 0.0f;
            _inertia = 0.0f;
            InverseInertia = 0.0f;

            _mass = massData.Mass;
            if (_mass <= 0.0f)
            {
                _mass = 1.0f;
            }

            InvMass = 1.0f / _mass;

            if (massData.RotationInertia > 0.0f && !Flags.HasSetFlag(BodyFlags.FixedRotation)) // 存在转动惯量且物体可旋转
            {
                _inertia = massData.RotationInertia - _mass * FVector2.Dot(massData.Center, massData.Center);
                Debug.Assert(_inertia > 0.0f);
                InverseInertia = 1.0f / _inertia;
            }

            // Move center of mass.
            var oldCenter = Sweep.C;
            Sweep.LocalCenter = massData.Center;
            Sweep.C0 = Sweep.C = MathUtils.Mul(Transform, Sweep.LocalCenter);

            // Update center of mass velocity.
            LinearVelocity += MathUtils.Cross(AngularVelocity, Sweep.C - oldCenter);
        }

        /// This resets the mass properties to the sum of the mass properties of the fixtures.
        /// This normally does not need to be called unless you called SetMassData to override
        /// the mass and you later want to reset the mass.
        /// 重置质量数据
        private void ResetMassData()
        {
            // Compute mass data from shapes. Each shape has its own density.
            // 从所有形状计算质量数据,每个形状都有各自的密度
            _mass = 0.0f;
            InvMass = 0.0f;
            _inertia = 0.0f;
            InverseInertia = 0.0f;
            Sweep.LocalCenter.SetZero();

            // Static and kinematic bodies have zero mass.
            if (_type == BodyType.StaticBody || _type == BodyType.KinematicBody)
            {
                Sweep.C0 = Transform.Position;
                Sweep.C = Transform.Position;
                Sweep.A0 = Sweep.A;
                return;
            }

            Debug.Assert(_type == BodyType.DynamicBody);

            // Accumulate mass over all fixtures.
            var localCenter = FVector2.Zero;
            foreach (var f in Fixtures)
            {
                if (f.Density.Equals(0.0f))
                {
                    continue;
                }

                f.GetMassData(out var massData);
                _mass += massData.Mass;
                localCenter += massData.Mass * massData.Center;
                _inertia += massData.RotationInertia;
            }

            // Compute center of mass.
            if (_mass > 0.0f)
            {
                InvMass = 1.0f / _mass;
                localCenter *= InvMass;
            }

            if (_inertia > 0.0f && !Flags.HasSetFlag(BodyFlags.FixedRotation)) // 存在转动惯量且物体可旋转
            {
                // Center the inertia about the center of mass.
                _inertia -= _mass * FVector2.Dot(localCenter, localCenter);
                Debug.Assert(_inertia > 0.0f);
                InverseInertia = 1.0f / _inertia;
            }
            else
            {
                _inertia = 0.0f;
                InverseInertia = 0.0f;
            }

            // Move center of mass.
            var oldCenter = Sweep.C;
            Sweep.LocalCenter = localCenter;
            Sweep.C0 = Sweep.C = MathUtils.Mul(Transform, Sweep.LocalCenter);

            // Update center of mass velocity.
            LinearVelocity += MathUtils.Cross(AngularVelocity, Sweep.C - oldCenter);
        }

        /// Get the world coordinates of a point given the local coordinates.
        /// @param localPoint a point on the body measured relative the the body's origin.
        /// @return the same point expressed in world coordinates.
        public FVector2 GetWorldPoint(in FVector2 localPoint)
        {
            return MathUtils.Mul(Transform, localPoint);
        }

        /// Get the world coordinates of a vector given the local coordinates.
        /// @param localVector a vector fixed in the body.
        /// @return the same vector expressed in world coordinates.
        public FVector2 GetWorldVector(in FVector2 localVector)
        {
            return MathUtils.Mul(Transform.Rotation, localVector);
        }

        /// Gets a local point relative to the body's origin given a world point.
        /// @param a point in world coordinates.
        /// @return the corresponding local point relative to the body's origin.
        public FVector2 GetLocalPoint(in FVector2 worldPoint)
        {
            return MathUtils.MulT(Transform, worldPoint);
        }

        /// Gets a local vector given a world vector.
        /// @param a vector in world coordinates.
        /// @return the corresponding local vector.
        public FVector2 GetLocalVector(in FVector2 worldVector)
        {
            return MathUtils.MulT(Transform.Rotation, worldVector);
        }

        /// Get the world linear velocity of a world point attached to this body.
        /// @param a point in world coordinates.
        /// @return the world velocity of a point.
        public FVector2 GetLinearVelocityFromWorldPoint(in FVector2 worldPoint)
        {
            return LinearVelocity + MathUtils.Cross(AngularVelocity, worldPoint - Sweep.C);
        }

        /// Get the world velocity of a local point.
        /// @param a point in local coordinates.
        /// @return the world velocity of a point.
        public FVector2 GetLinearVelocityFromLocalPoint(in FVector2 localPoint)
        {
            return GetLinearVelocityFromWorldPoint(GetWorldPoint(localPoint));
        }

        /// Dump this body to a log file
        public void Dump()
        {
            // Todo
        }

        /// <summary>
        /// 同步夹具
        /// </summary>
        internal void SynchronizeFixtures()
        {
            var broadPhase = World.ContactManager.BroadPhase;

            if (Flags.HasSetFlag(BodyFlags.IsAwake))
            {
                var xf1 = new Transform();
                xf1.Rotation.Set(Sweep.A0);
                xf1.Position = Sweep.C0 - MathUtils.Mul(xf1.Rotation, Sweep.LocalCenter);

                for (var index = 0; index < Fixtures.Count; index++)
                {
                    Fixtures[index].Synchronize(broadPhase, xf1, Transform);
                }
            }
            else
            {
                for (var index = 0; index < Fixtures.Count; index++)
                {
                    Fixtures[index].Synchronize(broadPhase, Transform, Transform);
                }
            }

            // var xf1 = new Transform();
            // xf1.Rotation.Set(Sweep.A0);
            // xf1.Position = Sweep.C0 - MathUtils.Mul(xf1.Rotation, Sweep.LocalCenter);
            //
            // var broadPhase = _world.ContactManager.BroadPhase;
            // for (var index = 0; index < Fixtures.Count; index++)
            // {
            //     Fixtures[index].Synchronize(broadPhase, xf1, Transform);
            // }
        }

        /// <summary>
        /// 同步位置
        /// </summary>
        internal void SynchronizeTransform()
        {
            Transform.Rotation.Set(Sweep.A);
            Transform.Position = Sweep.C - MathUtils.Mul(Transform.Rotation, Sweep.LocalCenter);
        }

        /// <summary>
        /// This is used to prevent connected bodies from colliding.
        /// It may lie, depending on the collideConnected flag.
        /// 判断物体之间是否应该检测碰撞
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        internal bool ShouldCollide(Body other)
        {
            // At least one body should be dynamic.
            if (_type != BodyType.DynamicBody && other._type != BodyType.DynamicBody)
            {
                return false;
            }

            // Does a joint prevent collision?
            var node = JointEdges.First;
            while (node != null)
            {
                var joint = node.Value;
                node = node.Next;
                if (joint.Other == other && joint.Joint.CollideConnected == false)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 在安全时间段内快进,此时不同步粗检测
        /// </summary>
        /// <param name="alpha"></param>
        internal void Advance(FP alpha)
        {
            // Advance to the new safe time. This doesn't sync the broad-phase.
            Sweep.Advance(alpha);
            Sweep.C = Sweep.C0;
            Sweep.A = Sweep.A0;
            Transform.Rotation.Set(Sweep.A);
            Transform.Position = Sweep.C - MathUtils.Mul(Transform.Rotation, Sweep.LocalCenter);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void SetFlag(BodyFlags flag)
        {
            Flags |= flag;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void UnsetFlag(BodyFlags flag)
        {
            Flags &= ~flag;
        }
    }
}