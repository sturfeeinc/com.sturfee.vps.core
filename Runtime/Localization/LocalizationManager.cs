using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using SturfeeVPS.Core.Proto;
using UnityEngine;

using Quaternion = UnityEngine.Quaternion;

namespace SturfeeVPS.Core
{
    internal partial class LocalizationManager
    {
        public List<IScanner> Scans;   // number of scans in this session

        // Request
        private int _requestNum = 0;
        private int _requestId = 0;
        private Vector3 _positionOnScanStart;

        // Response
        private string _trackingId;

        private IScanner _scanner;
        private OffsetType _offsetType = OffsetType.Quaternion;
        private bool _accumulateOffsets = false;

        public LocalizationManager(bool accumulateOffsets = false)
        {
            YawOrientationCorrection = Quaternion.identity;
            PitchOrientationCorrection = Quaternion.identity;
            EulerOrientationCorrection = Vector3.zero;

            Scans = new List<IScanner>();
            _accumulateOffsets = accumulateOffsets;
        }

        public async Task InitializeScan(IScanner scanner, ScanConfig scanConfig, string accessToken, string language = "en-US", CancellationToken cancellationToken = default)
        {
            if(_scanner != null)
            {
                SturfeeDebug.Log($" Previous scanner ({_scanner.ScanType}) still active. ");
                _scanner.Disconnect();
                _scanner = null;
            }

            _scanner = scanner;

            try
            {

                _scanner.OnFrameCaptured += (frameNum, request, frame) =>
                {
                    SturfeeDebug.Log($"{frameNum} captured");
                    SturfeeEventManager.Instance.FrameCaptured(frameNum, request, frame);
                };

                _scanner.OnLocalizationLoading += () =>
                {
                    SturfeeDebug.Log($"Loading...");
                    SturfeeEventManager.Instance.LocalizationLoading();
                };

                await _scanner.Initialize(scanConfig, cancellationToken);  
                await _scanner.Connect(accessToken, language);

                _scanner.OnLocalizationResponse += OnLocalizationResonse;
            }
            catch (Exception ex)
            {
                SturfeeDebug.LogError(ex.Message);
                _scanner = null;
                throw;
            }
        }

        public void StartScan(LocalizationMode localizationMode = LocalizationMode.WebServer)
        {
            if (localizationMode == LocalizationMode.Local)
            {
                AlignLocally();
                return;
            }

            if (_scanner == null)
            {
                Debug.LogError($"LocalizationManager :: Cannot perform localization. Scanner is NULL !");
                return;
            }

            _requestNum++;
            _requestId = (_requestNum * 10) + (int)OperationMessages.Alignment;
            _positionOnScanStart = XRSessionManager.GetSession().PoseProvider.GetPosition();

            _scanner.StartScan(_requestId);

            Scans.Add(_scanner);
        }

        public void StopScan()
        {
            if (_scanner == null)
            {
                SturfeeDebug.LogWarning($"Cannot stop scan. No scanner running");
                return;
            }

            if(_scanner.IsScanning)
            {
                SturfeeDebug.Log($" Stopping {_scanner.ScanType} scan but keeping socket connection alive");
                _scanner.StopScan();
            }
            else
            {
                SturfeeDebug.Log($" Disconnecting {_scanner.ScanType} scanner's socket connection");
                _scanner.Disconnect();                
                _scanner = null;
            }
        }

        public void DisableVPS()
        {
            _scanner?.Disconnect();
            _scanner = null;
            
            ResetOffsets();
        }

        private void OnLocalizationResonse(ResponseMessage responseMessage, OffsetType _offsetType)
        {            
            if (responseMessage.Error != null)
            {
                SturfeeDebug.LogError("Server Error : " + responseMessage.Error);
                
                ErrorMessages.ServerError = (responseMessage.Error.Code.ToString(), responseMessage.Error.Message);

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

            var pitch = new Quaternion(
                (float)responseMessage.Response.PitchOffsetQuaternion.X,
                (float)responseMessage.Response.PitchOffsetQuaternion.Y,
                (float)responseMessage.Response.PitchOffsetQuaternion.Z,
                (float)responseMessage.Response.PitchOffsetQuaternion.W
            );

            Vector3 eulerOffset = Vector3.zero;
            if (responseMessage.Response.EulerOffset != null)
            {
                eulerOffset = new Vector3
                {
                    x = (float)responseMessage.Response.EulerOffset.PitchOffset,
                    y = (float)responseMessage.Response.EulerOffset.RollOffset,
                    z = (float)responseMessage.Response.EulerOffset.YawOffset
                };

                EulerOrientationCorrection = eulerOffset;
                SturfeeDebug.Log($" Euler offset : {EulerOrientationCorrection}");
            }


            LocationCorrection = new GeoLocation
            {
                Latitude = responseMessage.Response.Position.Lat,
                Longitude = responseMessage.Response.Position.Lon,
                Altitude = responseMessage.Response.Position.Height
            };

            if (_accumulateOffsets)
            {
                YawOrientationCorrection = yaw * YawOrientationCorrection;
                PitchOrientationCorrection *= PitchOrientationCorrection;
                EulerOrientationCorrection += eulerOffset;
            }
            else
            {
                YawOrientationCorrection = yaw;
                PitchOrientationCorrection = pitch;
                EulerOrientationCorrection = eulerOffset;
            }

            _offsetType = _scanner.OffsetType;
            SturfeeDebug.Log($"Localization completed using {_scanner.ScanType} Scanner. Applying {_scanner.OffsetType} offsets");

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
            EulerOrientationCorrection = Vector3.zero;
        }
    }
}