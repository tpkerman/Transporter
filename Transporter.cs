using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using KSP;
using KSP.IO;
using KSP.UI.Screens.Flight;
using MuMech;

namespace Transporter
{
    
    public class transportPlugin : PartModule
    {
        MechjebWrapper mucore = new MechjebWrapper();
        ProtoCrewMember buffer;
        Part destination;
        Vessel targetVes;
        public bool refresh = false;
        
        public static Vessel thisVes { get { return FlightGlobals.ActiveVessel; } }
        

       

        [KSPField(isPersistant = false, guiActive = true, guiName = "Active Vessel")]
        public string vesselName;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Occupant")]
        public string inPod;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Buffer")]
        public string inBuffer;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Target Vessel")]
        public string targetVesName;

        [KSPField(isPersistant = false, guiActive = true, guiName = "CoOrd")]
        public double displayLat;

        [KSPField(isPersistant = false, guiActive = true, guiName = "Target Status")]
        public string targetStat = "Error";
        
        [KSPField(isPersistant = false, guiActive = true, guiName = "Distance to Target", guiFormat = "F3")]
        public double distance;

        [KSPField(isPersistant = false, guiActive = true, guiName = "MJ Core")]
        public bool MJActive;

        [KSPEvent(isPersistent = false, guiActive = true, guiName = "Init")]
        public void tryInit()
        {
            mucore.init();
            
        }

        

        [KSPEvent(isPersistent = false, guiActive = true, guiName = "CoOrd Pick")]
        public void tryPick()
        {
            mucore.getCoOrd();
            displayLat = (double)mucore.mjLat;
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Send Crewmember", name = "transfer")]
        public void sendEvent()
        {
            if (targetVes.loaded == false)
            {
                targetVes.Load();
            }
            if ((targetVes.parts.Find(x => x.name.Contains("landerCabinSmall"))) & (targetStat.Equals("Ready")))
            {
                buffer = this.part.protoModuleCrew[0];
                this.part.RemoveCrewmember(this.part.protoModuleCrew[0]);
                destination = this.vessel.targetObject.GetVessel().parts.Find(destination => destination.name.Contains("landerCabinSmall"));
                //this.vessel.targetObject.GetVessel().MakeActive();
                destination.force_activate();
                destination.AddCrewmember(buffer);
            }

            if (distance >= 2200 & (targetVes.isActiveVessel == false))
            {
                targetVes.Unload();
            }
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Recieve Crewmember", guiActiveUncommand = true,requireFullControl = false, name = "recieve")]
        public void recieveEvent()
        {
            buffer = targetVes.GetVesselCrew()[0];
            targetVes.Die();
            targetVes.DestroyVesselComponents();
            targetVes.Unload();
            this.part.AddCrewmember(buffer);
            KerbalPortraitGallery.Instance.Awake();
            KerbalPortraitGallery.Instance.Start();
            refresh = true;
        }

        [KSPEvent(active = true, guiActive = true, guiName = "Hail Destination", name = "hail")]
        public void hailDestination()
        {
            

            if (targetVes.loaded == false)
            {
                targetVes.Load();
            }
            if ((targetVes.GetCrewCapacity() > targetVes.GetCrewCount()) & (distance <= 1350000))
            {
                targetStat = "Ready";
            }
            else
            {
                targetStat = "Error";
            }
            if (distance >= 2200 & (targetVes.isActiveVessel == false))
            {
                targetVes.Unload();
            }
            
        }

        
        

        public override void OnUpdate()
        {
            inPod = this.part.protoModuleCrew[0].name.ToString();
            

            //mucore.getCoOrd();
            //displayLat = mucore.mjLat;



        }
        public void FixedUpdate()
        {
            distance = Vector3.Distance(FlightGlobals.ActiveVessel.GetWorldPos3D(), this.vessel.targetObject.GetVessel().GetWorldPos3D());
            targetVes = this.vessel.targetObject.GetVessel();
            targetVesName = targetVes.protoVessel.vesselName;
            if(mucore.Initialized)
            { MJActive = true; }
            
            if (refresh)
            {
                this.part.protoModuleCrew[0].KerbalRef.state = Kerbal.States.ALIVE;
                this.part.protoModuleCrew[0].KerbalRef.SetVisibleInPortrait(true);
                
                this.part.protoModuleCrew[0].KerbalRef.randomizeOnStartup = false;
                this.part.protoModuleCrew[0].KerbalRef.Start();
                KerbalPortraitGallery.Instance.SpawnPortrait(buffer.KerbalRef);
                KerbalPortraitGallery.Instance.StartReset(thisVes);

                refresh = false;
            }
        }
    }


    
    
}

