using System;
using System.Collections.Generic;
using System.Text;

namespace BleacherRandomizer
{
    public class GlobalVariables {
        public const string BLEACHER_PATH = "@os2022bgsxobj/statue/other/estrades.jm @os2022bgsxobj/statue/other/estrades.png null";
        public const double BLEACHER_LENGTH = 69.48;
        public const double BLEACHER_WIDTH = 25.03;
        public const double BLEACHER_STEP_WIDTH = 1.47235294;
        public const double EDGE_PADDING = 2;
        public const double BILLBOARD_PADDING = 4;

        public struct Step_Heights {
            public const double STEP_1 = 0.5;
            public const double STEP_2 = 1.75;
            public const double STEP_3 = 3;
            public const double STEP_4 = 4.25;
            public const double STEP_5 = 5.5;
            public const double STEP_6 = 6.75;
            public const double STEP_7 = 7.75;
            public const double STEP_8 = 9;
        }
        public static bool STATUES_EXIST = false;

        public static List<Billboard> billboards_list = new List<Billboard>();
        public static List<Bleacher> bleachers_list = new List<Bleacher>();
        public const string CROWD_BILLBOARD_PATH = "@os2022bgsxobj/billboards/crowd/";
    }
}
