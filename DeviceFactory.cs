using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Length_Stds_Environmental_Monitoring
{
    public sealed class DeviceFactory
    {
        private readonly EquipmentRegister _register;

        public DeviceFactory(EquipmentRegister register)
        {
            _register = register;
        }

        public BridgeDevice CreateBridgeDevice(InventoryItem item)
        {
            string eq_id = item.Id;

            // ✅ --- TRANSPORT RESOLUTION (correct location) ---

            string SICL;
            string ipaddr;
            string gpibaddr;

            string s = _register.SpecificationElement(item.Id, "sicl");

            if (s != null && s.Contains("GPIB"))
            {
                // ✅ Direct connection (e.g. Agilent 34972A with built-in LAN)
                SICL = _register.SpecificationElement(item.Id, "sicl");
                ipaddr = _register.Address(item.Id, "ip");
                gpibaddr = _register.Address(item.Id, "gpib");
            }
            else
            {
                // ✅ Gateway-based connection
                var gateways = _register.WildCardInventory("Gateway");

                var gateway = gateways
                    .FirstOrDefault(g => g.Location == item.Location);

                if (gateway == null)
                    throw new Exception("No gateway found for bridge location");

                SICL = _register.SpecificationElement(gateway.Id, "siclInterfaceID");
                ipaddr = _register.Address(gateway.Id, "ip");
                gpibaddr = _register.Address(item.Id, "gpib");
            }

            if (string.IsNullOrEmpty(SICL))
                throw new Exception("Invalid SICL configuration");

            // ✅ Build transport
            var transport = new GpibOverLanTransport(
                ipaddr,
                502,
                SICL,
                Convert.ToInt32(gpibaddr));

            // ✅ --- CALIBRATION (unchanged but correct) ---

            string eq1 = _register.GetLatestEquationValuePartialMatch(eq_id, "Bridge 1") ?? "r";
            string eq2 = _register.GetLatestEquationValuePartialMatch(eq_id, "Bridge 2") ?? "r";
            string eq3 = _register.GetLatestEquationValuePartialMatch(eq_id, "Bridge 3") ?? "r";

            var cal1 = new EquationCalibration(eq1, "r");
            var cal2 = new EquationCalibration(eq2, "r");
            var cal3 = new EquationCalibration(eq3, "r");

            // ✅ --- Bridge selection ---
            string r = _register.SpecificationElement(item.Id, "internalResistor");
            double internal_resistor = 0.0;
            try
            {
                internal_resistor = Convert.ToDouble(r);
            }
            catch (FormatException)
            {
                internal_resistor = 0.0;
            }

            ResistanceBridge bridge;

            if (item.Model.Contains("Micro"))
            {
                bridge = new IsotechMicroBridge(transport, cal1, cal2, cal3,internal_resistor);
            }
            else if (item.Model.Contains("3497"))
            {
                bridge = new AgilentBridge(transport, cal1, cal2, cal3);
            }
            else
            {
                throw new NotSupportedException($"Unsupported bridge model: {item.Model}");
            }

            // ✅ --- MUX selection ---

            MUX mux;

            if (item.Model.Contains("Micro"))
            {
                mux = new IsotechMux(transport);
            }
            else if (item.Model.Contains("3497"))
            {
                mux = new AgilentMUX();
            }
            else
            {
                throw new NotSupportedException($"Unsupported MUX model: {item.Model}");
            }

            // ✅ --- Return device ---
            return new BridgeDevice(bridge, mux);
        }
    }
}
