using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using sistdev.GameBase;
using sistdev.Helpers;

namespace sistdev.GameLogic
{
    public class Mosquito : FlyingUnit
    {
        public enum MosquitoAnimations
        {
            Fly = 0,
            Die
        }

        public enum MosquitoState
        {
            Flying = 0,
            Dead
        }

        static float DISTANCE_EPSILON = 1.0f;
        static float LINEAR_VELOCITY_CONSTANT = 15.0f;
        static float ANGULAR_VELOCITY_CONSTANT = 60.0f;

        MosquitoState state;
        float nextActionTime;
        long timerDirection;

        UnitTypes.MosquitoType mosquitoType;

        #region Properties
        public MosquitoAnimations CurrentAnimation
        {
            get
            {
                return (MosquitoAnimations)CurrentAnimationId;
            }
        }

        public MosquitoState State
        {
            get
            {
                return state;
            }
            set
            {
                state = value;
            }
        }

        public override Transformation Transformation
        {
            get
            {
                return AnimatedModel.Transformation;
            }
            set
            {
                base.Transformation = value;
            }
        }
        #endregion

        public Mosquito(Game game, UnitTypes.MosquitoType mosquitoType)
            : base(game)
        {
            this.mosquitoType = mosquitoType;

            rnd = new Random(DateTime.Now.Second);
            timerDirection = 0;
            stopChangingDirection = false;

            LinearVelocity = Vector3.Zero;
            AngularVelocity = Vector3.Zero;
            direction = Vector3.Zero;
        }

        protected override void LoadContent()
        {
            Load(UnitTypes.MosquitoModelFileName[(int)mosquitoType]);

            // Unit configurations
            Life = UnitTypes.EnemyLife[(int)mosquitoType];
            MaxLife = Life;
            Speed = UnitTypes.EnemySpeed[(int)mosquitoType];

            SetAnimation(MosquitoAnimations.Fly, false, true, false);

            base.LoadContent();
        }

        public override void ReceiveDamage(int damageValue)
        {
            base.ReceiveDamage(damageValue);

            if (Life > 0)
            {
                
            }
            else
            {
                state = MosquitoState.Dead;
                SetAnimation(MosquitoAnimations.Die, false, false, false);
            }
        }

        public void SetAnimation(MosquitoAnimations animation, bool reset, bool enableLoop, bool waitFinish)
        {
            SetAnimation((int)animation, reset, enableLoop, waitFinish);
        }

        private void Move(Vector3 direction)
        {
            SetAnimation(MosquitoAnimations.Fly, false, true, (CurrentAnimation == MosquitoAnimations.Fly));
            LinearVelocity = direction * LINEAR_VELOCITY_CONSTANT;

            // Angle between heading and move direction
            float radianAngle = (float)Math.Acos(Vector3.Dot(HeadingVector, direction));
            if (radianAngle >= 0.1f)
            {
                // Find short side to rodade CW or CCW
                float sideToRotate = Vector3.Dot(StrafeVector, direction);

                Vector3 rotationVector = new Vector3(0, ANGULAR_VELOCITY_CONSTANT * radianAngle, 0);
                if (sideToRotate > 0)
                    AngularVelocity = -rotationVector;
                else
                    AngularVelocity = rotationVector;
            }
        }

        public override void Update(GameTime time)
        {
            if ((rnd.Next(50) == 23) && !stopChangingDirection)
            {
                //direction = new Vector3(rnd.Next(6), rnd.Next(6), rnd.Next(6));
                Vector3 vec = Vector3.One;

                if (rnd.Next(50) == 27)
                {
                    vec = Transformation.Translate;
                    vec.Normalize();
                    vec *= -1.0f;
                    stopChangingDirection = true;
                }

                direction = new Vector3(vec.X * rnd.Next(6), vec.Y * rnd.Next(6), vec.Z * rnd.Next(6));
            }
            if (stopChangingDirection)
            {
                if (timerDirection++ > 100)
                {
                    stopChangingDirection = false;
                }
            }

            Move(direction);

            base.Update(time);
        }

        public override void Draw(GameTime time)
        {
            base.Draw(time);
        }
    }
}
