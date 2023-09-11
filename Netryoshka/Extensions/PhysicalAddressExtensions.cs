using System;
using System.Net.NetworkInformation;
using System.Text;

namespace Netryoshka.Extensions
{

    public enum MacAddressFormat
    {
        ColonSeparated,
        HyphenSeparated,
        CiscoThreeGroup,
        NoSeparators
    }

    public static class PhysicalAddressExtensions
    {
        public static string ToFormattedString(this PhysicalAddress address, MacAddressFormat format)
        {
            string unformatted = address.ToString();
            StringBuilder formatted = new();

            string separator = "";
            int step = 2;

            switch (format)
            {
                case MacAddressFormat.ColonSeparated:  separator = ":"; break;
                case MacAddressFormat.HyphenSeparated: separator = "-"; break;
                case MacAddressFormat.CiscoThreeGroup: separator = "."; step = 4; break;
                case MacAddressFormat.NoSeparators: return unformatted;
            }

            for (int i = 0; i < unformatted.Length; i += step)
            {
                formatted.Append(unformatted.AsSpan(i, step));
                if (i < unformatted.Length - step)
                {
                    formatted.Append(separator);
                }
            }

            return formatted.ToString();
        }
    }

}
