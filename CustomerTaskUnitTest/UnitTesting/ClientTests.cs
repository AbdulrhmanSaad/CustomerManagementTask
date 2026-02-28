using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CustomerTaskUnitTest.UnitTesting;
using Newtonsoft.Json;
using NSubstitute;
using Xunit;

namespace CustomerTaskUnitTest.Tests
{
    /// <summary>
    /// A fake HttpMessageHandler that lets us control what the HttpClient returns.
    /// We cannot use NSubstitute here because SendAsync is protected — so we use
    /// a simple hand-rolled fake instead.
    /// </summary>
    public class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        public FakeHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        public HttpRequestMessage LastRequest { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(_response);
        }

        // Helper factory methods
        public static FakeHttpMessageHandler ReturnsJson(object body, HttpStatusCode status = HttpStatusCode.OK)
        {
            var json = JsonConvert.SerializeObject(body);
            var response = new HttpResponseMessage(status)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            return new FakeHttpMessageHandler(response);
        }

        public static FakeHttpMessageHandler ReturnsEmpty(HttpStatusCode status = HttpStatusCode.OK)
        {
            var response = new HttpResponseMessage(status)
            {
                Content = new StringContent(string.Empty, Encoding.UTF8, "application/json")
            };
            return new FakeHttpMessageHandler(response);
        }
    }

    public class ClientTests
    {
        private const string BaseUrl = "https://localhost:7120";

        private (Client client, FakeHttpMessageHandler handler) CreateClient(object responseBody, HttpStatusCode status = HttpStatusCode.OK)
        {
            var handler = FakeHttpMessageHandler.ReturnsJson(responseBody, status);
            var httpClient = new HttpClient(handler);
            var client = new Client(httpClient);
            return (client, handler);
        }

        private (Client client, FakeHttpMessageHandler handler) CreateClientEmpty(HttpStatusCode status = HttpStatusCode.OK)
        {
            var handler = FakeHttpMessageHandler.ReturnsEmpty(status);
            var httpClient = new HttpClient(handler);
            var client = new Client(httpClient);
            return (client, handler);
        }


        #region CustomerAllAsync

        [Fact]
        public async Task CustomerAllAsync_Returns200_ShouldReturnCustomerList()
        {
            // Arrange
            var expected = new List<CustomerDto>
            {
                new CustomerDto { Id = 1, Name = "Alice", Phone = "01013513652" },
                new CustomerDto { Id = 2, Name = "Bob",   Phone = "01013513656" }
            };
            var (client, handler) = CreateClient(expected);

            // Act
            var result = await client.CustomerAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("GET", handler.LastRequest.Method.Method);
            Assert.Contains("api/Customer", handler.LastRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task CustomerAllAsync_Returns200_EmptyList_ShouldReturnEmptyCollection()
        {
            // Arrange
            var (client, _) = CreateClient(new List<CustomerDto>());

            // Act
            var result = await client.CustomerAllAsync();

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task CustomerAllAsync_Returns500_ShouldThrowApiException()
        {
            // Arrange
            var (client, _) = CreateClientEmpty(HttpStatusCode.InternalServerError);

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(() => client.CustomerAllAsync());
        }

        [Fact]
        public async Task CustomerAllAsync_WithCancellationToken_ShouldPassTokenThrough()
        {
            // Arrange
            var expected = new List<CustomerDto> { new CustomerDto { Id = 1, Name = "Alice" } };
            var (client, _) = CreateClient(expected);
            var cts = new CancellationTokenSource();

            // Act — should not throw
            var result = await client.CustomerAllAsync(cts.Token);

            // Assert
            Assert.NotNull(result);
        }

        #endregion

        #region CustomerGETAsync (GET by id)

        [Fact]
        public async Task CustomerGETAsync_Returns200_ShouldReturnCustomer()
        {
            // Arrange
            var expected = new CustomerDto { Id = 5, Name = "Charlie", Phone = "01234567890" };
            var (client, handler) = CreateClient(expected);

            // Act
            var result = await client.CustomerGETAsync(5);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(5, result.Id);
            Assert.Equal("Charlie", result.Name);
            Assert.Contains("api/Customer/5", handler.LastRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task CustomerGETAsync_Returns404_ShouldThrowApiException()
        {
            // Arrange
            var problem = new ProblemDetails { Title = "Not Found", Status = 404 };
            var (client, _) = CreateClient(problem, HttpStatusCode.NotFound);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerGETAsync(999));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task CustomerGETAsync_Returns500_ShouldThrowApiException()
        {
            // Arrange
            var (client, _) = CreateClientEmpty(HttpStatusCode.InternalServerError);

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(() => client.CustomerGETAsync(1));
        }

        [Fact]
        public async Task CustomerGETAsync_ShouldSendGetRequest()
        {
            // Arrange
            var expected = new CustomerDto { Id = 3, Name = "Dave" };
            var (client, handler) = CreateClient(expected);

            // Act
            await client.CustomerGETAsync(3);

            // Assert
            Assert.Equal("GET", handler.LastRequest.Method.Method);
        }

        #endregion

        #region CustomerPOSTAsync (Create)

        [Fact]
        public async Task CustomerPOSTAsync_Returns200_ShouldCompleteWithoutException()
        {
            // Arrange
            var (client, handler) = CreateClientEmpty(HttpStatusCode.OK);
            var command = new CreateCustomerCommand { Name = "Eve", Phone = "01000000001" };

            // Act
            await client.CustomerPOSTAsync(command); // should not throw

            // Assert
            Assert.Equal("POST", handler.LastRequest.Method.Method);
            Assert.Contains("api/Customer", handler.LastRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task CustomerPOSTAsync_Returns400_ShouldThrowApiException()
        {
            // Arrange
            var problem = new ProblemDetails { Title = "Bad Request", Status = 400 };
            var (client, _) = CreateClient(problem, HttpStatusCode.BadRequest);
            var command = new CreateCustomerCommand { Name = string.Empty };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerPOSTAsync(command));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task CustomerPOSTAsync_Returns500_ShouldThrowApiException()
        {
            // Arrange
            var (client, _) = CreateClientEmpty(HttpStatusCode.InternalServerError);
            var command = new CreateCustomerCommand { Name = "Frank" };

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(() => client.CustomerPOSTAsync(command));
        }



        #endregion

        #region CustomerDELETEAsync

        [Fact]
        public async Task CustomerDELETEAsync_Returns200_ShouldCompleteWithoutException()
        {
            // Arrange
            var (client, handler) = CreateClientEmpty(HttpStatusCode.OK);

            // Act
            await client.CustomerDELETEAsync(1); // should not throw

            // Assert
            Assert.Equal("DELETE", handler.LastRequest.Method.Method);
            Assert.Contains("api/Customer/1", handler.LastRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task CustomerDELETEAsync_Returns403_ShouldThrowApiException()
        {
            // Arrange
            var problem = new ProblemDetails { Title = "Forbidden", Status = 403 };
            var (client, _) = CreateClient(problem, HttpStatusCode.Forbidden);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerDELETEAsync(1));

            Assert.Equal(403, ex.StatusCode);
        }

        [Fact]
        public async Task CustomerDELETEAsync_Returns404_ShouldThrowApiException()
        {
            // Arrange
            var problem = new ProblemDetails { Title = "Not Found", Status = 404 };
            var (client, _) = CreateClient(problem, HttpStatusCode.NotFound);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerDELETEAsync(999));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task CustomerDELETEAsync_Returns500_ShouldThrowApiException()
        {
            // Arrange
            var (client, _) = CreateClientEmpty(HttpStatusCode.InternalServerError);

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(() => client.CustomerDELETEAsync(1));
        }

        #endregion

        #region CustomerPUTAsync (Update)

        [Fact]
        public async Task CustomerPUTAsync_Returns200_ShouldCompleteWithoutException()
        {
            // Arrange
            var (client, handler) = CreateClientEmpty(HttpStatusCode.OK);
            var command = new UpdateCustomerCommand { Id = 1, Name = "Updated Name" };

            // Act
            await client.CustomerPUTAsync(1, command); // should not throw

            // Assert
            Assert.Equal("PUT", handler.LastRequest.Method.Method);
            Assert.Contains("api/Customer/1", handler.LastRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task CustomerPUTAsync_Returns400_ShouldThrowApiException()
        {
            // Arrange
            var problem = new ProblemDetails { Title = "Bad Request", Status = 400 };
            var (client, _) = CreateClient(problem, HttpStatusCode.BadRequest);
            var command = new UpdateCustomerCommand { Id = 1, Name = string.Empty };

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.CustomerPUTAsync(1, command));

            Assert.Equal(400, ex.StatusCode);
        }

        [Fact]
        public async Task CustomerPUTAsync_Returns500_ShouldThrowApiException()
        {
            // Arrange
            var (client, _) = CreateClientEmpty(HttpStatusCode.InternalServerError);
            var command = new UpdateCustomerCommand { Id = 1, Name = "Test" };

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(() => client.CustomerPUTAsync(1, command));
        }


        #endregion

        #region HistoryAsync

        [Fact]
        public async Task HistoryAsync_Returns200_ShouldReturnCustomerHistory()
        {
            // Arrange
            var expected = new CustomerHistoryResponse { Name = "Abdo Saad" };
            var (client, handler) = CreateClient(expected);

            // Act
            var result = await client.HistoryAsync(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("GET", handler.LastRequest.Method.Method);
            Assert.Contains("api/Customer/history/1", handler.LastRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task HistoryAsync_Returns404_ShouldThrowApiException()
        {
            // Arrange
            var problem = new ProblemDetails { Title = "Not Found", Status = 404 };
            var (client, _) = CreateClient(problem, HttpStatusCode.NotFound);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.HistoryAsync(999));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task HistoryAsync_Returns500_ShouldThrowApiException()
        {
            // Arrange
            var (client, _) = CreateClientEmpty(HttpStatusCode.InternalServerError);

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(() => client.HistoryAsync(1));
        }

        #endregion

        #region AddressHistoryAsync

        [Fact]
        public async Task AddressHistoryAsync_Returns200_ShouldReturnCustomerHistory()
        {
            // Arrange
            var expected = new CustomerHistoryResponse { Name = "Abdo Saad" };
            var (client, handler) = CreateClient(expected);

            // Act
            var result = await client.AddressHistoryAsync(2);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("GET", handler.LastRequest.Method.Method);
            Assert.Contains("api/Customer/AddressHistory/2", handler.LastRequest.RequestUri.ToString());
        }

        [Fact]
        public async Task AddressHistoryAsync_Returns404_ShouldThrowApiException()
        {
            // Arrange
            var problem = new ProblemDetails { Title = "Not Found", Status = 404 };
            var (client, _) = CreateClient(problem, HttpStatusCode.NotFound);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<ApiException<ProblemDetails>>(
                () => client.AddressHistoryAsync(999));

            Assert.Equal(404, ex.StatusCode);
        }

        [Fact]
        public async Task AddressHistoryAsync_Returns500_ShouldThrowApiException()
        {
            // Arrange
            var (client, _) = CreateClientEmpty(HttpStatusCode.InternalServerError);

            // Act & Assert
            await Assert.ThrowsAsync<ApiException>(() => client.AddressHistoryAsync(1));
        }

        #endregion

        #region ApiException

        [Fact]
        public void ApiException_ToString_ShouldIncludeResponse()
        {
            // Arrange
            var headers = new Dictionary<string, IEnumerable<string>>();
            var ex = new ApiException("Error occurred", 500, "Internal error body", headers, null);

            // Act
            var str = ex.ToString();

            // Assert
            Assert.Contains("Internal error body", str);
        }

        [Fact]
        public void ApiException_StatusCode_ShouldBeSetCorrectly()
        {
            // Arrange
            var headers = new Dictionary<string, IEnumerable<string>>();
            var ex = new ApiException("Not found", 404, "Resource missing", headers, null);

            // Assert
            Assert.Equal(404, ex.StatusCode);
            Assert.Equal("Resource missing", ex.Response);
        }

        [Fact]
        public void ApiExceptionGeneric_Result_ShouldBeSetCorrectly()
        {
            // Arrange
            var headers = new Dictionary<string, IEnumerable<string>>();
            var problem = new ProblemDetails { Title = "Bad Request", Status = 400 };
            var ex = new ApiException<ProblemDetails>("Bad Request", 400, "body", headers, problem, null);

            // Assert
            Assert.Equal(400, ex.StatusCode);
            Assert.NotNull(ex.Result);
            Assert.Equal("Bad Request", ex.Result.Title);
        }

        #endregion
    }
}