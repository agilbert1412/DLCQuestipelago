using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DLCQuestipelago.Gifting
{
    [Flags]
    public enum GiftingMode
    {
        Disabled =       0b00000,
        ReceiveGifts =   0b00001,
        TrapsToEnemies = 0b00010,
        TrapsToAllies =  0b00100,
        GiftsToAllies =  0b01000,
        GiftsToEnemies = 0b10000,
        TrapsToAll =     ReceiveGifts | TrapsToEnemies | TrapsToAllies,
        GiftsToAll =     ReceiveGifts | GiftsToAllies | GiftsToEnemies,
        Strategic =      ReceiveGifts | TrapsToEnemies | GiftsToAllies,
        Stupid =         ReceiveGifts | TrapsToAllies | GiftsToEnemies,
        Everything =     TrapsToAll | GiftsToAll,
    }

    public class GiftingModJsonConverter : JsonConverter<GiftingMode>
    {
        public override void WriteJson(JsonWriter writer, GiftingMode value, JsonSerializer serializer)
        {
            var valueString = value.ToString();
            var token = JToken.FromObject(valueString);
            token.WriteTo(writer);
        }

        public override GiftingMode ReadJson(JsonReader reader, Type objectType, GiftingMode existingValue, bool hasExistingValue,
            JsonSerializer serializer)
        {
            throw new NotImplementedException("CanRead is false");
        }

        public override bool CanRead => false;
    }
}
