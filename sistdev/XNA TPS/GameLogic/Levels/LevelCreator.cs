using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Windows.Forms;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

using sistdev.GameBase;
using sistdev.GameBase.Materials;
using sistdev.GameBase.Cameras;
using sistdev.GameBase.Lights;
using sistdev.GameBase.Shapes;
using sistdev.Helpers;


namespace sistdev.GameLogic.Levels
{
    public static class LevelCreator
    {
        public enum Levels
        {
            Forest
        }

        public static GameLevel CreateLevel(Game game, Levels level)
        {
            // Remove all services from the last level
            game.Services.RemoveService(typeof(CameraManager));
            game.Services.RemoveService(typeof(LightManager));
            game.Services.RemoveService(typeof(Terrain));

            switch (level)
            {
                case Levels.Forest:
                    return CreateForestLevel(game);

                default:
                    throw new ArgumentException("Invalid game level");
            }
        }

        private static GameLevel CreateForestLevel(Game game)
        {
            ContentManager Content = game.Content;
            GameLevel gameLevel = new GameLevel();

            // Cameras and lights
            AddCameras(game, ref gameLevel);
            gameLevel.LightManager = new LightManager();
            gameLevel.LightManager.AmbientLightColor = new Vector3(0.1f, 0.1f, 0.1f);
            gameLevel.LightManager.Add("MainLight", 
                new PointLight(new Vector3(10000, 10000, 10000), new Vector3(0.2f, 0.2f, 0.2f)));
            gameLevel.LightManager.Add("CameraLight",
                new PointLight(Vector3.Zero, Vector3.One));

            game.Services.AddService(typeof(CameraManager), gameLevel.CameraManager);
            game.Services.AddService(typeof(LightManager), gameLevel.LightManager);

            // Terrain
            gameLevel.Terrain = new Terrain(game);
            gameLevel.Terrain.Initialize();
            gameLevel.Terrain.Load(game.Content, "Terrain1", 8.0f, 1.0f);

            // Terrain material
            TerrainMaterial terrainMaterial = new TerrainMaterial();
            terrainMaterial.LightMaterial = new LightMaterial(
                new Vector3(0.8f), new Vector3(0.3f), 32.0f);
            terrainMaterial.DiffuseTexture1 = GetTextureMaterial(game, "Terrain1", new Vector2(40, 40));
            terrainMaterial.DiffuseTexture2 = GetTextureMaterial(game, "Terrain2", new Vector2(25, 25));
            terrainMaterial.DiffuseTexture3 = GetTextureMaterial(game, "Terrain3", new Vector2(15, 15));
            terrainMaterial.DiffuseTexture4 = GetTextureMaterial(game, "Terrain4", Vector2.One);
            terrainMaterial.AlphaMapTexture = GetTextureMaterial(game, "AlphaMap", Vector2.One);
            terrainMaterial.NormalMapTexture = GetTextureMaterial(game, "Rockbump", new Vector2(128, 128));
            gameLevel.Terrain.Material = terrainMaterial;
            game.Services.AddService(typeof(Terrain), gameLevel.Terrain);

            // Sky
            gameLevel.SkyDome = new SkyDome(game);
            gameLevel.SkyDome.Initialize();
            gameLevel.SkyDome.Load("SkyDome");
            gameLevel.SkyDome.TextureMaterial = GetTextureMaterial(game, "SkyDome", Vector2.One);

            // Player
            gameLevel.Player = new Player(game, UnitTypes.PlayerType.Frog);
            gameLevel.Player.Initialize();
            gameLevel.Player.Transformation = new Transformation(new Vector3(50, 0, 50),
                new Vector3(0, 0, 0), 2*Vector3.One);
            gameLevel.Player.TransformationOld = gameLevel.Player.Transformation;
            gameLevel.Player.AnimatedModel.AnimationSpeed = 1.0f;

            // Player chase camera offsets
            gameLevel.Player.ChaseOffsetPosition = new Vector3[2];
            gameLevel.Player.ChaseOffsetPosition[0] = new Vector3(0.0f, 7.0f, -3.0f);
            gameLevel.Player.ChaseOffsetPosition[1] = new Vector3(3.0f, 4.0f, 0.0f);

            // Enemies
            gameLevel.EnemyList = ScatterEnemies(game, 10, 50, 200, gameLevel.Player);

            // Mosquitos
            gameLevel.MosquitoList = ScatterMosquitos(game, 15, 10, 300, gameLevel.Player);

            // StaticUnits
            //gameLevel.StaticUnitList = ScatterStaticUnits(game, 10, 50, 300, gameLevel.Player);
            gameLevel.StaticUnitList = PlaceStaticUnits(game, "Forest");

            gameLevel.Bound2D = ReadBounding2D("Forest");

            return gameLevel;
        }

