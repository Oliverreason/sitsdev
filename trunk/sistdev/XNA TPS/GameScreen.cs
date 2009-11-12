using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.DirectX;
using Microsoft.DirectX.Direct3D;
using Microsoft.DirectX.PrivateImplementationDetails;

using XNA_TPS.GameBase.Cameras;
using XNA_TPS.GameBase.Lights;
using XNA_TPS.GameBase.Shapes;
using XNA_TPS.GameLogic;
using XNA_TPS.GameLogic.Levels;
using XNA_TPS.Helpers;

namespace XNA_TPS
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
        Vector3 weaponTargetPosition;
        Vector3 cursorPosition;
        Vector3 cursorLastPosition;

        // Aimed enemy
        Enemy aimEnemy;
        int numEnemiesAlive;

        // Frame counter helper
        FrameCounterHelper frameCounter;
        
        // Necessary services
        InputHelper inputHelper;

        Vector2 resolution;
        Vector2 XYCurs;

        Vector3 firePos;

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
            firePos = Vector3.Zero;
            resolution.X = 1024;
            resolution.Y = 768;
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

            // Weapon target
            weaponTargetTexture2 = Game.Content.Load<Texture2D>(GameAssetsPath.TEXTURES_PATH +
                "weaponTarget2");
            
            // Weapon target
            firePosTexture = Game.Content.Load<Texture2D>(GameAssetsPath.TEXTURES_PATH +
                "firePos");
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
                if (/*inputHelper.IsKeyJustPressed(Buttons.A)*/(inputHelper.GetMouseState().LeftButton == ButtonState.Pressed) && player.Weapon.BulletsCount > 0)
                {
                    // Wait the last shoot animation finish
                    if (player.AnimatedModel.IsAnimationFinished)
                    {
                        player.SetAnimation(Player.PlayerAnimations.Shoot, true, false, false);

                        // Damage the enemy
                        player.Weapon.BulletsCount--;
                        if (aimEnemy != null)
                            aimEnemy.ReceiveDamage(player.Weapon.BulletDamage);
                    }
                }
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
                if (!(playerStrafing || playerRuning) && (inputHelper.GetMouseState().LeftButton == ButtonState.Pressed) && player.Weapon.BulletsCount > 0)
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
                        player.Weapon.BulletsCount--;
                        if (aimEnemy != null)
                        {
                            aimEnemy.ReceiveDamage(player.Weapon.BulletDamage);
                        }
                    }
                }

                if (isPlayerIdle)
                    player.SetAnimation(Player.PlayerAnimations.Idle, false, true, false);
            }
        }

        private void UpdateWeaponTarget()
        {
            aimEnemy = null;
            numEnemiesAlive = 0;

            ThirdPersonCamera cam = gameLevel.CameraManager.ActiveCamera as ThirdPersonCamera;
            Vector3 proj = GraphicsDevice.Viewport.Unproject(cursorPosition, cam.Projection, cam.View, Matrix.CreateTranslation(0, 0, 0));
            
            // Shoot ray
            Ray ray = new Ray(gameLevel.Player.Weapon.FirePosition, gameLevel.Player.Weapon.TargetDirection);
            Ray ray2 = new Ray(proj, cam.HeadingVector);
            
            // Distance from the ray start position to the terrain
            float? distance = gameLevel.Terrain.Intersects(ray);
            float? distance2 = gameLevel.Terrain.Intersects(ray2);

            if (distance != null)
            firePos = gameLevel.Player.Weapon.FirePosition +
                gameLevel.Player.Weapon.TargetDirection * (float)distance;
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
            }

            // Weapon target position
            weaponTargetPosition = gameLevel.Player.Weapon.FirePosition +
                gameLevel.Player.Weapon.TargetDirection * 300;
        }

        public override void Update(GameTime gameTime)
        {
            // Restart game
            if (gameLevel.Player.IsDead || numEnemiesAlive == 0)
                gameLevel = LevelCreator.CreateLevel(Game, currentLevel);

            UpdateInput();

            // Update player
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

                    enemy.Update(gameTime);

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

            spriteBatch.Begin(SpriteBlendMode.AlphaBlend, SpriteSortMode.Deferred, SaveStateMode.SaveState);

            // Project weapon target
            weaponTargetPosition = GraphicsDevice.Viewport.Project(weaponTargetPosition,
                activeCamera.Projection, activeCamera.View, Matrix.Identity);
            
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

                spriteBatch.Draw(weaponTargetTexture2, new Rectangle(
                        (int)(weaponTargetPosition.X - weaponRectangleSize * 0.5f),
                        (int)(weaponTargetPosition.Y - weaponRectangleSize * 0.5f),
                        weaponRectangleSize, weaponRectangleSize),
                        (aimEnemy == null) ? Color.White : Color.Red);
            }

            // Draw GUI text
            spriteBatch.DrawString(spriteFont, "Health: " + gameLevel.Player.Life + "/" +
                gameLevel.Player.MaxLife, new Vector2(10, 5), Color.Green);
            spriteBatch.DrawString(spriteFont, "Bullets: " + gameLevel.Player.Weapon.BulletsCount + "/" +
                gameLevel.Player.Weapon.MaxBullets, new Vector2(10, 25), Color.Green);
            spriteBatch.DrawString(spriteFont, "Enemies Alive: " + numEnemiesAlive + "/" +
                gameLevel.EnemyList.Count, new Vector2(10, 45), Color.Green);
            

            spriteBatch.DrawString(spriteFont, "FPS: " + frameCounter.LastFrameFps, new Vector2(10, 75),
                Color.Red);

            spriteBatch.DrawString(spriteFont, "Cursor: " + cursorPosition.X + ":" + cursorPosition.Y, new Vector2(10, 105),
                Color.Red);

            /*spriteBatch.DrawString(spriteFont, "Mouse: " + inputHelper.GetMouseState().X + ":" + inputHelper.GetMouseState().Y, new Vector2(10, 125),
                Color.Red);*/

            spriteBatch.DrawString(spriteFont, "Weapon pos: " + gameLevel.Player.Weapon.FirePosition.X + "  " +
                                                            gameLevel.Player.Weapon.FirePosition.Y + "  " +
                                                            gameLevel.Player.Weapon.FirePosition.Z, new Vector2(10, 165),
                Color.Red);

            spriteBatch.DrawString(spriteFont, "Weapon TD: " + gameLevel.Player.Weapon.TargetDirection.X + "  " + 
                                                               gameLevel.Player.Weapon.TargetDirection.Y + "  " +
                                                               gameLevel.Player.Weapon.TargetDirection.Z, new Vector2(10, 185),
                Color.Red);


            /*Vector3 MyTD = gameLevel.Player.Weapon.TargetDirection + 
                new Vector3(cursorPosition.X - 500, 
                            0, 
                            cursorPosition.Y - 390);*/

            Vector3 curPosNorm = cursorPosition-(new Vector3(500, 390, 0));
            //if (curPosNorm != Vector3.Zero) curPosNorm.Normalize();
            spriteBatch.DrawString(spriteFont, "curPosNorm: " + curPosNorm, new Vector2(10, 305), Color.Red);
            double x = curPosNorm.LengthSquared();
            x = Math.Acos((double)(2-x)/2);
            
            ThirdPersonCamera cam = gameLevel.CameraManager.ActiveCamera as ThirdPersonCamera;
            Vector3 proj = GraphicsDevice.Viewport.Unproject(cursorPosition, cam.Projection, cam.View, Matrix.CreateTranslation(0, 0, 0));

            Vector3 MyTD = proj;//Vector3.Transform(gameLevel.Player.HeadingVector, Matrix.CreateFromAxisAngle(Vector3.UnitY, (float)x));
            //MyTD.Normalize();
            spriteBatch.DrawString(spriteFont, "My TD    : " + MyTD.X + "  " +
                                                               MyTD.Y + "  " +
                                                               MyTD.Z, new Vector2(10, 205),
                Color.Red);
            MyTD.Normalize();
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
                Color.Red);

            spriteBatch.DrawString(spriteFont, "Fire Pos : " + firePos.X + "  " +
                                                               firePos.Y + "  " +
                                                               firePos.Z, new Vector2(10, 285),
                Color.Red);
            
            spriteBatch.End();

            base.Draw(gameTime);

            frameCounter.Update(gameTime);
        }
    }
}
