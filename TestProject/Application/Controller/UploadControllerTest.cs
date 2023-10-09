using System;
using System.Net;
using aia_api.Configuration.Azure;
using Microsoft.Extensions.Options;
using Moq;
using Microsoft.AspNet.SignalR;
using aia_api.src.Application;
using System.IO.Abstractions.TestingHelpers;
using aia_api.Services;
using aia_api.Application.Helpers.Factories;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.SignalR.Client;
using aia_api.src.Services;
using Microsoft.AspNet.SignalR.Hubs;

namespace TestProject.Application.Controller
{

    public class SignalRHub : Hub
    {
        public void GetCallControlData()
        {
            Clients.Caller.SetServer("Server");
        }
    }

    public class UploadControllerTest
    {
        private Mock<IOptions<Settings>> _settingsMock;
        private MockFileSystem _fileSystemMock;
        private Mock<BlobServiceClient> _blobServiceClientMock;
        private Mock<IOptions<AzureBlobStorageSettings>> _blobStoragesettingsMock;
        private AzureService _azureService;
        private Mock<FileHandlerFactory> _fileHandlerFactoryMock;
        private Mock<StorageService> _mockStorageService;

        public interface IClientContract
        {
            void SetServer(string s);
        }

        [SetUp]
        public void SetUp()
        {
            //_settingsMock = new Mock<IOptions<Settings>>();
            //_fileSystemMock = new MockFileSystem();

            //_blobServiceClientMock = new Mock<BlobServiceClient>();
            //_blobStoragesettingsMock = new Mock<IOptions<AzureBlobStorageSettings>>();
            //_blobStoragesettingsMock.Setup(s => s.Value).Returns(new AzureBlobStorageSettings { BlobContainerName = "testContainer" });
            //_azureService = new AzureService(_blobServiceClientMock.Object, _blobStoragesettingsMock.Object, _fileSystemMock);

            //_mockStorageService = new Mock<StorageService>(_settingsMock.Object, _fileSystemMock);
        }

        [Test]
        public async Task ZipHandler_ProcessesZipFileCorrectly()
        {
            //var serviceBusServiceMock = new Mock<ServiceBusService>(_settingsMock.Object);
            //var clientsMock = new Mock<IHubCallerConnectionContext<dynamic>>();
            //var fileHandlerFactoryMock = new Mock<FileHandlerFactory>(_settingsMock.Object, _azureService, _fileSystemMock, serviceBusServiceMock.Object);
            //var hubConnection = new HubConnectionBuilder().WithUrl("http://localhost/echo").Build();

            //serviceBusServiceMock.Object.Connection = hubConnection;
            ////await serviceBusServiceMock.Object.Connection.StartAsync();

            //UploadController uploadController = new UploadController(serviceBusServiceMock.Object,
            //                                                        fileHandlerFactoryMock.Object,
            //                                                        _mockStorageService.Object);
            //uploadController.ZipHandler("testName", "testContentType", "");


            //await hubConnection.InvokeAsync("UploadSuccess",
            //                        "File successfully uploaded.",
            //                        It.IsAny<CancellationToken>());




            //var contract = new Mock<IClientContract>();
            //contract.Setup(_ => _.SetServer(It.IsAny<string>()));

            //var mockClients = new Mock<IHubCallerConnectionContext<dynamic>>();
            //mockClients.Setup(_ => _.Caller).Returns(contract.Object);

            //var hub = new SignalRHub()
            //{
            //    Clients = mockClients.Object
            //};

            ////Act
            //hub.GetCallControlData();

            ////Assert
            //contract.Verify(_ => _.SetServer("Server"));
        }
    }
}

