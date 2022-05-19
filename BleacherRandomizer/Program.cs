using System;
using System.Collections.Generic;
using System.IO;

namespace BleacherRandomizer {
    class Program {
        static int Main(string[] args) {
            while (!GlobalVariables.STATUES_EXIST) {
                Prompt_Statues();
            }

            // Add all billboards to a billboards list
            Billboard.Add_All_Billboards();

            Console.WriteLine("\nFinding bleachers...");
            StreamReader statues_file = File.OpenText(Environment.CurrentDirectory + "\\statues");
            int Num_Bleachers = 0;
            int line_num = 0;

            // Read each bleacher into a bleacher list
            while (statues_file.EndOfStream == false) {
                line_num++;
                string line = statues_file.ReadLine();

                if (!line.Contains(GlobalVariables.BLEACHER_PATH)) {
                    continue;
                }

                Num_Bleachers++;

                // Get The Coords and Angle of Each Bleacher
                string[] line_args = line.Split(' ');
                if ((!line_args[0].Contains('[') && !line_args[2].Contains(']')) || line_args.Length != 7) {
                    Console.WriteLine("\x1b[31mFormat error line " + line_num + "\x1b[0m");
                    continue;
                }

                line_args[0] = line_args[0].Replace("[", string.Empty);
                line_args[2] = line_args[2].Replace("]", string.Empty);
                bool can_parse = true;

                if (!double.TryParse(line_args[0], out double center_x)) can_parse = false;
                if (!double.TryParse(line_args[1], out double center_y)) can_parse = false;
                if (!double.TryParse(line_args[2], out double center_z)) can_parse = false;
                if (!double.TryParse(line_args[3], out double angle)) can_parse = false;

                if (!can_parse) {
                    Console.WriteLine("\x1b[31mCoordinate/Angle error on line " + line_num + "\x1b[0m");
                    continue;
                }

                Calculate_Slopes_And_Add_Bleacher(center_x, center_y, center_z, angle);
            }

            if (Num_Bleachers == 0) {
                Console.WriteLine("\x1b[31mNo Bleachers Found!\x1b[0m");
                return -1;
            }


            Console.WriteLine("\x1b[32mCompleted\x1b[0m");
            return 0;
        }

        private static void Prompt_Statues() {
            Console.WriteLine("> Copy Statues File into Application Directory");
            Console.WriteLine("> Press any key to continue...");
            Console.ReadKey(true);
            if (File.Exists(Environment.CurrentDirectory + "\\statues")) {
                GlobalVariables.STATUES_EXIST = true;
                return;
            }
            Console.WriteLine("\n\x1b[31m> No File named 'statues' found!\x1b[0m\n");
        }

