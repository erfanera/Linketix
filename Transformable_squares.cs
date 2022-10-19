using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Oricut;
namespace Linketix
{
    public class Transformable_squares : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Transformable_squares()
          : base("Rectangular 1", "Rectangular 1",
              "Description",
              "Linketix", "Auxetic Materials")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "P ", "Base plane for the auxetic pattern", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddNumberParameter("Length", "L", "The Length of the Rectangles", GH_ParamAccess.item ,3);
            pManager.AddNumberParameter("Width", "W", "The Width of the Rectangles", GH_ParamAccess.item ,1);
            pManager.AddIntegerParameter("Extent X", "Ex", "Number of Auxetic cells in base plane x direction", GH_ParamAccess.item,3);
            pManager.AddIntegerParameter("Extent y", "Ey", "Number of Auxetic cells in base plane y direction", GH_ParamAccess.item,3);
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
            Plane p = new Plane(new Point3d(0,0,0) , new Vector3d(0,0,1));
            DA.GetData(0, ref p);

            double X = 0;
            double Y = 0;
            DA.GetData(1, ref Y);
            
            DA.GetData(2, ref X);
            int Ex = 0;
            DA.GetData(3, ref Ex);
            int Ey = 0;
            DA.GetData(4, ref Ey);
            double Angle = 0;
            DA.GetData(5, ref Angle);
            Plane base_plane = Plane.WorldXY;

            Ex = Ex - 1;
            Ey = Ey - 1;
            //var alter_rotate = Transform.Rotation(Math.PI*, new Point3d(0, 0, 0));
            Rhino.Geometry.Transform orient = Rhino.Geometry.Transform.ChangeBasis(p, base_plane);
            //var overal_transfromation = Transform.Rotation(overal_dif, new Point3d(0, 0, 0));
            Angle = functions.Remap(Angle , 0, 1, 0.5 * Math.PI,0);
            //___________________________________________________________________________________CREATE BASE RECTANGLE
            List<Rectangle3d> baseModules = new List<Rectangle3d>();

            Plane pl = Plane.WorldXY;
            //pl.Rotate(Angle , new Vector3d(0,0,1));

            Rectangle3d rect = new Rectangle3d(pl, X, Y);
            Point3d anchor = rect.Corner(1);

            Vector3d bridge_victory = new Point3d(0, 0, 0) - anchor;
            
            rect.Transform(Transform.Rotation(Angle, anchor));
        
            Point3d pp = rect.BoundingBox.Corner(true, false, false);
            Point3d pp2 = rect.BoundingBox.Corner(false, true, false);
            

            var xfor = Transform.Mirror(new Plane(pp, new Vector3d(1,0,0)));
            var rect2 = rect;
            
            rect2.Transform(xfor);


            var xfor_xz = Transform.Mirror(new Plane(pp, new Vector3d(0, 1, 0)));
            var rect3 = rect2;
            var rect4 = rect;

            rect3.Transform(xfor_xz);
            rect4.Transform(xfor_xz);



            var a = rect.ToPolyline();
            var b = rect.ToPolyline();
            var c = rect.ToPolyline();
            var d = rect.ToPolyline();

            b.Reverse();
            d.Reverse();



            rect3.Transform(Transform.Translation(bridge_victory));
            rect4.Transform(Transform.Translation(bridge_victory));
            rect2.Transform(Transform.Translation(bridge_victory));
            rect.Transform(Transform.Translation(bridge_victory));
            baseModules.Add(rect);
            baseModules.Add(rect2);
            baseModules.Add(rect3);
            baseModules.Add(rect4);

            Point3d mi_axis = rect.Corner(1);


            BoundingBox bb = new BoundingBox();
            bb.Union(rect.BoundingBox);
            bb.Union(rect2.BoundingBox);
            bb.Union(rect3.BoundingBox);
            bb.Union(rect4.BoundingBox);





            ////________________________________________________________transform bboxes to the origin


            

            //__________________________________________________________generate xis 
            List<Rectangle3d> rects = new List<Rectangle3d>() { rect, rect2, rect3, rect4 };
            List<Rectangle3d> Last_base_Module = new List<Rectangle3d>();
            Last_base_Module = baseModules;

            

            Point3d main_anchor = bb.Min;
            Vector3d anchor_travel = new Vector3d(-bb.Max.X + main_anchor.X, 0, 0);

            Vector3d anchor_travel_Z_tarvel = new Vector3d(0, -main_anchor.Y + bb.Max.Y, 0);
            Plane iter_plane = new Plane(main_anchor, new Vector3d(+1, 0, 0));
            var Ex_trans = Transform.Mirror(iter_plane);



            List<Rectangle3d> ListPline = new List<Rectangle3d>() { rect, rect2, rect3, rect4 };

           var mi =  Transform.Mirror(new Plane(mi_axis, new Vector3d(1, 0, 0)));


            for (int i = 0; i < Ex; i++)
            {


                List<Rectangle3d> fakeList = new List<Rectangle3d>();
                for (int j = 0; j < 4; j++)
                {
                    var r = Last_base_Module[j];

                
                    r.Transform(Ex_trans);
                  
                    rects.Add(r);
                    ListPline.Add(r);
                    fakeList.Add(r);

                }

                main_anchor += (anchor_travel) ;
                iter_plane = new Plane(main_anchor, new Vector3d(-1, 0, 0));
                Ex_trans = Transform.Mirror(iter_plane);
                Last_base_Module = fakeList;


            }


            Vector3d v = anchor_travel_Z_tarvel;
            var Ey_trans = Transform.Translation(v);
            
            var Last_base_Module_y = rects;



            
            for (int i = 0; i < Ey; i++)
            {
                List<Rectangle3d> fakeList = new List<Rectangle3d>();
                for (int j = 0; j < rects.Count; j++)
                {
                    var r = Last_base_Module_y[j];
                    r.Transform(Ey_trans);

                    fakeList.Add(r);
                    ListPline.Add(r);


                }
                Last_base_Module_y = fakeList;
            }
            
            List<PolylineCurve> pll = new List<PolylineCurve>();
            for (int i = 0; i < ListPline.Count; i++)
            {
                Rectangle3d test_rect = ListPline[i];
                //test_rect.Transform(base_trans);
                test_rect.Transform(mi);

                test_rect.Transform(orient);
                Polyline e = test_rect.ToPolyline();
                double shit = i / 4;


                Point3d final_center = test_rect.Center;
                Circle cc = new Circle(final_center , 2);
                var ccc = e.ToNurbsCurve();
                var ca = cc.ToNurbsCurve();

                if (Curve.DoDirectionsMatch(ca, ccc))
                {
                    e.Reverse();
                    var ee = e.ToPolylineCurve();
                    pll.Add(ee);
                }

                else
                {
                    var ee = e.ToPolylineCurve();
                    pll.Add(ee);
                }







                ListPline[i] = test_rect;


            }
            //_______________________________________________ouput

            DA.SetDataList(0, pll);

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
                return Resource1._3;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1b1b9d38-f828-469d-b9fb-e24e8431fb2d"); }
        }
    }
}