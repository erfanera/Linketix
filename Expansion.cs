using System;
using System.Collections.Generic;

using Grasshopper.Kernel;
using Rhino.Geometry;
using Oricut;


namespace Linketix
{


    public class Expansion : GH_Component
    {
        /// <summary>
        /// Initializes a new instance of the MyComponent1 class.
        /// </summary>
        public Expansion()
          : base("Expansion Force", "Expansion Force",
              "Description",
              "Linketix", "Forces")
        {
        }

        /// <summary>
        /// Registers all the input parameters for this component.
        /// </summary>
        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Expand-Shrinik", "E/S", "Expand or shrink", GH_ParamAccess.item);
            pManager.AddBooleanParameter("Trigger", "T", "Trigger", GH_ParamAccess.item);
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
            bool shrink = false;
            bool trigger = false;
            
            DA.GetData(0,  ref shrink);
            DA.GetData(1, ref trigger);
            forces new_force = new forces();
            new_force.name = "Expansion";
            new_force.situation = shrink;
            new_force.trigger = trigger;

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
                return Resource1.Artboard_1_copy_30;
            }
        }

        /// <summary>
        /// Gets the unique ID for this component. Do not change this ID after release.
        /// </summary>
        public override Guid ComponentGuid
        {
            get { return new Guid("1c716dc5-4498-4c84-bf67-75eface9df97"); }
        }
    }
}