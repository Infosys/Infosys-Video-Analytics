/*=============================================================================================================== *
 * Copyright 2025 Infosys Ltd.                                                                                    *
 * Use of this source code is governed by Apache License Version 2.0 that can be found in the LICENSE file or at  *
 * http://www.apache.org/licenses/                                                                                *
 * ===============================================================================================================*/
﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infosys.Solutions.Ainauto.VideoAnalytics.BusinessEntity
{
    public class FrameDetails
    {
        public string DeviceId { get; set; }
        public byte[] Frame { get; set; }
        public List<ObjectDetails> Objects { get; set; }
        public string TenantId { get; set; }
        public string FrameId { get; set; }

        public Boolean IsFirstFrame { get; set; }

        public string Ad { get; set; }
    }




    public class ObjectDetails
    {
        public string Id { get; set; }
        public string Class { get; set; }
        public double ConfidenceScore { get; set; }



    }
    public class MaskDetails
    {
        public string tenantId { get; set; }
        public string deviceId { get; set; }
        public string frameid { get; set; }
        public long timestamp { get; set; }
        public int maskCount { get; set; }
        public int noMaskCount { get; set; }

    }

    public class TransportFrameDetails
    {
        public byte[] Data { get; set; }

        public int TenantId { get; set; }
        public string DeviceId { get; set; }

        public string IpAddress { get; set; }

        public int Port { get; set; }

        public bool IsFirstFrame { get; set; }
        public string FfmpegArguments { get; set; }
        public string Fid { get; set; }
        public int VideoStreamingOption { get; set; }
        public string SequenceNumber { get; set; }
        public string FeedId { get; set; } // Feed Id
        public string FrameNumber { get; set; } //Current Frame Number
    }

    public class TransportSequenceDetails
    {
        public int PreviousSeqNumber { get; set; }
        public int LastFrameNumberSendForPredict { get; set; }
        public int TotalFrameCount { get; set; }
        public int TotalFrameprocessed { get; set; }
        public int CurrentMaxSeqNumber { get; set; }
        public bool IsNextSeqBeyondLastFrame { get; set; }
        public int FrameToPredict { get; set; }
        public int MaxSequenceNumber { get; set; }
        public int FrameSequencingMessageStuckDuration { get; set; }
        public int FrameSequencingMessageRetry { get; set; }

        public List<int> SkippedSequenceNumbers { get; set; }
    }



}
