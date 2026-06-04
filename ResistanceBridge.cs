using NCalc;

namespace Length_Stds_Environmental_Monitoring
{
    public abstract class ResistanceBridge
    {
        protected ResistanceBridge()
        {
        }

        public abstract double ReadResistance(int channel);

    }

}
