using System;
using System.Collections.Generic;
using System.Linq;
using DV.Booklets;
using DV.RenderTextureSystem.BookletRender;
using DV.ThingTypes;
using HarmonyLib;

namespace DvMod.Randomizer.HarmonyPatches;

[HarmonyPatch(typeof(BookletCreator_JobReport))]
public class BookletCreator_JobReportPatch {
    private static void AddData(ref List<TemplatePaperData> mainList, List<JobReportTasksTemplatePaperData.JobReportEntry> toAdd) {
        if (toAdd.Count == 0) return;
        TemplatePaperData lastPage = mainList[mainList.Count-2];
        List<JobReportTasksTemplatePaperData.JobReportEntry> missing;
        int canFit;
        switch (lastPage) {
            case JobReportOverviewTemplatePaperData p : 
                canFit = Math.Min(toAdd.Count, 5-p.reportEntries.Count);
                p.reportEntries.AddRange(toAdd.Take(canFit));
                missing = [.. toAdd.Skip(canFit)];
                break;
            case JobReportTasksTemplatePaperData p : 
                canFit = Math.Min(toAdd.Count, 9-p.reportEntries.Count);
                p.reportEntries.AddRange(toAdd.Take(canFit));
                missing = [.. toAdd.Skip(canFit)];
                break;
            default :
                missing = toAdd;
                break;
        }
        if (missing.Count > 0) {
            int pagesToAdd = missing.Count / 9 + (missing.Count % 9 > 0 ? 1 : 0);
            JobReportPaymentTemplatePaperData lastPageData = (JobReportPaymentTemplatePaperData) mainList.Last();
            string newTotalPages = (int.Parse(lastPageData.totalPages) + pagesToAdd).ToString();
            int newLastPage = int.Parse(lastPageData.pageNumber) + pagesToAdd;
            foreach (TemplatePaperData middlePage in mainList) {
                switch (middlePage) {
                    case JobReportOverviewTemplatePaperData p: p.totalPages = newTotalPages; break;
                    case JobReportTasksTemplatePaperData p: p.totalPages = newTotalPages; break;
                    case JobReportPaymentTemplatePaperData p: p.totalPages = newTotalPages; p.pageNumber = newLastPage.ToString(); break;
                }
            }
            mainList.InsertRange(mainList.Count-1, 
                Enumerable.Range(0, pagesToAdd).Select(
                    i => new JobReportTasksTemplatePaperData([.. missing.Skip(9*i).Take(9)], (newLastPage - pagesToAdd + i).ToString(), newTotalPages)
                )
            );
                
        }
    }

    [HarmonyPostfix, HarmonyPatch("GetReportTemplateData")]
    public static void GetReportTemplateData_Postfix(ref List<TemplatePaperData> __result, Job_data data) {
        if (!Main.IsConnected) return;
        if (data.state != JobState.Completed) return;
        JobFinishState jobState = Main.Player.FinishJob(data);
        List<JobReportTasksTemplatePaperData.JobReportEntry> toAdd = [];
        string job = jobState.IsShunting?"shunting":"transport";
        string otherJob = !jobState.IsShunting?"shunting":"transport";
        if (jobState.GotStationLicense){
            if (jobState.ItemJob1 != null) {
                toAdd.Add(new JobReportTasksTemplatePaperData.JobReportEntry($"You got a {jobState.ItemJob1.ItemDisplayName} for {jobState.ItemJob1.Player.Name} by {job} in {jobState.Station}.", "", JobReportTasksTemplatePaperData.EntryState.COMPLETED));
            }
            if (jobState.ItemJob2 != null) {
                toAdd.Add(new JobReportTasksTemplatePaperData.JobReportEntry($"Doubled! You got a {jobState.ItemJob2.ItemDisplayName} for {jobState.ItemJob2.Player.Name} by {job} in {jobState.Station}.", "", JobReportTasksTemplatePaperData.EntryState.COMPLETED));
            }
            if (jobState.RemainingJobs > 0) 
                toAdd.Add(new JobReportTasksTemplatePaperData.JobReportEntry($"There are {jobState.RemainingJobs} rewards left for {job} in {jobState.Station}", "", JobReportTasksTemplatePaperData.EntryState.IN_PROGRESS));
            else 
                toAdd.Add(new JobReportTasksTemplatePaperData.JobReportEntry($"You got all rewards for {job} in {jobState.Station}", "", JobReportTasksTemplatePaperData.EntryState.COMPLETED));
            if (jobState.RemainingOtherJobs > 0) 
                toAdd.Add(new JobReportTasksTemplatePaperData.JobReportEntry($"There are {jobState.RemainingJobs} rewards left for {otherJob} in {jobState.Station}", "", JobReportTasksTemplatePaperData.EntryState.IN_PROGRESS));
            else 
                toAdd.Add(new JobReportTasksTemplatePaperData.JobReportEntry($"You got all rewards for {otherJob} in {jobState.Station}", "", JobReportTasksTemplatePaperData.EntryState.COMPLETED));
            if (jobState.LastCar == null)
                toAdd.Add(new JobReportTasksTemplatePaperData.JobReportEntry("Could not find your last loco", "", JobReportTasksTemplatePaperData.EntryState.IN_PROGRESS_WITH_X_MARK));
            else {
                string locoName = RandoCommonData.GetLocoNameFromType(jobState.LastCar.Value);
                if (jobState.ItemLoco != null) 
                    toAdd.Add(new JobReportTasksTemplatePaperData.JobReportEntry($"You got a {jobState.ItemLoco.ItemDisplayName} for {jobState.ItemLoco.Player.Name} for finishing enough jobs with a {locoName}", "", JobReportTasksTemplatePaperData.EntryState.COMPLETED));
                else if (jobState.RemainingLoco == 0)
                    toAdd.Add(new JobReportTasksTemplatePaperData.JobReportEntry("You already got the reward for driving the "+locoName, "", JobReportTasksTemplatePaperData.EntryState.COMPLETED));
                else
                    toAdd.Add(new JobReportTasksTemplatePaperData.JobReportEntry($"You need to finish {jobState.RemainingLoco} jobs with a {locoName} to earn a reward", "", JobReportTasksTemplatePaperData.EntryState.IN_PROGRESS));
            }
                
        } else {
            toAdd.Add(new JobReportTasksTemplatePaperData.JobReportEntry("You do not have the required station license. You cannot earn any item for this job", "", JobReportTasksTemplatePaperData.EntryState.IN_PROGRESS_WITH_X_MARK));
        }
        if (jobState.HasWon) {
            toAdd.Add(new JobReportTasksTemplatePaperData.JobReportEntry("You have completed the game!", "", JobReportTasksTemplatePaperData.EntryState.COMPLETED));
        } else if (jobState.RemainingForVictory == 0) {
            toAdd.Add(new JobReportTasksTemplatePaperData.JobReportEntry("You have completed enough jobs in this station","", JobReportTasksTemplatePaperData.EntryState.COMPLETED));
        } else  {
            toAdd.Add(new JobReportTasksTemplatePaperData.JobReportEntry($"You need {jobState.RemainingForVictory} jobs to finish this station", "", JobReportTasksTemplatePaperData.EntryState.IN_PROGRESS));
        } 
        AddData(ref __result, toAdd);
    }
    
}