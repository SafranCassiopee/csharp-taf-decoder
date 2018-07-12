﻿using System;
using System.Collections.Generic;

namespace csharp_taf_decoder.entity
{
    public class Evolution : AbstractEntity, ICloneable
    {
        /// <summary>
        /// annotation corresponding to the type of evolution (FM, BECMG or TEMPO)
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// day when the evolution occurs (FM) or starts (BECMG/TEMPO)
        /// </summary>
        public int FromDay { get; set; }
        /// <summary>
        /// hour and minute UTC (as string) when the evolution occurs (FM)
        /// or hour UTC (as string) when the evolution starts (BECMG/TEMPO)
        /// </summary>
        public string FromTime { get; set; }
        /// <summary>
        /// day when the evolution ends (BECMG/tEMPO)
        /// </summary>
        public int ToDay { get; set; }
        /// <summary>
        /// hour UTC (as string) when the evolution ends (BECMG/TEMPO)
        /// </summary>
        public string ToTime { get; set; }

        public object Entity { get; set; }

        /// <summary>
        /// weather entity (i.e. SurfaceWind, Temperature, Visibility, etc.)
        /// public Entity entity { get; set; }
        /// </summary>
        public bool Cavok { get; set; }
        /// <summary>
        /// optional annotation corresponding to the probability (PROBnn)
        /// </summary>
        public string Probability { get; set; }


        public object Clone()
        {
            return Cloner.Clone(this);
        }
    }
}