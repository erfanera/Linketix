using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Linketix
{
    public class Trident_rectangular : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Trident_rectangular()
          : base("3Pt Star_Array", "3Pt Star_Array",
              "Description",
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
            pManager.AddIntegerParameter("Extent Y ", "Ey", "Number of cells in y direction", GH_ParamAccess.item, 4);
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
            int Ey = 4;
            DA.GetData(2, ref Ey);


            double Motion = 0.2;
            DA.GetData(3, ref Motion);


            Point3d center_pt = new Point3d(0, 0, 0);
            Motion = Oricut.functions.Remap(Motion, 0, 1, 0, -0.33333 * Math.PI);
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


            
            List<Line> TempLines = new List<Line>() { l1, l2, l3 };
            List<Line> alllines = new List<Line>();
            List<Line> alllines2 = new List<Line>();
            int eval = 2;
            Transform Ex_Transform = Transform.Mirror(p1);

            

            
            for (int i = 0; i < Ex; i++)
            {

                List<Line> TempLines2 = new List<Line>();
                for (int j = 0; j < 3; j++)
                {
                    Line lTemp = TempLines[j];
                    lTemp.Transform(Ex_Transform);
                    TempLines2.Add(lTemp);
                    alllines.Add(lTemp);


                    Plane mirror_Plane = new Plane(lTemp.PointAt(0), new Vector3d(0, 1, 0));
                    Transform mirror_trans = Transform.Mirror(mirror_Plane);
                    lTemp.Transform(mirror_trans);

                    alllines2.Add(lTemp);
                }
               
                
                Plane new_pl = new Plane(TempLines2[eval].PointAt(1), Normals[0]);
                if (i % 2 == 0) { eval = 0; }
                if (i % 2 == 1) { eval = 2; }
                Ex_Transform = Transform.Mirror(new_pl);
                TempLines = TempLines2;
             
            }
            //Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Line> layer01 = new Grasshopper.Kernel.Data.GH_Structure<Grasshopper.Kernel.Types.GH_Line>();
            Grasshopper.DataTree<Line> layer1 = new Grasshopper.DataTree<Line>();
            Grasshopper.DataTree<Line> layer2 = new Grasshopper.DataTree<Line>();
            //Grasshopper.DataTree<int> layer1 = new Grasshopper.DataTree<int>();
            List<Line> pl = new List<Line>();
            List<Line> pl2 = new List<Line>();

            
            for(int i = 0; i < Ex; i++)
            {
                
            }


            for (int i = 0; i < alllines.Count; i++)
            {
                
                
                for (int j = 0; j < Ey; j++)
                {
                    decimal fraction = i / 3;
                    var far2 = Math.Floor(fraction);
                    var far3  = decimal.ToInt32(far2);
                    
                    Grasshopper.Kernel.Data.GH_Path new_path = new Grasshopper.Kernel.Data.GH_Path(j,far3);
                    Vector3d legnth_translation = new Vector3d(0, Math.Abs(len), 0);
                    Line ll1 = alllines[i];
                    ll1.Transform(Transform.Translation(legnth_translation * j));
                    

                    //mirror
                    Line ll1_M = alllines2[i];
                    ll1_M.Transform(Transform.Translation(legnth_translation * j));

                    if (far2 % 2 == 0)
                    {
                        layer2.Add(ll1_M, new_path);
                        layer1.Add(ll1, new_path);
                    }
                    else
                    {
                        layer2.Add(ll1, new_path);
                        layer1.Add(ll1_M, new_path);
                    }

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
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resource1.Artboard_1_copy_25;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("57340618-bf66-4077-884b-42e41bf4cda4"); }
        }
    }
}