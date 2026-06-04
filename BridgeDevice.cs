using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Length_Stds_Environmental_Monitoring
{

    public sealed class BridgeDevice : IMeasurementDevice
    {
        private readonly ResistanceBridge _bridge;
        private readonly MUX _mux;

        private readonly object _lock = new object();

        private double _cachedResistance;
        private double _cachedTemperature;
        private bool _hasReadThisCycle = false;

        public BridgeDevice(ResistanceBridge bridge, MUX mux)
        {
            _bridge = bridge;
            _mux = mux;
        }

        public void PrepareSensor(object sensor)
        {
            lock (_lock)
            {
                if (_hasReadThisCycle)
                    return;

                ReadSensor(sensor);
                _hasReadThisCycle = true;
            }
        }

        public void ResetCycle()
        {
            _hasReadThisCycle = false;
        }

        public double ReadSensor(object sensor)
        {
            if (sensor is not TemperatureSensor ts)
                return double.NaN;

           
            int channel = ts.Channel;

            // ✅ 1. Select channel
            _mux.SelectChannel(channel);

            // ✅ 2. Read resistance (already bridge-corrected)
            double correctedResistance = _bridge.ReadResistance(channel);

            if (double.IsNaN(correctedResistance))
                return double.NaN;

            // ✅ 3. Convert to temperature (PRT handles this)
            double temperature = ts.ConvertResistanceToTemperature(correctedResistance);

            return temperature;

        }
    }

}
