using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf;
using SturfeeVPS.Core.Proto;
using UnityEngine;

using Quaternion = UnityEngine.Quaternion;

namespace SturfeeVPS.Core
{
    internal partial class LocalizationManager
    {
        public Quaternion YawOrientationCorrection { get; private set; }
        public Quaternion PitchOrientationCorrection { get; private set; }
        public GeoLocation LocationCorrection { get; private set; }

        public List<Scanner> Scans;   // number of scans in this session

        private WebSocketService _webSocketService;

        // Request
        private int _requestNum = 0;
        private int _requestId = 0;
        private Vector3 _trackingOrigin;

        // Response
        private string _trackingId;

        private Scanner _currentScanner;
        
        private Coroutine _scanRoutine;

        public LocalizationManager()
        {
            YawOrientationCorrection = Quaternion.identity;
            PitchOrientationCorrection = Quaternion.identity;

            Scans = new List<Scanner>();
        }

        public async Task PrepareForScan(WebSocketService webSocketService, GeoLocation location)
        {

            if(_webSocketService != null)
            {
                if (_webSocketService.IsConnected)
                {
                    SturfeeDebug.Log($"Previous Socket connection was not closed correctly. Closing it now..");
                    _webSocketService.Close();
                }

                _webSocketService = null;
            }

            _webSocketService = webSocketService;

            try
            {
                await _webSocketService.Connect(location.Latitude, location.Longitude);
            }
            catch (Exception ex)
            {
                SturfeeDebug.LogError(ex.Message);
                throw new SessionException(ErrorMessages.SocketConnectionFail);
            }
        }

        public void PerformLocalization(ScanType scanType, LocalizationMode localizationMode = LocalizationMode.WebServer)
        {
            if(localizationMode == LocalizationMode.Local)
            {
                AlignLocally();
                return;                   
            }

            XRSession xRSession = XRSessionManager.GetSession();
            if(xRSession.Status < XRSessionStatus.Ready)
            {
                SturfeeEventManager.Instance.LocalizationFail(ErrorMessages.SessionNotReady);
                return;
            }

            if(_scanRoutine != null)
            {
                StopScan();
            }

            _scanRoutine = xRSession.StartCoroutine(PerformLocalizationAsync(scanType).AsCoroutine());            
        }

        public void CancelScan() 
        {
            _webSocketService?.Close();
            _webSocketService = null;
        }
        
        public void StopScan()
        {
            if (_scanRoutine != null)
            {
                XRSessionManager.GetSession().StopCoroutine(_scanRoutine);
                _scanRoutine = null;
            }

        }

        public void DisableVPS()
        {
            CancelScan();
            ResetOffsets();
        }

        private async Task PerformLocalizationAsync(ScanType scanType)
        {
            _requestNum++;
            _trackingOrigin = XRSessionManager.GetSession().PoseProvider.GetPosition();

            if (scanType == ScanType.Satellite)
            {
                _requestId = (_requestNum * 10) + (int)OperationMessages.Alignment;
                _currentScanner = new SatelliteScanner(_requestId, _webSocketService, OnLocalizationResonse);
            }
            else
            {
                SturfeeDebug.LogError("Scan Error" + scanType.ToString() + " scanner not yet available");
                SturfeeEventManager.Instance.LocalizationFail(ErrorMessages.ServerError);                
                return;
            }

            Scans.Add(_currentScanner);

            try
            {
                await _currentScanner.StartScan();
            }
            catch (IdException e)
            {
                SturfeeEventManager.Instance.LocalizationFail(e.IdError);
            }
            
        }

        private void OnLocalizationResonse(ResponseMessage responseMessage)
        {            
            _webSocketService.Close();
            _webSocketService = null;


            // TODO: check if response is error response

            if (responseMessage.Error != null)
            {
                SturfeeDebug.LogError("Server Error : " + responseMessage.Error);
                
                if(responseMessage.Error.Code == Error.Types.ErrorCodes.OutOfCoverageError)
                {
                    ErrorMessages.ServerError = (responseMessage.Error.Code.ToString(), responseMessage.Error.Message);
                }

                SturfeeEventManager.Instance.LocalizationFail(ErrorMessages.ServerError);                 
                return;
            }


            _trackingId = responseMessage.TrackingId;

            var yaw = new Quaternion(
                (float)responseMessage.Response.YawOffsetQuaternion.X,
                (float)responseMessage.Response.YawOffsetQuaternion.Y,
                (float)responseMessage.Response.YawOffsetQuaternion.Z,
                (float)responseMessage.Response.YawOffsetQuaternion.W
            );

            YawOrientationCorrection = yaw * YawOrientationCorrection;

            var pitch = new Quaternion(
                (float)responseMessage.Response.PitchOffsetQuaternion.X,
                (float)responseMessage.Response.PitchOffsetQuaternion.Y,
                (float)responseMessage.Response.PitchOffsetQuaternion.Z,
                (float)responseMessage.Response.PitchOffsetQuaternion.W
            );

            PitchOrientationCorrection *= pitch;

            LocationCorrection = new GeoLocation
            {
                Latitude = responseMessage.Response.Position.Lat,
                Longitude = responseMessage.Response.Position.Lon,
                Altitude = responseMessage.Response.Position.Height
            };

            _currentScanner.Response = new LocalizationResponse
            {
                location = LocationCorrection,
                yawOrientationCorrection = yaw,
                pitchOrientationCorrection = pitch
            };


            JsonFormatter jsonFormatter = new JsonFormatter(JsonFormatter.Settings.Default);
            SturfeeDebug.Log(jsonFormatter.Format(responseMessage));


            SturfeeEventManager.Instance.LocalizationSuccessful();
        }

        private void AlignLocally()
        {
            string path = Application.persistentDataPath + "/VPSPose.txt";

            string json = File.ReadAllText(path);
            LocalizationResponse response = JsonUtility.FromJson<LocalizationResponse>(json);

            var yaw = response.yawOrientationCorrection;
            YawOrientationCorrection = yaw * YawOrientationCorrection;

            var pitch = response.pitchOrientationCorrection;
            PitchOrientationCorrection *= pitch;

            LocationCorrection = response.location;
            Debug.Log(LocationCorrection.ToFormattedString());
            SturfeeEventManager.Instance.LocalizationSuccessful();

        }

        private void ResetOffsets()
        {
            YawOrientationCorrection = Quaternion.identity;
            PitchOrientationCorrection = Quaternion.identity;
        }
    }
}