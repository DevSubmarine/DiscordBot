﻿using System.Runtime.Serialization;
using Newtonsoft.Json.Converters;

namespace DevSubmarine.DiscordBot.PasteMyst
{
    /// <summary>Possible expiration values for PasteMyst.</summary>
    /// <seealso href="https://paste.myst.rs/api-docs/paste"/>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum PasteExpiration
    {
        [EnumMember(Value = "never")]
        Never,
        [EnumMember(Value = "1h")]
        OneHour,
        [EnumMember(Value = "2h")]
        TwoHours,
        [EnumMember(Value = "10h")]
        TenHours,
        [EnumMember(Value = "1d")]
        OneDay,
        [EnumMember(Value = "2d")]
        TwoDays,
        [EnumMember(Value = "1w")]
        OneWeek,
        [EnumMember(Value = "1m")]
        OneMonth,
        [EnumMember(Value = "1y")]
        OneYear
    }
}
