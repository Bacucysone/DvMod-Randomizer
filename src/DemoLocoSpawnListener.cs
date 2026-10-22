using System.Collections.Generic;
using System.IO;
using Archipelago.MultiClient.Net.Models;
using DV.OriginShift;
using DV.TerrainSystem;
using DV.ThingTypes;
using DV.ThingTypes.TransitionHelpers;
using DV.Utils;
using UnityEngine;

namespace DvMod.Randomizer;

/// <summary>
/// Helper class to scan the demonstrator possible locations to check the corresponding positions
/// </summary>
public class DemoLocoSpawnListener {
    
    /// <summary>
    /// Archipelago location ID for current listener
    /// </summary>
    private readonly long _checkId;
    
    /// <summary>
    /// World position of the AP location
    /// </summary>
    private readonly Vector3 _locoPosition;
    
    /// <summary>
    /// Time of last check
    /// </summary>
    private float _lastTime;
    
    /// <summary>
    /// How close you need to be to trigger the check
    /// </summary>
    private readonly float _spatialThreshold;
    
    /// <summary>
    /// How often the check is triggered. Allow for fewer computations by not checking and sending notifications
    /// every frame
    /// </summary>
    private readonly float _timeThreshold;
    
    /// <summary>
    /// Number of the location check
    /// </summary>
    private readonly int _idx;

    private static Sprite CanCheck;
    private static Sprite CannotCheck;

    private GameObject _positionSign;

    private Vector3 _swayDirection = Vector3.up;
    private float _swayDistance = 0f;

    private static List<DemoLocoSpawnListener> Instances = [];

    public static void ReloadAllSigns() => Instances.ForEach(listener => listener.LoadSign());

    private void LoadSign() {
        string stationNeeded = RandoCommonData.GetStationFromLocoLocation(_idx);
        bool stationOk = Main.Player.GotStationLicense(stationNeeded);
        bool museumOk = SingletonBehaviour<LicenseManager>.Instance.IsGeneralLicenseAcquired(GeneralLicenseType.MuseumCitySouth.ToV2());
        
        if (_positionSign != null)
            Object.Destroy(_positionSign); 
        _positionSign = new GameObject();
        SpriteRenderer renderer = _positionSign.AddComponent<SpriteRenderer>();
        renderer.sprite = stationOk && museumOk ? CanCheck : CannotCheck;
        renderer.transform.SetAbsolutePosition(_locoPosition + new Vector3(0,1,0));
        renderer.transform.localScale = new Vector3(0.05f, 0.05f, 1);
    }
    
    static DemoLocoSpawnListener() {
        Texture2D texture1 = new(1,1);
        texture1.LoadImage(File.ReadAllBytes(Path.Combine(Main.Mod.Path, "icons", "color-icon.png")));
        CanCheck = Sprite.Create(texture1, new Rect(0, 0, texture1.width, texture1.height), new Vector2(0.5f, 0.5f));
        Texture2D texture2 = new(1, 1);
        texture2.LoadImage(File.ReadAllBytes(Path.Combine(Main.Mod.Path, "icons", "blue-icon.png")));
        CannotCheck = Sprite.Create(texture2, new Rect(0, 0, texture2.width, texture2.height), new Vector2(0.5f, 0.5f));
        SingletonBehaviour<LicenseManager>.Instance.LicenseAcquired += license => {
            if (license.v1 == GeneralLicenseType.MuseumCitySouth) ReloadAllSigns();
        };
    }

    public DemoLocoSpawnListener(int idx, float spatialThreshold = 5f, float timeThreshold = 20f) {
        _spatialThreshold = spatialThreshold;
        _timeThreshold = timeThreshold;
        (_locoPosition, _checkId) = RandoCommonData.GetInfoRestorationFromLocoLocationOrder(idx);
        _lastTime = 0f;
        _idx = idx;
        
        Instances.Add(this);

        WorldStreamingInit.LoadingFinished += LoadSign;
        TerrainGrid.TerrainDataLoaded += (_, _) => LoadSign();
        PlayerManager.PlayerTeleportFinished += LoadSign;
    }
    
    /// <summary>
    /// Actual routine of position checking.
    /// Check every frame if the conditions are met (player close enough and last check long enough)
    /// </summary>
    public void CheckPosition() {
        if (PlayerManager.PlayerTransform == null) return;
        _positionSign.transform.localRotation *= Quaternion.AngleAxis(0.4f, Vector3.up);
        _positionSign.transform.localPosition += 0.0008f * _swayDirection;
        _swayDistance += 0.0008f * _swayDirection.y;
        if (_swayDistance is > 0.2f or < -0.2f)
            _swayDirection *= -1f;
        if (Time.time - _lastTime <= _timeThreshold ||
            (PlayerManager.PlayerTransform.AbsolutePosition() - _locoPosition).magnitude >=
              _spatialThreshold) return;
        string stationNeeded = RandoCommonData.GetStationFromLocoLocation(_idx);
        bool stationOk = Main.Player.GotStationLicense(stationNeeded);
        bool museumOk = SingletonBehaviour<LicenseManager>.Instance.IsGeneralLicenseAcquired(GeneralLicenseType.MuseumCitySouth.ToV2());
        if (stationOk && museumOk) {
            ItemInfo item = Main.Player.UnlockCheck(_checkId);
            Main.Player.CheckRestoLoco(_idx);
            Main.NotifyPlayer($"You found a {item.ItemDisplayName} for {item.Player.Name} on the ground!");
            Main.Player.UpdateEvent -= CheckPosition;
            Object.Destroy(_positionSign);
            Instances.Remove(this);
        } else {
            _lastTime = Time.time;
            if (stationOk)
                Main.NotifyPlayer("There is something here but you cannot take it... You need the museum license");
            else if (museumOk)
                Main.NotifyPlayer("There is something here but you cannot take it... You need the "+stationNeeded+" station license");
            else
                Main.NotifyPlayer("There is something here but you cannot take it... You need the museum license and the "+stationNeeded+" station license");
        }
    }
}