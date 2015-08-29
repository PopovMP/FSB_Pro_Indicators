using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ForexStrategyBuilder.Infrastructure.Interfaces;

namespace ForexStrategyBuilder.Services
{
    public class Repository
    {
        public Dictionary<string, IIndicator> GetIndicators()
        {
            return Assembly.GetExecutingAssembly().GetTypes()
                .Where(type => type.GetInterfaces().Contains(typeof (IIndicator)))
                .Where(type => type.GetConstructor(Type.EmptyTypes) != null)
                .Select(type => Activator.CreateInstance(type) as IIndicator)
                .Where(indicator => !string.IsNullOrEmpty(indicator.IndicatorName))
                .ToDictionary(indicator => indicator.IndicatorName, indicator => indicator);
        }

        public IIndicator CreateInstance(IIndicator indicator)
        {
            return (IIndicator) Activator.CreateInstance(indicator.GetType());
        }
    }
}