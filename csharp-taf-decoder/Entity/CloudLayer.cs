﻿using System.ComponentModel;

namespace csharp_taf_decoder.entity
{
    public sealed class CloudLayer : AbstractEntity
    {
        public enum CloudAmount
        {
            NULL,
            [Description("Cumulonimbus")]
            FEW,
            [Description("Scattered")]
            SCT,
            [Description("Broken")]
            BKN,
            [Description("Overcast")]
            OVC,
            [Description("Vertical Visibility")]
            VV,
        }

        public enum CloudType
        {
            NULL,
            [Description("Cumulonimbus")]
            CB,
            [Description("Towering cumulonimbus")]
            TCU,
            [Description("Cannot measure")]
            CannotMeasure,
        }

        /// <summary>
        /// Annotation corresponding to amount of clouds (FEW/SCT/BKN/OVC)
        /// </summary>
        public CloudAmount Amount { get; set; } = CloudAmount.NULL;

        /// <summary>
        /// Height of cloud base
        /// </summary>
        public Value BaseHeight { get; set; }

        /// <summary>
        /// Cloud type cumulonimbus, towering cumulonimbus (CB/TCU)
        /// </summary>
        public CloudType Type { get; set; }
    }
}