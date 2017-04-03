using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using System.Reflection;
using MuMech;

namespace Transporter
{
    class MechjebWrapper
    {
        
        System.Type CoreType;

        public bool Initialized = false;
        public static Vessel vessel { get { return FlightGlobals.ActiveVessel; } }
        PartModule core = null;
        

        bool GetCore()
        {
            foreach (Part p in vessel.parts)
            {
                foreach (PartModule module in p.Modules)
                {
                    if (module.GetType() == CoreType)
                    {
                        core = module;
                        return true;
                    }
                }

            }
            return false;
        }

        System.Type FindMechJebModule(string module)
        {
            Type type = null;
            AssemblyLoader.loadedAssemblies.TypeOperation(t =>
            {
                if (t.FullName == module)
                {
                    type = t;
                }
            });

            return type;
        }

        public bool init()
        {
            if (Initialized)
                return true;
            CoreType = FindMechJebModule("MuMech.MechJebCore");

            if (CoreType == null)
            {
              
                return false;
            }
            if (!GetCore())
            {
             
                return false;
            }

            Initialized = true;
            return true;
        }

        public EditableAngle mjLat;
        public EditableAngle mjLon;
        public double mjAlt;
        //public ITargetable mjTar;
        public Transform mjTran;
        public void getCoOrd()
        {
            // get target field info in mechjeb core calss
            var targetInfo = CoreType.GetField("target");
            // get object instance of "target" field in mechjeb core
            var targetControllerObject = targetInfo.GetValue(core);
            // get the class type for target controller
            System.Type MechJebModuleTargetControllerType = FindMechJebModule("MuMech.MechJebModuleTargetController");

            // get targetLatitude field info in target controller class
            var targetLatitudeInfo = MechJebModuleTargetControllerType.GetField("targetLatitude");
            // get object (value) of targetLatitude in target field
            var targetLatitudeObject = targetLatitudeInfo.GetValue(targetControllerObject);
            // type cast to actual editable angle 
            mjLat = (EditableAngle)targetLatitudeObject;

            // get targetLatitude field info in target controller class
            var targetLongitudeInfo = MechJebModuleTargetControllerType.GetField("targetLongitude");
            // get object (value) of targetLatitude in target field
            var targetLongitudeObject = targetLongitudeInfo.GetValue(targetControllerObject);
            // type cast to actual editable angle 
            mjLon = (EditableAngle)targetLongitudeObject;

            mjAlt = FlightGlobals.ActiveVessel.mainBody.TerrainAltitude((double)mjLat, (double)mjLon);


        }
        public void getTrans()
        {
            // get target field info in mechjeb core calss
            var targetInfo = CoreType.GetField("target");
            // get object instance of "target" field in mechjeb core
            var targetControllerObject = targetInfo.GetValue(core);
            // get the class type for target controller
            System.Type MechJebModuleTargetControllerType = FindMechJebModule("MuMech.MechJebModuleTargetController");
            var targetTransformInfo = MechJebModuleTargetControllerType.GetField("Transform");
            var targetTransformObject = targetTransformInfo.GetValue(targetControllerObject);
            mjTran = (Transform)targetTransformObject;
            //mjTran = mjTar.GetTransform();
        }
        public void coordUpdate()
        {
            
        }
    }
}
