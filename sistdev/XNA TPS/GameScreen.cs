using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.PrivateImplementationDetails;

using sistdev.GameBase.Cameras;
using sistdev.GameBase.Lights;
using sistdev.GameBase.Shapes;
using sistdev.GameLogic;
using sistdev.GameLogic.Levels;
using sistdev.Helpers;

namespace sistdev
{
    public class GameScreen : DrawableGameComponent
    {
        // Modified level that we are playing
        LevelCreator.Levels currentLevel;
        GameLevel gameLevel;

        // Text
        SpriteBatch spriteBatch;
        SpriteFont spriteFont;

        // Weapon target sprite
        Texture2D weaponTargetTexture;
        Texture2D weaponTargetTexture2;
        Texture2D firePosTexture;
        Texture2D healthBar;
        Vector3 weaponTargetPosition;
        Vector3 cursorPosition;
        Vector3 cursorLastPosition;

        // Aimed enemy
        Enemy aimEnemy;
        int numEnemiesAlive;
        int numMosquitosAlive;

        // Frame counter helper
        FrameCounterHelper frameCounter;
        
        // Necessary services
        InputHelper inputHelper;

        Vector2 resolution;
        Vector2 XYCurs;

        Vector3 firePos;
        Vector3 firePos2;

        public GameScreen(Game game, LevelCreator.Levels currentLevel)
            : base(game)
        {
            this.currentLevel = currentLevel;
        }

        public override void Initialize()
        {
            // Frame counter
            frameCounter = new FrameCounterHelper(Game);

            XYCurs = new Vector2(200, 200);
            firePos  = Vector3.Zero;
            firePos2 = Vector3.Zero;
            resolution.X = 800;
            resolution.Y = 600;
            cursorPosition.X = resolution.X / 2;
            cursorPosition.Y = resolution.Y / 2;
            cursorPosition.Z = 0;

            // Get services
            inputHelper = Game.Services.GetService(typeof(InputHelper)) as InputHelper;
            if (inputHelper == null)
                throw new InvalidOperationException("Cannot find an input service");

            base.Initialize();
        }

        protected override void LoadContent()
        {
            // Create SpriteBatch and add services
            spriteBatch = new SpriteBatch(GraphicsDevice);

            // Font 2D
            spriteFont = Game.Content.Load<SpriteFont>(GameAssetsPath.FONTS_PATH +
                "BerlinSans");

            // Weapon target
            weaponTargetTexture = Game.Content.Load<Texture2D>(GameAssetsPath.TEXTURES_PATH +
                "weaponTarget");

            // Weapon target 2
            weaponTargetTexture2 = Game.Content.Load<Texture2D>(GameAssetsPath.TEXTURES_PATH +
                "weaponTarget2");
            
            // Weapon target 3
            firePosTexture = Game.Content.Load<Texture2D>(GameAssetsPath.TEXTURES_PATH +
                "firePos");

            // Health Bar
            healthBar = Game.Content.Load<Texture2D>(GameAssetsPath.TEXTURES_PATH +
                "healthBar");

            // Load game level
            gameLevel = LevelCreator.CreateLevel(Game, currentLevel);

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            base.UnloadContent();
        }

