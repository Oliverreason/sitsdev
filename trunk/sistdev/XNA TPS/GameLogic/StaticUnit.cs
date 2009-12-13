using System;
using System.Collections.Generic;
using System.Text;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using sistdev.GameBase;
using sistdev.GameBase.Cameras;
using sistdev.GameBase.Shapes;
using sistdev;

namespace sistdev.GameLogic
{
    public class StaticUnit : TerrainUnit
    {
        UnitTypes.StaticUnitType statUnitType;

        #region Properties
        
        #endregion

        public StaticUnit(Game game, UnitTypes.StaticUnitType _type)
            : base(game)
        {
            statUnitType = _type;
        }

        protected override void LoadContent()
        {
            Load(UnitTypes.TreeModelFileName[(int)statUnitType]);

            // Unit configurations
            SetAnimation(0, false, true, false);

            base.LoadContent();
        }

        public override void Update(GameTime time)
        {
            base.Update(time);
        }

        public override void Draw(GameTime time)
        {
            base.Draw(time);
        }
    }
}
