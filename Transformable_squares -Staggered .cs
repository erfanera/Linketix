using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Oricut;
namespace Linketix
{
    public class Transformable_squares_Staggered : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Transformable_squares_Staggered()
          : base("Rectangular 2", "Rectangularw 2",
              "Generates a Triangularw Auxetic pattern",
              "Linketix", "Auxetic Materials")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "P", "Base plane for the auxetic pattern", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddNumberParameter("Length", "L", "The Length of the Rectangles", GH_ParamAccess.item,3);
            pManager.AddNumberParameter("Width", "W", "The Width of the Rectangles", GH_ParamAccess.item , 1);
            pManager.AddIntegerParameter("Extent X", "Ex", "Number of Auxetic cells in base plane x direction", GH_ParamAccess.item ,2);
            pManager.AddIntegerParameter("Extent y", "Ey", "Number of Auxetic cells in base plane y direction", GH_ParamAccess.item , 2);
            pManager.AddAngleParameter("Motion", "M", "A number between 0 and 1", GH_ParamAccess.item , 0.2);
            
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddCurveParameter("Polyline", "P", "Polyline output for the Auxetic pattern", GH_ParamAccess.list);
            

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            //____________________________________________________Get inputs
            Plane p = new Plane();
            DA.GetData(0, ref p);


            double X = 0;
            DA.GetData(1, ref X);
            double Y = 0;
            DA.GetData(2, ref Y);
            int Ex = 0;
            DA.GetData(3, ref Ex);
            int Ey = 0;
            DA.GetData(4, ref Ey);
            double Angle = 0;
            DA.GetData(5, ref Angle);

           

            //______________________remap angle

            Angle = functions.Remap(Angle * Math.PI, 0, 1 * Math.PI, 0, 1 * Math.PI);
            //___________________________________________________________________________________CREATE BASE RECTANGLE
            List<Rectangle3d> baseModules = new List<Rectangle3d>();
            
            Plane pl = Plane.WorldXY;
            pl.Rotate(-Angle , new Vector3d(0,0,1));

            Rectangle3d rect = new Rectangle3d(pl, X, Y);

            Point3d vicoty_anchor = rect.Corner(3);
            var set_zero = Transform.Translation(new Point3d(0,0,0)-vicoty_anchor);
            Point3d pp = rect.Corner(2);
            Rectangle3d rect2 = new Rectangle3d(new Plane(pp, new Vector3d(0, 0, 1)), Y, -X);
            Vector3d Ax = rect2.Corner(0) - rect.Corner(3);
            
            Vector3d Ay = rect2.Corner(2) - rect.Corner(1);


            ////__________________________________________________________generate xis
            ///
            var Base_rotate = Transform.Rotation(((Angle / 2) + 0.25 * Math.PI), vicoty_anchor);
            List<Rectangle3d> rects = new List<Rectangle3d>();
            List<Rectangle3d> allPline = new List<Rectangle3d>();
            for (int i = 0; i < Ex; i++)
            {
                var Ax_trans = Transform.Translation(Ax*i);
                var l = rect;
                l.Transform(Ax_trans);
                rects.Add(l);
                l = rect2;
                l.Transform(Ax_trans);
                rects.Add(l);
            }
            for (int i = 0; i < Ey; i++)
            {
                for (int j = 0; j < rects.Count; j++)
                {
                    var Ay_trans = Transform.Translation(Ay * i);
                    var l = rects[j];
                
                    l.Transform(Ay_trans);
                    l.Transform(Base_rotate);
                    //l.Transform(set_zero);
                    allPline.Add(l);

                }
            }
            Rhino.Geometry.Transform orient = Rhino.Geometry.Transform.ChangeBasis(p, Plane.WorldXY);
            for (int i = 0; i < allPline.Count; i++)
            {
                var test_rect = allPline[i];
                test_rect.Transform(set_zero);
                test_rect.Transform(orient);
                allPline[i] = test_rect;
            }

                //_______________________________________________ouput

                DA.SetDataList(0, allPline);
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
                return Resource1._1 ;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8b1bad38-f818-469d-b9fb-e24e8431fb2d"); }
        }
    }
}