        private void UpdateInput()
        {
            ThirdPersonCamera fpsCamera = gameLevel.CameraManager["FPSCamera"] as ThirdPersonCamera;
            ThirdPersonCamera followCamera = gameLevel.CameraManager["FollowCamera"] as ThirdPersonCamera;

            Player player = gameLevel.Player;
            Vector2 leftThumb = inputHelper.GetLeftThumbStick();
            Vector2 leftMouseThumb = inputHelper.GetMouseLeftThumbStick();
            MouseState mouseState = inputHelper.GetMouseState();
            MouseState lastMouseState = inputHelper.GetLastMouseState();

            if (mouseState.LeftButton == ButtonState.Pressed)
            {
                
            }
            

            // Aim Mode
            if (inputHelper.IsKeyPressed(Buttons.LeftShoulder)&& player.IsOnTerrain)
            {
                // Reset follow camera 
                if (gameLevel.CameraManager.ActiveCamera != fpsCamera)
                {
                    gameLevel.CameraManager.SetActiveCamera("FPSCamera"); //!
                    fpsCamera.IsFirstTimeChase = true; //!
                    player.SetAnimation(Player.PlayerAnimations.Aim, false, false, false);

                    Microsoft.Xna.Framework.Input.Mouse.SetPosition((int)resolution.X/2, (int)resolution.Y/2);
                }

                cursorLastPosition = cursorPosition;

                if (mouseState.X < 300)
                {
                    Mouse.SetPosition(300, mouseState.Y);
                }
                if (mouseState.X > (resolution.X - 300))
                {
                    Mouse.SetPosition((int)resolution.X - 300, mouseState.Y);
                }
                if (mouseState.Y < 200)
                {
                    Mouse.SetPosition(mouseState.X, 200);
                }
                if (mouseState.Y > (resolution.Y - 200))
                {
                    Mouse.SetPosition(mouseState.X, (int)resolution.Y - 200);
                }

                cursorPosition.X = mouseState.X;
                cursorPosition.Y = mouseState.Y;

                weaponTargetPosition.X = mouseState.X;
                weaponTargetPosition.Y = mouseState.Y;  
                

                // Rotate the camera and move the player's aim
                /*fpsCamera.EyeRotateVelocity = new Vector3(-(cursorPosition.Y-cursorLastPosition.Y) * 50.0f, 0.0f, 0.0f);
                player.LinearVelocity = Vector3.Zero;
                player.AngularVelocity = new Vector3(0.0f, -(cursorPosition.X - cursorLastPosition.X) * 70.0f, 0.0f);
                player.RotateWaistVelocity = -(cursorPosition.Y - cursorLastPosition.Y) * 0.8f;*/

                // Rotate the camera and move the player's aim
                fpsCamera.EyeRotateVelocity = new Vector3(leftMouseThumb.Y * 50.0f, 0.0f, 0.0f);
                player.LinearVelocity = Vector3.Zero;
                player.AngularVelocity = new Vector3(0.0f, -leftMouseThumb.X * 70.0f, 0.0f);
                player.RotateWaistVelocity = leftMouseThumb.Y * 0.8f; 

                
                // Rotate the camera and move the player's aim
                /*fpsCamera.EyeRotateVelocity = new Vector3(leftThumb.Y * 50.0f, 0.0f, 0.0f);
                player.LinearVelocity = Vector3.Zero;
                player.AngularVelocity = new Vector3(0.0f, -leftThumb.X * 70.0f, 0.0f);
                player.RotateWaistVelocity = leftThumb.Y * 0.8f; */
                

                // Shoot
                /*if ((inputHelper.GetMouseState().LeftButton == ButtonState.Pressed) )
                {
                    // Wait the last shoot animation finish
                    if (player.AnimatedModel.IsAnimationFinished)
                    {
                        player.SetAnimation(Player.PlayerAnimations.Shoot, true, false, false);

                        // Damage the enemy
                        if (aimEnemy != null)
                            aimEnemy.ReceiveDamage(0);
                    }
                }*/
            }
            // Normal Mode
            else
            {
                bool isPlayerIdle = true;

                if (gameLevel.CameraManager.ActiveCamera != followCamera)
                {
                    // Reset fps camera 
                    gameLevel.CameraManager.SetActiveCamera("FollowCamera");
                    followCamera.IsFirstTimeChase = true;
                    player.RotateWaist = 0.0f;
                    player.RotateWaistVelocity = 0.0f;
                }
                
                followCamera.EyeRotateVelocity = new Vector3(leftThumb.Y * 50.0f, 0.0f, 0.0f);
                player.AngularVelocity = new Vector3(0.0f, -leftThumb.X * 70.0f, 0.0f);

                if (mouseState.X > 0 && mouseState.X < resolution.X &&
                    mouseState.Y > 0 && mouseState.Y < resolution.Y)
                {
                    //cursorPosition.X = mouseState.X;
                    //cursorPosition.Y = mouseState.Y;

                    cursorLastPosition = cursorPosition;

                    Boolean updateCamera = true;

                    if (mouseState.X > (resolution.X / 2 + XYCurs.X))
                    {
                        cursorPosition.X = resolution.X / 2 + XYCurs.X-1;
                    }
                    else if (mouseState.X < (resolution.X / 2 - XYCurs.X))
                    {
                        cursorPosition.X = resolution.X / 2 - XYCurs.X+1;
                    }
                    else if (cursorPosition.Y > resolution.Y / 2 + XYCurs.Y)
                    {
                        cursorPosition.Y = resolution.Y / 2 + XYCurs.Y-1;
                    }
                    else if (cursorPosition.Y < resolution.Y / 2 - XYCurs.Y)
                    {
                        cursorPosition.Y = resolution.Y / 2 - XYCurs.Y+1;
                    }
                    else
                    {
                        updateCamera = false;
                    }

                    cursorPosition.Y = mouseState.Y;
                    if (!updateCamera)
                    {
                        cursorPosition.X = mouseState.X;
                        //cursorPosition.Y = mouseState.Y;
                    }
                    
                    if (updateCamera)
                    {
                        //followCamera.EyeRotateVelocity = new Vector3((mouseState.Y - lastMouseState.Y) * 5.0f, 0.0f, 0.0f);
                        player.AngularVelocity = new Vector3(0.0f, (cursorPosition.X - mouseState.X) * 7.0f, 0.0f);
                        Microsoft.Xna.Framework.Input.Mouse.SetPosition((int)cursorPosition.X, (int)cursorPosition.Y);
                    }
                }

                Boolean playerRuning = true;
                Boolean playerStrafing = true;

                // Run foward 
                if (inputHelper.IsKeyPressed(Buttons.X))
                {
                    player.SetAnimation(Player.PlayerAnimations.Run, false, true, false);
                    player.LinearVelocity = player.HeadingVector * 25.0f;
                    isPlayerIdle = false;
                }
                // Run backward
                else if (inputHelper.IsKeyPressed(Buttons.A))
                {
                    player.SetAnimation(Player.PlayerAnimations.Run, false, true, false);
                    player.LinearVelocity = -player.HeadingVector * 20.0f;
                    isPlayerIdle = false;
                }
                else
                {
                    playerRuning = false;
                }
                
                // Strafe Left
                if (inputHelper.IsKeyPressed(Buttons.B))
                {
                    player.SetAnimation(Player.PlayerAnimations.Run, false, true, false);
                    player.LinearVelocity = ((playerRuning) ? (player.LinearVelocity) : (Vector3.Zero)) - player.StrafeVector * 10.0f;
                    isPlayerIdle = false;
                }
                // Strafe Righth
                else if (inputHelper.IsKeyPressed(Buttons.Y))
                {
                    player.SetAnimation(Player.PlayerAnimations.Run, false, true, false);
                    player.LinearVelocity = ((playerRuning) ? (player.LinearVelocity) : (Vector3.Zero)) + player.StrafeVector * 10.0f;
                    isPlayerIdle = false;
                }
                else
                {
                    playerStrafing = false;
                }

                if (!(playerStrafing || playerRuning))
                {
                    player.LinearVelocity = Vector3.Zero;
                }

                // Jump
                if (inputHelper.IsKeyJustPressed(Buttons.LeftStick))
                {
                    player.Jump(4f);
                    isPlayerIdle = false;
                }

                // Shoot
                /*if (!(playerStrafing || playerRuning) && (inputHelper.GetMouseState().LeftButton == ButtonState.Pressed) )
                {
                    //player.SetAnimation(Player.PlayerAnimations.Aim, false, false, false);
                    player.SetAnimation(Player.PlayerAnimations.Shoot, true, false, false);
                    isPlayerIdle = false;
                    // Wait the last animation finish
                    if (player.AnimatedModel.IsAnimationFinished)
                    {
                        //player.SetAnimation(Player.PlayerAnimations.Aim, false, false, false);
                        player.SetAnimation(Player.PlayerAnimations.Shoot, true, false, false);
                        
                        // Damage the enemy
                        if (aimEnemy != null)
                        {
                            aimEnemy.ReceiveDamage(0);
                        }
                    }
                }*/

                if (isPlayerIdle)
                    player.SetAnimation(Player.PlayerAnimations.Idle, false, true, false);
            }
        }

