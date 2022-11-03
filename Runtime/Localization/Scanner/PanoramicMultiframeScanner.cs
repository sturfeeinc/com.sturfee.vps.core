using Google.Protobuf;
using SturfeeVPS.Core.Proto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;


namespace SturfeeVPS.Core
{
    public class PanoramicMultiframeScanner : ScannerBase
    {
        protected string serviceUrl;

        protected XRSession xrSession;
        protected bool initialized;
        
        public async override Task Initialize(ScanConfig scanConfig= null, CancellationToken cancellationToken = default)
        {
            xrSession = XRSessionManager.GetSession();
            if(xrSession == null)
            {
                throw new Exception($"{ScanType} Scanner : ERROR => cannot initialize. XRSession is NULL");
            }

            try
            {

                List<Task> providerTasks = new List<Task>() {
                    xrSession.WaitForGps(cancellationToken),
                    xrSession.WaitForPose(cancellationToken),
                    xrSession.WaitForVideo(cancellationToken)
                };
                await Task.WhenAll(providerTasks);
                SturfeeDebug.Log("All providers ready");

                if(!await xrSession.CheckCoverage())
                {
                    throw new IdException(ErrorMessages.NoCoverageArea);
                }

                List<Task> preScanTasks = new List<Task>() {
                    xrSession.GpsProvider.PrepareForScan(cancellationToken),
                    xrSession.PoseProvider.PrepareForScan(cancellationToken),
                    xrSession.VideoProvider.PrepareForScan(cancellationToken)
                };
                await Task.WhenAll(preScanTasks);
                SturfeeDebug.Log("All providers ready for scan");

                initialized = true;
            }
            catch (Exception e)
            {
                SturfeeDebug.LogError(e.Message);
                if (e is SessionException)
                {
                    throw;
                }

                if(e is IdException)
                {
                    throw;
                }

                throw new SessionException(ErrorMessages.SessionNotReady);
            }
        }
        
        public async override Task Connect(string accessToken, string language = "en-US")
        {
            if (!initialized)
            {
                throw new Exception($"{ScanType} Scanner not initialized");
            }

            SturfeeDebug.Log($" Establishing socket connection for {ScanType} scanner");
            
            await base.Connect(accessToken, language);

            localizationService = new LocalizationService(serviceUrl, accessToken, language);
        }

        public async override void StartScan(int scanId)
        {
            base.StartScan(scanId);

            await CaptureAndSendAsync(scanId);
        }

        protected virtual async Task CaptureAndSendAsync(int requestId)
        {
            float startYaw = XRSessionManager.GetSession().GetXRCameraOrientation().QuaternionToEuler(RotSeq.zyx).z;
            int frameOrder = 0;
            int numOfFrames = ScanProperties.TargetCount;
            float currentTargetYaw = 0;

            // Wait till all the frames are captured 
            while (frameOrder < numOfFrames && IsScanning)
            {
                float currentYaw = XRSessionManager.GetSession().GetXRCameraOrientation().QuaternionToEuler(RotSeq.zyx).z;
                float yawDiff = GetYawDiff(currentYaw, startYaw);
                if (currentTargetYaw - yawDiff < 3 && currentTargetYaw - yawDiff > -3)
                {
                    if (!HasRequestError())
                    {
                        Request request = CaptureRequest(
                            (uint)requestId,
                            OperationMessages.Alignment,
                            (uint)ScanProperties.TargetCount,
                            (uint)frameOrder,
                            ""
                        );

                        SendRequest(request);

                        frameOrder++;
                        currentTargetYaw = frameOrder * ScanProperties.YawAngle;
                    }
                    else
                    {
                        // Wait till device is adjusted to come out of Request Error
                        await Task.Yield();
                    }
                }

                await Task.Yield();
            }

            // Loading is skipped if server responds before all requests are sent
            if (frameOrder >= numOfFrames)
            {
                SturfeeEventManager.Instance.LocalizationLoading();
            }
        }
        
        protected virtual Request CaptureRequest(uint requestId, OperationMessages operationMessage, uint numOfFrames, uint frameOrder, string trackingId)
        {
            GeoLocation location = XRSessionManager.GetSession().GetXRCameraLocation();

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
            };

