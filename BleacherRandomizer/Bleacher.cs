using System;
using System.Collections.Generic;
using System.Text;

namespace BleacherRandomizer
{
    public class Bleacher {
        public double Top_Left_X { get; set; }
        public double Y { get; set; }
        public double Top_Left_Z { get; set; }
        public double Top_Right_X { get; set; }
        // Holds the Slope of the Vector between the X points
        public double Length_Slope { get; set; }
        // Holds the Slope of the Vector between the Z points
        public double Width_Slope { get; set; }
        // Holds the Change in X per Step
        public double Change_X { get; set; }
        // Holds the Change in Z per Step
        public double Change_Z { get; set; }


        public static void Add_Bleacher(double x, double y, double z, double right_x, double l_slope, double w_slope, double dx, double dz) {
            GlobalVariables.bleachers_list.Add(new Bleacher {
                Top_Left_X = x,
                Y = y,
                Top_Left_Z = z,
                Top_Right_X = right_x,
                Length_Slope = l_slope,
                Width_Slope = w_slope,
                Change_X = dx,
                Change_Z = dz
            });
        }

    }
}
