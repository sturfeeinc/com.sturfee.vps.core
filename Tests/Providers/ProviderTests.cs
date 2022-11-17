using NUnit.Framework;
using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ProviderTests
{
    [Test]
    public void GpsProvider_ApproximateLocation()
    {
        SessionTests.CreateSession();
        var xrSession = XrSessionManager.GetSession();

        var approximateLocation = new GeoLocation { Latitude = 37.332271, Longitude = -121.890196 };
        var fineLocation = new GeoLocation { Latitude = 37.332243, Longitude = -121.889652 };

        var gpsProvider = new TestGpsProvider();
        gpsProvider.ApproximateLocation = approximateLocation;
        gpsProvider.FineLocation = fineLocation;
        gpsProvider.SetProviderStatus(ProviderStatus.Initializing);

        XrSessionManager.RegisterProvider<IGpsProvider>(gpsProvider);
        
        var distanceToApproxLocation = GeoLocation.Distance(approximateLocation, xrSession.Location);
        var distanceToFineLocation = GeoLocation.Distance(fineLocation, xrSession.Location);

        Debug.Log($"session location : {xrSession.Location.ToFormattedString()}");
        Debug.Log($"apprroximate location : {approximateLocation.ToFormattedString()}");
        Debug.Log($"fine location : {fineLocation.ToFormattedString()}");

        Debug.Log($" distanceToApproxLocation : {distanceToApproxLocation}");
        Assert.AreEqual(distanceToApproxLocation, 0, 0.01f);
        Assert.Greater(distanceToFineLocation, 0.01f);
    }

    [Test]
    public void GpsProvider_FineLocation()
    {
        SessionTests.CreateSession();
        var xrSession = XrSessionManager.GetSession();

        var approximateLocation = new GeoLocation { Latitude = 37.332271, Longitude = -121.890196 };
        var fineLocation = new GeoLocation { Latitude = 37.332243, Longitude = -121.889652 };

        var gpsProvider = new TestGpsProvider();
        gpsProvider.ApproximateLocation = approximateLocation;
        gpsProvider.FineLocation = fineLocation;
        gpsProvider.SetProviderStatus(ProviderStatus.Ready);

        XrSessionManager.RegisterProvider<IGpsProvider>(gpsProvider);

        var distanceToApproxLocation = GeoLocation.Distance(approximateLocation, xrSession.Location);
        var distanceToFineLocation = GeoLocation.Distance(fineLocation, xrSession.Location);

        Debug.Log($"session location : {xrSession.Location.ToFormattedString()}");
        Debug.Log($"apprroximate location : {approximateLocation.ToFormattedString()}");
        Debug.Log($"fine location : {fineLocation.ToFormattedString()}");

        Debug.Log($" distanceToFineLocation : {distanceToFineLocation}");
        Assert.AreEqual(distanceToFineLocation, 0, 0.01f);
        Assert.Greater(distanceToApproxLocation, 0.01f);
    }

    [Test]
    public void TilesProvider()
    {
        SessionTests.CreateSession();
        var xrSession = XrSessionManager.GetSession();
        var startLocation = PositioningUtils.GetReferenceLocation;

        var tilesProvider = new TestTilesProvider();
        tilesProvider.Elevation = 15;

        // Elevation set but provider not registered
        var distance = GeoLocation.Distance(startLocation, XrSessionManager.GetSession().Location, true);
        Assert.AreEqual(distance, 0, 0.01f);

        // provider registered
        XrSessionManager.RegisterProvider<ITilesProvider>(tilesProvider);
        
        // startLocation and current location should not be same because of added elevation
        distance = GeoLocation.Distance(startLocation, XrSessionManager.GetSession().Location, true);
        Debug.Log($" startLocation {startLocation.ToFormattedString()}, xr location : {XrSessionManager.GetSession().Location.ToFormattedString()}");
        Assert.Greater(distance, 0.01f);

        // startLocationEithElevatino and current location should be same because of added elevation
        var startLocationWithElevation = startLocation;
        startLocationWithElevation.Altitude = tilesProvider.Elevation;
        distance = GeoLocation.Distance(startLocationWithElevation, XrSessionManager.GetSession().Location, true);
        Debug.Log($" startLocation + elevation {startLocationWithElevation.ToFormattedString()}, xr location : {XrSessionManager.GetSession().Location.ToFormattedString()}");
        Assert.AreEqual(distance, 0, 0.01f);

    }

}
