using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using eShopOnContainers.Core.Models.Catalog;
using eShopOnContainers.Core.Services.RequestProvider;
using eShopOnContainers.Core.Extensions;
using System.Collections.Generic;
using eShopOnContainers.Core.Services.FixUri;

namespace eShopOnContainers.Core.Services.Catalog
{
    public class CatalogService : ICatalogService
    {
        private readonly IRequestProvider _requestProvider;
        private readonly IFixUriService _fixUriService;

        public CatalogService(IRequestProvider requestProvider, IFixUriService fixUriService)
        {
            _requestProvider = requestProvider;
            _fixUriService = fixUriService;
        }

        public async Task<ObservableCollection<CatalogItem>> FilterAsync(int catalogBrandId, int catalogTypeId)
        {
            var builder = new UriBuilder(GlobalSetting.Instance.CatalogEndpoint);
            builder.Path = string.Format("api/v1/catalog/items/type/{0}/brand/{1}", catalogTypeId, catalogBrandId);
            var uri = builder.ToString();

            var catalog = await _requestProvider.GetAsync<CatalogRoot>(uri);

            if (catalog?.Data != null)
                return catalog?.Data.ToObservableCollection();
            else
                return new ObservableCollection<CatalogItem>();
        }

        public async Task<ObservableCollection<CatalogItem>> GetCatalogAsync()
        {
            var builder = new UriBuilder(GlobalSetting.Instance.CatalogEndpoint);
            builder.Path = "api/v1/catalog/items";
            var uri = builder.ToString();

            var catalog = await _requestProvider.GetAsync<CatalogRoot>(uri);

            if (catalog?.Data != null)
            {
                _fixUriService.FixCatalogItemPictureUri(catalog?.Data);
                return catalog?.Data.ToObservableCollection();
            }
            else
                return new ObservableCollection<CatalogItem>();
        }

        public async Task<ObservableCollection<CatalogBrand>> GetCatalogBrandAsync()
        {
            var builder = new UriBuilder(GlobalSetting.Instance.CatalogEndpoint);
            builder.Path = "api/v1/catalog/catalogbrands";
            var uri = builder.ToString();

            var brands = await _requestProvider.GetAsync<IEnumerable<CatalogBrand>>(uri);

            if (brands != null)
                return brands?.ToObservableCollection();
            else
                return new ObservableCollection<CatalogBrand>();
        }

        public async Task<ObservableCollection<CatalogType>> GetCatalogTypeAsync()
        {
            var builder = new UriBuilder(GlobalSetting.Instance.CatalogEndpoint);
            builder.Path = "api/v1/catalog/catalogtypes";
            var uri = builder.ToString();

            var types = await _requestProvider.GetAsync<IEnumerable<CatalogType>>(uri);

            if (types != null)
                return types.ToObservableCollection();
            else
                return new ObservableCollection<CatalogType>();
        }
    }
}
