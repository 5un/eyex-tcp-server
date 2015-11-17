//-----------------------------------------------------------------------
// Copyright 2014 Tobii Technology AB. All rights reserved.
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
                tcpServer.sendToAllClients(frameData.toJson());
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
                    Console.WriteLine("This sample requires the EyeX Engine, but it isn't available.");
                    Console.WriteLine("Please install the EyeX Engine and try again.");
                    return;

                case EyeXAvailability.NotRunning:
                    Console.WriteLine("This sample requires the EyeX Engine, but it isn't running.");
                    Console.WriteLine("Please make sure that the EyeX Engine is started.");
                    break;
            }

            Thread clientUpdateThread = new Thread(new ThreadStart(StartUpdatingToClients));

            using (var eyeXHost = new EyeXHost())
            {
                
                eyeXHost.Start();
                //Console.WriteLine("Screen Bounds in pixels (initial value): {0}", eyeXHost.ScreenBounds);
                //Console.WriteLine("Display Size in millimeters (initial value): {0}", eyeXHost.DisplaySize);
                //Console.WriteLine("Eye tracking device status (initial value): {0}", eyeXHost.EyeTrackingDeviceStatus);
                //Console.WriteLine("User presence (initial value): {0}", eyeXHost.UserPresence);
                //Console.WriteLine("User profile name (initial value): {0}", eyeXHost.UserProfileName);
                Console.WriteLine("SERVER: eyeXHost started");

                
                // Create a data stream: lightly filtered gaze point data.
                // Other choices of data streams include EyePositionDataStream and FixationDataStream.
                
                using (var gazeDataStream = eyeXHost.CreateGazePointDataStream(GazePointDataMode.LightlyFiltered))
                {

                    Console.WriteLine("SERVER: GazeDataStream started");
                    using (var eyePositionStream = eyeXHost.CreateEyePositionDataStream())
                    {

                        Console.WriteLine("SERVER: EyePositionStream started");

                        // Write the data to the console.
                        gazeDataStream.Next += (s, e) => {

                            //Console.WriteLine("Gaze point at ({0:0.0}, {1:0.0}) @{2:0}", e.X, e.Y, e.Timestamp);

                            frameData.Gaze = e;
                            frameData.userPresence = eyeXHost.UserPresence;

                            //Console.WriteLine("User presence (initial value): {0}", eyeXHost.UserPresence.Value == UserPresence.Present);
                           
                            // If socket is open, send to all sockets

                        };
                    
                        eyePositionStream.Next += (s, e) =>
                        {
                            //Console.WriteLine("3D Position: ({0:0.0}, {1:0.0}, {2:0.0})                   ",
                            //   e.LeftEye.X, e.LeftEye.Y, e.LeftEye.Z);

                            frameData.updateEyePosition(e);
                            frameData.userPresence = eyeXHost.UserPresence;

                            //Console.WriteLine("User presence (initial value): {0}", eyeXHost.UserPresence.Value == UserPresence.Present);

                            //updateAllClients();
                            
                        };

                        tcpServer.ClientMessageReceieved += (s, json) =>
                        {
                            if (json["category"].ToString() == "calibration" && json["request"].ToString() == "start")
                            {
                                Console.WriteLine("calibration requested");
                                eyeXHost.LaunchGuestCalibration();
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
