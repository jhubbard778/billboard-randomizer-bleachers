using System;
using System.Collections.Generic;
using System.Text;

namespace BleacherRandomizer
{
    public class Bleacher {
        public double Top_Left_X { get; set; }
        public double Top_Left_Z { get; set; }
        public double Top_Right_X { get; set; }
        public double Top_Right_Z { get; set; }

        // Holds the Slope of the Vector that represents the Length of the Bleacher
        public double Length_Slope { get; set; }

        // Holds the Slope of the Vector that represents the Width of the Bleacher
        public double Width_Slope { get; set; }

        // Holds the Change in X per Step
        public double Step_Change_X { get; set; }
        // Holds the Change in Z per Step
        public double Step_Change_Z { get; set; }


        public static void Add_Bleacher(double x, double z, double right_x, double right_z,
            double l_slope, double w_slope, double dx, double dz) {
            GlobalVariables.bleachers_list.Add(new Bleacher {
                Top_Left_X = x,
                Top_Left_Z = z,
                Top_Right_X = right_x,
                Top_Right_Z = right_z,
                Length_Slope = l_slope,
                Width_Slope = w_slope,
                Step_Change_X = dx,
                Step_Change_Z = dz
            });
        }

    }
}
