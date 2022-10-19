using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Linketix
{
    public class MyComponent1 : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public MyComponent1()
          : base("3Pt Star_Fractal", "3Pt Star_Fractal",
              "Generates a three pointed start Linkage pattern",
              "Linketix", "Linkage")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddNumberParameter("Length ", "Length", "length of the link", GH_ParamAccess.item, 1);
            pManager.AddIntegerParameter("Extent X ", "Ex", "Number of cells in x direction", GH_ParamAccess.item, 4);
            pManager.AddNumberParameter("Motion ", "M", "A number between 0 and 1", GH_ParamAccess.item, 0.2);


        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddLineParameter("Layer1", "L1", "output linkage in layer1", GH_ParamAccess.tree);

            pManager.AddLineParameter("Layer2", "L2", "output linkage in layer2", GH_ParamAccess.tree);
            
        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //import datas
            double Length = 1;
            DA.GetData(0, ref Length);

            int Ex = 4;
            DA.GetData(1,ref  Ex);
            //int Ey = 4;
            //DA.GetData(2, ref Ey);


            double Motion = 0.2;
            DA.GetData(2, ref Motion);

            Motion = Oricut.functions.Remap(Motion, 0, 1, 0, 0.105 * Math.PI);
            Point3d center_pt = new Point3d(0, 0, 0);

            //_______________________________________________________________________

            //generate base triangle 
            Circle c = new Circle(Plane.WorldXY, Length);
            Polyline base_triangle = Polyline.CreateInscribedPolygon(c, 3);
            Line l1 = new Line(new Point3d(0, 0, 0), base_triangle[0]);
            Line l2 = new Line(new Point3d(0, 0, 0), base_triangle[1]);
            Line l3 = new Line(new Point3d(0, 0, 0), base_triangle[2]);

            


            //construct base planes 
            //Plane p1 = new Plane(base_triangle[0], base_triangle[0] - center_pt);
            //Plane p2 = new Plane(base_triangle[1], base_triangle[1] - center_pt);
            //Plane p3 = new Plane(base_triangle[2], base_triangle[2] - center_pt);
            //constructt normal vectors 

            Vector3d N1 = base_triangle[0] - center_pt;
            Vector3d N2 = base_triangle[1] - center_pt;
            Vector3d N3 = base_triangle[2] - center_pt;

            

            List<Vector3d> Normals = new List<Vector3d>() { N1, N2, N3 };

            var Base_Rotation = Transform.Rotation(Motion * Math.PI, new Point3d(0, 0, 0));
            l1.Transform(Base_Rotation);
            l2.Transform(Base_Rotation);
            l3.Transform(Base_Rotation);



            var l11 = l1;
            var l12 = l2;
            var l13 = l3;


            //l1.Transform(Transform.Translation(5, 0, 0));
            //l2.Transform(Transform.Translation(5, 0, 0));
            //l3.Transform(Transform.Translation(5, 0, 0));

            l11.Transform(Transform.Mirror(l11.PointAt(0), new Vector3d(0, 1, 0)));
            l12.Transform(Transform.Mirror(l12.PointAt(0), new Vector3d(0, 1, 0)));
            l13.Transform(Transform.Mirror(l13.PointAt(0), new Vector3d(0, 1, 0)));

            
            double len = l2.PointAt(1).Y - l12.PointAt(1).Y;
            //create base length 
            double length_pt = l2.PointAt(1).Y;
            


            List<Line> MainTriangle = new List<Line>() { l1, l2, l3 };
            Plane p1 = new Plane(l1.PointAt(1), N1);
            Plane p2 = new Plane(l2.PointAt(1), N2);
            Plane p3 = new Plane(l3.PointAt(1), N3);

            List<Plane> MainPlanes = new List<Plane>() { p1,p2, p3 };


            
            List<List<Line>> baseLines = new List<List<Line>>() { new List<Line>() { l1, l2, l3 } };


            List<Line> alllines = new List<Line>();
            List<Line> alllines2 = new List<Line>();
    
            Transform Ex_Transform = Transform.Mirror(p1);

            List<Point3d> duplicate_pt = new List<Point3d>() { new Point3d(0, 0, 0) };
            List<List<Line>> bucket = new List<List<Line>>() { new List<Line>() { l1, l2, l3 } };
            List<List<bool>> bools = new List<List<bool>>() { new List<bool>() { true, true, true } };

            Grasshopper.DataTree<Line> layer1 = new Grasshopper.DataTree<Line>();
            Grasshopper.DataTree<Line> layer2 = new Grasshopper.DataTree<Line>();
            Grasshopper.Kernel.Data.GH_Path base_path = new Grasshopper.Kernel.Data.GH_Path(0,0,0);
            layer2.Add(l1, base_path);
            layer2.Add(l2, base_path);
            layer2.Add(l3, base_path);
            layer1.Add(l11, base_path);
            layer1.Add(l12, base_path);
            layer1.Add(l13, base_path);

            int direction = 1;
            for (int i = 1; i < Ex+1; i++)
            {
                direction *= -1;
                List<List<Line>> temp_bucket = new List<List<Line>>();
                List<List<bool>> temp_bools = new List<List<bool>>();
                for (int j = 0; j < bucket.Count; j++)
                {
                    

                    for (int k = 0; k < 3; k++)
                    {   //                                                      for each plane find base plane

                        if (bools[j][k])
                        {
                            Line indi = bucket[j][k];

                            Line mindi = indi;

                            Vector3d vvv = indi.PointAt(0) - indi.PointAt(1);
                            vvv.Rotate(direction * Motion * Math.PI, new Vector3d(0, 0, 1));
                            Plane mir = new Plane(indi.PointAt(1), vvv);
                            var mir_trans = Transform.Mirror(mir);

                            List<Line> new_module = new List<Line>();
                            List<bool> modules_bool = new List<bool>() { true, true, true };
                            Point3d mindi_pt = mindi.PointAt(1);
                            //mindi_pt.Transform(mir_trans);
                            //Point3d new_center = mindi_pt;

                            mindi_pt -= vvv;
                            
                            int count = 0;
                            for (int indices = 0; indices < duplicate_pt.Count; indices++)
                            {

                                double distance = mindi_pt.DistanceTo(duplicate_pt[indices]);
                                if (distance < 0.01)
                                {
                                    count += 1;
                                }

                            }

                            if (count == 0)
                            {
                                Grasshopper.Kernel.Data.GH_Path new_path = new Grasshopper.Kernel.Data.GH_Path(i,j,k);

                                duplicate_pt.Add(mindi_pt);
                                duplicate_pt.Add(bucket[j][k].PointAt(0));
                                for (int kk = 0; kk < 3; kk++)
                                {
                                    Line new_line = bucket[j][kk];
                                    
                                    new_line.Transform(mir_trans);
                                    Line layer1line = new_line;
                                    
                                    new_module.Add(new_line);
                                    Line layer2_line = new_line;
                                    layer2_line.Transform(Transform.Mirror(new Plane(new_line.PointAt(0), new Vector3d(0, 1, 0))));

                                    if (i % 2 == 0)
                                    {

                                        layer2.Add(layer1line, new_path);
                                        layer1.Add(layer2_line, new_path);
                                    }
                                    else
                                    {
                                        layer2.Add(layer2_line, new_path);
                                        layer1.Add(layer1line, new_path);

                                    
                                    }
                                    if (kk == k)
                                    {
                                        modules_bool[kk] = false;
                                    }
                                }


                                
                                
                                temp_bucket.Add(new_module);
                                temp_bools.Add(modules_bool);
                            }
                        }
                    }

                    

                    
                }
                bucket = temp_bucket;
                bools = temp_bools;


            }

            List<Line> alll = new List<Line>();

            for(int i = 0; i < baseLines.Count; i++)
            {

                for(int j = 0; j < 3; j++)
                {

                    alll.Add(baseLines[i][j]);
                }

            }

                
                




            DA.SetDataTree(0, layer1);
            DA.SetDataTree(1, layer2);


        }

        /// <summary>
        /// Provides an Icon for the component.
        /// </summary>
        protected override System.Drawing.Bitmap Icon
        {
            get
            {

                return Resource1.Artboard_1_copy_26;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("57340618-bf6f-4077-884b-42e41bf4cda4"); }
        }
    }
}