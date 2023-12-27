using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WindyGridWorld
{
    public class Windy
    {
        public static int WORLD_HEIGHT = 7;
        public static int WORLD_WIDTH = 10;

        public static int[] WIND = { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
        //public static int[] WIND = { 0, 0, 0, 1, 1, 1, 2, 2, 1, 0 };
        //public static int[] WIND = { 1, 1, 2, 2, 1, 1, 0, 0, 0, 0 };

        public const int ACTION_UP = 0;
        public const int ACTION_DOWN = 1;
        public const int ACTION_LEFT = 2;
        public const int ACTION_RIGHT = 3;

        static double REWARD = -1.0;

        public static int[,] START = { { 3, 0 } };
        public static int[,] GOAL = { { 3, 7 } };


        static int[] ACTIONS = { ACTION_UP, ACTION_DOWN, ACTION_LEFT, ACTION_RIGHT };

        public static StepResult step(int[,] state, int action)
        {
            int i = state[0, 0];
            int j = state[0, 1];


            if (action == ACTION_UP)
            {
                state = new int[,] { { Math.Max(i - 1 - WIND[j], 0), j } };
            }
            else if (action == ACTION_DOWN)
            {
                state = new int[,] { { Math.Max(Math.Min(i + 1 - WIND[j], WORLD_HEIGHT - 1), 0), j } };
            }
            else if (action == ACTION_LEFT)
            {
                state = new int[,] { { Math.Max(i - WIND[j], 0), Math.Max(j - 1, 0) } };
            }
            else if (action == ACTION_RIGHT)
            {
                state = new int[,] { { Math.Max(i - WIND[j], 0), Math.Min(j + 1, WORLD_WIDTH - 1) } };
            }

            double reward = REWARD;

            if (state[0, 0] == GOAL[0, 0] && state[0, 1] == GOAL[0, 1])
            {
                reward = 50;
            }

            return new StepResult() { State = state, Reward = reward };
        }

    }
}
