namespace GlobalKiosk.Integrations.QualityAssurance
{
    using System;
    using System.Text;
    using System.Net.Http;
    using System.Threading.Tasks;
    using System.Net.Http.Headers;

    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;

    using GlobalKiosk.Integrations.QualityAssurance.Common;
    using GlobalKiosk.Integrations.QualityAssurance.Evaluations.Criterions;
	using RestSharp;
	using System.Collections.Generic;

	public class IntegrationClient
    {
        #region Fields

        private readonly HttpClient httpClient;
        private ManagementAPI managementAPI;

		#endregion Fields

		#region Constructor

		public IntegrationClient(bool isTestMode, string baseUrl)
        {
            var uriString = "http://localhost:48817/";
            if (!isTestMode)
            {
                uriString = baseUrl;
            }
            this.httpClient = new HttpClient();
            this.httpClient.BaseAddress = new Uri(uriString: uriString);
            this.httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            ManagementAPI managementAPI = new ManagementAPI(baseUrl, "6vbksca8c8dtjbj4qp55eomfnr", "3v4i8f5oe94cn01tnbnciojis7gslmdeo9o5phumceh4ad12g9n");
        }

        #endregion Constructor

        #region Methods

        public async Task<CommandResult> CreateEvaluationAsync(EvaluationCriteria criteria)
        {
            var stringContent = Serialize(criteria);
            var content = new StringContent(stringContent, Encoding.UTF8, "application/json");
            var requestUri = "api/QualityAssurance/evaluations";
            var response = await this.httpClient.PostAsync(requestUri: requestUri, content: content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.DeserializeAsString());
            }

            return response.Deserialize<CommandResult>();
        }

		public async Task<RestResponse> TransmitKioskMessageAsync(Guid kioskId, string messageTypeName, Guid? componentTypeId, Guid? componentRuleViolationId,
		   string kmsUserName, string stageChangeReason, Guid? stageId,
		   int? maintenanceModeErrorCode = null)
		{
			try
			{

				var url = APIMethodConstants.TransmitKioskMessage;

				var criteria = TransmitKioskMessageCriteria.New(
					kioskId: kioskId,
					messageTypeName: messageTypeName,
					componentTypeId: componentTypeId,
					componentRuleViolationId: componentRuleViolationId,
					userName: kmsUserName,
					maintenanceModeErrorCode: maintenanceModeErrorCode,
					stageChangeReason: stageChangeReason,
					stageId: stageId,
					initiatedByIoT: false
				);

				var response = await managementAPI.Post(url, criteria);

				return response;
			}
			catch (Exception ex)
			{
				throw;
			}
		}

		public async Task<CommandResult> CreateResultAsync(ResultCriteria criteria)
        {
            var stringContent = Serialize(criteria);
            var content = new StringContent(stringContent, Encoding.UTF8, "application/json");
            var requestUri = "api/QualityAssurance/results";
            var response = await this.httpClient.PostAsync(requestUri: requestUri, content: content);
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(response.DeserializeAsString());
            }

            return response.Deserialize<CommandResult>();
        }

        #endregion Methods

        #region Private Methods

        private static string Serialize<T>(T obj)
        {
            var settings = new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver(), NullValueHandling = NullValueHandling.Ignore };

            return JsonConvert.SerializeObject(obj, settings);
        }

		// Use Management : 
		//public async Task<List<KioskComponentAssociation>> GetKioskComponentRelationshipsAsync(string KioskNumber)
		//{
		//	try
		//	{
		//		var url = "Relationships/kioskComponent/associations";

		//		var queryHeaders = new Dictionary<string, string>
		//		{
		//			{ "kioskNumber", KioskNumber },
		//			{ "isFilteringByActive", "true" }
		//		};

		//		var queryParameters = new Dictionary<string, string>{
		//			{ "PageSize", "500" }
		//		}; 

		//		PagedResultViewModel<KioskComponentAssociation> sitePaged = JsonConvert.DeserializeObject<PagedResultViewModel<KioskComponentAssociation>>(await ManagementAPI.Get(url, queryHeaders, queryParameters));

		//		if (sitePaged != null && sitePaged.Items != null && sitePaged.Items.Count > 0)
		//			return sitePaged.Items.ToList();
		//		else
		//			return null;
		//	}
		//	catch (Exception ex)
		//	{
		//		throw;
		//	}

		//}

		#endregion Private Methods
	}
}
