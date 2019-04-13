using Marksman.Service.Descriptors;
using SnipeSharp;
using SnipeSharp.Endpoints.Models;
using SnipeSharp.Endpoints.SearchFilters;
using System;
using System.Linq;

namespace Marksman.Service
{
    internal static class SnipeApiExtensions
    {
        #region Class Methods
        
        public static void SyncAssetDetails(this SnipeItApi snipe, AssetDescriptor assetDetails)
        {
            // Validate parameters.
            if (snipe == null)
            {
                throw new ArgumentNullException("snipe");
            }
            if (assetDetails == null)
            {
                throw new ArgumentNullException("assetDetails");
            }


            Asset snipeAsset = snipe.AssetManager.FindOne(new SearchFilter { Search = assetDetails.Serial });
            var manufacturer = EnsureManufacturer(snipe, assetDetails.Manufacturer);
            var category = EnsureAssetCategory(snipe, assetDetails.AssetType == 2 ? "Laptops" : "Desktops");
            var model = EnsureModel(snipe, manufacturer, category, assetDetails.Model, assetDetails.ModelNumber);           

            if (snipeAsset == null)
            {
                snipeAsset = 
                    new Asset()
                    {
                        Name = assetDetails.Name,
                        ModelNumber = assetDetails.ModelNumber,
                        Manufacturer = manufacturer,
                        Category = category,
                        Model = model
                    };

                var response = snipe.AssetManager.Create(snipeAsset);
            }
            else
            {
                snipeAsset.Name = assetDetails.Name;
                snipeAsset.ModelNumber = assetDetails.ModelNumber;
                snipeAsset.Manufacturer = manufacturer;
                snipeAsset.Category = category;
                snipeAsset.Model = model;

                var response = snipe.AssetManager.Update(snipeAsset);
            }
        }

        public static Manufacturer EnsureManufacturer(this SnipeItApi snipe, string manufacturer)
        {
            // Validate parameters.
            if (snipe == null)
            {
                throw new ArgumentNullException("snipe");
            }
            if (String.IsNullOrWhiteSpace(manufacturer))
            {
                throw new ArgumentNullException("manufacturer");
            }


            Manufacturer snipeManufacturer = snipe.ManufacturerManager.FindAll(new SearchFilter { Search = manufacturer }).Rows.FirstOrDefault();
            if (snipeManufacturer == null)
            {
                snipeManufacturer = new Manufacturer() { Name = manufacturer };
                var response = snipe.ManufacturerManager.Create(snipeManufacturer);
            }
            return snipeManufacturer;
        }

        public static Model EnsureModel(this SnipeItApi snipe, Manufacturer manufacturer, Category category, string modelName, string modelNumber)
        {
            // Validate parameters.
            if (snipe == null)
            {
                throw new ArgumentNullException("snipe");
            }
            if (manufacturer == null)
            {
                throw new ArgumentNullException("manufacturer");
            }
            if (category == null)
            {
                throw new ArgumentNullException("category");
            }
            if (String.IsNullOrWhiteSpace(modelName))
            {
                throw new ArgumentNullException("modelName");
            }
            if (String.IsNullOrWhiteSpace(modelNumber))
            {
                throw new ArgumentNullException("modelNumber");
            }


            Model snipeModel = snipe.ModelManager.FindAll(new SearchFilter { Search = modelName }).Rows.FirstOrDefault();
            if (snipeModel == null)
            {
                snipeModel = new Model() { Name = modelName, ModelNumber = modelNumber, Manufacturer = manufacturer, Category = category };
                var response = snipe.ModelManager.Create(snipeModel);
            }
            return snipeModel;
        }

        public static Category EnsureAssetCategory(this SnipeItApi snipe, string assetCategory)
        {
            // Validate parameters.
            if (snipe == null)
            {
                throw new ArgumentNullException("snipe");
            }           
            if (String.IsNullOrWhiteSpace(assetCategory))
            {
                throw new ArgumentNullException("assetCategory");
            }

            Category snipeCategory = snipe.CategoryManager.FindAll(new SearchFilter { Search = assetCategory }).Rows.FirstOrDefault();
            if (snipeCategory == null)
            {
                snipeCategory = new Category() { Name = assetCategory };
                var response = snipe.CategoryManager.Create(snipeCategory);
            }
            return snipeCategory;
        }
        
        #endregion
    }
}
