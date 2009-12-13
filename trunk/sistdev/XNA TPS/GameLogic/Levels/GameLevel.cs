using System;
using System.Collections.Generic;
using System.Text;

using sistdev.GameBase.Cameras;
using sistdev.GameBase.Lights;
using sistdev.GameBase.Shapes;
using sistdev.GameLogic;

namespace sistdev.GameLogic.Levels
{
    public struct GameLevel
    {
        // Cameras, Lights, Terrain and Sky
        public CameraManager CameraManager;
        public LightManager LightManager;
        public Terrain Terrain;
        public SkyDome SkyDome;

        // Player and enemies
        public Player Player;
        public List<Enemy> EnemyList;
        public List<Mosquito> MosquitoList;
        public List<StaticUnit> StaticUnitList;
        
        public List<Bounding2D> Bound2D;
    }
}
