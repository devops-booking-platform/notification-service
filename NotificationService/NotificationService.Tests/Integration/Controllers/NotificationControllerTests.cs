using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using NotificationService.Data;
using NotificationService.Domain.DTOs;

namespace NotificationService.Tests.Integration.Controllers;

[Collection("NotificationController Collection")]
public class NotificationControllerTests : IClassFixture<NotificationServiceWebApplicationFactory>
{
    private readonly NotificationServiceWebApplicationFactory _factory;
    private readonly HttpClient _client;
    private readonly Guid _testUserId = Guid.NewGuid();

    public NotificationControllerTests(NotificationServiceWebApplicationFactory factory)
    {
        _factory = factory;
        
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddAuthentication(TestAuthHandler.AuthenticationScheme)
                    .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                        TestAuthHandler.AuthenticationScheme, options => { });
            });
        }).CreateClient();
    }

    [Fact]
    public async Task Search_ShouldReturnPagedNotifications_WhenNoFilterApplied()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var notifications = new List<Notification>
        {
            Notification.Create(_testUserId, NotificationType.ReservationCreated, "Test message 1"),
            Notification.Create(_testUserId, NotificationType.ReservationCanceled, "Test message 2"),
            Notification.Create(_testUserId, NotificationType.HostRated, "Test message 3")
        };
        
        await context.Set<Notification>().AddRangeAsync(notifications);
        await context.SaveChangesAsync();
        _client.DefaultRequestHeaders.Add("X-Test-UserId", _testUserId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "User");
        // Act
        var response = await _client.GetAsync("/api/notification?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetNotificationResponse>>();
        
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(3);
        result.TotalCount.Should().Be(3);
        result.Page.Should().Be(1);
        result.PageSize.Should().Be(10);
    }

    [Fact]
    public async Task Search_ShouldReturnFilteredNotifications_WhenReadFilterApplied()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var notification1 = Notification.Create(_testUserId, NotificationType.ReservationCreated, "Unread message");
        var notification2 = Notification.Create(_testUserId, NotificationType.ReservationCanceled, "Read message");
        notification2.MarkRead();
        
        await context.Set<Notification>().AddRangeAsync(notification1, notification2);
        await context.SaveChangesAsync();
        _client.DefaultRequestHeaders.Add("X-Test-UserId", _testUserId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "User");
        // Act
        var response = await _client.GetAsync("/api/notification?page=1&pageSize=10&read=false");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetNotificationResponse>>();
        
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(1);
        result.Items.Should().OnlyContain(n => n.Read == false);
    }

    [Fact]
    public async Task Search_ShouldReturnFilteredNotifications_WhenNotificationTypeFilterApplied()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var notifications = new List<Notification>
        {
            Notification.Create(_testUserId, NotificationType.ReservationCreated, "Message 1"),
            Notification.Create(_testUserId, NotificationType.ReservationCanceled, "Message 2"),
            Notification.Create(_testUserId, NotificationType.ReservationCreated, "Message 3")
        };
        
        await context.Set<Notification>().AddRangeAsync(notifications);
        await context.SaveChangesAsync();
        _client.DefaultRequestHeaders.Add("X-Test-UserId", _testUserId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "User");
        // Act
        var response = await _client.GetAsync($"/api/notification?page=1&pageSize=10&notificationType={NotificationType.ReservationCreated}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetNotificationResponse>>();
        
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.Items.Should().OnlyContain(n => n.NotificationType == NotificationType.ReservationCreated);
    }

    [Fact]
    public async Task Search_ShouldReturnOnlyUserNotifications_WhenMultipleUsersExist()
    {
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var otherUserId = Guid.NewGuid();
        var notifications = new List<Notification>
        {
            Notification.Create(_testUserId, NotificationType.ReservationCreated, "User 1 message"),
            Notification.Create(otherUserId, NotificationType.ReservationCanceled, "User 2 message"),
            Notification.Create(_testUserId, NotificationType.HostRated, "User 1 message 2")
        };
        
        await context.Set<Notification>().AddRangeAsync(notifications);
        await context.SaveChangesAsync();
        _client.DefaultRequestHeaders.Add("X-Test-UserId", _testUserId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "User");
        // Act
        var response = await _client.GetAsync("/api/notification?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetNotificationResponse>>();
        
        result.Should().NotBeNull();
        result!.Items.Should().HaveCount(2);
        result.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Search_ShouldReturnEmptyResult_WhenNoNotificationsExist()
    {
        _client.DefaultRequestHeaders.Add("X-Test-UserId", _testUserId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "User");
        // Act
        var response = await _client.GetAsync("/api/notification?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<GetNotificationResponse>>();
        
        result.Should().NotBeNull();
        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Get_ShouldReturnNotification_WhenNotificationExists()
    {
        _client.DefaultRequestHeaders.Add("X-Test-UserId", _testUserId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "User");
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var notification = Notification.Create(_testUserId, NotificationType.ReservationCreated, "Test message");
        await context.Set<Notification>().AddAsync(notification);
        await context.SaveChangesAsync();

        // Act
        var response = await _client.GetAsync($"/api/notification/{notification.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<GetNotificationResponse>();
        
        result.Should().NotBeNull();
        result!.Id.Should().Be(notification.Id);
        result.Message.Should().Be("Test message");
        result.NotificationType.Should().Be(NotificationType.ReservationCreated);
        result.Read.Should().BeFalse();
    }

    [Fact]
    public async Task Get_ShouldReturnNotFound_WhenNotificationDoesNotExist()
    {
        _client.DefaultRequestHeaders.Add("X-Test-UserId", _testUserId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "User");
        // Act
        var response = await _client.GetAsync($"/api/notification/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task MarkAsRead_ShouldMarkNotificationAsRead_WhenNotificationExists()
    {
        _client.DefaultRequestHeaders.Add("X-Test-UserId", _testUserId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "User");
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var notification = Notification.Create(_testUserId, NotificationType.ReservationCreated, "Test message");
        await context.Set<Notification>().AddAsync(notification);
        await context.SaveChangesAsync();
        
        var command = new MarkNotificationAsReadCommand { Id = notification.Id };

        // Act
        var response = await _client.PostAsJsonAsync("/api/notification", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        
        // Verify in database
        using var verifyScope = _factory.Services.CreateScope();
        var verifyContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var updatedNotification = await verifyContext.Set<Notification>().FindAsync(notification.Id);
        
        updatedNotification.Should().NotBeNull();
        updatedNotification!.Read.Should().BeTrue();
    }

    [Fact]
    public async Task MarkAsRead_ShouldReturnNotFound_WhenNotificationDoesNotExist()
    {
        _client.DefaultRequestHeaders.Add("X-Test-UserId", _testUserId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "User");
        // Arrange
        var command = new MarkNotificationAsReadCommand { Id = Guid.NewGuid() };

        // Act
        var response = await _client.PostAsJsonAsync("/api/notification", command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Search_ShouldHandlePagination_WhenMultiplePages()
    {
        _client.DefaultRequestHeaders.Add("X-Test-UserId", _testUserId.ToString());
        _client.DefaultRequestHeaders.Add("X-Test-Role", "User");
        // Arrange
        using var scope = _factory.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        
        var notifications = Enumerable.Range(1, 15)
            .Select(i => Notification.Create(_testUserId, NotificationType.ReservationCreated, $"Message {i}"))
            .ToList();
        
        await context.Set<Notification>().AddRangeAsync(notifications);
        await context.SaveChangesAsync();

        // Act - Get first page
        var responsePage1 = await _client.GetAsync("/api/notification?page=1&pageSize=10");
        var resultPage1 = await responsePage1.Content.ReadFromJsonAsync<PagedResult<GetNotificationResponse>>();
        
        // Act - Get second page
        var responsePage2 = await _client.GetAsync("/api/notification?page=2&pageSize=10");
        var resultPage2 = await responsePage2.Content.ReadFromJsonAsync<PagedResult<GetNotificationResponse>>();

        // Assert
        responsePage1.StatusCode.Should().Be(HttpStatusCode.OK);
        responsePage2.StatusCode.Should().Be(HttpStatusCode.OK);
        
        resultPage1.Should().NotBeNull();
        resultPage1!.Items.Should().HaveCount(10);
        resultPage1.TotalCount.Should().Be(15);
        resultPage1.TotalPages.Should().Be(2);
        
        resultPage2.Should().NotBeNull();
        resultPage2!.Items.Should().HaveCount(5);
        resultPage2.TotalCount.Should().Be(15);
        resultPage2.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task Search_ShouldRequireAuthentication()
    {
        
        // Arrange
        var unauthenticatedClient = _factory.CreateClient();

        // Act
        var response = await unauthenticatedClient.GetAsync("/api/notification?page=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
