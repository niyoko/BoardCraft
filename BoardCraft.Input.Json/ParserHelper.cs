namespace BoardCraft.Input
{
    using System.Threading.Tasks;
    using Newtonsoft.Json.Linq;

    internal static class ParserHelper
    {
        public static T GetValue<T>(this JObject jObject, string propertyName)
        {
            var jVal = jObject.GetValue(propertyName);
            return jVal.Value<T>();
        }
    }
}
