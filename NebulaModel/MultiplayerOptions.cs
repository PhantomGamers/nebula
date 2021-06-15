﻿using NebulaModel.Attributes;
using System;
using System.ComponentModel;

namespace NebulaModel
{
    [System.Serializable]
    public class MultiplayerOptions : ICloneable
    {
        [DisplayName("Host Port")]
        [UIRange(1, ushort.MaxValue)]
        public ushort HostPort { get; set; } = 8469;

        [DisplayName("Disable Tutorials")]
        public bool TutorialDisabled { get; set; } = true;

        [DisplayName("Disable Advisors")]
        public bool AdvisorDisabled { get; set; } = true;

        [DisplayName("UPNP")]
        public bool UPNP {  get; set; } = true;

        public object Clone()
        {
            return this.MemberwiseClone();
        }
    }
}
