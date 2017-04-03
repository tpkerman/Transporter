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
		static Kerbal bufferKerb;
		//Part destination;
		Vessel targetVes;
        public static Vessel thisVes { get { return FlightGlobals.ActiveVessel; } }

        [KSPField(isPersistant = false, guiActive = true, guiName = "ThisPart")]
		public string thisPartName;

		[KSPField(isPersistant = false, guiActive = true, guiName = "TargetPart")]
		public string partName;

		[KSPField(isPersistant = false, guiActive = true, guiName = "Target Occupant")]
		public string inPod;

		[KSPField(isPersistant = false, guiActive = true, guiName = "Buffer")]
		public string inBuffer;

		[KSPField(isPersistant = false, guiActive = true, guiName = "Target Vessel")]
		public string targetVesName;

		//[KSPField(isPersistant = false, guiActive = true, guiName = "Lat")]
		//public string displayLat;

		//[KSPField(isPersistant = false, guiActive = true, guiName = "Lon")]
		//public string displayLon;

		//[KSPField(isPersistant = false, guiActive = true, guiName = "Alt")]
		//public string displayAlt;

		//[KSPField(isPersistant = false, guiActive = true, guiName = "Target Status")]
		//public string targetStat = "Error";

		//[KSPField(isPersistant = false, guiActive = true, guiName = "Distance to Target", guiFormat = "F3")]
		//public double distance;

		//[KSPField(isPersistant = false, guiActive = true, guiName = "MJ Core")]
		//public bool MJActive;

		//[KSPEvent(isPersistent = false, guiActive = true, guiName = "Init")]
		//public void tryInit()
		//{
		//    mucore.init();
		//    if (mucore.init())
		//    { MJActive = true; }

		//}



		

		//[KSPEvent(isPersistent = false, guiActive = true, guiName = "CoOrd Pick")]
		//public void tryPick()
		//{
		//    mucore.getCoOrd();
		//    displayLat = (((double)mucore.mjLat.degrees).ToString() + " " + ((double)mucore.mjLat.minutes).ToString() + " " + ((double)mucore.mjLat.seconds).ToString());
		//    displayLon = (((double)mucore.mjLon.degrees).ToString() + " " + ((double)mucore.mjLon.minutes).ToString() + " " + ((double)mucore.mjLon.seconds).ToString());
		//    displayAlt = mucore.mjAlt.ToString();
		//}

		//[KSPEvent(active = true, guiActive = true, guiName = "Send to Coord", name = "send")]
		//public void sendEventGround()
		//{
		//	bufferKerb = this.part.protoModuleCrew[0].KerbalRef;
		//	Vector3 targetVec = new Vector3((float)mucore.mjLat, (float)mucore.mjLon, (float)mucore.mjAlt);
		//	//Transform targetTrans = mucore.mjTran;
		//	KerbalEVA teleBody = FlightEVA.SpawnEVA(bufferKerb);

		//	teleBody.vessel.vesselTransform = mucore.mjTran;
		//	teleBody.vessel.SetPosition(targetVec, true);
			

		//}

		//[KSPEvent(active = true, guiActive = true, guiName = "Send to Craft", name = "transfer")]
		//public void sendEventShip()
		//{
			
		//	if (targetVes.loaded == false)
		//	{
		//		targetVes.Load();
		//	}
		//	if ((targetVes.parts.Find(x => x.name.Contains("landerCabinSmall"))) & (targetStat.Equals("Ready")))
		//	{
		//		bufferKerb = this.part.protoModuleCrew[0].KerbalRef;
		//		this.part.RemoveCrewmember(this.part.protoModuleCrew[0]);
		//		destination = this.vessel.targetObject.GetVessel().parts.Find(destination => destination.name.Contains("landerCabinSmall"));
		//		//this.vessel.targetObject.GetVessel().MakeActive();
		//		destination.force_activate();
		//		destination.AddCrewmember(buffer);
		//	}

		//	//if (distance >= 2200 & (targetVes.isActiveVessel == false))
		//	//{
		//	//    targetVes.Unload();
		//	//}
		//}

		[KSPEvent(isPersistent = false, guiActive = true, guiActiveUncommand = true, requireFullControl = false, guiName = "killswitch")]
		public void killswtichEngage()
		{
			targetVes.RemoveCrew(targetVes.GetVesselCrew()[0]);
			inPod = targetVes.GetVesselCrew()[0].ToString();
		}

		internal void fireOnVesselChange()
		{
			GameEvents.onVesselChange.Fire(vessel);
		}
		private void setseatstaticoverlay(InternalSeat seat)
		{
			
				if (seat.kerbalRef != null)
				{
					seat.kerbalRef.staticOverlayDuration = 0f;
					seat.kerbalRef.state = Kerbal.States.ALIVE;
				}
			
		}
		public virtual void waitAndCompleteTransfer()
		{
			vessel.SpawnCrew();
		}
		public Part TargetPod;
		public Part thisPod;
		[KSPEvent(isPersistent = false, active = true, guiActive = true, guiName = "Recieve Crewmember", guiActiveUncommand = true, requireFullControl = false, name = "recieve")]
		public void recieveEvent()
		{
			
			
			if (TargetPod != null)
			{
				
					
				bufferKerb = TargetPod.protoModuleCrew[0].KerbalRef;
					buffer = TargetPod.protoModuleCrew[0];
				TargetPod.RemoveCrewmember(buffer);
				
					Vessel.CrewWasModified(TargetPod.vessel);
					targetVes.DespawnCrew();
					StartCoroutine(CallbackUtil.DelayedCallback(1, waitAndCompleteTransfer));

					Vessel.CrewWasModified(targetVes);
					bufferKerb.InPart = null;
					bufferKerb.SetVisibleInPortrait(false);
					bufferKerb.state = Kerbal.States.NO_SIGNAL;
					for (int i = KerbalPortraitGallery.Instance.ActiveCrew.Count - 1; i >= 0; i--)
					{
						//If we find an ActiveCrew entry where the crewMemberName is equal to our kerbal's
						if (KerbalPortraitGallery.Instance.ActiveCrew[i].crewMemberName == bufferKerb.crewMemberName)
						{
							KerbalPortraitGallery.Instance.ActiveCrew.RemoveAt(i);
						}


					}
					KerbalPortraitGallery.Instance.DespawnInactivePortraits(); //Despawn any portraits where CrewMember == null
					KerbalPortraitGallery.Instance.DespawnPortrait(bufferKerb); //Despawn our Kerbal's portrait
					KerbalPortraitGallery.Instance.UIControlsUpdate(); //Update UI controls


				
			}
		}

		[KSPEvent(isPersistent = false, active = true, guiActive = true, guiName = "Spawn from Buffer", guiActiveUncommand = true, requireFullControl = false, name = "spawn")]
		public void spawnFromBuffer()
		{
			
			
				foreach (Part p in thisVes.parts)
				{
					if (p.name.Contains("landerCabinSmall"))
					{
						thisPod = p;
					}
					thisPartName = thisPod.name;
				}
			
				int freeSeat = thisPod.internalModel.GetNextAvailableSeatIndex();
				thisPod.internalModel.SitKerbalAt(buffer, thisPod.internalModel.seats[freeSeat]);
				buffer.seat.SpawnCrew();
				// Think this will get rid of the static that appears on the portrait camera
				setseatstaticoverlay(thisPod.internalModel.seats[freeSeat]);
				buffer.KerbalRef.InPart = thisPod; //Put their kerbalref back in the part
				buffer.KerbalRef.rosterStatus = ProtoCrewMember.RosterStatus.Assigned;
				buffer.KerbalRef.state = Kerbal.States.ALIVE;
				buffer.KerbalRef.SetVisibleInPortrait(true);
				buffer.KerbalRef.Start();
			base.StartCoroutine(CallbackUtil.DelayedCallback(1, new Callback(this.fireOnVesselChange)));
			
			//TargetPod.protoModuleCrew[0] = null;
			inPod = TargetPod.protoModuleCrew[0].name.ToString();

			buffer = null;
				
			
		}

		[KSPEvent(active = true, guiActive = true, guiName = "Hail Destination", name = "hail")]
		public void hailDestination()
		{

			foreach (Part p in targetVes.parts)
			{
				if (p.name.Contains("landerCabinSmall"))
				{
					TargetPod = p;
				}
				partName = TargetPod.name;
			}
			foreach (Part p in thisVes.parts)
			{
				if (p.name.Contains("landerCabinSmall"))
				{
					thisPod = p;
				}
				thisPartName = thisPod.name;
			}
			inPod = TargetPod.protoModuleCrew[0].name.ToString();
			//if (targetVes.loaded == false)
			//{
			//    targetVes.Load();
			//}
			//if ((targetVes.GetCrewCapacity() > targetVes.GetCrewCount()) & (distance <= 1350000))
			//{
			//    targetStat = "Ready";
			//}
			//else
			//{
			//    targetStat = "Error";
			//}
			//if (distance >= 2200 & (targetVes.isActiveVessel == false))
			//{
			//    targetVes.Unload();
			//}
			
		}

		
		

		public override void OnUpdate()
		{

			inBuffer = buffer.name;
			//mucore.getCoOrd();
			//displayLat = mucore.mjLat;
			buffer = bufferKerb.protoCrewMember;



		}
		public void FixedUpdate()
		{
			//distance = Vector3.Distance(FlightGlobals.ActiveVessel.GetWorldPos3D(), this.vessel.targetObject.GetVessel().GetWorldPos3D());
			if(this.vessel.targetObject.GetVessel() != targetVes)
			{
				targetVes = this.vessel.targetObject.GetVessel();
			}
			targetVesName = targetVes.protoVessel.vesselName;
			inPod = TargetPod.protoModuleCrew[0].name.ToString();
			inBuffer = buffer.name;
			
		}
	}


	
	
}

