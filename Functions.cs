using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Grasshopper.Kernel;
using Rhino.Geometry;

namespace Oricut
{

    public class forces
    {
        public string name = "shit";
        public List<Point3d> anchors;
        public List<Point3d> targets;
        public List<Point3d> pins;
        public bool trigger = false;
        public bool situation = false;
      

    }


    public class node
    {
        
        public Point3d position;
        public int ID = 0;
        public List<int> neighbours = new List<int>();
        public List<double> distances = new List<double>();
        public List<int> associateLines = new List<int>();
        public List<int> lines = new List<int>();

        public bool target_bool = false;
        public int desire_pt;
        public bool pinned = false;

    }

      
    public class test
    {
        public bool a = false;
    }
    public class functions
    {

        //__________________________________________________________________________________________________find pins

        public static void pinne(List<node> nodes, List<Point3d> pin)
        {
            for (int i = 0; i < pin.Count; i++)
            {
                int closest_id = 0;

                for (int j = 0; j < nodes.Count; j++)
                {

                    double distance = pin[i].DistanceTo(nodes[j].position);
                    if (distance < 0.01)
                    {

                        closest_id = j;

                    }
                }

                nodes[closest_id].pinned = true;
            }

        }
        //____________________________________________________________________________________________________check to add custom force
        public static void create_targest(List<node> nodes , List<Point3d> anchor  , List<Point3d> target )
        {
            
            for (int i = 0; i < anchor.Count; i++)
            {
                int closest_id = 0;
                double distance_record = 10000;
                for(int j = 0; j < nodes.Count; j++)
                {
                    
                    double distance = anchor[i].DistanceTo(nodes[j].position);
                    if (distance < distance_record)
                    {

                        closest_id = j;
                        distance_record = distance;
                    }
                }

                nodes[closest_id].target_bool = true;
                nodes[closest_id].desire_pt = i;

            }
            
        }
        //_____________________________________________________________________________________________________ON GROUND
        public static Vector3d on_ground(Point3d pt)
        {
            Vector3d push_ground = new Vector3d(0, 0, 0);
            Point3d ground_pt = new Point3d(pt.X, pt.Y, 0);
            push_ground = ground_pt - pt;
            
            double vect_size = Remap(push_ground.Length, 0,0.1, 0, 0.1);
            push_ground.Unitize();
            push_ground *= vect_size;
                

            return push_ground;
        }
            //____________________________________________________________________________________________________REMAP
            public static double Remap(double val, double from1, double to1, double from2, double to2)
        {


            double remapped = (val - from1) / (to1 - from1) * (to2 - from2) + from2;
            if (val > to1) { remapped = to2; }
            if (val < from1) { remapped = from2; }
            return remapped;
        }
        //____________________________________________________________________________________________________CALCULATE NORMAL
        public static Vector3d calculateNormal(Point3d pt, Line ln,Vector3d old_pt )
        {
         
     

            var closestpt = ln.ClosestPoint(pt, true);
  
     

            Vector3d normal = (pt - closestpt);
            double factor = old_pt.Length/ normal.Length;            //Remap(normal.Length, 0, old_pt.Length, 1, 0);
            normal.Unitize();
            normal *= factor ;

            


            return normal;
        }

        //____________________________________________________________________________________________________COHESION AND EXPANSION
        public static List<Vector3d> diffusion(List<node> nodes , bool direction)
        {
            List<Vector3d> cohesion = new List<Vector3d>();
            for (int i = 0; i < nodes.Count; i++)
            {
                cohesion.Add(new Vector3d(0, 0, 0));
            }

            int direction_integer = 1;

            if (direction)
            {
                direction_integer = -1;


            }

            for (int i = 0; i < nodes.Count; i++)
            {
                double distance = 0;
                for (int j = i + 1; j < nodes.Count; j++)
                {
                    Vector3d realvect = (nodes[j].position - nodes[i].position);

                    realvect.Unitize();
                    
                    Vector3d vvv = direction_integer * (realvect) *(1/realvect.Length);
                    vvv.Unitize();
                    cohesion[i] += vvv;
                    cohesion[j] -= vvv;

                    distance += realvect.Length;

                }

                


            }


            return cohesion;

        }
        //_____________________________________________________________________________________________________________________curve to plines

        public static List<Polyline> crvToPline(List<Curve> lines)
        {
            
            List<Polyline> pl = new List<Polyline>();
            
            for (int i = 0; i < lines.Count; i++)
            {
                Polyline pline;
                lines[i].TryGetPolyline(out pline);
                pl.Add(pline);
            }
            return (pl);
        
        }
        //_____________________________________________________________________________________________________________________create additive lines for polylines
        public static List<Line> plineToline(List<Polyline> lines, out List<bool> sit)
        {
            List<bool> boo = new List<bool>();
            List<Line> allLines = new List<Line>();
            for(int i=0;i< lines.Count; i++)
            {
                int count = lines[i].Count;

                Line[] ln = lines[i].GetSegments();
                for (int j = 0; j < ln.Length; j++)
                {
                    allLines.Add(ln[j]);
                    boo.Add(true);
                }
                    if (count >= 2)
                {
                    for (int j = 0; j < count; j++)
                    {
                   
                        int index = j+2;
         
                        Point3d pt1 = lines[i][j];
                        Point3d pt2 = lines[i][(j+2)%count];
                        allLines.Add(new Line(pt1, pt2));
                        boo.Add(false);
                    }
                   
                }
            }
            sit = boo;
            return allLines;
        }
        //_____________________________________________________________________________________________________________________________2d or not
        public static bool planarornot(List<node> nd)
        {
            bool is2d = true;
            int count = 0;
            for(int i = 0; i < nd.Count; i++)
            {
                if(!(nd[i].position.Z<0.1 & nd[i].position.Z < -0.1))
                {
                    count += 1;

                }
                
            }
            if (count != 0)
            {
                is2d = true;
            }
            return is2d;
        }
            //__________________________________________________________________________________________________________________________CREATE NETWORK

