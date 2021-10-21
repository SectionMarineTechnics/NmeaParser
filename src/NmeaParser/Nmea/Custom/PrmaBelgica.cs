//  *******************************************************************************
//  *  Licensed under the Apache License, Version 2.0 (the "License");
//  *  you may not use this file except in compliance with the License.
//  *  You may obtain a copy of the License at
//  *
//  *  http://www.apache.org/licenses/LICENSE-2.0
//  *
//  *   Unless required by applicable law or agreed to in writing, software
//  *   distributed under the License is distributed on an "AS IS" BASIS,
//  *   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//  *   See the License for the specific language governing permissions and
//  *   limitations under the License.
//  ******************************************************************************

using System;
using System.Globalization;

namespace NmeaParser.Messages
{
    /// <summary>
    /// Recommended minimum specific Loran-C Data
    /// </summary>
    /// <remarks>
    /// <para>Position, course and speed data provided by a Loran-C receiver. Time differences A and B are those used in computing latitude/longitude.
    /// This sentence is transmitted at intervals not exceeding 2-seconds and is always accompanied by <see cref="Rmb"/> when a destination waypoint is active.</para>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "PrmaBelgica")]
    [NmeaMessageType("PRMA")]
    public class PrmaBelgica : NmeaMessage
    {
        /// <summary>
        /// Alert identification
        /// </summary>
        public enum AlertId
        {
            /// <summary>
            /// Alarm message
            /// </summary>
            Alarm = 1,
            /// <summary>
            /// Alive message
            /// </summary>
            Alive = 2,
            /// <summary>
            /// Message with the SFI and ISA address in the description
            /// </summary>
            Sfi_isa = 3,
            /// <summary>
            /// Door hatch message
            /// </summary>
            DoorHatch = 4
        }

        /// <summary>
        /// Alarm state
        /// </summary>
        public enum AlarmState
        {
            /// <summary>
            /// Null state
            /// </summary>
            NL = -1,
            /// <summary>
            /// Normal (no alarm)
            /// </summary>
            OK = 0,
            /// <summary>
            /// Inhibited alarm state
            /// </summary>
            INH ,
            /// <summary>
            /// Blocked alarm state
            /// </summary>
            BLC,
            /// <summary>
            /// Transferred
            /// </summary>
            TR,
            /// <summary>
            /// Cleared, rectified
            /// </summary>
            CLR,
            /// <summary>
            /// Acknowledged alarm state
            /// </summary>
            AA,
            /// <summary>
            /// Unacknowledged alarm state
            /// </summary>
            UA
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PrmaBelgica"/> class.
        /// </summary>
        /// <param name="type">The message type</param>
        /// <param name="message">The NMEA message values.</param>
        public PrmaBelgica(string type, string[] message) : base(type, message)
        {
            if (message == null || message.Length < 2)
            {
                throw new ArgumentException("Invalid PRMA Belgica", "message");
            }

            Alert_id = (AlertId)Enum.Parse(typeof(AlertId), message[0]);

            switch (Alert_id)
            {
                case AlertId.Alarm:
                    if (message[1].Contains(":"))
                    {
                        TimeOfStateChange = new TimeSpan(0, int.Parse(message[1].Substring(0, 2)), int.Parse(message[1].Substring(3, 2)), int.Parse(message[1].Substring(6, 2)), 10 * int.Parse(message[1].Substring(9, 2)));
                    }
                    else
                    {
                        TimeOfStateChange = new TimeSpan(0, int.Parse(message[1].Substring(0, 2)), int.Parse(message[1].Substring(2, 2)), int.Parse(message[1].Substring(4, 2)), 10 * int.Parse(message[1].Substring(7, 2)));
                    }
                    State = (AlarmState)Enum.Parse(typeof(AlarmState), message[2]);
                    AlarmDescription = message[3];
                    AlarmText = message[4];
                    DoorHatchesStatus = null;
                    break;
                case AlertId.Alive:

                    AlarmDescription = message[1];

                    //put other values to 0
                    TimeOfStateChange = TimeSpan.Zero;
                    State = AlarmState.NL;
                    AlarmText = "";
                    DoorHatchesStatus = null;
                    break;
                case AlertId.Sfi_isa:
                    if (message[1].Contains(":"))
                    {
                        TimeOfStateChange = new TimeSpan(0, int.Parse(message[1].Substring(0, 2)), int.Parse(message[1].Substring(3, 2)), int.Parse(message[1].Substring(6, 2)), 10 * int.Parse(message[1].Substring(9, 2)));
                    }
                    else
                    {
                        TimeOfStateChange = new TimeSpan(0, int.Parse(message[1].Substring(0, 2)), int.Parse(message[1].Substring(2, 2)), int.Parse(message[1].Substring(4, 2)), 10 * int.Parse(message[1].Substring(7, 2)));
                    }
                    State = (AlarmState)Enum.Parse(typeof(AlarmState), message[2]);
                    //only interested in the first part of the description -> SFI address
                    AlarmDescription = (message[3].Split('_'))[0]; 
                    AlarmText = message[4];
                    DoorHatchesStatus = null;
                    break;
                case AlertId.DoorHatch:
                    int message_array_length = message.Length;
                    DoorHatchesStatus = new bool[message_array_length - 1];
                    for(int i = 1; i < message_array_length; i++)
                    {
                        if (message[i].Equals("1"))
                        {
                            DoorHatchesStatus[i - 1] = true;
                        }
                        else
                        {
                            DoorHatchesStatus[i - 1] = false;
                        }
                    }

                    TimeOfStateChange = TimeSpan.Zero;
                    State = AlarmState.NL;
                    AlarmDescription = "";
                    AlarmText = "";
                    break;
                default:
                    throw new ArgumentException("Invalid Alert Id PrmaBelgica", "message");
            }
        }

        /// <summary>
        /// Alert Id status
        /// </summary>
        public AlertId Alert_id { get; }

        /// <summary>
        /// Alarm state (INH,BLC,SET,AE,AA,CE,UE,UA)
        /// </summary>
        public AlarmState State { get; }

        /// <summary>
        /// Time of state change
        /// </summary>
        public TimeSpan TimeOfStateChange { get; }

        /// <summary>
        /// Alarm Description
        /// </summary>
        public string AlarmDescription { get; } = "";

        /// <summary>
        /// Alarm text ("Normal"/"Alarm" etc)
        /// </summary>
        public string AlarmText { get; } = "";

        /// <summary>
        /// Door hatches status (false = open, true = closed)
        /// </summary>
        public bool[]? DoorHatchesStatus { get; } = null;

    }
}
