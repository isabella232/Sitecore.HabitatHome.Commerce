﻿using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Sitecore.Commerce.Core;
using Sitecore.Commerce.Plugin.Inventory;
using Sitecore.Framework.Pipelines;
using Sitecore.HabitatHome.Feature.NearestStore.Engine.Components;
using Sitecore.HabitatHome.Feature.NearestStore.Engine.Entities;
using Sitecore.HabitatHome.Feature.NearestStore.Engine.Pipelines.Arguments;

namespace Sitecore.HabitatHome.Feature.NearestStore.Engine.Pipelines.Blocks
{
    [PipelineDisplayName("StoreInventory.block.GetStoreDetails")]
    public class GetNearestStoreDetailsByLocationBlock : PipelineBlock<GetNearestStoreDetailsByLocationArgument, List<NearestStoreLocation>, CommercePipelineExecutionContext>
    {
        private readonly IFindEntitiesInListPipeline _findEntitiesInListPipeline;

        public GetNearestStoreDetailsByLocationBlock(IFindEntitiesInListPipeline findEntitiesInListPipeline)
      : base((string)null)
        {
            this._findEntitiesInListPipeline = findEntitiesInListPipeline;
        }

        public override async Task<List<NearestStoreLocation>> Run(GetNearestStoreDetailsByLocationArgument locationInfo, CommercePipelineExecutionContext context)
        {
            List<NearestStoreLocation> stores = new List<NearestStoreLocation>();
            List<InventorySet> inventorySets = new List<InventorySet>();

            GetNearestStoreDetailsByLocationBlock getNearestStoreDetailsByLocationBlock = this;

            FindEntitiesInListArgument entitiesInListArgument = await getNearestStoreDetailsByLocationBlock._findEntitiesInListPipeline.Run(new FindEntitiesInListArgument(typeof(InventorySet), string.Format("{0}", (object)CommerceEntity.ListName<InventorySet>()), 0, int.MaxValue), context).ConfigureAwait(false);

            if (entitiesInListArgument != null)
            {
                CommerceList<CommerceEntity> list = entitiesInListArgument.List;
                if (list != null)
                    list.Items.ForEach((Action<CommerceEntity>)(item =>
                    {
                        InventorySet inventorySet = (InventorySet)item;
                        

                        inventorySets.Add(inventorySet);
                    }));
            }

            if (inventorySets.Any())
            {
                var storeComponents = inventorySets.Select(x => x.GetComponent<StoreDetailsComponent>());

                storeComponents = storeComponents.Where(x => x.Lat != null).ToList();

                List<Locations> locations = new List<Locations>();
                locations.AddRange(storeComponents.Select(x => x != null ? new Locations() { City = x.City, Latitude = Convert.ToDouble(x.Lat, System.Globalization.CultureInfo.InvariantCulture), Longitude = Convert.ToDouble(x.Long, System.Globalization.CultureInfo.InvariantCulture) } : new Locations()));

                var coord = new GeoCoordinate(locationInfo.Latitude, locationInfo.Longitude);


                var nearestStoresinOrder = locations.Select(x => new GeoCoordinate(x.Latitude, x.Longitude))
                                       .OrderBy(x => x.GetDistanceTo(coord)).Select(z => new Locations { Distance = z.GetDistanceTo(coord), Latitude = z.Latitude, Longitude = z.Longitude }).ToList();
                
                context.Logger.LogInformation("GetNearestStoreDetailsByLocationBlock: nearestStores found: " + nearestStoresinOrder.Count());

                stores.AddRange(nearestStoresinOrder.Select(x => new NearestStoreLocation()
                {
                    Distance = x.Distance,
                    InventoryStoreId = GetStoreId(x.Latitude, x.Longitude, inventorySets),
                    Address = GetStoreDetails(x.Latitude, x.Longitude, inventorySets).GetComponent<StoreDetailsComponent>().Address,
                    Longitude = x.Longitude,
                    Latitude = x.Latitude,
                    Name = GetStoreDetails(x.Latitude, x.Longitude, inventorySets).GetComponent<StoreDetailsComponent>().Name,
                    City = GetStoreDetails(x.Latitude, x.Longitude, inventorySets).GetComponent<StoreDetailsComponent>().City,
                    Zip = GetStoreDetails(x.Latitude, x.Longitude, inventorySets).GetComponent<StoreDetailsComponent>().ZipCode,
                    StateCode = GetStoreDetails(x.Latitude, x.Longitude, inventorySets).GetComponent<StoreDetailsComponent>().StateCode,
                    CountryCode = GetStoreDetails(x.Latitude, x.Longitude, inventorySets).GetComponent<StoreDetailsComponent>().CountryCode
                }));
            }

            return stores;
        }

        private InventorySet GetStoreDetails(double latitude, double longitude, List<InventorySet> inventorySets)
        {
            return inventorySets.Where(x => x.GetComponent<StoreDetailsComponent>().Lat == Convert.ToString(latitude, CultureInfo.InvariantCulture)).FirstOrDefault();            
        }

        private string GetStoreId(double latitude, double longitude, List<InventorySet> inventorySets)
        {
            return inventorySets.Where(x => x.GetComponent<StoreDetailsComponent>().Lat == Convert.ToString(latitude, CultureInfo.InvariantCulture)).FirstOrDefault().FriendlyId;
        }
    }

    public class Locations
    {
        public double Longitude { get; set; }
        public double Latitude { get; set; }
        public string City { get; set; }
        public double Distance { get; set; }
    }
}
