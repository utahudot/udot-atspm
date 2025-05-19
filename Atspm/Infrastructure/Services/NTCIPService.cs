#region license
// Copyright 2025 Utah Departement of Transportation
// for Infrastructure - Utah.Udot.Atspm.Infrastructure.Services/NTCIPService.cs
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
#endregion

using Lextm.SharpSnmpLib;
using Lextm.SharpSnmpLib.Messaging;
using System.Net;

namespace Utah.Udot.Atspm.Infrastructure.Services
{
    /// <summary>
    /// SNMP client for the national transportation communcations for intelligent transportation system protocol
    /// </summary>
    public class NTCIPService
    {
        public void Test()
        {

            //foreach (var d in devices)
            //{
            //    if (d.Ipaddress.IsValidIpAddress())
            //    {
            //        var result = await SnmpGet(d.Ipaddress, "1.3.6.1.4.1.1206.3.5.2.9.17.1.0", "1", "i");

            //        if (result != 1)
            //        {
            //            Console.WriteLine($"{d} --- first: {result}");

            //            await SmnpSet(d.Ipaddress, "1.3.6.1.4.1.1206.3.5.2.9.17.1.0", "0", "i", 161);
            //            await Task.Delay(TimeSpan.FromMilliseconds(350));
            //            await SmnpSet(d.Ipaddress, "1.3.6.1.4.1.1206.3.5.2.9.17.1.0", "1", "i", 161);

            //            await Task.Delay(TimeSpan.FromMilliseconds(350));

            //            result = await SnmpGet(d.Ipaddress, "1.3.6.1.4.1.1206.3.5.2.9.17.1.0", "1", "i");

            //            Console.WriteLine($"{d} --- second: {result}");
            //        };
            //    }
            //}

            //Console.WriteLine($"--- complete");
        }

        private static async Task<int> SnmpGet(string controllerAddress, string objectIdentifier, string value, string type)
        {
            var ipControllerAddress = IPAddress.Parse(controllerAddress);
            var community = "public";
            //var timeout = 1000;
            var version = VersionCode.V1;
            var receiver = new IPEndPoint(ipControllerAddress, 161);
            var oid = new ObjectIdentifier(objectIdentifier);
            var vList = new List<Variable>();
            ISnmpData data = new Integer32(int.Parse(value));
            var oiddata = new Variable(oid, data);
            vList.Add(new Variable(oid));
            var retrievedValue = 0;
            try
            {
                var ts = new CancellationTokenSource(1000);
                var t = await Messenger.GetAsync(version, receiver, new OctetString(community), vList, ts.Token);
                var variable = t.FirstOrDefault();

                retrievedValue = int.Parse(variable.Data.ToString());
            }
            catch (Exception ex)
            {
                //Console.WriteLine(controllerAddress + " - " + ex.ToString());
            }
            return retrievedValue;
        }

        private static async Task SmnpSet(string controllerAddress, string objectIdentifier, string value, string type, int snmpPort)
        {
            var ipControllerAddress = IPAddress.Parse(controllerAddress);
            var community = "public";
            //var timeout = 1000;
            var version = VersionCode.V1;
            var receiver = new IPEndPoint(ipControllerAddress, snmpPort);
            var oid = new ObjectIdentifier(objectIdentifier);
            var vList = new List<Variable>();
            ISnmpData data = new Integer32(int.Parse(value));
            var oiddata = new Variable(oid, data);
            vList.Add(oiddata);
            try
            {
                var ts = new CancellationTokenSource(1000);
                await Messenger.SetAsync(version, receiver, new OctetString(community), vList, ts.Token);
            }
            catch (Exception ex)
            {
                //Console.WriteLine(controllerAddress + " - " + ex.ToString());
            }
        }
    }
}
