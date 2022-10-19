using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Oricut;

namespace Linketix
{
    public class Seek_Force : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Seek_Force()
          : base("Seek Force", "Seek Force",
              "Description",
              "Linketix", "Forces")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddPointParameter("Anchor", "A", "The strength of the force", GH_ParamAccess.list);
            pManager.AddPointParameter("Target", "T", "The strength of the force", GH_ParamAccess.list);
        }

        /// <summary>
        /// Registers all the output parameters for this component.
        /// </summary>
        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddGenericParameter("Force Object", "F", "generate seek force", GH_ParamAccess.item);

        }

        /// <summary>
        /// This is the method that actually does the work.
        /// </summary>
        /// <param name="DA">The DA object is used to retrieve from inputs and store in outputs.</param>
        protected override void SolveInstance(IGH_DataAccess DA)
        {
            forces new_force = new forces();

            List<Point3d> anchors = new List<Point3d>();
            List<Point3d> targets = new List<Point3d>();
            DA.GetDataList(0, anchors);
            DA.GetDataList(1, targets);
            new_force.anchors = anchors;
            new_force.targets = targets;
            new_force.name = "Seek Force";


            DA.SetData(0, new_force);

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
                return Resource1.Artboard_1_copy_31;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1c776dc5-4498-4c84-bf67-75eface9df97"); }
        }
    }
}