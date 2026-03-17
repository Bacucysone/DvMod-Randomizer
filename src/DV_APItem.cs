using System;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Enums;
using DV;
using DV.Booklets;
using DV.CabControls;
using DV.Customization.Paint;
using DV.Damage;
using DV.InventorySystem;
using DV.LocoRestoration;
using DV.Simulation.Cars;
using DV.ThingTypes;
using DV.Utils;
using UnityEngine;

namespace DvMod.Randomizer {
    public class CannotAcquireAPItem : Exception
    {
        public CannotAcquireAPItem()
        {
        }

        public CannotAcquireAPItem(string message)
            : base("<AP>"+message)
        {
        }

        public CannotAcquireAPItem(string message, Exception innerException)
            : base("<AP>"+message, innerException)
        {
        }
    }

    public abstract class DV_APItem(int idx, ItemInfo item) {
        public int Idx {get;} = idx;
        protected ItemInfo Item = item;
        public long Id {get => Item.ItemId;}
        public string LocationDisplayName {
            get => Item.Player.Name + " ("+Item.LocationDisplayName+")";
        }
        protected abstract string Name {get;}
        public string DisplayName {
            get => Name+RandoCommonData.GetFromFlags(Item.Flags);
        }
        
        public async Task Acquire() {
            if (IsObtainable){
                bool GotItem;
                do {
                    while (!WorldStreamingInit.IsLoaded) await Task.Yield();
                    Main.Log("Here is a "+DisplayName);
                    GotItem = AcquireUnconditional();
                    await Task.Yield();
                } while (!GotItem);
            } else 
                Main.Log("There is a "+DisplayName+" but you cannot have anymore");
            Main.player!.AddItem(Idx);
        }
        protected abstract bool AcquireUnconditional();
        public abstract bool IsObtainable {get;}
    }

    public class AP_StationLicense(int idx, ItemInfo item) : DV_APItem(idx, item)
    {
        private string Station => RandoCommonData.GetStationNameFromId(Id);
        protected override string Name => Station+" station license";
        protected override bool AcquireUnconditional() {
            Main.player!.AcquireLicense(Station);
            RandoCommonData.AcquireStationLicense(Station);
            return true;
        }
        
        
        public override bool IsObtainable
        {
            get => !Main.player!.GotStationLicense(Station);
        }
    }

    public class AP_GameLicense : DV_APItem
    {
        private readonly GeneralLicenseType_v2[] GLicenseFamily;
        private readonly JobLicenseType_v2[] JLicenseFamily;
        private readonly bool IsGeneral;
        private int LicenseIdx;
        private readonly int NbLicenses;
        public AP_GameLicense(int idx, ItemInfo item) :
            base(idx, item) {
            GLicenseFamily = RandoCommonData.GetGeneralLicenseFromId(Id).CopyLast();
            JLicenseFamily = RandoCommonData.GetJobLicenseFromId(Id).CopyLast();
            IsGeneral = GLicenseFamily.Count() > 0;
            NbLicenses = Math.Max(GLicenseFamily.Count(), JLicenseFamily.Count());
            LicenseIdx = 0;
            if (IsGeneral)
                while (LicenseIdx < NbLicenses && SingletonBehaviour<LicenseManager>.Instance.IsGeneralLicenseAcquired(GLicenseFamily[LicenseIdx])) LicenseIdx++;
            else 
                while (LicenseIdx < NbLicenses && SingletonBehaviour<LicenseManager>.Instance.IsJobLicenseAcquired(JLicenseFamily[LicenseIdx])) LicenseIdx++;
        }

        protected override bool AcquireUnconditional()
        {
            if (IsGeneral){
                SingletonBehaviour<LicenseManager>.Instance.AcquireGeneralLicense(GLicenseFamily[Idx]);
                BookletCreator.CreateLicense(GLicenseFamily[LicenseIdx++], Main.player!.Position, Main.player.Rotation, WorldMover.OriginShiftParent);
            } else {
                SingletonBehaviour<LicenseManager>.Instance.AcquireJobLicense(JLicenseFamily[Idx]);
                BookletCreator.CreateLicense(JLicenseFamily[LicenseIdx++], Main.player!.Position, Main.player.Rotation, WorldMover.OriginShiftParent);
            }
            return true;
        }  

        public override bool IsObtainable
        {
            get => Idx < NbLicenses;
        }

        protected override string Name => IsGeneral?GLicenseFamily[LicenseIdx].v1.ToString():JLicenseFamily[LicenseIdx].v1.ToString();
    }

