using Google.Protobuf;
using SturfeeVPS.Core.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace SturfeeVPS.Core
{
    internal class HDScanner : PanoramicMultiframeScanner
    {
        public async override Task Initialize(ScanConfig scanConfig = null, CancellationToken cancellationToken = default)
        {
            SturfeeDebug.Log("Initializing HDScanner");

            this.scanConfig = scanConfig;
            serviceUrl = ServerInfo.VPSHD_WEBSOCKET;
            ScanType = ScanType.HD;
            OffsetType = OffsetType.Euler;

            xrSession = XRSessionManager.GetSession();
            if (xrSession == null)
            {
                throw new Exception($"HDScanner : ERROR => cannot initialize. XRSession is NULL");
            }

            // set scan properties
            ScanProperties.YawAngle = ScanProperties.Defaults.HD.YawAngle;
            ScanProperties.TargetCount = ScanProperties.Defaults.HD.TargetCount;
            ScanProperties.PitchMin = ScanProperties.Defaults.HD.PitchMin;
            ScanProperties.PitchMax = ScanProperties.Defaults.HD.PitchMax;
            ScanProperties.RollMin = ScanProperties.Defaults.HD.RollMin;
            ScanProperties.RollMax = ScanProperties.Defaults.HD.RollMax;

            List<Task> providerTasks = new List<Task>() {
                //xrSession.WaitForGps(cancellationToken),                      // we use site's gps
                xrSession.WaitForPose(cancellationToken),
                xrSession.WaitForVideo(cancellationToken)
            };
            await Task.WhenAll(providerTasks);
            SturfeeDebug.Log("All providers ready");

            await xrSession.CheckCoverage(scanConfig.HD.Location);

            List<Task> preScanTasks = new List<Task>() {
                //xrSession.GpsProvider.PrepareForScan(cancellationToken),      // We use site's gps
                xrSession.PoseProvider.PrepareForScan(cancellationToken),
                xrSession.VideoProvider.PrepareForScan(cancellationToken)
            };
            await Task.WhenAll(preScanTasks);
            SturfeeDebug.Log("All providers ready for scan");

            initialized = true;
        }

        public override async Task Connect(string accessToken, string language = "en-US")
        {
            await base.Connect(accessToken, language);

            try
            {
                var location = scanConfig.HD.Location;
                SturfeeDebug.Log($" Using site location {location.ToFormattedString()} to establish socket connection");

                await localizationService.Connect(location.Latitude, location.Longitude);
            }
            catch (Exception e)
            {
                SturfeeDebug.LogError(e.Message);
                throw new SessionException(ErrorMessages.SocketConnectionFail);
            }
        }

        protected override Request CaptureRequest(uint requestId, OperationMessages operationMessage, uint numOfFrames, uint frameOrder, string trackingId)
        {
            GeoLocation location = scanConfig.HD.Location;

            Request request = new Request
            {
                Operation = operationMessage,
                RequestId = requestId,
                ExternalParameters = new Proto.ExternalParameters
                {
                    Position = new Position
                    {
                        Lat = location.Latitude,
                        Lon = location.Longitude,
                        Height = location.Altitude,
                    },
                    Quaternion = new Proto.Quaternion
                    {
                        X = XRSessionManager.GetSession().PoseProvider.GetOrientation().x,
                        Y = XRSessionManager.GetSession().PoseProvider.GetOrientation().y,
                        Z = XRSessionManager.GetSession().PoseProvider.GetOrientation().z,
                        W = XRSessionManager.GetSession().PoseProvider.GetOrientation().w
                    }
                },
                InternalParameters = new Proto.InternalParameters
                {
                    SceneHeight = (uint)XRSessionManager.GetSession().VideoProvider.GetHeight(),
                    SceneWidth = (uint)XRSessionManager.GetSession().VideoProvider.GetWidth(),
                    Fov = XRSessionManager.GetSession().VideoProvider.GetFOV(),
                    ProjectionMatrix = {
                        XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix().m00,
                        XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix().m01,
                        XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix().m02,
                        XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix().m03,
                        XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix().m10,
                        XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix().m11,
                        XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix().m12,
                        XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix().m13,
                        XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix().m20,
                        XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix().m21,
                        XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix().m22,
                        XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix().m23,
                        XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix().m30,
                        XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix().m31,
                        XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix().m32,
                        XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix().m33,
                    }
                },

                DevRadius = ScanProperties.InitialiRadius,

                // Multiframe
                FrameOrder = frameOrder,
                TotalNumOfFrames = numOfFrames,

                // Reloc
                TrackingId = trackingId,

                // Image
                SourceImage = ByteString.CopyFrom(
                    XRSessionManager.GetSession().VideoProvider.GetCurrentFrame().EncodeToJPG()),

                SiteId = scanConfig.HD.SiteId
            };


            
            SturfeeDebug.Log($" Adding SiteId {request.SiteId} to request");

            return request;
        }
    }
}
