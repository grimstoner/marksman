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
using System.Linq;
using System.Management;
using System.Diagnostics;
using System.DirectoryServices.AccountManagement;
using System.Collections.Specialized;
using SnipeSharp;
using SnipeSharp.Endpoints.Models;


namespace Marksman
{
    public class Sentry // Data acquissition
    {
        private Dictionary<string, List<string>> Queries; // Where key = query type, value = query itself
        private Dictionary<string, string> Values; // Internal representation of query results
        private System.Collections.Specialized.NameValueCollection Settings;
        public  Dictionary<string, string> rawResults // Public representation of query results - raw values. Useful for debug
        {
            get { return this.Values;  }
        }



        public Sentry(System.Collections.Specialized.NameValueCollection appSettings) // constructor 
        {
            Queries = new Dictionary<string, List<string>>();
            Settings = appSettings;
        }

        public Location GetLocation(NameValueCollection appSettings, SnipeItApi snipe)
        {
            string assetLocation = this.Values["Location"];
            Location currentLocation = new Location(assetLocation);
            return currentLocation;
        }

        public StatusLabel GetStatusLabel(NameValueCollection appSettings, SnipeItApi snipe)
        {
            string defaultLabel = appSettings["DefaultStatusLabel"];
            StatusLabel defaultStatusLabel = new StatusLabel(defaultLabel);
            return defaultStatusLabel;
        }

        public Company GetCompany(NameValueCollection appSettings, SnipeItApi snipe)
        {
            string companyName = appSettings["Company"];
            Company currentCompany = new Company(companyName);
            return currentCompany;
        }

        public Category GetCategory(NameValueCollection appSettings, SnipeItApi snipe)
        {
            string systemType = GetOutputVariable("Win32_ComputerSystem.PCSystemType");
            // TODO: Place in a separate enum class:
            WindowsSystemTypes winTypes = new WindowsSystemTypes();
            string systemTypeFull = "Undefined";
            try
            {
                systemTypeFull = winTypes.SystemTypes[systemType];
            }
            catch (Exception e)
            {
                Trace.WriteLine("Exception encountered while processing WinSystemType: " + e.ToString());
            }
            Category currentCategory = new Category(systemTypeFull);
            return currentCategory;
        }

        public Manufacturer GetManufacturer(NameValueCollection appSettings, SnipeItApi snipe)
        {
            string manufacturer = GetOutputVariable("Win32_ComputerSystem.Manufacturer");
            Manufacturer systemManufacturer = new Manufacturer(manufacturer);
            return systemManufacturer;
        }

        public Model GetModel(NameValueCollection appSettings, SnipeItApi snipe)
        {
            string modelTotal = GetOutputVariable("Win32_ComputerSystem.Model");
            // TODO: This only works is in the exact format "ModelName ModelNumber"
            List<String> modelFragments = modelTotal.Split(' ').ToList();
            string modelNumber = modelFragments[modelFragments.Count() - 1];
            string modelMake = modelFragments[0];

            Model currentModel = new Model
            {
                Name = modelTotal,
                Manufacturer = null,
                Category = null,
                ModelNumber = modelNumber,
            };
            return currentModel;
        }

        public Asset GetAsset(NameValueCollection appSettings, SnipeItApi snipe)
        {
            string systemName = GetOutputVariable("Win32_ComputerSystem.Name");
            string serialNumber = GetOutputVariable("Win32_ComputerSystemProduct.IdentifyingNumber");
            string macAddress = GetOutputVariable("Win32_NetworkAdapter.MACAddress");
            Dictionary<string, string> customFields = new Dictionary<string, string>();
            customFields.Add("_snipeit_macaddress_1", macAddress);
            string warrantyMonths = appSettings["WarrantyMonths"];

            bool isInteractive = false;
            bool interactiveParseSuccess = Boolean.TryParse(appSettings["Interactive"], out isInteractive);
            if (interactiveParseSuccess && isInteractive)
            {
                Console.WriteLine("Enter the computer name: ");
                systemName = Console.ReadLine();
            }
            
            Asset currentComputer = new SnipeSharp.Endpoints.Models.Asset
            {
                Company = null,
                AssetTag = appSettings["AssetTagPrefix"] + "-" + serialNumber, // <-- to be implemented.. somehow, somewhere
                Model = null,
                StatusLabel = null,
                RtdLocation = null,
                Name = systemName,
                Serial = serialNumber,
                WarrantyMonths = warrantyMonths,
                CustomFields = customFields,
            };

            return currentComputer;
        }

