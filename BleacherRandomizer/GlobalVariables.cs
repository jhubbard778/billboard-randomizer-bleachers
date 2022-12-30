using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace BleacherRandomizer
{
    public class GlobalVariables {
        public const string BLEACHER_PATH = "@os2022bgsxfobj/statue/other/estrades.jm @os2022bgsxfobj/statue/other/estrades.png null";
        // Bleacher dimensions in ft
        public const double BLEACHER_LENGTH = 69.48;
        public const double BLEACHER_WIDTH = 25.03;
        public const double BLEACHER_STEP_WIDTH = 1.47235294;
        
        // Min distance from edge to first/last billboard in ft
        public const double EDGE_PADDING = 2;
        // Min distance between two billboards in ft
        public const double BILLBOARD_PADDING = 4;

        // heights of the individual steps in ft
        public readonly static double[] STEP_HEIGHTS = new double[] { 0.5, 1.75, 3, 4.25, 5.5, 6.75, 7.75, 9 };
        public readonly static int NUM_OF_STEPS = STEP_HEIGHTS.Length;
        public static bool STATUES_EXIST = false;

        public static List<Billboard> billboards_list = new List<Billboard>();
        public static List<Bleacher> bleachers_list = new List<Bleacher>();
        public const string CROWD_BILLBOARD_PATH = "@os2022bgsxfobj/billboards/crowd/";

        public static List<Billboard_Coords> current_row_billboards;
        public static List<double> billboard_distances;

        public static StreamWriter billboard_outfile;
    }
}