        private void UpdateWeaponTarget()
        {
            aimEnemy = null;
            //numEnemiesAlive = 0;

            /*Vector3 fp;

            ThirdPersonCamera cam = gameLevel.CameraManager.ActiveCamera as ThirdPersonCamera;
            Vector3 cursorPositionNorm = cursorPosition;// -(new Vector3(500, 0, 390));
            cursorPositionNorm.Normalize();
            cursorPositionNorm.X = 0;
            cursorPositionNorm.Y = 0;
            cursorPositionNorm.Z = 0;
            Vector3 proj = GraphicsDevice.Viewport.Unproject(cursorPositionNorm, cam.Projection, cam.View, Matrix.CreateTranslation(0, 0, 0));
            
            Vector3 plh = gameLevel.Player.Weapon.TargetDirection;// gameLevel.Player.HeadingVector;
            Vector3 vec = Vector3.Transform(plh,
                Matrix.CreateFromAxisAngle(plh, MathHelper.ToRadians(0)) *
                Matrix.CreateFromAxisAngle(gameLevel.Player.UpVector, MathHelper.ToRadians(-10)) *
                Matrix.CreateFromAxisAngle(gameLevel.Player.StrafeVector, MathHelper.ToRadians(0)));

            vec = proj + cam.HeadingVector;
            fp = proj + (new Vector3(0,0,0));
            //vec = gameLevel.Player.HeadingVector;
            //Vector3 vec = Vector3.Transform(gameLevel.Player.HeadingVector,
              //  Matrix.CreateFromAxisAngle(Vector3.UnitY, (float)Math.Acos((2 - (new Vector3(cursorPositionNorm, 0, cursorPositionNorm.)).LengthSquared()) / 2)));
            //vec.Normalize();
            // Shoot ray
            Ray ray = new Ray(gameLevel.Player.Weapon.FirePosition, gameLevel.Player.Weapon.TargetDirection);
            Ray ray2 = new Ray(fp, vec);
            //Ray ray3 = new Ray(proj, cam.HeadingVector);
            
            // Distance from the ray start position to the terrain
            float? distance = gameLevel.Terrain.Intersects(ray);
            float? distance2 = gameLevel.Terrain.Intersects(ray2);

            if (distance != null)
                firePos = gameLevel.Player.Weapon.FirePosition +
                    gameLevel.Player.Weapon.TargetDirection * (float)distance;

            if (distance2 != null)
                firePos2 = fp +
                    vec * (float)distance2;

            // Test intersection with enemies
            foreach (Enemy enemy in gameLevel.EnemyList)
            {
                if (!enemy.IsDead)
                {
                    numEnemiesAlive++;

                    float? enemyDistance = enemy.BoxIntersects(ray);
                    float? enemyDistance2 = enemy.BoxIntersects(ray2);
                    if (enemyDistance != null)
                    {
                        if (distance == null || enemyDistance <= distance)
                        {
                            distance = enemyDistance;
                            aimEnemy = enemy;
                        }
                    }
                    if (enemyDistance2 != null)
                    {
                        if (distance2 == null || enemyDistance2 <= distance2)
                        {
                            distance2 = enemyDistance2;
                            aimEnemy = enemy;
                        }
                    }
                }
                if (distance != null)
                firePos = gameLevel.Player.Weapon.FirePosition +
                    gameLevel.Player.Weapon.TargetDirection * (float)distance;

                if (distance2 != null)
                    firePos2 = fp +
                        vec * (float)distance2;
            }

            //firePos2 = GraphicsDevice.Viewport.Unproject(cursorPosition, cam.Projection, cam.View, Matrix.CreateTranslation(0, 0, 0));
            //firePos2 = GraphicsDevice.Viewport.Project(firePos2, cam.Projection, cam.View, Matrix.CreateTranslation(0, 0, 0));
            // Weapon target position
            weaponTargetPosition = gameLevel.Player.Weapon.FirePosition +
                gameLevel.Player.Weapon.TargetDirection * 300;*/
        }

