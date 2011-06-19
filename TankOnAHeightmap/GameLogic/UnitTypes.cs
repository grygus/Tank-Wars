using System;
using System.Collections.Generic;
using System.Text;

namespace TanksOnAHeightmap.GameLogic
{
    public static class UnitTypes
    {
        // Player
        // ---------------------------------------------------------------------------
        public enum PlayerType
        {
            TankPlayer
        }
        public static int[] PlayerLife = { 100 };
        public static float[] PlayerSpeed = { 1.0f };

        // Player Weapons
        // ---------------------------------------------------------------------------
        public enum PlayerWeaponType
        {
            Canon
        }
        public static int[] BulletDamage = { 12 };
        public static int[] BulletsCount = { 300 };

        // Enemies
        // ---------------------------------------------------------------------------
        public enum EnemyType
        {
            TankEnemy
        }
        public static int[] EnemyLife = { 20 };
        public static float[] EnemySpeed = { 1.0f };
        public static int[] EnemyPerceptionDistance = { 700 };
        public static int[] EnemyAttackDistance = { 500 }; // 80
        public static int[] EnemyAttackDamage = { 8 };
    }
}
