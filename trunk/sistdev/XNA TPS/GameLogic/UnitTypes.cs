using System;
using System.Collections.Generic;
using System.Text;

namespace sistdev.GameLogic
{
    public static class UnitTypes
    {
        // Player
        // ---------------------------------------------------------------------------
        public enum PlayerType
        {
            Frog
        }
        public static string[] PlayerModelFileName = { "funky_old" };
        public static int[] PlayerLife = { 100 };
        public static float[] PlayerSpeed = { 1.0f };

        // Enemies
        // ---------------------------------------------------------------------------
        public enum EnemyType
        {
            Beast
        }
        public static string[] EnemyModelFileName = { "EnemyBeast" };
        public static int[] EnemyLife = { 150 };
        public static float[] EnemySpeed = { 1.0f };
        public static int[] EnemyPerceptionDistance = { 120 };
        public static int[] EnemyAttackDistance = { 30 };
        public static int[] EnemyAttackDamage = { 8 };

        // Enemies
        // ---------------------------------------------------------------------------
        public enum MosquitoType
        {
            Mosquito
        }
        public static string[] MosquitoModelFileName = { "EnemyBeast" };
        public static int[] MosquitoLife = { 150 };
        public static float[] MosquitoSpeed = { 1.0f };
        public static int[] MosquitoPerceptionDistance = { 120 };
        public static int[] MosquitoAttackDistance = { 30 };
        public static int[] MosquitoAttackDamage = { 8 };


        // Trees
        // ---------------------------------------------------------------------------
        public enum StaticUnitType
        {
            Tree
        }
        public static string[] TreeModelFileName = { /*"Tee"*/ "PlayerMarine" };
    }
}
