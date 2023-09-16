using System.IO.Pipelines;
using aia_api.Application.EndpointFilter;
using Microsoft.AspNetCore.Http;
using Moq;

namespace TestProject.Application.EndpointFilter;

[TestFixture]
public class EmptyFileFilterTest
{
    private Mock<EndpointFilterInvocationContext> _context;
    private Mock<EndpointFilterDelegate> _next;

    [SetUp]
    public void Setup()
    {
        _context = new Mock<EndpointFilterInvocationContext>();
        _next = new Mock<EndpointFilterDelegate>();
    }

    [Test]
    public async Task InvokeAsync_ShouldReturn400_WhenContentZero()
    {
        // Arrange
        var requestMock = new Mock<HttpRequest>();
        var responseMock = new Mock<HttpResponse>();
        var expectedMessage = "No file received or file is empty.";
        var memoryStream = new MemoryStream();
        var pipeWriter = PipeWriter.Create(memoryStream);

        _context.Setup(x => x.HttpContext).Returns(new DefaultHttpContext());
        _context.Setup(x => x.HttpContext.Request).Returns(requestMock.Object);
        _context.Setup(x => x.HttpContext.Response).Returns(responseMock.Object);
        requestMock.SetupGet(x => x.ContentLength).Returns(0);
        responseMock.SetupGet(x => x.BodyWriter).Returns(pipeWriter);

        // Act
        var result = await new EmptyFileFilter().InvokeAsync(_context.Object, _next.Object);

        // Assert
        memoryStream.Position = 0;
        var reader = new StreamReader(memoryStream);
        var capturedMessage = await reader.ReadToEndAsync();

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo(null));
            Assert.That(capturedMessage, Is.EqualTo(expectedMessage));
        });
    }

}
