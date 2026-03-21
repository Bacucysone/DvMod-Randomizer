using System;
using DV.InventorySystem;
using DV.ThingTypes;
using DV.Utils;
using DvMod.Randomizer;

public abstract class DV_APLocation(long Id) {

    protected long Id = Id;
    public double Amount {get; protected set;} = 0;

    protected abstract void ChangeLocalState();
    public void FullCheck() {
        ChangeLocalState();
        Main.player!.AddLocation(Id);
        Main.player.Session.Locations.CompleteLocationChecks(Id);
    }
    public void EmergencyCheck() {
        WorldStreamingInit.LoadingFinished -= EmergencyCheck;
        ChangeLocalState();
        Main.player!.AddLocation(Id);
        if (!SingletonBehaviour<Inventory>.Instance.RemoveMoney(Amount)) {
            Main.Log("Not enough money when trying to regularize, setting money to 0");
            SingletonBehaviour<Inventory>.Instance.SetMoney(0);
        }
    }
}
public class APLoc_Nothing(long Id) : DV_APLocation(Id) {
    protected override void ChangeLocalState() {}
}
public class APLoc_DemoLocoSpawn(long Id) : DV_APLocation(Id) {
    protected override void ChangeLocalState() =>
        Main.player!.CheckRestoLoco(Id);
}
public class APLoc_ShuntingJobs(long Id) : DV_APLocation(Id) {
    protected override void ChangeLocalState() =>
        Main.player!.FinishShunting(RandoCommonData.GetStationNameFromFinishingJobId(Id));
}
public class APLoc_TransportJobs(long Id) : DV_APLocation(Id) {
    protected override void ChangeLocalState() =>
        Main.player!.FinishTransport(RandoCommonData.GetStationNameFromFinishingJobId(Id));
}
public class APLoc_LocoJobs(long Id) : DV_APLocation(Id) {
    protected override void ChangeLocalState() =>
        Main.player!.FinishLoco(RandoCommonData.GetCarTypeFromID(Id - RandoCommonData.AP_ID.LOC_LOCO_NB_JOBS));
}

public class APLoc_GeneralLicense : DV_APLocation {
    public APLoc_GeneralLicense(long Id) : base(Id) {
        GeneralLicenseType_v2 license = RandoCommonData.GetGeneralLicenseLocFromId(Id);
        Amount = license.price;
    }
    protected override void ChangeLocalState() =>
        Main.player!.CheckGLicense(Id);
}
public class APLoc_JobLicense : DV_APLocation {
    public APLoc_JobLicense(long Id) : base(Id) {
        JobLicenseType_v2 license = RandoCommonData.GetJobLicenseLocFromId(Id);
        Amount = license.price;
    }
    protected override void ChangeLocalState() =>
        Main.player!.CheckJLicense(Id);
}
public class APLoc_UnlockedGarage(long Id) : DV_APLocation(Id) {
    protected override void ChangeLocalState() => Main.player!.UnlockGarage(Id);
}