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

            Console.WriteLine("Bleachers Added...");
            Console.WriteLine("\nAdding Billboards...");
            GlobalVariables.billboard_outfile = File.CreateText(Environment.CurrentDirectory + "\\billboards_out");
            Random Random_Num = new Random();
            // Go through the bleachers list
            for (int i = 0; i < GlobalVariables.bleachers_list.Count; i++) {
                Bleacher current_bleacher = GlobalVariables.bleachers_list[i];
                Console.WriteLine("\n\tAdding to bleacher " + (i + 1).ToString() + "...");
                
                double min_x = current_bleacher.Top_Left_X;
                double max_x = current_bleacher.Top_Right_X;
                double min_z = current_bleacher.Top_Left_Z;
                double max_z = current_bleacher.Top_Right_Z;

                // Go through each bleacher
                for (int current_row = GlobalVariables.NUM_OF_STEPS; current_row > 0; current_row--) {
                    Console.WriteLine("\t\tAdding billboards to row " + current_row.ToString() + "...");
                    GlobalVariables.current_row_billboards = new List<Billboard_Coords>();
                    GlobalVariables.billboard_distances = new List<double>();

                    double max_distance_between_billboards = GlobalVariables.BLEACHER_LENGTH;
                    // While we can fit another billboard between two billboards
                    while (max_distance_between_billboards > 2 * GlobalVariables.BILLBOARD_PADDING) {
                        double x, y, z;
                        int index;

                        // If we have a constant X set the X to the min_x and get random Z
                        if (min_x == max_x) {
                            x = min_x;
                            z = (Random_Num.NextDouble() * (max_z - min_z)) + min_z;
                            if (!Check_Distances(x, z)) continue;
                            double distance = Get_Distance(min_x, min_z, x, z);
                            index = Add_To_Distances_List(distance);
                        } else {
                            // Get random X if min_x != max_x
                            x = (Random_Num.NextDouble() * (max_x - min_x)) + min_x;
                            z = min_z;
                            // If we do not have a constant Z calculate the Z based on the slope of the length and X value
                            if (min_z != max_z) {
                                // y - y1 = m(x-x1), but y axis flipped in sim.  So y + y1 = m(x-x1)
                                z = Math.Abs(current_bleacher.Length_Slope * (x - min_x) - min_z);
                            }
                            if (!Check_Distances(x, z)) continue;
                            double distance = Get_Distance(min_x, min_z, x, z);
                            index = Add_To_Distances_List(distance);
                        }

                        y = GlobalVariables.STEP_HEIGHTS[current_row - 1];
                        // Add the billboard to billboard coords list
                        GlobalVariables.current_row_billboards.Insert(index, new Billboard_Coords {
                            X_coord = x,
                            Y_coord = y,
                            Z_coord = z
                        });

                        max_distance_between_billboards = Get_Max_Distance_Between_Billboards(min_x, min_z, max_x, max_z);

                    }

                    Write_Current_Row_Billboards(min_x, min_z, max_x, max_z, current_row, i);

                    min_x += current_bleacher.Step_Change_X;
                    max_x += current_bleacher.Step_Change_X;
                    min_z += current_bleacher.Step_Change_Z;
                    max_z += current_bleacher.Step_Change_Z;
                    
                }
            }
            // Close the StreamReader
            GlobalVariables.billboard_outfile.Close();
            Console.WriteLine("\n\x1b[32mCompleted\x1b[0m");
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
            // Since the bleacher isn't exactly a perfect rectangle, we'll use 1.7 * the bleacher step width to get
            // to the first step where we actually want to place crowd members.  1.5 would be the ideal number though.
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
            double padded_rot_top_right_z = Rotate_Z(padded_top_right_x, padded_top_right_z, center_x, center_z, angle);

            // Vector Slope for the length and width
            double vector_slope_length = Get_Slope(rotated_top_left_x, rotated_top_left_z, rotated_top_right_x, rotated_top_right_z);
            double vector_slope_width = Get_Slope(rotated_top_left_x, rotated_top_left_z, rotated_bottom_left_x, rotated_bottom_left_z);

            // Flip the Z's back to Sim Origin Plane
            padded_rot_top_left_z = Math.Abs(padded_rot_top_left_z);
            padded_rot_top_right_z = Math.Abs(padded_rot_top_right_z);

            // Calculate step changes X and Z for the width
            double[] step_changes = new double[2];
            if (Double.IsNaN(vector_slope_width)) {

                // For Undefined Slope
                int multiplier = 2;
                // If the angle is 3pi/2 then we travel negative in the X direction
                if (Math.Round((angle % (Math.PI * 2)), 3) == Math.Round(((3 * Math.PI) / 2), 3)) {
                    multiplier = -2;
                }
                step_changes[0] = multiplier * GlobalVariables.BLEACHER_STEP_WIDTH;
                step_changes[1] = 0;
                
                // For Zero Slope
                if (rotated_top_left_z != rotated_bottom_left_z) {
                    // If the angle is pi then we travel in the negative Z direction
                    if (Math.Round((angle % (Math.PI * 2)), 3) == Math.Round(Math.PI, 3)) {
                        multiplier = -2;
                    }
                    step_changes[0] = 0;
                    step_changes[1] = multiplier * GlobalVariables.BLEACHER_STEP_WIDTH;
                }

            } else step_changes = Get_Step_Changes(vector_slope_length, vector_slope_width);

            Bleacher.Add_Bleacher(padded_rot_top_left_x, padded_rot_top_left_z, padded_rot_top_right_x, 
                padded_rot_top_right_z, vector_slope_length, vector_slope_width, step_changes[0], step_changes[1]);
        }

        private static double[] Get_Step_Changes(double slope_L, double slope_W) {
            double[] steps = new double[2];

            // Slope is RISE/RUN So we can use our slope value as RISE and 1 as our RUN
            // Now we calculate the magnitude of a vector with components <1,slope>


            // 1) Calculate Standard Magnitude of Width
            double mag_W = Math.Sqrt(Math.Pow(slope_W, 2) + 1);

            // 2) Divide desired magnitude by standard magnitude
            double factor_Width = (2 * GlobalVariables.BLEACHER_STEP_WIDTH) / mag_W;

            // 3) Multiply Standard Vector Components by Factor, Z is inverted so we need to flip it to sim axis
            double x = 1 * factor_Width;
            // We have to use -1 * the number instead of Math.Abs because there is the possiblity where we need to decrease
            // the z value, we can't always increase
            double z = -1 * (slope_W * factor_Width);

            // if our length slope is negative flip the signs of X and Z
            if (slope_L < 0) {
                x *= -1;
                if (slope_W < 0) {
                    z *= -1;
                }
            }

            steps[0] = x;
            steps[1] = z;

            return steps;
        }

        private static bool Check_Distances(double x1, double z1) {
            // if we don't have any billboards yet just add it
            if (GlobalVariables.current_row_billboards.Count == 0) return true;

            // Calculate the distance from x and z from each point in the billboard coords
            for (int i = 0; i < GlobalVariables.current_row_billboards.Count; i++) {
                double x2, z2, distance;
                x2 = GlobalVariables.current_row_billboards[i].X_coord;
                z2 = GlobalVariables.current_row_billboards[i].Z_coord;
                distance = Get_Distance(x1, z1, x2, z2);
                // If the distance between the billboard and any other coordinate is less than
                // the billboard padding we will return false
                if (distance < GlobalVariables.BILLBOARD_PADDING) return false;
            }
            return true;
        }

        private static int Add_To_Distances_List(double distance) {
            int count = GlobalVariables.billboard_distances.Count;
            if (count == 0) {
                GlobalVariables.billboard_distances.Insert(0, distance);
                return 0;
            }

            // If the distance is shorter than the lowest distance
            if (distance < GlobalVariables.billboard_distances[0]) {
                GlobalVariables.billboard_distances.Insert(0, distance);
                return 0;
            }

            for (int i = 0; i < count; i++) {
                // if we've reached the end
                if (i == count - 1) {
                    // and the distance is greater, then add
                    if (distance > GlobalVariables.billboard_distances[i]) {
                        GlobalVariables.billboard_distances.Add(distance);
                        return i + 1;
                    }
                    // If it's less, then insert
                    GlobalVariables.billboard_distances.Insert(i, distance);
                    return i;
                }
                // If the distance is greater than the current item but less than the next item, insert after current item
                if (distance > GlobalVariables.billboard_distances[i] && distance < GlobalVariables.billboard_distances[i + 1]) {
                    GlobalVariables.billboard_distances.Insert(i + 1, distance);
                    return i + 1;
                }
            }

            // Upon error
            return -1;
        }

        private static double Get_Max_Distance_Between_Billboards(double origin_x, double origin_z, double end_x, double end_z) {
            int count = GlobalVariables.current_row_billboards.Count;
            double max_distance = 0;
            double x1, z1, x2, z2, distance;

            // First get the distance from the origin to the first billboard
            x1 = origin_x;
            z1 = origin_z;
            x2 = GlobalVariables.current_row_billboards[0].X_coord;
            z2 = GlobalVariables.current_row_billboards[0].Z_coord;
            distance = Get_Distance(x1, z1, x2, z2);
            // This will help us decide in our loop if we can fit a billboard between the origin and the first billboard
            // since we don't need to 'fit' another billboard between two billboards
            distance *= 2;
            if (distance > max_distance) max_distance = distance;

            for (int i = 0; i < count - 1; i++) {
                // Get the distance between each billboard
                x1 = GlobalVariables.current_row_billboards[i].X_coord;
                z1 = GlobalVariables.current_row_billboards[i].Z_coord;
                x2 = GlobalVariables.current_row_billboards[i+1].X_coord;
                z2 = GlobalVariables.current_row_billboards[i+1].Z_coord;
                distance = Get_Distance(x1, z1, x2, z2);
                if (distance > max_distance) max_distance = distance;
            }

            // Finally get the distance between the last billboard and the edge
            x1 = GlobalVariables.current_row_billboards[count-1].X_coord;
            z1 = GlobalVariables.current_row_billboards[count-1].Z_coord;
            x2 = end_x;
            z2 = end_z;
            distance = Get_Distance(x1, z1, x2, z2);
            // Same thing as origin, figure out if we can fit a billboard between last billboard and end point
            distance *= 2;
            if (distance > max_distance) max_distance = distance;

            return max_distance;
        }

        private static void Write_Current_Row_Billboards(double min_x, double min_z, double max_x, double max_z, int current_row, int bleacher) {

            List<Billboard_Coords> row_coords = GlobalVariables.current_row_billboards;
            int count = row_coords.Count;

            GlobalVariables.billboard_outfile.WriteLine("\nBleacher " + (bleacher + 1).ToString() + " - Row " + current_row.ToString());
            GlobalVariables.billboard_outfile.WriteLine("\tTOP LEFT COORDINATE: (" + min_x.ToString() + ", " + min_z.ToString() + ')');
            GlobalVariables.billboard_outfile.WriteLine("\tTOP RIGHT COORDINATE: (" + max_x.ToString() + ", " + max_z.ToString() + ')');

            // Go through the current row billboards and output the coords
            for (int i = 0; i < count; i++) {
                GlobalVariables.billboard_outfile.WriteLine("\t\t(" + row_coords[i].X_coord.ToString() + ", " + 
                    row_coords[i].Y_coord.ToString() + ", " + row_coords[i].Z_coord.ToString() + ')');
            }

        }

        private static double Rotate_X(double x0, double z0, double xc, double zc, double angle) {
            return (x0 - xc) * Math.Cos(angle) - (z0 - zc) * Math.Sin(angle) + xc;
        }
        private static double Rotate_Z(double x0, double z0, double xc, double zc, double angle) {
            return (x0 - xc) * Math.Sin(angle) + (z0 - zc) * Math.Cos(angle) + zc;
        }
        private static double Get_Distance(double x1, double y1, double x2, double y2) {
            return Math.Sqrt(Math.Pow(x2 - x1, 2) + Math.Pow(y2 - y1, 2));
        }
        private static double Get_Slope(double x1, double y1, double x2, double y2) {
            // If we have undefined slope or 0 slope, return so
            if (Math.Round(x1, 8) == Math.Round(x2, 8) || Math.Round(y1, 8) == Math.Round(y2, 8)) {
                return double.NaN;
            }
            return ((y2 - y1) / (x2 - x1));
        }
    }
}
