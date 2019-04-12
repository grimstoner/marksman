/*
 * Copyright 2019 marksman Contributors (https://github.com/Scope-IT/marksman)
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using Marksman.Types;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SnipeSharp;
using System.Diagnostics;
using SnipeSharp.Endpoints.Models;
using SnipeSharp.Endpoints.SearchFilters;
using SnipeSharp.Common;
using System.Net;

namespace Marksman
{
    /// <summary>
    /// 
    /// </summary>
    class Broker
    {
        /// <summary>
        /// 
        /// </summary>
        public Broker()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="appSettings"></param>
        /// <returns></returns>
        public bool CheckConnection(NameValueCollection appSettings)
        {
            // This method might seem overly complicated for what it is doing (simply
            // checking a connection to the Snipe-IT instance. However, there are a lot
            // of different ways that the connection can fail (usually related to improperly
            // set values in the config file).

            // This method allows a set of specific, descriptive error messages to be passed
            // showing exactly what kind of configuration problem needs to be fixed.

            string uri = "";
            string query = "users?limit=0";
            string baseUri = appSettings["BaseURI"];

            // Note: The program should be able to handle a BaseURI that has a trailing '/' or not.

            if (baseUri.EndsWith("/"))
            {
                uri = baseUri + query;
            }
            else
            {
                uri = baseUri + "/" + query;
            }

            HttpWebRequest request;
            try
            {
                request = (HttpWebRequest)WebRequest.Create(uri);
                request.Headers["Authorization"] = "Bearer " + appSettings["API"];
                request.Accept = "application/json";
            }
            catch (System.NotSupportedException e)
            {
                Console.WriteLine(e);
                Console.WriteLine("Please double-check the BaseURI key in your <appSettings>\nblock of the Marksman config file and ensure it points to your instance of Snipe-IT.");
                return false;
            }
            try
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                if (response.StatusCode == HttpStatusCode.OK)
                {
                    Console.WriteLine("HTTP 200: Connection to Snipe-IT instance succeded.");
                    return true;
                }
                else
                {
                    Console.WriteLine("HTTP {0}", response.StatusCode);
                    Console.WriteLine("Unexpected HTTP response code, could not connect to Snipe-IT instance.");
                    return false;
                }
            }
            catch (WebException e)
            {
                HttpWebResponse r = (HttpWebResponse)e.Response;
                if (r == null)
                {
                    Console.WriteLine(e);
                    Console.WriteLine("Please double-check the BaseURI key in your <appSettings>\nblock of the Marksman config file and ensure it points to your instance of Snipe-IT.");
                }
                else if (r.StatusCode == HttpStatusCode.Unauthorized)
                {
                    Console.WriteLine("HTTP 403: Unauthorized. Please check the API key value in your <appSettings>\nblock of the Marksman config file and ensure it has been set to a valid key.");
                }
                else if (r.StatusCode == HttpStatusCode.NotFound)
                {
                    Console.WriteLine("HTTP 404: URL not found. Please double-check the BaseURI key in your <appSettings>\nblock of the Marksman config file and ensure it points to your instance of Snipe-IT.");
                }
                else
                {
                    Console.WriteLine("Unexpected error, could not connect to Snipe-IT instance.");
                    Console.WriteLine(e);
                }
                return false;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="snipe"></param>
        /// <param name="currentAsset"></param>
        /// <param name="currentModel"></param>
        /// <param name="currentManufacturer"></param>
        /// <param name="currentCategory"></param>
        /// <param name="currentCompany"></param>
        /// <param name="currentStatusLabel"></param>
        /// <param name="currentLocation"></param>
        /// <returns></returns>
        public List<IRequestResponse> SyncAll(SnipeItApi snipe, Asset currentAsset, Model currentModel, Manufacturer currentManufacturer,
            Category currentCategory, Company currentCompany, StatusLabel currentStatusLabel, Location currentLocation)
        {
            // Returns a list of messages with return info.
            // This could be broken down further


            List<IRequestResponse> messages = new List<IRequestResponse>();

            messages.Add(snipe.ManufacturerManager.Create(currentManufacturer));
            SearchFilter manufacturerFilter = new SearchFilter(currentManufacturer.Name);
            Manufacturer updatedManufacturer = snipe.ManufacturerManager.FindOne(manufacturerFilter);

            messages.Add(snipe.CategoryManager.Create(currentCategory));
            SearchFilter categoryFilter = new SearchFilter(currentCategory.Name);
            Category updatedCategory = snipe.CategoryManager.FindOne(categoryFilter);

            currentModel.Manufacturer = updatedManufacturer;
            currentModel.Category = updatedCategory;
            messages.Add(snipe.ModelManager.Create(currentModel));
            SearchFilter modelFilter = new SearchFilter(currentModel.Name);
            Model updatedModel = snipe.ModelManager.FindOne(modelFilter);

            messages.Add(snipe.CompanyManager.Create(currentCompany));
            SearchFilter companyFilter = new SearchFilter(currentCompany.Name);
            Company updatedCompany = snipe.CompanyManager.FindOne(companyFilter);

            messages.Add(snipe.StatusLabelManager.Create(currentStatusLabel));
            SearchFilter statusLabelFilter = new SearchFilter(currentStatusLabel.Name);
            StatusLabel updatedStatusLabel = snipe.StatusLabelManager.FindOne(statusLabelFilter);

            messages.Add(snipe.LocationManager.Create(currentLocation));
            SearchFilter locationFilter = new SearchFilter(currentLocation.Name);
            Location updatedLocation = snipe.LocationManager.FindOne(locationFilter);

            currentAsset.Model = updatedModel;
            currentAsset.Company = updatedCompany;
            currentAsset.StatusLabel = updatedStatusLabel;
            currentAsset.Location = updatedLocation;
            messages.Add(snipe.AssetManager.Create(currentAsset));

            return messages;
        }
    }
}
