using NUnit.Framework;
using SturfeeVPS.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SessionTests 
{
    [Test]
    public static void Test_CreateSession()
    {
        CreateSession();
    }

    public static GeoLocation CreateSession()
    {
        GeoLocation location = new GeoLocation
        {
            Latitude = 37.332093,
            Longitude = -121.890137
        };

        XrSessionManager.CreateSession(location);

        var distance = GeoLocation.Distance(location, XrSessionManager.GetSession().Location);
        Assert.AreEqual(distance, 0, 0.01f);

        Assert.AreEqual(GeoLocation.Distance(PositioningUtils.GetReferenceLocation, location), 0, 0.01f);

        return location;
    }
}
