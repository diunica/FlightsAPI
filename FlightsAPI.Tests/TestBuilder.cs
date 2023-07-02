using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Serialization;

namespace FlightsAPI.Tests
{
    public abstract class IntegrationTestBuilder : IDisposable
    {
        protected HttpClient TestClient;
        private bool Disposed;

        protected IntegrationTestBuilder()
        {
            BootstrapTestingSuite();
        }

        protected void BootstrapTestingSuite()
        {
            Disposed = false;
            var appFactory = new WebApplicationFactory<FlightsAPI.Startup>();
            appFactory.ClientOptions.BaseAddress = new Uri("http://localhost:5252/flyapi");
            TestClient = appFactory.CreateClient();
        }
       

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed)
                return;

            if (disposing)
            {
                TestClient.Dispose();
            }

            Disposed = true;
        }
    }
}