        public override void Update(GameTime gameTime)
        {
            // Restart game
            if (gameLevel.Player.IsDead || numEnemiesAlive == 0)
            {
                //Game.Exit();
                gameLevel = LevelCreator.CreateLevel(Game, currentLevel);
                numEnemiesAlive = gameLevel.EnemyList.Count;
                numMosquitosAlive = gameLevel.MosquitoList.Count;
            }

            UpdateInput();

            // Update player
            gameLevel.Player.Update2DCollision(gameLevel.Bound2D);
            gameLevel.Player.Update(gameTime);

            UpdateWeaponTarget();

            // Update camera
            BaseCamera activeCamera = gameLevel.CameraManager.ActiveCamera;
            activeCamera.Update(gameTime);

            // Update light position
            PointLight cameraLight = gameLevel.LightManager["CameraLight"] as PointLight;
            cameraLight.Position = activeCamera.Position;

            // Update enemies
            foreach (Enemy enemy in gameLevel.EnemyList)
            {
                if (enemy.BoundingSphere.Intersects(activeCamera.Frustum) ||
                    enemy.State == Enemy.EnemyState.ChasePlayer ||
                    enemy.State == Enemy.EnemyState.AttackPlayer)
                {
                    enemy.Update2DCollision(gameLevel.Bound2D);
                    enemy.Update(gameTime);
                }
            }

            // Update mosquito
            foreach (Mosquito mosquito in gameLevel.MosquitoList)
            {
                if (mosquito.BoundingSphere.Intersects(activeCamera.Frustum))
                {
                    mosquito.Update2DCollision(gameLevel.Bound2D);
                    mosquito.Update(gameTime);
                }
            }

            // Update stat units
            foreach (StaticUnit statUnit in gameLevel.StaticUnitList)
            {
                if (statUnit.BoundingSphere.Intersects(activeCamera.Frustum))
                {
                    statUnit.Update(gameTime);
                }
            }

            // Update scene objects
            gameLevel.SkyDome.Update(gameTime);
            gameLevel.Terrain.Update(gameTime);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(ClearOptions.DepthBuffer, Color.White, 1.0f, 255);

            BaseCamera activeCamera = gameLevel.CameraManager.ActiveCamera;

            gameLevel.SkyDome.Draw(gameTime);
            gameLevel.Terrain.Draw(gameTime);
            gameLevel.Player.Draw(gameTime);

            // Draw enemies
            foreach (Enemy enemy in gameLevel.EnemyList)
            {
                if (enemy.BoundingSphere.Intersects(activeCamera.Frustum))
                    enemy.Draw(gameTime);
            }

            // Draw mosquitos
            foreach (Mosquito mosquito in gameLevel.MosquitoList)
            {
                if (mosquito.BoundingSphere.Intersects(activeCamera.Frustum))
                    mosquito.Draw(gameTime);
            }

            // Draw static units
            foreach (StaticUnit statUnit in gameLevel.StaticUnitList)
            {
                if (statUnit.BoundingSphere.Intersects(activeCamera.Frustum))
                {
                    statUnit.Draw(gameTime);
                }
            }

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

            // Project weapon target
            //weaponTargetPosition = GraphicsDevice.Viewport.Project(weaponTargetPosition,
            //    activeCamera.Projection, activeCamera.View, Matrix.Identity);
            
            // Draw weapon target
            int weaponRectangleSize = GraphicsDevice.Viewport.Width / 40;
            if (activeCamera == gameLevel.CameraManager["FPSCamera"])
            {
                spriteBatch.Draw(weaponTargetTexture2, new Rectangle(
                    (int)(cursorPosition.X - weaponRectangleSize * 0.5f),
                    (int)(cursorPosition.Y - weaponRectangleSize * 0.5f),
                    weaponRectangleSize, weaponRectangleSize),
                    (aimEnemy == null) ? Color.White : Color.Red);
            }
            else
            {
                spriteBatch.Draw(weaponTargetTexture, new Rectangle(
                        (int)(cursorPosition.X - weaponRectangleSize * 0.5f),
                        (int)(cursorPosition.Y - weaponRectangleSize * 0.5f),
                        weaponRectangleSize, weaponRectangleSize),
                        (aimEnemy == null) ? Color.White : Color.Red);

                //spriteBatch.Draw(weaponTargetTexture2, new Rectangle(
                //     (int)(weaponTargetPosition.X - weaponRectangleSize * 0.5f),
                //      (int)(weaponTargetPosition.Y - weaponRectangleSize * 0.5f),
                //       weaponRectangleSize, weaponRectangleSize),
                //       (aimEnemy == null) ? Color.White : Color.Red);
            }

            // Draw Player Health
            spriteBatch.DrawString(spriteFont, "Health: ", new Vector2(10, 5), Color.Green);

            for (Int32 i = 0; i < gameLevel.Player.Life / (gameLevel.Player.MaxLife / 20); i++)
            {
                spriteBatch.Draw(healthBar, new Vector2(
                        120+i*10,
                        10),
                        (gameLevel.Player.Life >= gameLevel.Player.MaxLife/2)?Color.Green:Color.Red);
            }

                // Draw GUI text
                /*spriteBatch.DrawString(spriteFont, "Health: " + gameLevel.Player.Life + "/" +
                    gameLevel.Player.MaxLife, new Vector2(10, 5), Color.Green);*/
                //spriteBatch.DrawString(spriteFont, "Bullets: " + gameLevel.Player.Weapon.BulletsCount + "/" +
                //    gameLevel.Player.Weapon.MaxBullets, new Vector2(10, 25), Color.Green);
                spriteBatch.DrawString(spriteFont, "Beasts: " + numEnemiesAlive + "/" +
                    gameLevel.EnemyList.Count, new Vector2(10, 35), Color.Green);

                spriteBatch.DrawString(spriteFont, "Beasts: " + numMosquitosAlive + "/" +
                        gameLevel.MosquitoList.Count, new Vector2(10, 55), Color.Green);

                //spriteBatch.DrawString(spriteFont, "Trees: " + gameLevel.StaticUnitList.Count, 
                //    new Vector2(110, 35), Color.Green);
            

            spriteBatch.DrawString(spriteFont, "FPS: " + frameCounter.LastFrameFps, new Vector2(10, 75),
                Color.Red);

            /*spriteBatch.DrawString(spriteFont, "Cursor: " + cursorPosition.X + ":" + cursorPosition.Y, new Vector2(10, 105),
                Color.Red);

            /*spriteBatch.DrawString(spriteFont, "Mouse: " + inputHelper.GetMouseState().X + ":" + inputHelper.GetMouseState().Y, new Vector2(10, 125),
                Color.Red);

            spriteBatch.DrawString(spriteFont, "Weapon pos: " + gameLevel.Player.Weapon.FirePosition.X + "  " +
                                                            gameLevel.Player.Weapon.FirePosition.Y + "  " +
                                                            gameLevel.Player.Weapon.FirePosition.Z, new Vector2(10, 165),
                Color.Red);*/

            /*spriteBatch.DrawString(spriteFont, "Weapon TD: " + gameLevel.Player.Weapon.TargetDirection.X + "  " + 
                                                               gameLevel.Player.Weapon.TargetDirection.Y + "  " +
                                                               gameLevel.Player.Weapon.TargetDirection.Z, new Vector2(10, 185),
                Color.Red);*/


            /*Vector3 MyTD = gameLevel.Player.Weapon.TargetDirection + 
                new Vector3(cursorPosition.X - 500, 
                            0, 
                            cursorPosition.Y - 390);*/

            /*Vector3 curPosNorm = cursorPosition-(new Vector3(500, 390, 0));
            //if (curPosNorm != Vector3.Zero) curPosNorm.Normalize();
            spriteBatch.DrawString(spriteFont, "curPosNorm: " + curPosNorm, new Vector2(10, 305), Color.Red);
            double x = curPosNorm.LengthSquared();
            x = Math.Acos((double)(2-x)/2);*/
            
            ThirdPersonCamera cam = gameLevel.CameraManager.ActiveCamera as ThirdPersonCamera;
            Vector3 proj = GraphicsDevice.Viewport.Unproject(cursorPosition, cam.Projection, cam.View, Matrix.CreateTranslation(0, 0, 0));

            Vector3 MyTD = proj;//Vector3.Transform(gameLevel.Player.HeadingVector, Matrix.CreateFromAxisAngle(Vector3.UnitY, (float)x));
            //MyTD.Normalize();
            /*spriteBatch.DrawString(spriteFont, "My TD    : " + MyTD.X + "  " +
                                                               MyTD.Y + "  " +
                                                               MyTD.Z, new Vector2(10, 205),
                Color.Red);*/
            
            /*MyTD.Normalize();
            spriteBatch.DrawString(spriteFont, "TD Norm  : " + MyTD.X + "  " +
                                                               MyTD.Y + "  " +
                                                               MyTD.Z, new Vector2(10, 265),
                Color.Red);

            spriteBatch.DrawString(spriteFont, "Heading : " + gameLevel.Player.HeadingVector.X + "  " +
                                                              gameLevel.Player.HeadingVector.Y + "  " +
                                                              gameLevel.Player.HeadingVector.Z, new Vector2(10, 225),
                Color.Red);


            spriteBatch.DrawString(spriteFont, "CamHead : " + cam.HeadingVector.X + "  " +
                                                              cam.HeadingVector.Y + "  " +
                                                              cam.HeadingVector.Z, new Vector2(10, 245),
                Color.Red);
           
            
            spriteBatch.DrawString(spriteFont, "Anim: " + gameLevel.Player.AnimatedModel.IsAnimationFinished, new Vector2(10, 325),
                Color.Red);

            spriteBatch.DrawString(spriteFont, "Anim: " + gameLevel.Player.AnimatedModel.IsAnimationFinished, new Vector2(10, 325),
                Color.Red);*/

            /*spriteBatch.DrawString(spriteFont, "Fire Pos  : " + firePos.X + "  " +
                                                               firePos.Y + "  " +
                                                               firePos.Z, new Vector2(10, 285),
                Color.Red);

            spriteBatch.DrawString(spriteFont, "Fire Pos2 : " + firePos2.X + "  " +
                                                                firePos2.Y + "  " +
                                                                firePos2.Z, new Vector2(10, 305),
                Color.Red);
            */
            //Vector3 toE = GraphicsDevice.Viewport.Project(firePos2, cam.Projection, cam.View, Matrix.CreateTranslation(0, 0, 0));

            /*spriteBatch.DrawString(spriteFont, "Fire Pos3 : " + toE.X + "  " +
                                                                toE.Y + "  " +
                                                                toE.Z, new Vector2(10, 325),
                Color.Red);*/

            /*spriteBatch.Draw(firePosTexture, new Rectangle(
                        (int)(toE.X - weaponRectangleSize * 0.5f),
                        (int)(toE.Y - weaponRectangleSize * 0.5f),
                        weaponRectangleSize, weaponRectangleSize),
                        Color.White);

            Vector3 cursorPositionNorm = cursorPosition - (new Vector3(500, 0, 390));
            cursorPositionNorm.Normalize();
            spriteBatch.DrawString(spriteFont, "Alpha : " + (float)180 / Math.PI*Math.Acos((2 - cursorPositionNorm.LengthSquared()) / 2), new Vector2(10, 345),
                Color.Red);*/

            spriteBatch.DrawString(spriteFont, "Rotation : " + gameLevel.Player.Transformation.Rotate.X + "  " +
                                                               gameLevel.Player.Transformation.Rotate.Y + "  " +
                                                               gameLevel.Player.Transformation.Rotate.Z, new Vector2(10, 110),
                                                               Color.Red);

            spriteBatch.DrawString(spriteFont, "Translate : " + gameLevel.Player.Transformation.Translate.X + "  " +
                                                                gameLevel.Player.Transformation.Translate.Y + "  " +
                                                                gameLevel.Player.Transformation.Translate.Z, new Vector2(10, 130),
                Color.Red);

            /*spriteBatch.DrawString(spriteFont, "Scale : " + gameLevel.Player.Transformation.Scale.X + "  " +
                                                            gameLevel.Player.Transformation.Scale.Y + "  " +
                                                            gameLevel.Player.Transformation.Scale.Z, new Vector2(10, 150),
                Color.Red);

            spriteBatch.DrawString(spriteFont, "BS Centr : " + gameLevel.Player.BoundingSphere.Center.X + "  " +
                                                               gameLevel.Player.BoundingSphere.Center.Y + "  " +
                                                               gameLevel.Player.BoundingSphere.Center.Z, new Vector2(10, 180),
                Color.Red);

            spriteBatch.DrawString(spriteFont, "BS Box Min : " + gameLevel.Player.BoundingBox.Min.X + "  " +
                                                                 gameLevel.Player.BoundingBox.Min.Y + "  " +
                                                                 gameLevel.Player.BoundingBox.Min.Z, new Vector2(10, 210),
                Color.Red);

            spriteBatch.DrawString(spriteFont, "BS Box Max : " + gameLevel.Player.BoundingBox.Max.X + "  " +
                                                                 gameLevel.Player.BoundingBox.Max.Y + "  " +
                                                                 gameLevel.Player.BoundingBox.Max.Z, new Vector2(10, 230),
                Color.Red);

            */
            /*spriteBatch.DrawString(spriteFont, "Position : " + gameLevel.Player.X + "  " +
                                                               gameLevel.Player.Transformation.Scale.Y + "  " +
                                                               gameLevel.Player.Transformation.Scale.Z, new Vector2(10, 210),
                Color.Red);*/

            spriteBatch.End();

            base.Draw(gameTime);

            frameCounter.Update(gameTime);
        }
    }
}
