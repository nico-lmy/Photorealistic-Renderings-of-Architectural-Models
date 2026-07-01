using System;

using UnityEngine;

namespace Vrpn.Input
{
    /// <summary>
    /// Structure defining a Vrpn Input
    /// </summary>
    [Serializable]
    public class VrpnInput
    {
        #region Fields

        public string InputName;
        public string VrpnDeviceName;
        public string VrpnServerUrl;
        public int Index;
        public ClusterInputType InputType;

        #endregion
    }
}