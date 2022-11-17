using NUnit.Framework;
using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ARTests : MonoBehaviour
{
    [Test]
    public void AR_Before_Localization()
    {
        SessionTests.CreateSession();

        var startLocation = new GeoLocation{ Latitude = 37.332093, Longitude = -121.890137 };

        // Providers

        // gps
        var approximateLocation = new GeoLocation { Latitude = 37.332271, Longitude = -121.890196 };
        var fineLocation = new GeoLocation { Latitude = 37.332243, Longitude = -121.889652 };

        var gpsProvider = new TestGpsProvider();
        gpsProvider.ApproximateLocation = approximateLocation;
        gpsProvider.FineLocation = fineLocation;

        // tiles
        var tilesProvider = new TestTilesProvider();
        tilesProvider.Elevation = 15;


        // pose
        var poseProvider = new TestPoseProvider();

        poseProvider.Position = new Vector3(3, 4, 1);
        poseProvider.Rotation = Quaternion.Euler(3, 2, 45);
        poseProvider.HeightFromGround = 1.2f;

        
        var session = XrSessionManager.GetSession();
        Assert.True(session.Location.Equals(startLocation));
        //Assert.AreEqual(session.Location, startLocation.ToFormattedString());

        XrSessionManager.RegisterProvider<IGpsProvider>(gpsProvider);
        Assert.True(session.Location.Equals(new GeoLocation { Latitude = 37.332243, Longitude = -121.889652 }));
        Assert.AreEqual(GeoLocation.Distance(session.Location, startLocation, true), 46.08f, 0.01f);


        Debug.Log(session.PositionOffset.ToString("f4"));
        Debug.Log(session.RotationOffset.eulerAngles);

        XrSessionManager.RegisterProvider<ITilesProvider>(tilesProvider);        
        Assert.True(session.Location.Equals(new GeoLocation { Latitude = 37.332243, Longitude = -121.889652, Altitude = 15 }));
        Assert.AreEqual(GeoLocation.Distance(session.Location, startLocation, true), 48.46f, 0.01f);

        
        XrSessionManager.RegisterProvider<IPoseProvider>(poseProvider);
        Assert.True(session.Location.Equals(new GeoLocation { Latitude = 37.3322787339997, Longitude = -121.889617610851, Altitude = 17.2 }));
        Assert.AreEqual(GeoLocation.Distance(session.Location, startLocation, true), 53.27f, 0.01f);
        Assert.True(session.Orientation.eulerAngles == new Vector3(3, 2, 45)); 
    }

    [Test]
    public void AR_After_Localization()
    {
        AR_Before_Localization();

        var localizationProvider = new TestLocalizationProvider();
        localizationProvider.YawOffset = Quaternion.Euler(0, 0, 25);
        localizationProvider.PitchOffset = Quaternion.Euler(5, 0, 0);
        localizationProvider.RollOffset = Quaternion.Euler(0, -5, 0);
        localizationProvider.EulerOffset = new Vector3(5, -5, 25);
        localizationProvider.VpsLocation = new GeoLocation { Latitude = 37.332271, Longitude = -121.890196 };

        var session = XrSessionManager.GetSession();
        var startLocation = new GeoLocation { Latitude = 37.332093, Longitude = -121.890137 };

        Debug.Log(session.PositionOffset.ToString("f4")); 
        Debug.Log(session.RotationOffset.eulerAngles);

        Assert.True(session.Location.Equals(new GeoLocation { Latitude = 37.3322787339997, Longitude = -121.889617610851, Altitude = 17.2 }));
        Assert.True(session.Orientation.eulerAngles == new Vector3(3, 2, 45));
        Assert.True(Vector3.Distance(session.PositionOffset,new Vector3(45.7716f, 21.147f, 17.2f)) < 0.01f);
        Assert.True(session.RotationOffset.eulerAngles == new Vector3(0, 0, 0));

        // Register LocalizationProvider
        XrSessionManager.RegisterProvider<ILocalizationProvider>(localizationProvider);

        Debug.Log(session.PositionOffset);
        Debug.Log(session.RotationOffset.eulerAngles);


        //Assert.True(session.Location.Equals(new GeoLocation { Latitude = 37.3322787339997, Longitude = -121.889617610851, Altitude = 17.2 }));
        //Assert.True(session.Orientation.eulerAngles == new Vector3(3, 2, 45));
        //Assert.True(session.PositionOffset == new Vector3(0, 0, 0));
        //Assert.True(session.RotationOffset.eulerAngles == new Vector3(0, 0, 0));


        XrSessionManager.UnregisterProvider<IPoseProvider>(XrSessionManager.GetSession().GetProvider<IPoseProvider>());
        Debug.Log(session.PositionOffset);
        Debug.Log(session.RotationOffset.eulerAngles);
    }

    private void Arrangement_Before_Localization()
    {
        SessionTests.CreateSession();

        var startLocation = new GeoLocation { Latitude = 37.332093, Longitude = -121.890137 };

        // Providers

        // gps
        var approximateLocation = new GeoLocation { Latitude = 37.332271, Longitude = -121.890196 };
        var fineLocation = new GeoLocation { Latitude = 37.332243, Longitude = -121.889652 };

        var gpsProvider = new TestGpsProvider();
        gpsProvider.ApproximateLocation = approximateLocation;
        gpsProvider.FineLocation = fineLocation;

        // tiles
        var tilesProvider = new TestTilesProvider();
        tilesProvider.Elevation = 15;


        // pose
        var poseProvider = new TestPoseProvider();

        poseProvider.Position = new Vector3(3, 4, 1);
        poseProvider.Rotation = Quaternion.Euler(3, 2, 45);
        poseProvider.HeightFromGround = 1.2f;


        var session = XrSessionManager.GetSession();
        Assert.True(session.Location.Equals(startLocation));
        //Assert.AreEqual(session.Location, startLocation.ToFormattedString());

        XrSessionManager.RegisterProvider<IGpsProvider>(gpsProvider);
        Assert.True(session.Location.Equals(new GeoLocation { Latitude = 37.332243, Longitude = -121.889652 }));
        Assert.AreEqual(GeoLocation.Distance(session.Location, startLocation, true), 46.08f, 0.01f);

        XrSessionManager.RegisterProvider<ITilesProvider>(tilesProvider);
        Assert.True(session.Location.Equals(new GeoLocation { Latitude = 37.332243, Longitude = -121.889652, Altitude = 15 }));
        Assert.AreEqual(GeoLocation.Distance(session.Location, startLocation, true), 48.46f, 0.01f);


        XrSessionManager.RegisterProvider<IPoseProvider>(poseProvider);
        Assert.True(session.Location.Equals(new GeoLocation { Latitude = 37.3322787339997, Longitude = -121.889617610851, Altitude = 17.2 }));
        Assert.AreEqual(GeoLocation.Distance(session.Location, startLocation, true), 53.27f, 0.01f);
        Assert.True(session.Orientation.eulerAngles == new Vector3(3, 2, 45));
    }
}
