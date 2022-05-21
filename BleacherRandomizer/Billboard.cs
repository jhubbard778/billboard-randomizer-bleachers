using System;
using System.Collections.Generic;
using System.Text;

namespace BleacherRandomizer
{
    public class Billboard {
        public double Billboard_Size { get; set; }
        public double Aspect { get; set; }
        public string PNG { get; set; }

        public static void Add_All_Billboards() {
            // Adds 1-6.png as they all have same aspect ratio and size
            for (int i = 0; i < 6; i++) {
                Add_Billboard(7, 1, (i + 1).ToString() + ".png");
            }
            Add_Billboard(8, 0.7, "7.png");
            Add_Billboard(7, 0.7, "8.png");
            Add_Billboard(7, 0.5, "9.png");
            Add_Billboard(8, 1, "dude.png");
            Add_Billboard(9, 1, "g1.png");
            Add_Billboard(9, 1, "g2.png");
            Add_Billboard(9, 1, "g3.png");
            Add_Billboard(9, 1, "g4.png");
            Add_Billboard(7, 1.5, "g5.png");
            Add_Billboard(11, 1, "g6.png");
            Add_Billboard(9, 1, "groupsit.png");
            Add_Billboard(7, 0.5, "rogan.png");
            Add_Billboard(7, 0.5, "rogan2.png");
        }

        private static void Add_Billboard(double size, double aspect, string png) {
            GlobalVariables.billboards_list.Add(new Billboard {
                Billboard_Size = size,
                Aspect = aspect,
                PNG = GlobalVariables.CROWD_BILLBOARD_PATH + png
            });
        }
    }

    public class Billboard_Coords {
        public double X_coord { get; set; }
        public double Y_coord { get; set; }
        public double Z_coord { get; set; }
    }

}
