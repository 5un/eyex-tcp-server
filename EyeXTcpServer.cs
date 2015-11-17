//-----------------------------------------------------------------------
// Copyright 2015 Soravis Prakkamakul
//-----------------------------------------------------------------------

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EyeXFramework;
using Tobii.EyeX.Framework;
using Tobii.EyeX.Client;
using System.Collections.Generic;

namespace EyeXTcpServer
{
    
    public static class EyeXTcpServer
    {

        private static JsonTcpServer tcpServer;
        private static FrameData frameData;

        public static void StartUpdatingToClients()
        {
            while (true)
            {
                EyeXAPIEvent message = new EyeXAPIEvent();
                message.eventType = "frame";
                message.data = frameData.toJson();
                tcpServer.sendToAllClients(message.toJson());
                Thread.Sleep(60);
            }
        }

        public static void Main(string[] args)
        {
            tcpServer = new JsonTcpServer(6555);
            frameData = new FrameData();

            switch (EyeXHost.EyeXAvailability)
            {
                case EyeXAvailability.NotAvailable:
                    Console.WriteLine("This server requires the EyeX Engine, but it isn't available.");
                    Console.WriteLine("Please install the EyeX Engine and try again.");
                    return;

                case EyeXAvailability.NotRunning:
                    Console.WriteLine("This server requires the EyeX Engine, but it isn't running.");
                    Console.WriteLine("Please make sure that the EyeX Engine is started.");
                    break;
            }

            Thread clientUpdateThread = new Thread(new ThreadStart(StartUpdatingToClients));

            using (var eyeXHost = new EyeXHost())
            {
                
                eyeXHost.Start();
                Console.WriteLine("SERVER: eyeXHost started");

                // Create a data stream: lightly filtered gaze point data.
                // Other choices of data streams include EyePositionDataStream and FixationDataStream.

                eyeXHost.ScreenBoundsChanged += (s, e) =>
                {
                    Console.WriteLine("[EVENT] Screen Bounds in pixels (state-changed event): {0}", e);
                };

                eyeXHost.DisplaySizeChanged += (s, e) => {
                    Console.WriteLine("[EVENT] Display Size in millimeters (state-changed event): {0}", e);
                };

                eyeXHost.EyeTrackingDeviceStatusChanged += (s, e) => {
                    Console.WriteLine("[EVENT] Eye tracking device status (state-changed event): {0}", e);
                    EyeXAPIEvent message = new EyeXAPIEvent();
                    message.eventType = "device_state_changed";
                    tcpServer.sendToAllClients(message.toJson());
                    Thread.Sleep(60);
                };

                eyeXHost.UserPresenceChanged += (s, e) => {
                    Console.WriteLine("[EVENT] User presence (state-changed event): {0}", e);
                    //TODO save it to send in frame
                };

                eyeXHost.UserProfileNameChanged += (s, e) =>
                {
                    Console.WriteLine("[EVENT] Active profile name (state-changed event): {0}", e);
                };
                
                // This state-changed event required EyeX Engine 1.4.
                eyeXHost.UserProfilesChanged += (s, e) =>
                {
                    Console.WriteLine("[EVENT] User profile names (state-changed event): {0}", e);
                };

                eyeXHost.GazeTrackingChanged += (s, e) =>
                {
                    Console.WriteLine("[EVENT] Gaze tracking (state-changed event): {0}", e);
                    //TODO save it to send in frame
                };

                using (var gazeDataStream = eyeXHost.CreateGazePointDataStream(GazePointDataMode.LightlyFiltered))
                {

                    Console.WriteLine("[EYEX]: GazeDataStream started");
                    using (var eyePositionStream = eyeXHost.CreateEyePositionDataStream())
                    {

                        Console.WriteLine("[EYEX]: EyePositionStream started");

                        // Write the data to the console.
                        gazeDataStream.Next += (s, e) => {

                            //Console.WriteLine("Gaze point at ({0:0.0}, {1:0.0}) @{2:0}", e.X, e.Y, e.Timestamp);

                            frameData.Gaze = e;
                            frameData.userPresence = eyeXHost.UserPresence;

                        };
                    
                        eyePositionStream.Next += (s, e) =>
                        {
                            //Console.WriteLine("3D Position: ({0:0.0}, {1:0.0}, {2:0.0})                   ",
                            //   e.LeftEye.X, e.LeftEye.Y, e.LeftEye.Z);

                            frameData.updateEyePosition(e);
                            frameData.userPresence = eyeXHost.UserPresence;
                            
                        };

                        tcpServer.ClientMessageReceieved += (TcpClient client,JObject json) =>
                        {
                            if (json["type"].ToString() == "request")
                            {
                                int requestId = (int)json["requestId"];
                                if (json["resource"].ToString() == "calibration" && json["path"].ToString() == "start")
                                {
                                    Console.WriteLine("[Client] Calibration requested");
                                    EyeXAPIResponse response = new EyeXAPIResponse();
                                    response.statusCode = 200;
                                    response.requestId = requestId;
                                    tcpServer.sendToClient(client, response.toJson());
                                    eyeXHost.LaunchGuestCalibration();

                                }

                                if (json["resource"].ToString() == "tracker")
                                {

                                    if (json["path"].ToString() == "get.basic_info")
                                    {

                                        EyeXAPIResponse response = new EyeXAPIResponse();
                                        response.statusCode = 200;
                                        response.requestId = requestId;
                                        Dictionary<string, object> result = new Dictionary<string, object>();
                                        result.Add("screen_bounds", eyeXHost.ScreenBounds.Value);
                                        result.Add("display_size", eyeXHost.DisplaySize.Value);
                                        response.results = result;
                                        tcpServer.sendToClient(client, response.toJson());

                                    }
                                    else
                                    {
                                        //TODO return api error: unknown method
                                    }
                                }
                            }
                            else if (json["type"].ToString() == "event")
                            {
                                // Client side events is not supported yet
                            }
                        };

                        clientUpdateThread.Start();

                        Console.WriteLine("Listening for gaze data, press any key to exit...");
                        Console.In.Read();

                    } // using EyePositionDataStream
                } // using GazeDataStream
                
                
            } // using eyeXHost
   
        }
    }
}
