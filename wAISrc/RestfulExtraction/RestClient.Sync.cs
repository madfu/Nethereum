using System;
using ai.wallet.mobile.logging;

namespace RestfulExtraction
{
	public partial class RestClient
	{
		/// <summary>
		///     Executes the specified request and downloads the response data
		/// </summary>
		/// <param name="request">Request to execute</param>
		/// <returns>Response data</returns>
		// ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
		public byte[] DownloadData(IRestRequest request) => DownloadData(request, false);

		/// <summary>
		///     Executes the specified request and downloads the response data
		/// </summary>
		/// <param name="request">Request to execute</param>
		/// <param name="throwOnError">Throw an exception if download fails.</param>
		/// <returns>Response data</returns>
		// ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Global
		public byte[] DownloadData(IRestRequest request, bool throwOnError)
		{
			var response = Execute(request);
			if (response.ResponseStatus == ResponseStatus.Error && throwOnError)
			{
				throw response.ErrorException;
			}

			return response.RawBytes;
		}

		/// <summary>
		///     Executes the request and returns a response, authenticating if needed
		/// </summary>
		/// <param name="request">Request to be executed</param>
		/// <returns>RestResponse</returns>
		public virtual IRestResponse Execute(IRestRequest request)
		{
			var method = Enum.GetName(typeof(Method), request.Method);

			switch (request.Method)
			{
				case Method.COPY:
				case Method.POST:
				case Method.PUT:
				case Method.PATCH:
				case Method.MERGE:
					return Execute(request, method, DoExecuteAsPost);

				default:
					return Execute(request, method, DoExecuteAsGet);
			}
		}

		public IRestResponse ExecuteAsGet(IRestRequest request, string httpMethod)
		{
			return Execute(request, httpMethod, DoExecuteAsGet);
		}

		public IRestResponse ExecuteAsPost(IRestRequest request, string httpMethod)
		{
			request.Method = Method.POST; // Required by RestClient.BuildUri... 

			return Execute(request, httpMethod, DoExecuteAsPost);
		}

		/// <summary>
		///     Executes the specified request and deserializes the response content using the appropriate content handler
		/// </summary>
		/// <typeparam name="T">Target deserialization type</typeparam>
		/// <param name="request">Request to execute</param>
		/// <returns>RestResponse[[T]] with deserialized data in Data property</returns>
		public virtual IRestResponse<T> Execute<T>(IRestRequest request) where T : new()
		{
			return Deserialize<T>(request, Execute(request));
		}

		public IRestResponse<T> ExecuteAsGet<T>(IRestRequest request, string httpMethod) where T : new()
		{
			return Deserialize<T>(request, ExecuteAsGet(request, httpMethod));
		}

		public IRestResponse<T> ExecuteAsPost<T>(IRestRequest request, string httpMethod) where T : new()
		{
			return Deserialize<T>(request, ExecuteAsPost(request, httpMethod));
		}

		private IRestResponse Execute(IRestRequest request, string httpMethod,
			Func<IHttp, string, HttpResponse> getResponse)
		{
			Console.WriteLine("RestClient.Sync.Execute -- pre authenticate");

			// holding for elimination
			// AuthenticateIfNeeded(this, request);

			IRestResponse response = new RestResponse();

			try
			{

				var http = ConfigureHttp(request);

				Console.WriteLine("RestClient.Sync.Execute -- post configure");

				// dump the particulars
				wAINetworkTransactionLogger.DumpRequestHttpComponents(http);

				response = ConvertToRestResponse(request, getResponse(http, httpMethod));
				Console.WriteLine("RestClient.Sync.Execute -- post convert");

				response.Request = request;
				response.Request.IncreaseNumAttempts();
			}
			catch (Exception ex)
			{
				response.ResponseStatus = ResponseStatus.Error;
				response.ErrorMessage = ex.Message;
				response.ErrorException = ex;
				Console.WriteLine("message : " + ex.Message);
				Console.WriteLine(ex.StackTrace);
			}

			return response;
		}

		private static HttpResponse DoExecuteAsGet(IHttp http, string method)
		{
			HttpResponse response = null;

			try
			{
				response = http.AsGet(method);
			}
			catch (Exception e)
			{
				Console.WriteLine("message : " + e.Message);
				Console.WriteLine(e.StackTrace);
			}
			return response;
		}

		private static HttpResponse DoExecuteAsPost(IHttp http, string method)
		{
			HttpResponse response = null;

			try
			{
				response = http.AsPost(method);
			}
			catch (Exception e)
			{
				Console.WriteLine("message : " + e.Message);
				Console.WriteLine(e.StackTrace);
			}
			return response;
		}
	}
}