            public static  List<node> createnetwork(List<Line> lines , List<bool> sit )
    {

        
        List<node> new_born = new List<node>();

        for (int i = 0; i < lines.Count; i++)
        {

            //variable decleration
            int duplcaite_start_index = 1000;
            int duplcaite_end_index = 1000;
            node new_end_node;
            node new_start_node;
            int duplicate_id_start = 1000;
            bool duplicate_bool_start = false;
            int duplicate_id_end = 1000;
            bool duplicate_bool_end = false;



            //extract end  points
            Point3d start = lines[i].PointAt(0);
            Point3d end = lines[i].PointAt(1);

            //-----------------------------------------------------------------------Check to find duplicates
            for (int j = 0; j < new_born.Count; j++)
            {



                double distance_start = start.DistanceTo(new_born[j].position);
                if (distance_start < 0.01)
                {//condition to check not duplicate
                    duplicate_bool_start = true;
                    duplicate_id_start = new_born[j].ID;
                    duplcaite_start_index = j + 1;
                    duplcaite_start_index -= 1;

                }
                double distance_end = end.DistanceTo(new_born[j].position);
                if (distance_end < 0.01)
                {//condition to check not duplicate
                    duplicate_bool_end = true;
                    duplicate_id_end = new_born[j].ID;
                    //duplcaite_end_index = j;
                    duplcaite_end_index = j + 1;
                    duplcaite_end_index -= 1;
                }



            }


            double distance;
                if (duplicate_bool_start == false & duplicate_bool_end == false)
                { //they were both uniqe
                    new_end_node = new node();
                    new_start_node = new node();
                    new_end_node.position = end;
                    new_end_node.ID = i + 1000;
                    new_start_node.position = start;
                    new_start_node.ID = i;
                    //---------------------------------------------------
                    new_start_node.neighbours.Add(new_end_node.ID);
                    new_end_node.neighbours.Add(new_start_node.ID);
                    //-------------------------------------------------------------
                    if (sit[i])
                    {
                        new_start_node.lines.Add(new_end_node.ID);
                        new_end_node.lines.Add(new_start_node.ID);
                    }
                    //--------------------------------------------
                distance = start.DistanceTo(end);
                new_start_node.distances.Add(distance);
                new_end_node.distances.Add(distance);
                new_born.Add(new_start_node);
                new_born.Add(new_end_node);
            }
            if (duplicate_bool_start == false & duplicate_bool_end)
            { //one was uniqe start

                //int endindex = new_born.FindIndex(xx => xx.ID == duplicate_id_end);
                new_born[duplcaite_end_index].neighbours.Add(i);
                distance = new_born[duplcaite_end_index].position.DistanceTo(start);
                new_born[duplcaite_end_index].distances.Add(distance);
                new_start_node = new node();
                new_start_node.position = start;
                new_start_node.ID = i;
                    //--------------------------------------------------------------------
                new_start_node.neighbours.Add(duplicate_id_end);
                    //---------------------------------------------------------
                    if (sit[i]) { new_start_node.lines.Add(duplicate_id_end);
                        new_born[duplcaite_end_index].lines.Add(i);
                    }
                    
                new_start_node.distances.Add(distance);

                new_born.Add(new_start_node);
            }
                if (duplicate_bool_start & duplicate_bool_end == false)
                { //one was uniqe start

                    int startindex = new_born.FindIndex(xx => xx.ID == duplicate_id_start);
                    new_born[duplcaite_start_index].neighbours.Add(i + 1000);
                    distance = new_born[duplcaite_start_index].position.DistanceTo(end);
                    new_born[duplcaite_start_index].distances.Add(distance);
                    new_end_node = new node();
                    new_end_node.position = end;
                    new_end_node.ID = i + 1000;
                    new_end_node.neighbours.Add(duplicate_id_start);

                    if (sit[i])
                    {
                        new_end_node.lines.Add(duplicate_id_start);
                        new_born[duplcaite_start_index].lines.Add(i + 1000);
                    }
                new_end_node.distances.Add(distance);
                new_born.Add(new_end_node);
            }
            if (duplicate_bool_start & duplicate_bool_end)
            { //one was uniqe start

                int startindex = new_born.FindIndex(xx => xx.ID == duplicate_id_start);
                

                int endindex = new_born.FindIndex(xx => xx.ID == duplicate_id_end);


                new_born[startindex].neighbours.Add(new_born[endindex].ID);
                new_born[endindex].neighbours.Add(new_born[startindex].ID);

                    if (sit[i])
                    {
                        new_born[startindex].lines.Add(new_born[endindex].ID);
                        new_born[endindex].lines.Add(new_born[startindex].ID);
                    }

                double dist = new_born[startindex].position.DistanceTo(new_born[duplcaite_end_index].position);
                new_born[startindex].distances.Add(dist);
                new_born[endindex].distances.Add(dist);



            }



        }
        int max_degree = 0;
        for (int i = 0; i < new_born.Count; i++)
        {
            int degree = new_born[i].neighbours.Count;
            if (degree > max_degree)
            {
                max_degree = degree;
            }
        }
        return new_born;
    }
        
        //_____________________________________________________________________________________________________________LIMIT VECTOR
        public Vector3d limitVector(Vector3d vect, double a, double b, double c, double d)
        {
            double vector_size = vect.Length;
            vect.Unitize();
            double remapped = Remap(vector_size, a, b, c, d);
            vect *= remapped;
            return vect;
        }


    }

}
