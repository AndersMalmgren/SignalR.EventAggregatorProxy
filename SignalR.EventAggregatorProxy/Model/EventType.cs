﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace SignalR.EventAggregatorProxy.Model
{
    public class EventType
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("genericArguments")]
        public string[] genericArguments { get; set; }
    }
}
