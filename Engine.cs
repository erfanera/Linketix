using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Oricut;

namespace Linketix
{
    public class Engine : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent2 class.
        /// </summary>
        public Engine()
          : base("Linketix Solver", "Linketix Solver",
              "Simulate",
              "Linketix", "Solver")
        {
        }


        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddCurveParameter("Lines", "L", "Creates the Pattern from these lines", GH_ParamAccess.list);                  //0
           // pManager.AddBooleanParameter("Shrink/Expand", "S/E", "True = Expand / False = Shrink", GH_ParamAccess.item, false);     //1
            pManager.AddBooleanParameter("Reset", "R", "Reset the simulation", GH_ParamAccess.item, false);                         //1
            pManager.AddIntegerParameter("Speed", "S", "Simulation Speed", GH_ParamAccess.item, 2);                                 //2
            pManager.AddNumberParameter("Force Strength", "FS", "The strength of the force", GH_ParamAccess.item , 1);              //3
            //____________
            pManager.AddGenericParameter("Force Object", "Force", "Force object", GH_ParamAccess.list);                              //4
            //____________




            //pManager.AddVectorParameter("vect", "vect", "vect", GH_ParamAccess.list);
        }

        
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
           // pManager.AddPointParameter("points", "points", "points ", GH_ParamAccess.list);

            pManager.AddCurveParameter("Lines", "L", "Final Result", GH_ParamAccess.list);
        }

        //__________________________________________________________________________________CREATE BUTTON
        bool direction;
        bool is2d = false;
        //____________________________________________________________________________________integrate new button

        
       

        int maintcounter = 0;
        List<Point3d> allpts = new List<Point3d>();    //STORE ALL POINTS
        
        
       
        List<Line> lines = new List<Line>();
        List<node> new_born = new List<node>();


        
        //________________________________________________________________________________________________________________SOLVER INSTANCE
        protected override void SolveInstance(IGH_DataAccess DA)
        {
          

            //_____________________________________________________________


            line_ids.Clear();
            lines.Clear();
            allpts.Clear();

            //import basic elements _______________________________________________________________________

            //the main list which we store all the nodes
            double accuracy = 1;     //the list of imported liens Not optimized       
            DA.GetData(3, ref accuracy);


            List<Curve> input_lines = new List<Curve>();      //the list of imported liens Not optimized       
            DA.GetDataList(0, input_lines);

            int speed = 4;                                  //simulation's speed for loop which repeat for each iteration inside it
            DA.GetData(2, ref speed);

            List<Vector3d> test_vect = new List<Vector3d>();
            List<Vector3d> test_vect2 = new List<Vector3d>();
            List<Vector3d> test_vect3 = new List<Vector3d>();
            List<int> test_ids = new List<int>(0);

                                 //change direction to create expansion or cohision translation 
            //DA.GetData(1, ref direction);

            bool reset = false;                             //reset the simulation
            DA.GetData(1, ref reset);


            List<forces> force_object = new List<forces>();
            DA.GetDataList(4,   force_object);

            List<Point3d> anchor = new List<Point3d>();
            List<Point3d> desire = new List<Point3d>();
            List<Point3d> pins = new List<Point3d>();

            string name = "test";
            for (int i = 0; i < force_object.Count; i++)
            {
                if (force_object[i].name == "Seek Force")
                {
                    anchor = force_object[i].anchors;
                    desire = force_object[i].targets;

                    name = force_object[i].name;
                }

                if(force_object[i].name == "Pin")
                {
                    pins = force_object[i].pins;
                    
                }

                if (force_object[i].name == "Expansion")
                {
                    name = "Expansion";
                }
            }








            ////draw outputed lines
            // { new Point3d(0, 0, 0) };
           
            List<int> ids = new List<int>();
            maintcounter += 1;
            List<bool> hidden = new List<bool>();
            List<bool> trigger_desire = new List<bool>();

            var allVect = new List<Vector3d>();

            
            //__________________________________________________________________________________________Create graph
            if (reset)
            {
                
    
                
                new_born = new List<node>();
                
                List<Line> temp_lines = functions.plineToline(functions.crvToPline( input_lines) , out hidden);
                new_born = functions.createnetwork(temp_lines , hidden);
                functions.create_targest(new_born, anchor, desire);
                functions.pinne(new_born, pins);

                is2d =functions.planarornot(new_born);
                

                allpts = new List<Point3d>();
                
                
                lines = new List<Line>();
                lines = DrawLines(new_born);
                for (int i = 0; i < new_born.Count; i++)
                {
                    allVect.Add(new Vector3d(0, 0, 0));
                    ids.Add(new_born[i].ID);
                }

            }
            //__________________________________________________________________________________________
            var lll = new List<Line>();

            lines = DrawLines(new_born);

            test_vect.Clear();
            if (force_object[0].name == "Expansion")
            {
                 allVect = functions.diffusion(new_born, force_object[0].situation);
            }

            Vector3d cohision = new Vector3d(0,0,0);
            Vector3d stretch= new Vector3d(0, 0, 0);
            Vector3d collisioin= new Vector3d(0, 0, 0);


                for (int iter = 0; iter < speed; iter++)
                {

                for (int i = 0; i < new_born.Count; i++)
                {
                    if (new_born[i].pinned == false)
                    {


                        Point3d old_pt = new_born[i].position;

                        Vector3d overall = new Vector3d(0, 0, 0);       //combines all the vectors
                                                                        //______________________________________________________________________________________________Cohision expansio



                        //________________________________________________________________________________________________Lines

                        stretch = new Vector3d();              //bing edges avoid collapse by edge


                        for (int j = 0; j < new_born[i].neighbours.Count; j++)
                        {

                            int new_id = new_born[i].neighbours[j];

                            int j_node_index = new_born.FindIndex(xx => xx.ID == new_id);

                            double new_dist = new_born[i].position.DistanceTo(new_born[j_node_index].position);
                            double delta = new_dist - new_born[i].distances[j];
                            Vector3d stretch_temp = (new_born[j_node_index].position - new_born[i].position);
                            stretch_temp.Unitize();
                            stretch_temp *= (delta);
                            stretch += stretch_temp;
                        }
                        stretch /= new_born[i].neighbours.Count;


                        //------------------------------------------------------------------------------




                        //--------------------------------------------------------------
                        int closest_id = 1000;
                        double closest_distance = 1000;
                        for (int segs = 0; segs < lines.Count; segs++)
                        {




                            if ((line_ids[segs][0] != new_born[i].ID & line_ids[segs][1] != new_born[i].ID))
                            {
                                double eval_dist = lines[segs].MinimumDistanceTo(new_born[i].position);
                                if (eval_dist < closest_distance)
                                {
                                    closest_distance = eval_dist;
                                    closest_id = segs;
                                }
                            }



                        }
                        cohision = new Vector3d(0, 0, 0);
                        
                            if (name == "Seek Force")
                            {

                                if (new_born[i].target_bool)
                                {
                                    cohision = (desire[new_born[i].desire_pt] - new_born[i].position) * 0.1;
                                    double len = cohision.Length;
                                    len = functions.Remap(len, 0, 1, 0, 1);
                                    cohision.Unitize();
                                }

                            }
                            if(name == "Expansion")
                            {
                                if (force_object[0].trigger)
                                {
                                cohision = ((allVect[i]) * 1);

                                cohision = ((cohision / new_born[i].neighbours.Count) * 0.01);

                                cohision *= functions.Remap(closest_distance, 0, accuracy, 0, accuracy);
                                }
                            }
                        
                        collisioin = functions.calculateNormal(new_born[i].position, lines[closest_id], (cohision * accuracy + stretch));

                        overall = ((cohision * accuracy) * 0.1 + collisioin * 0.1 + stretch*0.8);
                        double size = overall.Length;
                        size = functions.Remap(size, 0, accuracy, 0, accuracy);
                        overall.Unitize();
                        overall *= size;
                        new_born[i].position += overall;
                  



                        if (is2d == false)
                        {
                            new_born[i].position = new Point3d(new_born[i].position.X, new_born[i].position.Y, 0);
                        }


     
                    }





                }

                }




            


            //_________________________________________________________________Loop end

            

            //_________________________________________________________________Out puts
            //DA.SetDataList(0, allpts);
            DA.SetDataList(0, lines);


        }










        



        List<List<int>> line_ids = new List<List<int>>();
        public List<Line> DrawLines(List<node> new_born )
        {   
            double counter = 0;
            
            List<Line> liness = new List<Line>();
            List<int> black_ids = new List<int>();
            for (int i = 0; i < new_born.Count; i++)
            {

                

                for (int j = 0; j < new_born[i].lines.Count; j++)
                    {
                    int new_id = new_born[i].lines[j];
                    int j_node_index = new_born.FindIndex(xx => xx.ID == new_id);
                    bool isinlist = black_ids.Contains(new_born[j_node_index].ID );
                    if ( isinlist == false)
                    {
                        
                        

                        //
                        liness.Add(new Line(new_born[i].position, new_born[j_node_index].position));
                        List<int> temp_list = new List<int>();
                        temp_list.Add(new_born[i].ID);
                        temp_list.Add(new_born[i].lines[j]);
                        line_ids.Add(temp_list);
                        counter += 1;
                        
                    }
                    

                }
                black_ids.Add(new_born[i].ID);
            }

            return liness;
        }




 

 






        public override void DrawViewportWires(IGH_PreviewArgs args)
        {

            args.Display.DrawLines(lines, System.Drawing.Color.Red, 4);

            args.Display.DrawPoints(allpts, Rhino.Display.PointStyle.Circle, 2, System.Drawing.Color.White);
        }


        protected override System.Drawing.Bitmap Icon
        {
            get
            {
                //You can add image files to your project resources and access them like this:
                // return Resources.IconForThisComponent;
                return Resource1.solver;
            }
        }

        public override Guid ComponentGuid
        {
            get { return new Guid("e0c8a3a2-88dc-4b1b-907f-e385649be031"); }
        }
    }
}