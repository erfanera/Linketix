using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Oricut;
namespace Linketix
{
    public class Transformable_Triangles : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Transformable_Triangles()
          : base("Triangular", "Triangular",
              "Generates a Triangular Auxetic pattern",
              "Linketix", "Auxetic Materials")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "P", "Base plane for the auxetic pattern", GH_ParamAccess.item, Plane.WorldXY);
            pManager.AddNumberParameter("Length", "L", "The Length of the triangles", GH_ParamAccess.item,3);
            
            pManager.AddIntegerParameter("Extent X", "Ex", "Number of Auxetic cells in base plane x direction", GH_ParamAccess.item,3);
            pManager.AddIntegerParameter("Extent y", "Ey", "Number of Auxetic cells in base plane y direction", GH_ParamAccess.item,3);
            pManager.AddBooleanParameter("Invert?", "I", "True to invert the pattern", GH_ParamAccess.item, false);
            pManager.AddAngleParameter("Motion", "M", "A number between 0 and 1", GH_ParamAccess.item,0.2);
            

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

            int Ex = 0;
            DA.GetData(2, ref Ex);
            int Ey = 0;
            DA.GetData(3, ref Ey);
            bool invert = false;
            DA.GetData(4, ref invert);

            double Angle = 0;
            DA.GetData(5, ref Angle);
            Angle = functions.Remap(Angle, 0, 1, 0, 0.5 / 3 * Math.PI);
            if (invert)
            {
                Angle = functions.Remap(Angle, 0, 1, 0.5/3 * Math.PI, 0.5*Math.PI *0.96);

            }
            X = Math.Pow(3, 0.5) / 3 * X;
            //___________________________________________________________________________________CREATE BASE RECTANGLE
            List<Polyline> baseModules = new List<Polyline>();
            

            Circle c = new Circle(X);
            
            var rect = Polyline.CreateInscribedPolygon(c, 3);
            var b_form = Transform.Rotation(Angle, new Point3d(0, 0, 0));
            
            rect.Transform(b_form);
            rect.ToNurbsCurve();
            BoundingBox bb = new BoundingBox();
            Point3d pp = rect.BoundingBox.Corner(false, false, false);



            Point3d pp2 = rect.BoundingBox.Corner(false, false, false);
            
            
            var xfor = Transform.Mirror(new Plane(pp, new Vector3d(1,0,0)));
            var rect2 = rect.Duplicate();
            rect2.Transform(xfor);
            baseModules.Add(rect2);
            baseModules.Add(rect);

            var xfor_xz = Transform.Mirror(new Plane(pp, new Vector3d(0, 1, 0)));
            var rect3 = rect2.Duplicate(); ;
            var rect4 = rect.Duplicate(); 
            rect3.Transform(xfor_xz);
            rect4.Transform(xfor_xz);
            baseModules.Add(rect3);
            baseModules.Add(rect4);
            bb.Union(rect.BoundingBox);
            bb.Union(rect2.BoundingBox);
            bb.Union(rect3.BoundingBox);
            bb.Union(rect4.BoundingBox);


            Ex = Ex - 1;
            Ey = Ey - 1;

            ////__________________________________________________________generate xis 
            List<Polyline> rects = new List<Polyline>() { rect, rect2, rect3, rect4 };
            List<Polyline> Last_base_Module = new List<Polyline>();
            Last_base_Module = baseModules;


            Point3d main_anchor = bb.Min;
            Vector3d anchor_travel = new Vector3d(bb.Max.X - main_anchor.X, 0, 0);

            Vector3d anchor_travel_Z_tarvel = new Vector3d(0, main_anchor.Y - bb.Max.Y, 0);
            Plane iter_plane = new Plane(main_anchor, new Vector3d(-1, 0, 0));
            var Ex_trans = Transform.Mirror(iter_plane);


          

            
            List<PolylineCurve> ListPline = new List<PolylineCurve>(){ rect.ToPolylineCurve(), rect2.ToPolylineCurve(), rect3.ToPolylineCurve(), rect4.ToPolylineCurve() };
            for (int i = 0; i < Ex; i++)
            {


                List<Polyline> fakeList = new List<Polyline>();
                for (int j = 0; j < 4; j++)
                {
                    var r = Last_base_Module[j].Duplicate();
                    r.Transform(Ex_trans);
                
                    rects.Add(r);
                    fakeList.Add(r);
                    ListPline.Add(r.ToPolylineCurve());
                }

                main_anchor += (anchor_travel) * -1;
                iter_plane = new Plane(main_anchor, new Vector3d(-1, 0, 0));
                Ex_trans = Transform.Mirror(iter_plane);
                Last_base_Module = fakeList;


            }


            Vector3d v = anchor_travel_Z_tarvel;
            var Ey_trans = Transform.Translation(v);
           
            var Last_base_Module_y = rects;
            for (int i = 0; i < Ey; i++)
            {
                List<Polyline> fakeList = new List<Polyline>();
                for (int j = 0; j < rects.Count; j++)
                {
                    var r = Last_base_Module_y[j].Duplicate();
                    r.Transform(Ey_trans);
                    
                    fakeList.Add(r);
                    ListPline.Add(r.ToPolylineCurve());


                }
                Last_base_Module_y = fakeList;
            }

            
            Point3d cur = rects[2][1];
            var Victory = Transform.Translation(new Point3d(0, 0, 0) -cur);
            Plane base_plane = new Plane(cur, new Vector3d(0, 0, 1));
            var tt = Transform.Rotation(Math.PI, new Point3d(0,0,0));

            Rhino.Geometry.Transform orient = Rhino.Geometry.Transform.ChangeBasis(p,Plane.WorldXY );

            for (int i = 0; i < ListPline.Count; i++)
            {
                    
                    var test_rect = ListPline[i];
                    test_rect.Transform(Victory);
                test_rect.Transform(tt);
                test_rect.Transform(orient);
                    ListPline[i] = test_rect;
                
            }

            //_______________________________________________ouput
            
                DA.SetDataList(0, ListPline);
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
                return Resource1._4;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8b1b2d28-f828-469d-c9fb-e24e8431fb2d"); }
        }
    }
}