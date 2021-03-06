﻿namespace VanillaRat.Classes
{
    internal class Settings
    {
        public struct Values
        {
            //Get port from user settings
            public int GetPort()
            {
                return Client.Properties.Settings.Default.Port;
            }

            //Get update interval from settings
            public int GetUpdateInterval()
            {
                return Client.Properties.Settings.Default.UpdateInterval;
            }

            //Get notify on connection from settings
            public bool GetNotifyValue()
            {
                return Client.Properties.Settings.Default.Notfiy;
            }
        }
    }
}