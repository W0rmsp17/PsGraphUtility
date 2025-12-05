using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System;

namespace PsGraphUtility.Routes.Devices
{
    public static class DeviceRoutes
    {
        private const string BaseV1 = "https://graph.microsoft.com/v1.0";

        public static string Device(string id) =>
            $"{BaseV1}/devices/{id}";

        public static string ListAll() =>
            $"{BaseV1}/devices?$top=200";

        public static string ListEnabled() =>
            $"{BaseV1}/devices?$filter=accountEnabled eq true&$top=200";

    }
}
