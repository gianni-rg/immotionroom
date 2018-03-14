namespace ImmotionAR.ImmotionRoom.DataSource.ControlClient
{
#if !UNITY_5
    using System.Collections.Generic;
    using System.Threading.Tasks;
#endif

    public interface IRestClient
    {
#if UNITY_5
        TResponse Get<TResponse>() where TResponse : new();

        T Post<T>(T data) where T : new();
        TResponse Post<TResponse>() where TResponse : new();

        TResponse Post<TResponse, T>(T data)
            where TResponse : new()
            where T : new();

        string Put<T>(T data) where T : new();

        string Put<T>(int id, T data) where T : new();

        string Delete<TResponse>() where TResponse : new();
#else
        Task<TResponse> GetAsync<TResponse>();

        Task<T> PostAsync<T>(T data);
        Task<T> PostEmptyAsync<T>();
        Task<TResponse> PostAsync<TResponse, T>(T data);
        Task<TResponse> PostAsync<TResponse>();

        Task<string> PutAsync<T>(T data);
        Task<TResponse> PutAsync<TResponse, T>(T data);

        Task<string> PutAsync<T>(int id, T data);
        Task<TResponse> PutAsync<TResponse, T>(int id, T data);

        Task<TResponse> DeleteAsync<TResponse>();
        Task<TResponse> PostFormUrlEncoded<TResponse>(IDictionary<string, string> parameters);
#endif
    }
}