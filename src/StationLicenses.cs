using System.Collections.Generic;
using System.Linq;
using DV.Booklets;
using DV.Printers;
using DV.RenderTextureSystem.BookletRender;
using DV.ThingTypes;
using HarmonyLib;

namespace DvMod.Randomizer
{

    [HarmonyPatch(typeof(BookletCreator_JobMissingLicense), "GetMissingLicenseTemplateData")]
    public static class BookletPatch {
        public static void Postfix(List<TemplatePaperData> __result, Job_data job) {
            if (Main.player == null) 
                return;
            StationInfo sKey = job.type switch
            {
                JobType.ShuntingUnload => job.chainDestinationStationInfo,
                _ => job.chainOriginStationInfo,
            };
            MissingLicensesPageTemplatePaperData pData = (MissingLicensesPageTemplatePaperData)__result[0];
            pData.licensesData.Add(new(sKey.YardID+" station license", RandoCommonData.GetStationSprite(sKey.YardID), Main.player.GotStationLicense(sKey.YardID)));
        }
    }

    [HarmonyPatch(typeof(JobValidator), nameof(JobValidator.ProcessJobOverview))]
    public static class JobPatcher {
        public static bool Prefix(JobOverview jobOverview, PrinterController ___bookletPrinter) {
            if (Main.player == null)
                return true;

            if (jobOverview.job.State != JobState.Available) 
                return true;

            StationController stationController = StationController.allStations.FirstOrDefault(st => st.logicStation.availableJobs.Contains(jobOverview.job));
            if (stationController != null && !Main.player.GotStationLicense(stationController.stationInfo.YardID)) {
                BookletCreator.CreateMissingLicenseReport(jobOverview.job, true, ___bookletPrinter.spawnAnchor.position, ___bookletPrinter.spawnAnchor.rotation, WorldMover.OriginShiftParent);
                ___bookletPrinter.PlayErrorSound();
                ___bookletPrinter.Print();
                return false;
            }
            return true;
        }
    }
    
}