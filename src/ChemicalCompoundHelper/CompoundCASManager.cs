﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;
using System.Reflection;

namespace ChemicalCompoundHelper
{
    //https://commonchemistry.cas.org/api-overview?utm_campaign=GLO_GEN_ANY_CCH_LDG&utm_medium=EML_CAS%20_ORG&utm_source=EMCommon%20Chemistry%20API
    //https://github.com/RaoulWolf/cccapi

    /// <summary>
    /// Provides access to CAS (Chemistry Abstracts Service) information from the Common Chemistry API. https://commonchemistry.cas.org/
    /// </summary>
    public class CompoundCASManager
    {
        private HttpClient _httpClient;
        public readonly string URL = "https://commonchemistry.cas.org/api/";
        public readonly string SodiumChlorideExampleCAS ="7647-14-5";

        public CompoundCASManager(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.BaseAddress = new Uri(URL);
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<CASDetail?> GetCASDetail(string CASNumber)
        {
            try
            {
                string param = @$"detail?cas_rn={CASNumber}";

                var response = _httpClient.GetAsync(param).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //200
                    var content = await response.Content.ReadAsStringAsync();

                    CASDetail? result = JsonSerializer.Deserialize<CASDetail>(content);
                    return result;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    //400
                    Debug.Print("System.Net.HttpStatusCode.BadRequest");
                    return null;

                }
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

        //Provides the ability to search against the Common Chemistry data source
        //Allows searching by CAS RN(with or without dashes), SMILES(canonical or isomeric), InChI(with or without the "InChI=" prefix), InChIKey, and name
        //Searching by name allows use of a trailing wildcard(e.g., car*)
        //All searches are case-insensitive
        //The result is in the form of substance summary information
        //The results are paginated, with a maximum page size of 100            // Not yet implemented
        public async Task<SearchResponse?> SearchCompound(string searchString)
        {
            try
            {
                string param = @$"search?q={searchString}";

                var response = _httpClient.GetAsync(param).Result;
                if (response.StatusCode == System.Net.HttpStatusCode.OK)
                {
                    //200
                    var content = await response.Content.ReadAsStringAsync();

                    SearchResponse? searchResults = JsonSerializer.Deserialize<SearchResponse>(content);
                    return searchResults;
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    //400
                    Debug.Print("System.Net.HttpStatusCode.BadRequest");
                    return null;

                }
                else
                    return null;
            }
            catch
            {
                return null;
            }
        }

    }

    public class SearchResponse
    {
        public int count { get; set; }
        public List<SearchResult> results { get; set; }
    }

    public class SearchResult
    {
        public string rn { get; set; }
        public string name { get; set; }
        public List<string> images { get; set; }
    }

    public class CASDetailProperty
    {
        public string name { get; set; }
        public string property { get; set; }
        public int sourceNumber { get; set; }

    }

    public class CASDetail
    {
        public string uri { get; set; }
        public string rn { get; set; }
        public string name { get; set; }
        public string image { get; set; }
        public string inchi { get; set; }
        public string inchiKey { get; set; }
        public string smile { get; set; }
        public string canonicalSmile { get; set; }

        public string molecularFormula { get; set; }
        public string molecularMass { get; set; }
        public List<CASDetailProperty> experimentalProperties { get; set; }
        public List<CASDetailProperty> propertyCitations { get; set; }
        public List<string> synonyms { get; set; }
        public List<string> replaceRns { get; set; }

        public override string ToString()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var sb = new StringBuilder();
            foreach (var prop in properties)
            {
                var value = prop.GetValue(this, null) ?? "null";
                sb.AppendLine($"{prop.Name}: {value}");
            }
            return sb.ToString();
        }
    }
}