        private static void Calculate_Slopes_And_Add_Bleacher(double center_x, double center_y, double center_z, double angle) {

            // Origin (0,0) in MX Simulator is top left, so we need to flip our z-axis before rotation
            center_z = -center_z;
            
            // Calculate Upper Left Hand Corner (Max Height, Leftmost Seat), Upper Right Hand Corner, and Bottom Left Hand Corner
            double top_left_x = center_x - (0.5 * GlobalVariables.BLEACHER_LENGTH);
            double top_left_z = center_z + (0.5 * GlobalVariables.BLEACHER_WIDTH);
            double padded_top_left_x = top_left_x + GlobalVariables.EDGE_PADDING;
            double padded_top_left_z = top_left_z - (1.7 * GlobalVariables.BLEACHER_STEP_WIDTH);

            double top_right_x = center_x + (0.5 * GlobalVariables.BLEACHER_LENGTH);
            double top_right_z = top_left_z;
            double padded_top_right_x = top_right_x - GlobalVariables.EDGE_PADDING;
            double padded_top_right_z = top_right_z - (1.7 * GlobalVariables.BLEACHER_STEP_WIDTH);

            double bottom_left_x = top_left_x;
            double bottom_left_z = center_z - (0.5 * GlobalVariables.BLEACHER_WIDTH);

            /* Formula for Rotation around arbitrary Center: 
                x1 = (x0 – xc)cos(θ) – (z0 – zc)sin(θ) + xc
                z1 = (x0 – xc)sin(θ) + (z0 – zc)cos(θ) + zc
            Rotate by theta to get new vector coords */
            double rotated_top_left_x = Rotate_X(top_left_x, top_left_z, center_x, center_z, angle);
            double rotated_top_left_z = Rotate_Z(top_left_x, top_left_z, center_x, center_z, angle);
            double padded_rot_top_left_x = Rotate_X(padded_top_left_x, padded_top_left_z, center_x, center_z, angle);
            double padded_rot_top_left_z = Rotate_Z(padded_top_left_x, padded_top_left_z, center_x, center_z, angle);

            double rotated_bottom_left_x = Rotate_X(bottom_left_x, bottom_left_z, center_x, center_z, angle);
            double rotated_bottom_left_z = Rotate_Z(bottom_left_x, bottom_left_z, center_x, center_z, angle);

            double rotated_top_right_x = Rotate_X(top_right_x, top_right_z, center_x, center_z, angle);
            double rotated_top_right_z = Rotate_Z(top_right_x, top_right_z, center_x, center_z, angle);
            double padded_rot_top_right_x = Rotate_X(padded_top_right_x, padded_top_right_z, center_x, center_z, angle);

            // Vector Slope for the length and width
            double vector_slope_length = Get_Slope(rotated_top_left_x, rotated_top_left_z, rotated_top_right_x, rotated_top_right_z);
            double vector_slope_width = Get_Slope(rotated_top_left_x, rotated_top_left_z, rotated_bottom_left_x, rotated_bottom_left_z);

            // Flip the Z's back to Sim Origin Plane
            padded_rot_top_left_z = Math.Abs(padded_rot_top_left_z);

            double[] step_changes = new double[2];
            if (Double.IsNaN(vector_slope_width)) {

                // For Undefined Slope
                int multiplier = 1;
                // If the angle is 3pi/2 then we travel negative in the X direction
                if (Math.Round((angle % (Math.PI * 2)), 3) == Math.Round(((3 * Math.PI) / 2), 3)) {
                    multiplier = -1;
                }
                step_changes[0] = multiplier * GlobalVariables.BLEACHER_STEP_WIDTH;
                step_changes[1] = 0;
                
                // For Zero Slope
                if (rotated_top_left_z != rotated_bottom_left_z) {
                    if (Math.Round((angle % (Math.PI * 2)), 3) == 0) {
                        multiplier = -1;
                    }
                    step_changes[0] = 0;
                    step_changes[1] = multiplier * GlobalVariables.BLEACHER_STEP_WIDTH;
                }

            } else step_changes = Get_Step_Changes(vector_slope_width);

            Bleacher.Add_Bleacher(padded_rot_top_left_x, center_y, padded_rot_top_left_z, padded_rot_top_right_x,
                vector_slope_length, vector_slope_width, step_changes[0], step_changes[1]);
        }

        private static double Rotate_X(double x0, double z0, double xc, double zc, double angle) { 
            return (x0 - xc) * Math.Cos(angle) - (z0 - zc) * Math.Sin(angle) + xc;
        }
        private static double Rotate_Z(double x0, double z0, double xc, double zc, double angle) {
            return (x0 - xc) * Math.Sin(angle) + (z0 - zc) * Math.Cos(angle) + zc;
        }
        private static double Get_Slope(double x1, double y1, double x2, double y2) {
            // If we have undefined slope or 0 slope, return so
            if (Math.Round(x1, 8) == Math.Round(x2, 8) || Math.Round(y1,8) == Math.Round(y2, 8)) {
                return double.NaN;
            }
            return ((y2 - y1) / (x2 - x1));
        }

        private static double[] Get_Step_Changes(double slope_W) {
            double[] steps = new double[2];

            // Slope is RISE/RUN So we can use our slope value as RISE and 1 as our RUN
            // Now we calculate the magnitude of a vector with components <1,slope>

            // 1) Calculate Standard Magnitude of Width
            double mag_W = Math.Sqrt(Math.Pow(slope_W, 2) + 1);

            // 2) Divide desired magnitude by standard magnitude
            double factor_Width = GlobalVariables.BLEACHER_STEP_WIDTH / mag_W;

            // 3) Multiply Standard Vector Components by Factor
            double x = 1 * factor_Width;
            double z = slope_W * factor_Width;

            steps[0] = x;
            steps[1] = z;

            return steps;
        }

    }
}
