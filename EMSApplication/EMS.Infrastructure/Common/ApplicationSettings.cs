using EMS.Infrastructure.Persistence.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace EMS.Infrastructure.Common
{
    public static class ApplicationSettings
    {
        private static string separator;

        public static int TransactionRetryCount
        {
            get
            {
                if (Injector.IsReady)
                    return Settings.TransactionRetryCount;

                return 5;
            }
        }

        public static string Separator
        {
            get
            {
                if (separator == null && Injector.IsReady)
                {
                    separator = string.IsNullOrWhiteSpace(Settings.Separator) ? " " : Settings.Separator;
                }

                return separator;
            }
        }

        public static int UnitMoneyDecimalPlaces
        {
            get
            {
                if (Injector.IsReady)
                    return Settings.UnitMoneyDecimalPlaces;

                return 4;
            }
        }

        public static double TaxRate
        {
            get
            {
                if (Injector.IsReady)
                    return Settings.TaxRate;

                return 0.12;
            }
        }

        private static IApplicationSettings Settings
        {
            get { return Injector.Resolve<IApplicationSettings>(); }
        }
    }
}
