﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace KerbalConstructionTime
{
    public class Recon_Rollout : IKCTBuildItem
    {
        [Persistent] private string name = "";
        [Persistent] public double BP = 0, progress = 0, cost = 0;
        [Persistent] public string associatedID = "";
        [Persistent] public string launchPadID = "LaunchPad";
        public enum RolloutReconType { Reconditioning, Rollout, Rollback, Recovery, None };
        private RolloutReconType RRTypeInternal = RolloutReconType.None;
        public RolloutReconType RRType
        {
            get
            {
                if (RRTypeInternal != RolloutReconType.None)
                    return RRTypeInternal;
                else
                {
                    if (name == "LaunchPad Reconditioning")
                        RRTypeInternal = RolloutReconType.Reconditioning;
                    else if (name == "Vessel Rollout")
                        RRTypeInternal = RolloutReconType.Rollout;
                    else if (name == "Vessel Rollback")
                        RRTypeInternal = RolloutReconType.Rollback;
                    else if (name == "Vessel Recovery")
                        RRTypeInternal = RolloutReconType.Recovery;
                    return RRTypeInternal;
                }
            }
            set
            {
                RRTypeInternal = value;
            }
        }
        public BuildListVessel associatedBLV
        {
            get
            {
                if (KSC != null)
                {
                    BuildListVessel ves = KSC.VABWarehouse.Find(blv => blv.id == new Guid(associatedID));
                    if (ves != null)
                        return ves;

                    ves = KSC.VABList.Find(blv => blv.id == new Guid(associatedID));
                    if (ves != null)
                        return ves;

                    ves = KSC.SPHWarehouse.Find(blv => blv.id == new Guid(associatedID));
                    if (ves != null)
                        return ves;

                    ves = KSC.SPHList.Find(blv => blv.id == new Guid(associatedID));
                    if (ves != null)
                        return ves;
                }
                return null;
            }
        }
        public SpaceCenterConstruction KSC { get { return GameStates.KSCs.Count > 0 ? GameStates.KSCs.FirstOrDefault(k => k.Recon_Rollout.Exists(r=> r.associatedID == this.associatedID)) : null;} }

        public Recon_Rollout()
        {
            name = "LaunchPad Reconditioning";
            progress = 0;
            BP = 0;
            cost = 0;
            RRType = RolloutReconType.None;
            associatedID = "";
            launchPadID = "LaunchPad";
        }

        public Recon_Rollout(Vessel vessel, RolloutReconType type, string id, string launchSite)
        {
            RRType = type;
            associatedID = id;
            launchPadID = launchSite;
            Log.Trace("New recon_rollout at launchsite: " + launchPadID);
            //BP = vessel.GetTotalMass() * KCT_GameStates.timeSettings.ReconditioningEffect * KCT_GameStates.timeSettings.OverallMultiplier; //1 day per 50 tons (default) * overall multiplier
            //BP = KCT_MathParsing.GetStandardFormulaValue("Reconditioning", new Dictionary<string, string>() {{"M", vessel.GetTotalMass().ToString()}, {"O", KCT_PresetManager.Instance.ActivePreset.timeSettings.OverallMultiplier.ToString()},
            //    {"E", KCT_PresetManager.Instance.ActivePreset.timeSettings.ReconditioningEffect.ToString()}, {"X", KCT_PresetManager.Instance.ActivePreset.timeSettings.MaxReconditioning.ToString()}});
            //if (BP > KCT_GameStates.timeSettings.MaxReconditioning) BP = KCT_GameStates.timeSettings.MaxReconditioning;
            progress = 0;
            if (type == RolloutReconType.Reconditioning) 
            {
                try
                {
                    BP = KCT_MathParsing.ParseReconditioningFormula(new BuildListVessel(vessel), true);
                }
                catch
                {
                    Log.Trace("Error while determining BP for recon_rollout");
                }
                finally
                {
                    name = "LaunchPad Reconditioning";
                }
            }
            else if (type == RolloutReconType.Rollout)
            {
                try
                {
                    BP = KCT_MathParsing.ParseReconditioningFormula(new BuildListVessel(vessel), false);
                }
                catch
                {
                    Log.Trace("Error while determining BP for recon_rollout");
                }
                finally
                {
                    name = "Vessel Rollout";
                }
            }
            else if (type == RolloutReconType.Rollback)
            {
                try
                {
                    BP = KCT_MathParsing.ParseReconditioningFormula(new BuildListVessel(vessel), false);
                }
                catch
                {
                    Log.Trace("Error while determining BP for recon_rollout");
                }
                finally
                {
                    name = "Vessel Rollback";
                    progress = BP;
                }
            }
            else if (type == RolloutReconType.Recovery)
            {
                try
                {
                    BP = KCT_MathParsing.ParseReconditioningFormula(new BuildListVessel(vessel), false);
                }
                catch
                {
                    Log.Trace("Error while determining BP for recon_rollout");
                }
                finally
                {
                    name = "Vessel Recovery";
                    double KSCDistance = (float)SpaceCenter.Instance.GreatCircleDistance(SpaceCenter.Instance.cb.GetRelSurfaceNVector(vessel.latitude, vessel.longitude));
                    double maxDist = SpaceCenter.Instance.cb.Radius * Math.PI;
                    BP += BP * (KSCDistance / maxDist);
                }
            }
        }

        public Recon_Rollout(BuildListVessel vessel, RolloutReconType type, string id, string launchSite="")
        {
            RRType = type;
            associatedID = id;
            if (launchSite != "") //For when we add custom launchpads
                launchPadID = launchSite;
            else
                launchPadID = vessel.launchSite;
            //BP = vessel.GetTotalMass() * KCT_GameStates.timeSettings.ReconditioningEffect * KCT_GameStates.timeSettings.OverallMultiplier; //1 day per 50 tons (default) * overall multiplier
            //BP = KCT_MathParsing.GetStandardFormulaValue("Reconditioning", new Dictionary<string, string>() {{"M", vessel.GetTotalMass().ToString()}, {"O", KCT_PresetManager.Instance.ActivePreset.timeSettings.OverallMultiplier.ToString()},
            //    {"E", KCT_PresetManager.Instance.ActivePreset.timeSettings.ReconditioningEffect.ToString()}, {"X", KCT_PresetManager.Instance.ActivePreset.timeSettings.MaxReconditioning.ToString()}});
            //if (BP > KCT_GameStates.timeSettings.MaxReconditioning) BP = KCT_GameStates.timeSettings.MaxReconditioning;
            progress = 0;
            if (type == RolloutReconType.Reconditioning)
            {
                BP = KCT_MathParsing.ParseReconditioningFormula(vessel, true);
                //BP *= (1 - KCT_PresetManager.Instance.ActivePreset.timeSettings.RolloutReconSplit);
                name = "LaunchPad Reconditioning";
            }
            else if (type == RolloutReconType.Rollout)
            {
                BP = KCT_MathParsing.ParseReconditioningFormula(vessel, false);
                //BP *= KCT_PresetManager.Instance.ActivePreset.timeSettings.RolloutReconSplit;
                name = "Vessel Rollout";
                cost = KCT_MathParsing.ParseRolloutCostFormula(vessel);
            }
            else if (type == RolloutReconType.Rollback)
            {
                BP = KCT_MathParsing.ParseReconditioningFormula(vessel, false);
                //BP *= KCT_PresetManager.Instance.ActivePreset.timeSettings.RolloutReconSplit;
                progress = BP;
                name = "Vessel Rollback";
            }
            else if (type == RolloutReconType.Recovery)
            {
                BP = KCT_MathParsing.ParseReconditioningFormula(vessel, false);
                //BP *= KCT_PresetManager.Instance.ActivePreset.timeSettings.RolloutReconSplit;
                name = "Vessel Recovery";
                double maxDist = SpaceCenter.Instance.cb.Radius * Math.PI;
                BP += BP * (vessel.DistanceFromKSC / maxDist);
            }
        }

        public void SwapRolloutType()
        {
            if (RRType == RolloutReconType.Rollout)
            {
                RRType = RolloutReconType.Rollback;
                name = "Vessel Rollback";
            }
            else if (RRType == RolloutReconType.Rollback)
            {
                RRType = RolloutReconType.Rollout;
                name = "Vessel Rollout";
            }
        }

        public double ProgressPercent()
        {
            return Math.Round(100 * (progress / BP), 2);
        }

        string IKCTBuildItem.GetItemName()
        {
            return name;
        }

        double IKCTBuildItem.GetBuildRate()
        {
            List<double> rates = new List<double>();
            if (associatedBLV != null && associatedBLV.type == BuildListVessel.ListType.SPH)
                rates = KCT_Utilities.BuildRatesSPH(KSC);
            else
                rates = KCT_Utilities.BuildRatesVAB(KSC);
            double buildRate = 0;
            foreach (double rate in rates)
                buildRate += rate;
            if (RRType == RolloutReconType.Rollback)
                buildRate *= -1;
            return buildRate;
        }

        double IKCTBuildItem.GetTimeLeft()
        {
            double timeLeft = (BP - progress) / ((IKCTBuildItem)this).GetBuildRate();
            if (RRType == RolloutReconType.Rollback)
                timeLeft = (-progress) / ((IKCTBuildItem)this).GetBuildRate();
            return timeLeft;
        }

        BuildListVessel.ListType IKCTBuildItem.GetListType()
        {
            return BuildListVessel.ListType.Reconditioning;
        }

        bool IKCTBuildItem.IsComplete()
        {
            bool complete = progress >= BP;
            if (RRType == RolloutReconType.Rollback)
                complete = progress <= 0;
            return complete;
        }

        public IKCTBuildItem AsBuildItem()
        {
            return (IKCTBuildItem)this;
        }
    }
}
