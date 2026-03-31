using CustomersTask4.Resource;
using Microsoft.Extensions.Localization;
using System.Reflection;

namespace CustomersTask4.Services
{
    public interface ILocalizationService
    {
        string Localize(string key,params object[]args);
    }
    public class LocalizationService:ILocalizationService
    {
        private readonly IStringLocalizer _localizer;

        public LocalizationService(IStringLocalizerFactory factory)
        {
            var type = typeof(SharedResource);
            var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
            _localizer = factory.Create(nameof(SharedResource), assemblyName.Name);
        }

        public string Localize(string key, params object[] args)
              => _localizer[key,args];
    }
}
