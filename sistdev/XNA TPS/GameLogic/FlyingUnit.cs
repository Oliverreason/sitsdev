using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

using Microsoft.Xna.Framework;


using sistdev.GameBase;
using sistdev.GameBase.Cameras;
using sistdev.GameBase.Shapes;
using sistdev.GameLogic.Levels;


namespace sistdev.GameLogic
{
    public class FlyingUnit : DrawableGameComponent
    {
        //public static float MIN_GRAVITY = -1.5f;
        //public static float GRAVITY_ACCELERATION = 4.0f;

        int life;
        int maxLife;
        float speed;

        public Random rnd;
        public Vector3 direction;
        public Boolean stopChangingDirection;

        AnimatedModel animatedModel;
        int currentAnimationId;
        BoundingBox boundingBox;
        BoundingSphere boundingSphere;
        bool needUpdateCollision;

        Vector3 linearVelocity;
        Vector3 angularVelocity;
        //float gravityVelocity;

        Vector3 headingVec;
        Vector3 strafeVec;
        Vector3 upVec;

        //bool adjustJumpChanges;
        //bool isOnTerrain;
        bool isDead;

        // Necessary services
        protected CameraManager cameraManager;
        protected Terrain terrain;
        //protected List<Bounding2D> bound2DList;
        //protected GameLevel gameLevel;

        #region Properties
        public int Life
        {
            get
            {
                return life;
            }
            set
            {
                life = value;
            }
        }

        public int MaxLife
        {
            get
            {
                return maxLife;
            }
            set
            {
                maxLife = value;
            }
        }

        public float Speed
        {
            get
            {
                return speed;
            }
            set
            {
                speed = value;
            }
        }

        public Vector3 LinearVelocity
        {
            get
            {
                return linearVelocity;
            }
            set
            {
                linearVelocity = value;
            }
        }

        public Vector3 AngularVelocity
        {
            get
            {
                return angularVelocity;
            }
            set
            {
                angularVelocity = value;
            }
        }

        /*public float GravityVelocity
        {
            get
            {
                return gravityVelocity;
            }
            set
            {
                gravityVelocity = value;
            }
        }*/


        public Vector3 HeadingVector
        {
            get
            {
                return headingVec;
            }
        }

        public Vector3 StrafeVector
        {
            get
            {
                return strafeVec;
            }
        }

        public Vector3 UpVector
        {
            get
            {
                return upVec;
            }
        }

        public AnimatedModel AnimatedModel
        {
            get
            {
                return animatedModel;
            }
        }

        protected int CurrentAnimationId
        {
            get
            {
                return currentAnimationId;
            }
        }

        public virtual Transformation Transformation
        {
            get
            {
                return animatedModel.Transformation;
            }
            set
            {
                animatedModel.Transformation = value;

                // Upate
                UpdateHeight(0);
                NormalizeBaseVectors();
            }
        }
        public virtual Transformation TransformationOld
        {
            get
            {
                return animatedModel.TransformationOld;
            }
            set
            {
                animatedModel.TransformationOld = value;

                // Upate
                UpdateHeight(0);
                NormalizeBaseVectors();
            }
        }

        public BoundingBox BoundingBox
        {
            get
            {
                if (needUpdateCollision)
                    UpdateCollision();

                return boundingBox;
            }
        }

        public BoundingSphere BoundingSphere
        {
            get
            {
                if (needUpdateCollision)
                    UpdateCollision();

                return boundingSphere;
            }
        }

        /*public bool IsOnTerrain
        {
            get
            {
                return isOnTerrain;
            }
        }*/

        public bool IsDead
        {
            get
            {
                return isDead;
            }
        }

        #endregion

        public FlyingUnit(Game game)
            : base(game)
        {
            //gravityVelocity = 0.0f;
            //isOnTerrain = false;
            isDead = false;
            //adjustJumpChanges = false;

            needUpdateCollision = true;
        }

        protected void Load(string unitModelFileName)
        {
            animatedModel = new AnimatedModel(Game);
            animatedModel.Initialize();
            
            animatedModel.Load(unitModelFileName);

            // Put the player above the terrain
            UpdateHeight(0);
            //isOnTerrain = true;

            NormalizeBaseVectors();
        }

        public override void Initialize()
        {
            cameraManager = Game.Services.GetService(typeof(CameraManager)) as CameraManager;
            terrain = Game.Services.GetService(typeof(Terrain)) as Terrain;
            //gameLevel = Game.Services.GetService(typeof(GameLevel)) as GameLevel;

            base.Initialize();
        }

        protected void SetAnimation(int animationId, bool reset, bool enableLoop, bool waitFinish)
        {
            if (reset || currentAnimationId != animationId)
            {
                if (waitFinish && !AnimatedModel.IsAnimationFinished)
                    return;

                AnimatedModel.ActiveAnimation = AnimatedModel.Animations[animationId];
                AnimatedModel.EnableAnimationLoop = enableLoop;
                currentAnimationId = animationId;
            }
        }

        private void NormalizeBaseVectors()
        {
            headingVec = Transformation.Matrix.Forward;
            strafeVec = Transformation.Matrix.Right;
            upVec = Transformation.Matrix.Up;

            headingVec.Normalize();
            strafeVec.Normalize();
            upVec.Normalize();
        }