        private static TextureMaterial GetTextureMaterial(Game game, string textureFilename, Vector2 tile)
        {
            Texture2D texture = game.Content.Load<Texture2D>(GameAssetsPath.TEXTURES_PATH + textureFilename);
            return new TextureMaterial(texture, tile);
        }

        private static void AddCameras(Game game, ref GameLevel gameLevel)
        {
            float aspectRate = (float)game.GraphicsDevice.Viewport.Width /
                game.GraphicsDevice.Viewport.Height;

            ThirdPersonCamera followCamera = new ThirdPersonCamera();
            followCamera.SetPerspectiveFov(60.0f, aspectRate, 0.1f, 2000);
            followCamera.SetChaseParameters(3.0f, 9.0f, 7.0f, 14.0f);

            ThirdPersonCamera fpsCamera = new ThirdPersonCamera();
            fpsCamera.SetPerspectiveFov(45.0f, aspectRate, 0.1f, 2000);
            fpsCamera.SetChaseParameters(5.0f, 6.0f, 6.0f, 6.0f);
            
            gameLevel.CameraManager = new CameraManager();
            gameLevel.CameraManager.Add("FollowCamera", followCamera);
            gameLevel.CameraManager.Add("FPSCamera", fpsCamera);
        }

        private static List<Enemy> ScatterEnemies(Game game, int numEnemies,
            float minDistance, int distance, Player player)
        {
            List<Enemy> enemyList = new List<Enemy>();

            for (int i = 0; i < numEnemies; i++)
            {
                Enemy enemy = new Enemy(game, UnitTypes.EnemyType.Beast);
                enemy.Initialize();

                // Generate a random position
                Vector3 offset = RandomHelper.GeneratePositionXZ(distance);
                while (Math.Abs(offset.X) < minDistance && Math.Abs(offset.Z) < minDistance)
                    offset = RandomHelper.GeneratePositionXZ(distance);

                enemy.Transformation = new Transformation(player.Transformation.Translate +
                    offset, Vector3.Zero, /*Vector3.One*/ new Vector3((float)0.3, (float)0.3, (float)0.3));
                enemy.TransformationOld = enemy.Transformation;

                enemy.Player = player;
                enemyList.Add(enemy);
            }

            return enemyList;
        }

        private static List<Mosquito> ScatterMosquitos(Game game, int numMosquitos,
            float minDistance, int distance, Player player)
        {
            List<Mosquito> mosquitoList = new List<Mosquito>();

            for (int i = 0; i < numMosquitos; i++)
            {
                Mosquito mosquito = new Mosquito(game, UnitTypes.MosquitoType.Mosquito);
                mosquito.Initialize();

                // Generate a random position
                Vector3 offset = RandomHelper.GeneratePositionXZ(distance);
                while (Math.Abs(offset.X) < minDistance && Math.Abs(offset.Z) < minDistance)
                    offset = RandomHelper.GeneratePositionXZ(distance);

                mosquito.Transformation = new Transformation(player.Transformation.Translate +
                    offset, Vector3.Zero, /*Vector3.One*/ new Vector3((float)1, (float)1, (float)1));
                mosquito.TransformationOld = mosquito.Transformation;

                mosquitoList.Add(mosquito);
            }

            return mosquitoList;
        }

