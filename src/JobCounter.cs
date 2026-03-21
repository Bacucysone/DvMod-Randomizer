using DV.Booklets;
using HarmonyLib;
using DV.ThingTypes;
using UnityEngine;
using System.Collections.Generic;
using DV.RenderTextureSystem.BookletRender;
using System.Linq;
using DV.CabControls.Spec;
using Archipelago.MultiClient.Net.Models;
using System.Text;
namespace DvMod.Randomizer
{
    [HarmonyPatch(typeof(BookletCreator_JobReport))]
    public class JobCounter {

        [HarmonyPatch("GetReportTemplateData")]
        public static void Postfix(ref List<TemplatePaperData> __result, Job_data data) {
            if (Main.player == null) return;
            if (data.state != JobState.Completed) return;
            string Station = data.type switch {
                JobType.ShuntingUnload => data.chainDestinationStationInfo.YardID,
                _ => data.chainOriginStationInfo.YardID
            };
            bool IsShuntingJob = data.type == JobType.ShuntingLoad || data.type == JobType.ShuntingUnload;
            List<JobReportTasksTemplatePaperData.JobReportEntry> ToAdd = [];
            (int curr, int max) = IsShuntingJob ? Main.player.GetShuntingData(Station) : Main.player.GetTransportData(Station);
            StringBuilder sb = new();
            if (curr < max) {
                ItemInfo JobItem = Main.player.UnlockCheck(RandoCommonData.ComputeCheckForJob(IsShuntingJob, Station, curr));
                sb.AppendLine($"You got a {JobItem.ItemDisplayName}.");
            }
            string job = IsShuntingJob?"shunting":"transport";
            if (curr < max-1) 
                sb.AppendLine($"There are {max-curr+1} rewards left for {job} in station {Station}");
            else 
                sb.AppendLine($"You got all rewards for {job} in station {Station}");
            ToAdd.Add(new(sb.ToString(), "", curr < max-1?JobReportTasksTemplatePaperData.EntryState.IN_PROGRESS:JobReportTasksTemplatePaperData.EntryState.COMPLETED));

            TrainCarType LastLoco = PlayerManager.LastLoco.carType;
            (long checkLoco, int remainingLoco) = Main.player.FinishLoco(PlayerManager.LastLoco.carType);
            if (remainingLoco < 0)
                ToAdd.Add(new("You already got the reward for using the "+RandoCommonData.GetLocoNameFromType(LastLoco), "", JobReportTasksTemplatePaperData.EntryState.COMPLETED));
            else if (remainingLoco == 0) {
                ItemInfo LocoItem = Main.player.UnlockCheck(checkLoco);
                ToAdd.Add(new("You finished enough job with a "+RandoCommonData.GetLocoNameFromType(LastLoco)+" to get a "+LocoItem.ItemDisplayName, "", JobReportTasksTemplatePaperData.EntryState.COMPLETED));
            } else
                ToAdd.Add(new($"You still need {remainingLoco} jobs with a {RandoCommonData.GetLocoNameFromType(LastLoco)} to get a reward", "", JobReportTasksTemplatePaperData.EntryState.IN_PROGRESS));
            
            TemplatePaperData lastPage = __result[__result.Count-2];
            if (lastPage is JobReportOverviewTemplatePaperData overviewData) {
                if (overviewData.reportEntries.Count <= 3) {
                    overviewData.reportEntries.AddRange(ToAdd);
                    ToAdd = [];
                } else if (overviewData.reportEntries.Count == 4) {
                    overviewData.reportEntries.Add(ToAdd[0]);
                    ToAdd = [ToAdd[1]];
                } 
            } else if (lastPage is JobReportTasksTemplatePaperData reportData) {
                if (reportData.reportEntries.Count <= 7) {
                    reportData.reportEntries.AddRange(ToAdd);
                    ToAdd = [];
                } else if (reportData.reportEntries.Count == 8) {
                    reportData.reportEntries.Add(ToAdd[0]);
                    ToAdd = [ToAdd[1]];
                }
            }
            if (ToAdd.Count > 0) {
                JobReportPaymentTemplatePaperData lastPageData = (JobReportPaymentTemplatePaperData) __result.Last();
                string newTotalPage = (int.Parse(lastPageData.totalPages)+1).ToString();
                string newLastPage = (int.Parse(lastPageData.pageNumber)+1).ToString();
                foreach (TemplatePaperData page in __result){
                    switch (page) {
                        case JobReportOverviewTemplatePaperData p: p.totalPages = newTotalPage; break;
                        case JobReportTasksTemplatePaperData p: p.totalPages = newTotalPage; break;
                        case JobReportPaymentTemplatePaperData p: p.totalPages = newTotalPage; p.pageNumber = newLastPage; break;
                    }
                }
                __result.Insert(__result.Count-1, new JobReportTasksTemplatePaperData(ToAdd, (int.Parse(newLastPage)-1).ToString(), newTotalPage));
            }
        }
    
    }
}