            return request;
        }
        
        protected virtual void SendRequest(Request request)
        {
            byte[] buffer = request.ToByteArray();
            localizationService.Align(buffer,
                (response) =>
                {
                    ResponseMessage responseMessage = ResponseMessage.Parser.ParseFrom(response);

                    JsonFormatter jsonFormatter = new JsonFormatter(JsonFormatter.Settings.Default);
                    SturfeeDebug.Log(jsonFormatter.Format(responseMessage), false);

                    InvokeOnLocalizationResponse(responseMessage, OffsetType);

                    if (responseMessage.RequestId != request.RequestId)
                    {
                        SturfeeDebug.LogWarning("Request Response Ids mismatch");
                        SturfeeDebug.LogWarning($"Request Id : {request.RequestId}   Response Id : {responseMessage.RequestId}");
                    }
                },
                (error) =>
                {
                    SturfeeEventManager.Instance.LocalizationFail(ErrorMessages.ServerError);
                }
            );

            LocalizationRequest localizationRequest = RequestToLocalizationRequest(request);

            SturfeeEventManager.Instance.FrameCaptured((int)request.FrameOrder + 1, localizationRequest, request.SourceImage.ToByteArray());

            SturfeeDebug.Log(JsonUtility.ToJson(localizationRequest), false);
        }
        
        private float GetYawDiff(float yaw1, float yaw2)
        {
            float yawDiff = (yaw1 - yaw2);
            float absYawDiff = Mathf.Abs(yawDiff);

            if (absYawDiff > 180)
            {
                yawDiff = yawDiff > 0 ? -(360 - absYawDiff) : 360 - absYawDiff;
            }

            //If our capture range goes above 180
            float captureRange = (ScanProperties.TargetCount - 1) * ScanProperties.YawAngle;
            if (yawDiff > 0 && captureRange > 180)
            {
                if (yawDiff < 180 && yawDiff >= 360 - captureRange - 5)    // + 5 is added for sanity just in case we want cursorPos beyond last gaze target
                {
                    yawDiff -= 360;
                }
            }

            return -yawDiff;    // clockwise is -ve
        }
        
        private bool HasRequestError()
        {
            var request = XRSessionManager.GetSession().GetXRCameraOrientation().QuaternionToEuler(RotSeq.zyx);

            float pitch = request.x;
            float roll = request.y;

            if (pitch > 180)
            {
                pitch -= 360;
            }

            pitch = 90 - pitch;

            if (pitch < ScanProperties.PitchMin || pitch > ScanProperties.PitchMax)
            {
                return true;
            }

            if (roll > 180)
            {
                roll -= 360;
            }

            roll = -roll;
            if (roll < ScanProperties.RollMin || roll > ScanProperties.RollMax)
            {
                return true;
            }

            return false;
        }

        private LocalizationRequest RequestToLocalizationRequest(Request request)
        {
            LocalizationRequest localizationRequest = new LocalizationRequest
            {
                requestId = (int)request.RequestId,
                frame = new Multiframe
                {
                    order = (int)request.FrameOrder,
                    count = (int)request.TotalNumOfFrames
                },
                externalParameters = new ExternalParameters
                {
                    latitude = request.ExternalParameters.Position.Lat,
                    longitude = request.ExternalParameters.Position.Lon,
                    height = request.ExternalParameters.Position.Height,
                    quaternion = new Quaternion(
                        (float)request.ExternalParameters.Quaternion.X,
                        (float)request.ExternalParameters.Quaternion.Y,
                        (float)request.ExternalParameters.Quaternion.Z,
                        (float)request.ExternalParameters.Quaternion.W
                    )
                },
                internalParameters = new InternalParameters
                {
                    fov = request.InternalParameters.Fov,
                    sceneWidth = (int)request.InternalParameters.SceneWidth,
                    sceneHeight = (int)request.InternalParameters.SceneHeight,
                    isPortrait = XRSessionManager.GetSession().VideoProvider.IsPortrait() ? 1 : 0,
                    projectionMatrix = XRSessionManager.GetSession().VideoProvider.GetProjectionMatrix()
                },
                siteId = request.SiteId
            };

            return localizationRequest;
        }
    }
}
