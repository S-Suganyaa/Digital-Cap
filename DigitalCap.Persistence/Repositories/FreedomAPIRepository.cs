using DigitalCap.Core.Interfaces.Repository;
using DigitalCap.Core.Interfaces.Service;
using DigitalCap.Core.Models;
using DigitalCap.Core.Models.VesselModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Text;

namespace DigitalCap.Persistence.Repositories
{
    public class FreedomAPIRepository : IFreedomAPIRepository
    {
        private readonly string _endpointUrl;
        private readonly IHttpService _httpService;
        private class VesselObject : IJsonContainer<Vessel>
        {
            public Vessel[] Vessel { get; set; }
            public IEnumerable<Vessel> Data { get => Vessel; }
        }
        private class VesselsObject : IJsonContainer<Vessel>
        {
            public Vessel[] Vessels { get; set; }

            public IEnumerable<Vessel> Data { get => Vessels; }
        }
        private class SurveysObject : IJsonContainer<VesselSurvey>
        {
            public VesselSurvey[] Surveys { get; set; }

            public IEnumerable<VesselSurvey> Data
            {
                get => Surveys;
            }
        }
        public async Task<Vessel> GetVessel(string classNumber)
        {
            try
            {
                var vessel = await GetData<VesselObject, Vessel>(null, classNumber);
                if (vessel != null)
                {
                    return vessel.FirstOrDefault();
                }
                else return null;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        private async Task<IEnumerable<U>> GetData<T, U>(string query, string classNum = null)
          where U : CachedData
          where T : IJsonContainer<U>
        {
            string clientUrl = string.Empty;

            if (classNum == null)
            {
                clientUrl = $"{_endpointUrl}/vessel/{query}";
            }
            else
            {
                clientUrl = $"{_endpointUrl}/vessel/{classNum}";

            }

            if (!string.IsNullOrEmpty(query) && classNum != null)
            {
                clientUrl = $"{clientUrl}/{query}";
            }

            var data = _httpService.GetDataAsync<T, U>(clientUrl).Result;
            return data;
        }

        public async Task<IEnumerable<VesselSurvey>> GetSurveys(string classNum)

        {
            return await GetData<SurveysObject, VesselSurvey>("survey-audit", classNum);
        }
      
    }
}