        private static List<StaticUnit> ScatterStaticUnits(Game game, int numStatUnits,
            float minDistance, int distance, Player player)
        {
            List<StaticUnit> statUnitList = new List<StaticUnit>();

            for (int i = 0; i < numStatUnits; i++)
            {
                StaticUnit statUnit = new StaticUnit(game, UnitTypes.StaticUnitType.Tree);
                statUnit.Initialize();

                // Generate a random position
                Vector3 offset = RandomHelper.GeneratePositionXZ(distance);
                while (Math.Abs(offset.X) < minDistance && Math.Abs(offset.Z) < minDistance)
                    offset = RandomHelper.GeneratePositionXZ(distance);

                statUnit.Transformation = new Transformation(player.Transformation.Translate +
                    offset, Vector3.Zero, Vector3.One);

                statUnitList.Add(statUnit);
            }

            return statUnitList;
        }

        private static List<StaticUnit> PlaceStaticUnits(Game game, String LevelName)
        {
            List<StaticUnit> statUnitList = new List<StaticUnit>();

            FileStream levelStaticUnitsFS = new FileStream("Content/" + GameAssetsPath.LEVELS_PATH + LevelName + "/StaticUnits.list", FileMode.Open, FileAccess.Read);
            StreamReader levelStaticUnitsSR = new StreamReader(levelStaticUnitsFS);

            String str;
            while ((str = levelStaticUnitsSR.ReadLine()) != null)
            {
                if (str[0] == '#')
                {
                    //MessageBox.Show("Comment");
                    continue;
                }

                String [] strElems = str.Split(";".ToCharArray());
                Int32 type = Int32.Parse(strElems[0]);
                Int32 posX = Int32.Parse(strElems[1]);
                Int32 posZ = Int32.Parse(strElems[2]);
                Int32 width = Int32.Parse(strElems[3]);
                Int32 height = Int32.Parse(strElems[4]);

                //MessageBox.Show(type + " " + posX + " " + posY + " " + width + " " + height);

                StaticUnit statUnit = new StaticUnit(game, UnitTypes.StaticUnitType.Tree);
                statUnit.Initialize();
                statUnit.Transformation = new Transformation(new Vector3(posX, 0, posZ), Vector3.Zero, Vector3.One);

                statUnitList.Add(statUnit);
            }

            /*for (int i = 0; i < numStatUnits; i++)
            {
                StaticUnit statUnit = new StaticUnit(game, UnitTypes.StaticUnitType.Tree);
                statUnit.Initialize();

                // Generate a random position
                Vector3 offset = RandomHelper.GeneratePositionXZ(distance);
                while (Math.Abs(offset.X) < minDistance && Math.Abs(offset.Z) < minDistance)
                    offset = RandomHelper.GeneratePositionXZ(distance);

                statUnit.Transformation = new Transformation(player.Transformation.Translate +
                    offset, Vector3.Zero, Vector3.One);

                statUnitList.Add(statUnit);
            }*/

            return statUnitList;
        }

        private static List<Bounding2D> ReadBounding2D(String LevelName)
        {
            List<Bounding2D> Bounding2DList = new List<Bounding2D>();

            FileStream levelBounding2DFS = new FileStream("Content/" + GameAssetsPath.LEVELS_PATH + LevelName + "/Bounding2D.list", FileMode.Open, FileAccess.Read);
            StreamReader levelBounding2DSR = new StreamReader(levelBounding2DFS);

            String str;
            while ((str = levelBounding2DSR.ReadLine()) != null)
            {
                if (str[0] == '#')
                {
                    continue;
                }

                String[] strElems = str.Split(";".ToCharArray());
                Int32 type = Int32.Parse(strElems[0]);
                Int32 par1 = 0;
                Int32 par2 = 0;
                Int32 par3 = 0;
                Int32 par4 = 0;
                if (type == 0 || type == 1)
                {
                    par1 = Int32.Parse(strElems[1]);
                    par2 = Int32.Parse(strElems[2]);
                    par3 = Int32.Parse(strElems[3]);
                }
                if (type == 1)
                {
                    par4 = Int32.Parse(strElems[4]);
                }

                //MessageBox.Show(type + " " + par1 + " " + par2 + " " + par3 + " " + par4);

                Bounding2D bound2D = new Bounding2D(type, par1, par2, par3, par4);
                Bounding2DList.Add(bound2D);
            }

            return Bounding2DList;
        }
    }
}
