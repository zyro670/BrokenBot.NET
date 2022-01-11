using System;
using PKHeX.Core;

namespace SysBot.Pokemon
{
    public class BallPouchUtil
    {
        private int Master, Poke, Beast, Dive, Dream, Dusk, Fast, Friend, Great, Heal, Heavy, Level, Love, Lure, Luxury, Moon, Nest, Net, Premier, Quick, Repeat, Timer, Ultra, Sport, Safari;

        private readonly ushort[] Pouch_Ball_SWSH =
        {
            001, 002, 003, 004, 005, 006, 007, 008, 009, 010, 011, 012, 013, 014, 015, 016,
            492, 493, 494, 495, 496, 497, 498, 499, 500,
            576,
            851,
        };

        private InventoryPouch8 GetBallPouch(byte[] ballBlock)
        {
            var pouch = new InventoryPouch8(InventoryType.Balls, Pouch_Ball_SWSH, 999, 0, 28);
            pouch.GetPouch(ballBlock);
            return pouch;
        }

        public BallPouchUtil GetBallCounts(byte[] ballBlock)
        {
            var pouch = GetBallPouch(ballBlock);
            return ReadCounts(pouch);
        }

        private BallPouchUtil ReadCounts(InventoryPouch pouch)
        {
            var counts = new BallPouchUtil();
            foreach (var ball in pouch.Items)
                counts.SetCount(ball.Index, ball.Count);
            return counts;
        }

        private void SetCount(int ball, int count)
        {
            switch (ball)
            {
                case 1: Master = count; break;
                case 2: Ultra = count; break;
                case 3: Great = count; break;
                case 4: Poke = count; break;
                case 5: Safari = count; break;
                case 6: Net = count; break;
                case 7: Dive = count; break;
                case 8: Nest = count; break;
                case 9: Repeat = count; break;
                case 10: Timer = count; break;
                case 11: Luxury = count; break;
                case 12: Premier = count; break;
                case 13: Dusk = count; break;
                case 14: Heal = count; break;
                case 15: Quick = count; break;
                case 492: Fast = count; break;
                case 493: Level = count; break;
                case 494: Lure = count; break;
                case 495: Heavy = count; break;
                case 496: Love = count; break;
                case 497: Friend = count; break;
                case 498: Moon = count; break;
                case 499: Sport = count; break;
                case 576: Dream = count; break;
                case 851: Beast = count; break;
            };
        }

        public int PossibleCatches(Ball ball)
        {
            return ball switch
            {
                Ball.Master => Master,
                Ball.Poke => Poke,
                Ball.Beast => Beast,
                Ball.Dive => Dive,
                Ball.Dream => Dream,
                Ball.Dusk => Dusk,
                Ball.Fast => Fast,
                Ball.Friend => Friend,
                Ball.Great => Great,
                Ball.Heal => Heal,
                Ball.Heavy => Heavy,
                Ball.Level => Level,
                Ball.Love => Love,
                Ball.Lure => Lure,
                Ball.Luxury => Luxury,
                Ball.Moon => Moon,
                Ball.Nest => Nest,
                Ball.Net => Net,
                Ball.Premier => Premier,
                Ball.Quick => Quick,
                Ball.Repeat => Repeat,
                Ball.Timer => Timer,
                Ball.Ultra => Ultra,
                Ball.Safari => Safari,
                Ball.Sport => Sport,
                _ => throw new ArgumentOutOfRangeException(nameof(Ball))
            };
        }

        public int BallIndex(int ball) => Pouch_Ball_SWSH[ball == 25 || ball == 26 ? ball : ball - 1];
    }
}