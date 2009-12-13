using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;

using sistdev.GameBase.Cameras;

namespace sistdev.GameLogic
{
    public class Player : TerrainUnit
    {
        static float MAX_WAIST_BONE_ROTATE = 0.50f;
        static int WAIST_BONE_ID = 2;
        static int RIGHT_HAND_BONE_ID = 15;

        public enum PlayerAnimations
        {
            Idle = 0,
            Run,
            Aim,
            Shoot
        }

        // Player type
        UnitTypes.PlayerType playerType;
        // Camera chase position
        Vector3[] chaseOffsetPosition;
        // Rotate torso bone
        float rotateWaistBone;
        float rotateWaistBoneVelocity;

        Boolean needUpdateChasePosition;

        #region Properties
        public PlayerAnimations CurrentAnimation
        {
            get
            {
                return (PlayerAnimations)CurrentAnimationId;
            }
        }

        public Vector3[] ChaseOffsetPosition
        {
            get
            {
                return chaseOffsetPosition;
            }
            set
            {
                chaseOffsetPosition = value;
            }
        }

        public float RotateWaist
        {
            get
            {
                return rotateWaistBone;
            }
            set
            {
                rotateWaistBone = value;

                // Rotate torso bone
                Matrix rotate = Matrix.CreateRotationZ(rotateWaistBone);
                // TEMP Changes comment
                //AnimatedModel.BonesTransform[WAIST_BONE_ID] = rotate;
            }
        }

        public float RotateWaistVelocity
        {
            get
            {
                return rotateWaistBoneVelocity;
            }
            set
            {
                rotateWaistBoneVelocity = value;
            }
        }
        #endregion

        public Player(Game game, UnitTypes.PlayerType playerType)
            : base(game)
        {
            this.playerType = playerType;
        }

        protected override void LoadContent()
        {
            Load(UnitTypes.PlayerModelFileName[(int)playerType]);

            // Unit configurations
            Life = UnitTypes.PlayerLife[(int)playerType];
            MaxLife = Life;
            Speed = UnitTypes.PlayerSpeed[(int)playerType];
            //Transformation. = Transformation.Rotate + new Vector3(0,110,0);
            Transformation.Rotate = Transformation.Rotate;// +new Vector3(0, 180, 0);
            needUpdateChasePosition = true;
            SetAnimation(Player.PlayerAnimations.Idle, false, true, false);

            base.LoadContent();
        }

        public void SetAnimation(PlayerAnimations animation, bool reset, bool enableLoop, bool waitFinish)
        {
            // TEMP Changes - only one animation
            SetAnimation(0*(int)animation, reset, enableLoop, waitFinish);
        }

        private void UpdateChasePosition()
        {
            ThirdPersonCamera camera = cameraManager.ActiveCamera as ThirdPersonCamera;
            if (camera != null)
            {
                // Get camera offset position for the active camera
                Vector3 cameraOffset = Vector3.Zero;
                if (chaseOffsetPosition != null)
                    cameraOffset = chaseOffsetPosition[cameraManager.ActiveCameraIndex];

                // Get the model center
                Vector3 center = BoundingSphere.Center;

                // Calculate chase position and direction
                camera.ChasePosition = center +
                    cameraOffset.X * StrafeVector +
                    cameraOffset.Y * UpVector +
                    cameraOffset.Z * HeadingVector;
                camera.ChaseDirection = HeadingVector;
            }
        }

        private void UpdateWaistBone(float elapsedTimeSeconds)
        {
            if (rotateWaistBoneVelocity != 0.0f)
            {
                rotateWaistBone += rotateWaistBoneVelocity * elapsedTimeSeconds;
                rotateWaistBone = MathHelper.Clamp(rotateWaistBone, -MAX_WAIST_BONE_ROTATE, MAX_WAIST_BONE_ROTATE);

                // Rotate torso bone
                Matrix rotate = Matrix.CreateRotationZ(rotateWaistBone);
                // TEMP Changes comment
                //AnimatedModel.BonesTransform[WAIST_BONE_ID] = rotate;
            }
        }

        public override void Update(GameTime time)
        {
            float elapsedTimeSeconds = (float)time.ElapsedGameTime.TotalSeconds;
            //UpdateWaistBone(elapsedTimeSeconds);

            //Transformation.Translate = new Vector3(123, 0, 123);
//            Transformation.Rotate = new Vector3(180, 0, 0);
            
            base.Update(time);

            /*if ((cameraManager.ActiveCamera as ThirdPersonCamera).NeedFolow) */UpdateChasePosition();

            // Update player weapon
            // TEMP Changes - commented
            /*Matrix transformedHand = AnimatedModel.BonesAnimation[RIGHT_HAND_BONE_ID] * Transformation.Matrix;
            playerWeapon.TargetDirection = HeadingVector + UpVector * rotateWaistBone;
            playerWeapon.Update(time, transformedHand);*/
        }

        public override void Draw(GameTime time)
        {
            // TEMP Changes - no weapon
            //playerWeapon.Draw(time);

            base.Draw(time);
        }
    }
}