        public void AddQuery(string queryType, string queryString) { // safely addes queries to the queryList monstrocity, built for expandability (c) 
            List<string> queryList = new List<string>();


            if (this.Queries.ContainsKey(queryType))
            {
                queryList = Queries[queryType];
                queryList.Add(queryString);
                this.Queries[queryType] = queryList;
                return;
            } else
            {
                queryList.Add(queryString);
                this.Queries.Add(queryType, queryList);
                return;
            }
        }


        private void RunWMI() { // runs all WMI queries

            Dictionary<string, string> resultDictionary = new Dictionary<string, string>();
            ManagementObjectCollection queryCollection;


            //Query system for Operating System information
            foreach (string wmiQuery in this.Queries["WMI"])
            {
                int count = 0;

                SelectQuery selectQuery = new SelectQuery(wmiQuery);
                ManagementObjectSearcher searcher = new ManagementObjectSearcher(selectQuery);
            

                queryCollection = searcher.Get();

                foreach (ManagementObject m in queryCollection)
                {
                    // Display all properties.
                    foreach (PropertyData property in m.Properties)
                    {
                        string propertyValue = "<undefined>";
                        if  (!String.IsNullOrWhiteSpace(property.Value.ToString()))
                        {
                            propertyValue = property.Value.ToString().Trim();
                        }
                        if (!resultDictionary.ContainsKey(selectQuery.ClassName + "." + property.Name))
                        {
                            resultDictionary.Add(selectQuery.ClassName + "." + property.Name, propertyValue);
                        }
                        else
                        {
                            resultDictionary.Add(selectQuery.ClassName + "." + property.Name + "." + count.ToString(), propertyValue);
                        }
                    }
                    count++;
                }
            }

            this.Values = resultDictionary;
        }


        private void RunLocation() // Runs all code related to location & location sources
        {
            string location_string = "";
            foreach (string locationQuery in this.Queries["Location"])
            {
                if (locationQuery == "OU")
                {
                    try
                    {
                        int ouLevel;
                        bool ouLevelSuccess = int.TryParse(Settings["OULevel"], out ouLevel);
                        if (!ouLevelSuccess)
                        {
                            ouLevel = 1;
                        }
                        string[] machineOU;
                        using (var context = new PrincipalContext(ContextType.Domain))
                        using (var comp = ComputerPrincipal.FindByIdentity(context, Environment.MachineName))
                            machineOU = comp.DistinguishedName.Split(',').SkipWhile(s => !s.StartsWith("OU=")).ToArray();

                        location_string = machineOU[0].Split('=')[ouLevel];
                    }
                    catch (Exception e)
                    {
                        Trace.WriteLine("Could not get location from OU");
                        Trace.WriteLine(e.ToString());
                        Trace.WriteLine("Getting location from config file instead");
                        location_string = Settings["Location"];
                    }
                } else
                {
                    location_string = Settings["Location"];

                }
            }
            this.Values.Add("Location", location_string);

        }




        public string GetOutputVariable(string key)
        {
            if (this.Values.ContainsKey(key))
            {
                return this.Values[key];
            } else
            {
                return "";
            }
        }

        public string GetFormattedVariable(string key, string variable = "", string format="<name>=<var>") // produces formatted output, supposed to throw exception if no results in raw results
        {
            if (String.IsNullOrEmpty(variable))
            {
                format = "<var>";
            }
            if (this.Values.ContainsKey(key))
            {
                return format.Replace("<var>", this.Values[key]).Replace("<name>",variable);
            } else
            {
                return "ERROR: key \"" + key + "\" not found in the results of the query";
            }
        }



        public void Run() // supposed to run all queries of all types and handle per-type errors
        {
            this.RunWMI();
            this.RunLocation();
        }

    }
}
