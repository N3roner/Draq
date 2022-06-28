namespace Product
{
   class GameConfig : Config
   {
      public GameConfig() {
         OnLoadDev = DevConfig;
         OnLoadStaging = DevConfig;

         OnLoadProduction = () => {
            UnityEngine.Debug.LogError("No production config set");
         };
      }

      static void DevConfig() {
         CurrentConfig.SocketIOData.Connection.Url = "https://dev-seven-cm-1.7platform.com:8008/";
         CurrentConfig.SocketIOData.Connection.Token = "d";
         //CurrentConfig.SocketIOData.Connection.Username = "f0008dfe-212b-4492-b803-8877ff1d1ac4";
         CurrentConfig.SocketIOData.Connection.ClientType = "user";
         CurrentConfig.SocketIOData.Connection.ClientSubType = "Player";
         //CurrentConfig.SocketIOData.Subscription.Channel = "c422a17f-c7b8-46b8-906f-03fac529770c";
         CurrentConfig.SocketIOData.Subscription.Channel = "c422a17f-c7b8-46b8-906f-03fac529770c";
         CurrentConfig.SocketIOData.Subscription.SubChannels.DeviceUuid = "9B754E4C-A46D-E6BF-5379-9A9CFBD2F63F";
         CurrentConfig.SocketIOData.Subscription.SubChannels.DeliveryPlatform = "Retail";
         CurrentConfig.SocketIOData.Subscription.SubChannels.Language = "en";
      }
   }
}
