using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Oricut;
namespace Linketix
{
    public class Transformable_Hexagons : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Transformable_Hexagons()
          : base("Hexagonal Triangles", "Hexagonal Triangles",
              "Generates a Hexagonal Triangles Auxetic pattern",
              "Linketix", "Auxetic Materials")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPlaneParameter("Plane", "P ", "Base plane for the auxetic pattern", GH_ParamAccess.item, Plane.WorldXY);

            pManager.AddNumberParameter("Length", "L", "The Length of the Hexagons", GH_ParamAccess.item , 1);
          
            pManager.AddIntegerParameter("Extent X", "Ex", "Number of Auxetic cells in base plane x direction", GH_ParamAccess.item ,2);
            pManager.AddIntegerParameter("Extent y", "Ey", "Number of Auxetic cells in base plane y direction", GH_ParamAccess.item ,2);
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

            int Ex = 0;
            DA.GetData(2, ref Ex);
            int Ey = 0;
            DA.GetData(3, ref Ey);
            double Angle = 0;
            DA.GetData(4, ref Angle);
            Angle = functions.Remap( Angle , 0, 1, 0,0.667 * Math.PI);

            


            //___________________________________________________________________________________CREATE BASE RECTANGLE
            List<Rectangle3d> baseModules = new List<Rectangle3d>();
            
            Plane pl = Plane.WorldXY;
            pl.Rotate(Angle , new Vector3d(0,0,1));
            Circle c = new Circle(X);
            Polyline rect_orginal = Polyline.CreateInscribedPolygon(c, 6);
            Polyline rect_transformable = Polyline.CreateInscribedPolygon(c, 6);
            var rot = Transform.Rotation(Angle, new Point3d(0, 0, 0));
            var rot_original = Transform.Rotation(Math.PI*0.5, new Point3d(0, 0, 0));
            rect_orginal.Transform(rot_original);
            rect_transformable.Transform(rot);
            double height = (rect_orginal[0].Y - rect_orginal[1].Y)/3;
            double diameter = height*2;
            Point3d tri_center_1 = rect_transformable[1] + new Vector3d(0, diameter, 0);

            Circle c1 = new Circle(tri_center_1, diameter);
            Polyline tri1 = Polyline.CreateInscribedPolygon(c1, 3);
            var rot_tri1 = Transform.Rotation(0.5 / 3 * Math.PI, tri_center_1);
            tri1.Transform(rot_tri1);
            double len_tr1 =     (tri1.SegmentAt(0).Length)*0.5;
            Point3d Lower_pt = rect_transformable[0]+ len_tr1 * new Vector3d(1, 0, 0);
            Point3d tri_center_2 = new Vector3d(0,1,0)*height+ Lower_pt;

            Circle c2 = new Circle(tri_center_2, diameter);
            Polyline tri2 = Polyline.CreateInscribedPolygon(c2, 3);
            var rot_tri2 = Transform.Rotation(-0.5 / 3 * Math.PI, tri_center_2);
            tri2.Transform(rot_tri2);

            //Point3d pp = rect.Corner(2);
            //Rectangle3d rect2 = new Rectangle3d(new Plane(pp, new Vector3d(0, 0, 1)), Y, -X);
            Vector3d Ax = rect_transformable[3] -tri1[0];
            Vector3d Ax2 =( tri2[2] - rect_transformable[2]);
            Vector3d Ay = -(tri1[1] - rect_transformable[5]);


            ////__________________________________________________________generate xis
            ///

            //{-1.861403, 3.678708, 0}

            



            List < Polyline> rects = new List<Polyline>() { rect_transformable, tri1, tri2 };
            
            List<PolylineCurve> allPline = new List<PolylineCurve>();

            Vector3d initTransform = Ax;
            for (int i = 0; i < Ex; i++)
            {
             

           

                var Ax_trans = Transform.Translation(initTransform);
                var l1 = rects[rects.Count-3].Duplicate();
                l1.Transform(Ax_trans);
                
                var l2 = rects[rects.Count - 2].Duplicate();
                l2.Transform(Ax_trans);
                
                var l3 = rects[rects.Count -1].Duplicate();
                l3.Transform(Ax_trans);
                
                rects.Add(l1);
                rects.Add(l2);
                rects.Add(l3);
                initTransform = l1[3] - l2[0];
                if (i % 2 == 0)
                {
                    initTransform = (l1[2]- l3[0]);
                }


            }
            Vector3d vv = rects[4].CenterPoint()- new Point3d(0, 0, 0);

            double angle = Vector3d.VectorAngle(vv, new Vector3d(1, 0, 0));
            var overal_rotation = Transform.Rotation(angle, new Point3d(0, 0, 0));

            for (int i = 0; i < Ey; i++)
            {
                for (int j = 0; j < rects.Count; j++)
                {

                    var Ay_trans = Transform.Translation(Ay * (i ));
                    var l = rects[j].Duplicate();
                    l.Transform(Ay_trans);
                    l.Transform(overal_rotation);
                    allPline.Add(l.ToPolylineCurve());

                }
            }

            
            Rhino.Geometry.Transform orient = Rhino.Geometry.Transform.ChangeBasis( p, Plane.WorldXY);

            for (int i = 0; i < allPline.Count; i++)
            {

                var test_rect = allPline[i];
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
                return Resource1._2;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("8b1a9de8-f818-461d-b9fb-e24e8431fb2d"); }
        }
    }
}