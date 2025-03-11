using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Moq;
using Moq.Protected;
using Xunit;
using ChemicalCompoundHelper;
using System.Diagnostics;

namespace ChemicalCompoundHelper.Tests
{
    public class CompoundCASManagerTests
    {
        [Fact]
        public async Task GetCASDetail_ReturnsCASDetail_ActualIgnoreCase()
        {
            // Arrange
            var httpClient = new HttpClient()
            {
            };
            var manager = new CompoundCASManager(httpClient);

            //Act
            var result = await manager.GetCASDetail("7647-14-5");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("7647-14-5", result.rn);
            Assert.Equal("Sodium Chloride", result.name, ignoreCase: true);

            Debug.Print(result.ToString());
        }

        [Fact]
        public async Task GetCASDetail_ReturnsCASDetail_WhenStatusCodeIsOk()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"rn\":\"7647-14-5\",\"name\":\"Sodium Chloride\"}")
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://commonchemistry.cas.org/api/")
            };

            var manager = new CompoundCASManager(httpClient);

            // Act
            var result = await manager.GetCASDetail("7647-14-5");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("7647-14-5", result.rn);
            Assert.Equal("Sodium Chloride", result.name);
        }

        [Fact]
        public async Task GetCASDetail_ReturnsNull_WhenStatusCodeIsBadRequest()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest
                });

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://commonchemistry.cas.org/api/")
            };

            var manager = new CompoundCASManager(httpClient);

            // Act
            var result = await manager.GetCASDetail("invalid-cas-number");

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetCASDetail_ReturnsNull_WhenExceptionIsThrown()
        {
            // Arrange
            var mockHttpMessageHandler = new Mock<HttpMessageHandler>();
            mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .Throws(new HttpRequestException());

            var httpClient = new HttpClient(mockHttpMessageHandler.Object)
            {
                BaseAddress = new Uri("https://commonchemistry.cas.org/api/")
            };

            var manager = new CompoundCASManager(httpClient);

            // Act
            var result = await manager.GetCASDetail("7647-14-5");

            // Assert
            Assert.Null(result);
        }
    }
}
