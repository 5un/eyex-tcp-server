using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EyeXFramework;
using Tobii.EyeX.Framework;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace EyeXTcpServer
{
    class FrameData
    {

        public GazePointEventArgs Gaze;

        double leftEyeX = 0.0;
        double leftEyeY = 0.0;
        double leftEyeZ = 0.0;
        double leftEyeNormalizedX = 0.0;
        double leftEyeNormalizedY = 0.0;
        double leftEyeNormalizedZ = 0.0;
        double rightEyeX = 0.0;
        double rightEyeY = 0.0;
        double rightEyeZ = 0.0;
        double rightEyeNormalizedX = 0.0;
        double rightEyeNormalizedY = 0.0;
        double rightEyeNormalizedZ = 0.0;

        public EngineStateValue<UserPresence> userPresence;

        public FrameData()
        {
            Gaze = new GazePointEventArgs(0, 0, 0);
        }

        public void updateEyePosition(EyePositionEventArgs e)
        {
            if (e.LeftEye.IsValid)
            {
                this.leftEyeX = e.LeftEye.X;
                this.leftEyeY = e.LeftEye.Y;
                this.leftEyeZ = e.LeftEye.Z;
            }

            if (e.RightEye.IsValid)
            {
                this.rightEyeX = e.RightEye.X;
                this.rightEyeY = e.RightEye.Y;
                this.rightEyeZ = e.RightEye.Z;
            }

            if (e.LeftEyeNormalized.IsValid)
            {
                this.leftEyeNormalizedX = e.LeftEyeNormalized.X;
                this.leftEyeNormalizedY = e.LeftEyeNormalized.Y;
                this.leftEyeNormalizedZ = e.LeftEyeNormalized.Z;
            }

            if (e.RightEyeNormalized.IsValid)
            {
                this.rightEyeNormalizedX = e.RightEyeNormalized.X;
                this.rightEyeNormalizedY = e.RightEyeNormalized.Y;
                this.rightEyeNormalizedZ = e.RightEyeNormalized.Z;
            }
            
        }

        public JObject toJson()
        {
            JObject rootOject = new JObject();

            JObject gaze = new JObject();
            gaze["x"] = Gaze.X;
            gaze["y"] = Gaze.Y;
            rootOject["gaze"] = gaze;

            JObject eyePosition = new JObject();

            JObject leftEye = new JObject();
            leftEye["x"] = this.leftEyeX;
            leftEye["y"] = this.leftEyeY;
            JObject rightEye = new JObject();
            rightEye["x"] = this.rightEyeX;
            rightEye["y"] = this.rightEyeY;

            JObject leftNormalized = new JObject();
            leftNormalized["x"] = this.leftEyeNormalizedX;
            leftNormalized["y"] = this.leftEyeNormalizedY;

            JObject rightNormalized = new JObject();
            rightNormalized["x"] = this.rightEyeNormalizedX;
            rightNormalized["y"] = this.rightEyeNormalizedY;

            eyePosition["left"] = leftEye;
            eyePosition["right"] = rightEye;
            eyePosition["leftNormalized"] = leftNormalized;
            eyePosition["rightNormalized"] = rightNormalized;
            eyePosition["right"] = rightEye;
            rootOject["eyePosition"] = eyePosition;

            if (userPresence != null)
            {
                rootOject["userPresence"] = userPresence.Value == UserPresence.Present;
            }
            else
            {
                rootOject["userPresence"] = false;
            }

            return rootOject;
        }

        public string toJsonString()
        {
            return this.toJson().ToString();
        }
    }

}