    public class AP_PhysicalItem(int idx, ItemInfo item) : DV_APItem(idx, item)
    {
        protected override string Name => RandoCommonData.GetItemPrefabFromId(Id);
        protected override bool AcquireUnconditional()
        {
            InventoryItemSpec spec = Globals.G.Items.items.Find(sc => sc.itemPrefabName.Equals(DisplayName));
            InventoryItemSpec inventoryItemSpec = UnityEngine.Object.Instantiate(spec, Main.player!.Position, Main.player!.Rotation);
            inventoryItemSpec.BelongsToPlayer = true;
            ItemBase component = inventoryItemSpec.GetComponent<ItemBase>();
            SingletonBehaviour<StorageController>.Instance.AddItemToWorldStorage(component);
            return true;
        }

        public override bool IsObtainable
        {
            get => true;
        }
    }

    public class AP_Money(int idx, ItemInfo item) : DV_APItem(idx, item)
    {
        protected override string Name => "Money";
        protected override bool AcquireUnconditional()
        {
            SingletonBehaviour<Inventory>.Instance.AddMoney(5000);
            return true;
        }
        public override bool IsObtainable
        {
            get => true;
        }
    }

    public class AP_Nothing(int idx, ItemInfo item) : DV_APItem(idx, item)
    {
        protected override string Name => "Nothing";
        protected override bool AcquireUnconditional()
        {
            throw new ArgumentException("Cannot acquire a nothing item!");
        }
        public override bool IsObtainable
        {
            get => false;
        }
    }
    public class AP_RelicLoco(int idx, ItemInfo item) : DV_APItem(idx, item)
    {
        private PaintTheme? AbandonedTheme => LocoRestorationController.allLocoRestorationControllers?[0]?.abandonedTheme;
        protected override string Name => RandoCommonData.GetRelicNameFromId(Id)+" demo loco advancement";
        protected override bool AcquireUnconditional()
        {
            int RelicLevel = Main.player!.AddRelic(Id);
            LocoRestorationController controller = RandoCommonData.GetLocoControllerFromId(Id);
            switch (RelicLevel) {
                case 1:
                //First level relic: Spawn relic in museum
                try {
                    controller.loco = SpawnOneRelic(controller.garageSpawner.locoSpawnPoint.transform.position, controller.locoLivery, controller.garageSpawner.flipSpawnLoco);
                } catch (CannotAcquireAPItem) {
                    return false;
                }
                if (controller.secondCarLivery != null)
                    controller.secondCar = SpawnOneRelic(controller.garageSpawner.locoSpawnPoint.transform.position, controller.secondCarLivery, controller.garageSpawner.flipSpawnLoco);
                controller.SetState(LocoRestorationController.RestorationState.S4_OnDestinationTrack);
                controller.orderPartsModule.AddThingToCart();
                controller.orderPartsModule.ThingBought += controller.OnPartsOrdered;
                break;
                case 2:
                //Second level relic: Can buy parts installation
                if (controller.State == LocoRestorationController.RestorationState.S7_PartDelivered) {
                    controller.installPartsModule.AddThingToCart();
                    controller.installPartsModule.ThingBought += controller.OnInstallPartsPaid;
                }
                break;
                default:
                throw new ArgumentException("Relic level not right: "+RelicLevel);
            }
            return true;
        }
        private TrainCar SpawnOneRelic(Vector3 position, TrainCarLivery carLivery, bool flipLoco) {
            TrainCar car = SingletonBehaviour<CarSpawner>.Instance.SpawnCarOnClosestTrack(position, carLivery, flipLoco, true, true);
            if (AbandonedTheme is null){
                throw new CannotAcquireAPItem("Abandoned theme is not yet defined");
            }
            if (car.PaintExterior != null)
            {
                car.PaintExterior.CurrentTheme = AbandonedTheme;
            }
            if (car.PaintInterior != null)
            {
                car.PaintInterior.CurrentTheme = AbandonedTheme;
            }
            if (car.TryGetComponent<DamageController>(out var component))
            {
                component.DamageFullyAll();
                if (component.windows != null)
                {
                    component.windows.windowsBroken = true;
                }
            }
            if (car.TryGetComponent<SimController>(out var component2) && component2.resourceContainerController != null)
            {
                component2.resourceContainerController.DepleteAllResourceContainers();
            }
            car.preventDelete = true;
            return car;
        }
        public override bool IsObtainable
        {
            get => !Main.player!.CanFinishRelic(Id);
        }
    }

    public class AP_CrewVehicle(int idx, ItemInfo item) : DV_APItem(idx, item)
    {
        protected override bool AcquireUnconditional(){
            Main.player!.UnlockGarage(Id);
            return true;
        }
        public override bool IsObtainable
        {
            get => !Main.player!.HasUnlocked(RandoCommonData.GetGarageFromId(Id));
        }
        protected override string Name => RandoCommonData.GetNameFromGarageID(Id)+" spawn rights";
    }
}