        public override void Update(GameTime time)
        {
            //TransformationOld = Transformation;

            float elapsedTimeSeconds = (float)time.ElapsedGameTime.TotalSeconds;
            animatedModel.Update(time, Matrix.Identity);

            // Update the height and collision volumes
            if (linearVelocity != Vector3.Zero/* || gravityVelocity != 0.0f*/)
            {
                Transformation.Translate += linearVelocity * elapsedTimeSeconds * speed;
                UpdateHeight(elapsedTimeSeconds);
                needUpdateCollision = true;
            }

            // Update coordinate system when the unit rotates
            if (angularVelocity != Vector3.Zero)
            {
                Transformation.Rotate += angularVelocity * elapsedTimeSeconds * speed;
                NormalizeBaseVectors();
            }

            return;
            base.Update(time);

        }

        private void UpdateHeight(float elapsedTimeSeconds)
        {
            // Get terrain height
            float terrainHeight = terrain.GetHeight(Transformation.Translate);
            Vector3 newPosition = Transformation.Translate;

            // Unit is on terrain
            float HEIGHT_EPSILON = 10.0f;
            float MAX_HEIGHT = 30.0f;
            if (Transformation.Translate.Y <= (terrainHeight + HEIGHT_EPSILON)/* && gravityVelocity <= 0*/)
            {
                //isOnTerrain = true;
                //gravityVelocity = 0.0f;
                newPosition.Y = terrainHeight + HEIGHT_EPSILON;
                
                // Update camera chase speed and unit movement speed (hack)
                /*if (adjustJumpChanges)
                {
                    ThirdPersonCamera camera = cameraManager.ActiveCamera as ThirdPersonCamera;
                    camera.ChaseSpeed /= 4.0f;
                    speed /= 1.5f;
                    adjustJumpChanges = false;
                    (cameraManager.ActiveCamera as ThirdPersonCamera).NeedFolow = true;
                }*/
            }
            // Unit is above the terrain
            else if (Transformation.Translate.Y >= (terrainHeight + HEIGHT_EPSILON + MAX_HEIGHT))
            {
                newPosition.Y = terrainHeight + HEIGHT_EPSILON + MAX_HEIGHT;
                //isOnTerrain = false;
                // Gravity
                //if (gravityVelocity > MIN_GRAVITY)
                //    gravityVelocity -= GRAVITY_ACCELERATION * elapsedTimeSeconds;
                //newPosition.Y = Math.Max(terrainHeight, Transformation.Translate.Y + gravityVelocity);
            }
            else
            {
            }
            Transformation.Translate = newPosition;
        }

        public void Update2DCollision(List<Bounding2D> bound2DList)
        {
            Vector3 linVel = linearVelocity;
            linVel.Normalize();

            foreach (Bounding2D bound2D in bound2DList)
            {
                Double chk2DCollision = Check2DCollision(bound2D);
                if (chk2DCollision > 0)
                {
                    Console.WriteLine("Collision");
                    Transformation.Translate -= linVel * (float)chk2DCollision;
                    direction = new Vector3(-direction.X, direction.Y, -direction.Z);
                    stopChangingDirection = true;
                }
            }
        }

        public Double Check2DCollision(Bounding2D bound2D)
        {
            Double diffRXY = new Double();

            switch (bound2D.Type)
            {
                case 0:
                    diffRXY = Math.Pow(bound2D.Radius, 2) - (Math.Pow(Transformation.Translate.X - bound2D.X, 2) + Math.Pow(Transformation.Translate.Z - bound2D.Z, 2));
                    if (diffRXY > 0)
                    {
                        return Math.Sqrt(diffRXY);
                    }
                    break;
                case 1:
                    if ((Transformation.Translate.X > (bound2D.X - bound2D.Height)) &&
                         (Transformation.Translate.X < (bound2D.X + bound2D.Height)) &&
                         (Transformation.Translate.Z > (bound2D.Z - bound2D.Width)) &&
                         (Transformation.Translate.Z < (bound2D.Z + bound2D.Width)))
                    {
                        Console.WriteLine("Box collision!!!");
                        return 1;
                    }
                    break;
                default:
                    return -1;
            }

            return -1;
        }

        public override void Draw(GameTime time)
        {
            animatedModel.Draw(time);
        }

        public virtual void ReceiveDamage(int damageValue)
        {
            life = Math.Max(0, life - damageValue);
            if (life == 0)
                isDead = true;
        }

        private void UpdateCollision()
        {
            // Do not support scale

            // Update bounding box
            boundingBox = animatedModel.BoundingBox;
            boundingBox.Min += Transformation.Translate;
            boundingBox.Max += Transformation.Translate;

            // Update bounding sphere
            boundingSphere = animatedModel.BoundingSphere;
            boundingSphere.Center += Transformation.Translate;

            needUpdateCollision = false;
        }

        public float? BoxIntersects(Ray ray)
        {
            Matrix inverseTransformation = Matrix.Invert(Transformation.Matrix);
            ray.Position = Vector3.Transform(ray.Position, inverseTransformation);
            ray.Direction = Vector3.TransformNormal(ray.Direction, inverseTransformation);

            return animatedModel.BoundingBox.Intersects(ray);
        }
    }
}
