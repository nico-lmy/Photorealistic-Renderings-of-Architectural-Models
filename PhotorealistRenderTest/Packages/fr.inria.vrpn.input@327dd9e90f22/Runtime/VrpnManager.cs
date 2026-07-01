using System.Runtime.InteropServices;

using UnityEngine;

namespace Vrpn.Input
{
    /// <summary>
    /// This class is the interface to the native VRPN library. It can handle vrpn update in the editor
    /// </summary>
    public static class VrpnManager
    {
        #region Methods

        /// <summary>
        /// Internal frame count when using the editor. Updating the frame count triggers a new update of the VRPN device before getting the values
        /// </summary>
        static int internalFrameCount;

        public static double Analog(string address, int channel)
        {
            if (Application.isPlaying)
                return vrpnAnalogExtern(address, channel, Time.frameCount);
            else
                return vrpnAnalogExtern(address, channel, internalFrameCount++);
        }

        public static bool Button(string address, int channel)
        {
            if (Application.isPlaying)
                return vrpnButtonExtern(address, channel, Time.frameCount);
            else
                return vrpnButtonExtern(address, channel, internalFrameCount++);
        }

        public static Vector3 TrackerPos(string address, int channel)
        {
            if (Application.isPlaying)
            {
                return new Vector3(
                (float)vrpnTrackerExtern(address, channel, 0, Time.frameCount),
                (float)vrpnTrackerExtern(address, channel, 1, Time.frameCount),
                (float)vrpnTrackerExtern(address, channel, 2, Time.frameCount));
            }
            else
            {
                return new Vector3(
                (float)vrpnTrackerExtern(address, channel, 0, internalFrameCount),
                (float)vrpnTrackerExtern(address, channel, 1, internalFrameCount),
                (float)vrpnTrackerExtern(address, channel, 2, internalFrameCount++));
            }
        }

        public static Quaternion TrackerQuat(string address, int channel)
        {
            if (Application.isPlaying)
            {
                return new Quaternion(
                (float)vrpnTrackerExtern(address, channel, 3, Time.frameCount),
                (float)vrpnTrackerExtern(address, channel, 4, Time.frameCount),
                (float)vrpnTrackerExtern(address, channel, 5, Time.frameCount),
                (float)vrpnTrackerExtern(address, channel, 6, Time.frameCount));
            }
            else
            {
                return new Quaternion(
                (float)vrpnTrackerExtern(address, channel, 3, internalFrameCount),
                (float)vrpnTrackerExtern(address, channel, 4, internalFrameCount),
                (float)vrpnTrackerExtern(address, channel, 5, internalFrameCount),
                (float)vrpnTrackerExtern(address, channel, 6, internalFrameCount++));
            }
        }

        public static (Vector3, Quaternion) TrackerPosQuat(string address, int channel)
        {
            if (Application.isPlaying)
            {
                return (new Vector3(
                (float)vrpnTrackerExtern(address, channel, 0, Time.frameCount),
                (float)vrpnTrackerExtern(address, channel, 1, Time.frameCount),
                (float)vrpnTrackerExtern(address, channel, 2, Time.frameCount)),
                new Quaternion(
                (float)vrpnTrackerExtern(address, channel, 3, Time.frameCount),
                (float)vrpnTrackerExtern(address, channel, 4, Time.frameCount),
                (float)vrpnTrackerExtern(address, channel, 5, Time.frameCount),
                (float)vrpnTrackerExtern(address, channel, 6, Time.frameCount)));
            }
            else
            {
                return (new Vector3(
                (float)vrpnTrackerExtern(address, channel, 0, internalFrameCount),
                (float)vrpnTrackerExtern(address, channel, 1, internalFrameCount),
                (float)vrpnTrackerExtern(address, channel, 2, internalFrameCount)),
                 new Quaternion(
                (float)vrpnTrackerExtern(address, channel, 3, internalFrameCount),
                (float)vrpnTrackerExtern(address, channel, 4, internalFrameCount),
                (float)vrpnTrackerExtern(address, channel, 5, internalFrameCount),
                (float)vrpnTrackerExtern(address, channel, 6, internalFrameCount++)));
            }
        }

        [DllImport("unityVrpn")]
        private static extern double vrpnAnalogExtern(string address, int channel, int frameCount);

        [DllImport("unityVrpn")]
        private static extern bool vrpnButtonExtern(string address, int channel, int frameCount);

        [DllImport("unityVrpn")]
        private static extern double vrpnTrackerExtern(string address, int channel, int component, int frameCount);

        #endregion
    }
}