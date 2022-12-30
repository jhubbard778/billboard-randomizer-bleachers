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
            // Adds 1-12.png as they all have same aspect ratio and size
            for (int i = 0; i < 6; i++) {
                Add_Billboard(6, 1, (i + 1).ToString() + ".png");
            }
            Add_Billboard(7.5, 1, "7.png");
            Add_Billboard(7, 1, "8.png");
            Add_Billboard(7, 1, "9.png");
            Add_Billboard(7, 1, "dude.png");
            Add_Billboard(9, 1, "g2.png");
            Add_Billboard(8, 1, "g5.png");
            Add_Billboard(7, 1, "g6.png");
            Add_Billboard(8, 1, "groupsit.png");
            Add_Billboard(7, 1, "rogan.png");
            Add_Billboard(7, 1, "rogan2.png");
            Add_Billboard(7, 0.5, "decimation.png", "@os2022bgsxrd6virginiabeach/billboard/");
            Add_Billboard(7, 0.5, "objectification.png", "@os2022bgsxrd6virginiabeach/billboard/");
            Add_Billboard(7, 1, "greg.png", "@os2022bgsxrd6virginiabeach/billboard/");
        }

        private static void Add_Billboard(double size, double aspect, string png, string path = GlobalVariables.CROWD_BILLBOARD_PATH) {
            GlobalVariables.billboards_list.Add(new Billboard {
                Billboard_Size = size,
                Aspect = aspect,
                PNG = path + png
            });
        }
    }

    public class Billboard_Coords {
        public double X_coord { get; set; }
        public double Y_coord { get; set; }
        public double Z_coord { get; set; }
    }

}
