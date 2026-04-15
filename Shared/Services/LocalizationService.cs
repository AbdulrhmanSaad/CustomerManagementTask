using Microsoft.Extensions.Localization;
using Shared.Resource;
using System.Reflection;
namespace Shared.Services
{
        public interface ILocalizationService
        {
            string Localize(string key, params object[] args);
        }
        public class LocalizationService : ILocalizationService
        {
            private readonly IStringLocalizer _localizer;

            public LocalizationService(IStringLocalizerFactory factory)
            {
                var type = typeof(SharedResource);
                var assemblyName = new AssemblyName(type.GetTypeInfo().Assembly.FullName);
                _localizer = factory.Create(nameof(SharedResource), assemblyName.Name);
            }

            public string Localize(string key, params object[] args)
                  => _localizer[key, args];
        }
}
