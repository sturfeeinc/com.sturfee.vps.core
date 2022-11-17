using NUnit.Framework;
using SturfeeVPS.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


public class EventsTests
{
    [Test]
    public void Test_Events_Session()
    {
        List<string> receivedEvents = new List<string>();

        SturfeeEventManager.OnSessionReady += () => { receivedEvents.Add("OnSessionReady"); };
        SturfeeEventManager.OnSessionDestroy += () => { receivedEvents.Add("OnSessionDestroy"); };

        Assert.AreEqual(0, receivedEvents.Count);        

        SessionTests.CreateSession();
        Assert.AreEqual(1, receivedEvents.Count);
        Assert.AreEqual("OnSessionReady", receivedEvents[0]);

        XrSessionManager.DestroySession();
        Assert.AreEqual(2, receivedEvents.Count);
        Assert.AreEqual("OnSessionDestroy", receivedEvents[1]);

        // create again after destroy       
        SessionTests.CreateSession();
        Assert.AreEqual(3, receivedEvents.Count);
        Assert.AreEqual("OnSessionReady", receivedEvents[2]);
    }

    [Test]
    public void Test_Events_TileProvider()
    {
        List<string> receivedEvents = new List<string>();

        SturfeeEventManager.OnTilesLoaded += () => { receivedEvents.Add("OnTilesReady"); };

        SessionTests.CreateSession();

        var tilesProvider = new TestTilesProvider();

        XrSessionManager.RegisterProvider<ITilesProvider>(tilesProvider);

        Assert.AreEqual(0, receivedEvents.Count);

        tilesProvider.TriggerTileLoadEvent();

        Assert.AreEqual(1, receivedEvents.Count);
        Assert.AreEqual("OnTilesReady", receivedEvents[0]);

        XrSessionManager.UnregisterProvider<ITilesProvider>(tilesProvider);

        // trigger again after unregister
        tilesProvider.TriggerTileLoadEvent();

        // count should not go up
        Assert.AreEqual(1, receivedEvents.Count);

        // register again
        XrSessionManager.RegisterProvider<ITilesProvider>(tilesProvider);
        tilesProvider.TriggerTileLoadEvent();

        Assert.AreEqual(2, receivedEvents.Count);
        Assert.AreEqual("OnTilesReady", receivedEvents[1]);
    }

    [Test]
    public void Test_Events_LocalizationProvider()
    {
        List<string> receivedEvents = new List<string>();

        SturfeeEventManager.OnLocalizationRequested += () => { receivedEvents.Add("OnLocalizationRequested"); };
        SturfeeEventManager.OnLocalizationStart += () => { receivedEvents.Add("OnLocalizationStart"); };
        SturfeeEventManager.OnLocalizationLoading += () => { receivedEvents.Add("OnLocalizationLoading"); };
        SturfeeEventManager.OnLocalizationFail += (error, id) => { receivedEvents.Add("OnLocalizationFail"); };
        SturfeeEventManager.OnLocalizationSuccessful += () => { receivedEvents.Add("OnLocalizationSuccessful"); };
        SturfeeEventManager.OnLocalizationDisabled += () => { receivedEvents.Add("OnLocalizationDisabled"); };

        SessionTests.CreateSession();

        var localizationProvider = new TestLocalizationProvider();

        XrSessionManager.RegisterProvider<ILocalizationProvider>(localizationProvider);

        Assert.AreEqual(0, receivedEvents.Count);

        // requested
        localizationProvider.TriggerLocalizationRequestedEvent();
        Assert.AreEqual(1, receivedEvents.Count);
        Assert.AreEqual("OnLocalizationRequested", receivedEvents[0]);

        // start
        localizationProvider.TriggerLocalizationStartEvent();
        Assert.AreEqual(2, receivedEvents.Count);
        Assert.AreEqual("OnLocalizationStart", receivedEvents[1]);

        // loading
        localizationProvider.TriggerLocalizationLoadingEvent();
        Assert.AreEqual(3, receivedEvents.Count);
        Assert.AreEqual("OnLocalizationLoading", receivedEvents[2]);

        // fail
        localizationProvider.TriggerLocalizationFailEvent("localization fail");
        Assert.AreEqual(4, receivedEvents.Count);
        Assert.AreEqual("OnLocalizationFail", receivedEvents[3]);

        // success
        localizationProvider.TriggerLocalizationSuccessfulEvent();
        Assert.AreEqual(5, receivedEvents.Count);
        Assert.AreEqual("OnLocalizationSuccessful", receivedEvents[4]);

        // disabled
        localizationProvider.TriggerLocalizationDisabledEvent();
        Assert.AreEqual(6, receivedEvents.Count);
        Assert.AreEqual("OnLocalizationDisabled", receivedEvents[5]);

        XrSessionManager.UnregisterProvider<ILocalizationProvider>(localizationProvider);

        // trigger again after unregister
        localizationProvider.TriggerLocalizationRequestedEvent();
        localizationProvider.TriggerLocalizationStartEvent();
        localizationProvider.TriggerLocalizationLoadingEvent();
        localizationProvider.TriggerLocalizationFailEvent("");
        localizationProvider.TriggerLocalizationSuccessfulEvent();
        localizationProvider.TriggerLocalizationDisabledEvent();

        // count should not go up
        Assert.AreEqual(6, receivedEvents.Count);

        // register again
        XrSessionManager.RegisterProvider<ILocalizationProvider>(localizationProvider);

        // requested
        localizationProvider.TriggerLocalizationRequestedEvent();
        Assert.AreEqual(7, receivedEvents.Count);
        Assert.AreEqual("OnLocalizationRequested", receivedEvents[6]);

        // start
        localizationProvider.TriggerLocalizationStartEvent();
        Assert.AreEqual(8, receivedEvents.Count);
        Assert.AreEqual("OnLocalizationStart", receivedEvents[7]);

        // loading
        localizationProvider.TriggerLocalizationLoadingEvent();
        Assert.AreEqual(9, receivedEvents.Count);
        Assert.AreEqual("OnLocalizationLoading", receivedEvents[8]);

        // fail
        localizationProvider.TriggerLocalizationFailEvent("localization fail");
        Assert.AreEqual(10, receivedEvents.Count);
        Assert.AreEqual("OnLocalizationFail", receivedEvents[9]);

        // success
        localizationProvider.TriggerLocalizationSuccessfulEvent();
        Assert.AreEqual(11, receivedEvents.Count);
        Assert.AreEqual("OnLocalizationSuccessful", receivedEvents[10]);

        // disabled
        localizationProvider.TriggerLocalizationDisabledEvent();
        Assert.AreEqual(12, receivedEvents.Count);
        Assert.AreEqual("OnLocalizationDisabled", receivedEvents[11]);
    }

}

