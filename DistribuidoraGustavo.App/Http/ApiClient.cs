using Blazored.SessionStorage;
using DistribuidoraGustavo.App.Utils;
using FMCW.Common.Results;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace DistribuidoraGustavo.App.Http
{
    public class ApiClient
    {
        private readonly HttpClient _client;

        private readonly ISessionStorageService _sessionStorage;

        private readonly NavigationManager _navigationManager;

        public ApiClient(
            HttpClient client,
            ISessionStorageService sessionStorage,
            NavigationManager navigationManager)
        {
            _client = client;
            _sessionStorage = sessionStorage;
            _navigationManager = navigationManager;
        }

        public async Task<Tresult> Send<Tresult>(ApiRequest request)
            where Tresult : IBaseErrorResult, new()
        {
            var jwtToken = await _sessionStorage.GetJwt();

            // TODO add expiration validation too
            if (string.IsNullOrEmpty(jwtToken) && request.CheckAuth)
            {
                // TODO save state
                _navigationManager.NavigateTo(Views.Login.ToString());
                return new Tresult()
                {
                    ResultError = ErrorResult.Build("El usuario no esta logeado"),
                    Success = false,
                    ResultOperation = ResultOperation.Unauthorized,
                };
            }

            var requestMessage = new HttpRequestMessage()
            {
                Method = new HttpMethod(request.Method),
                RequestUri = new Uri(_client.BaseAddress.OriginalString + request.Url)
            };

            if (request.Parameter != null)
            {
                requestMessage.Content = JsonContent.Create(request.Parameter, request.Parameter.GetType());
            }

            if (request.CheckAuth)
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);
            }

            var response = await _client.SendAsync(requestMessage);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadFromJsonAsync<Tresult>();
                return jsonResponse;
            }

            return new Tresult()
            {
                ResultError = ErrorResult.Build(await response.Content.ReadAsStringAsync()),
                Success = false,
                ResultOperation = ResultOperation.Error,
            };


        }
    }
}
