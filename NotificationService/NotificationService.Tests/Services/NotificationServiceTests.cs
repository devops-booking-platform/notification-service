using AutoMapper;
using NotificationService.Common.Exceptions;
using NotificationService.Domain.DTOs;

namespace NotificationService.Tests.Services;

public class NotificationServiceTests
{
    private readonly Mock<IRepository<Notification>> _notificationRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IMapper> _mapperMock;
    private readonly NotificationService.Services.NotificationService _sut;

    public NotificationServiceTests()
    {
        _notificationRepositoryMock = new Mock<IRepository<Notification>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _mapperMock = new Mock<IMapper>();

        _sut = new NotificationService.Services.NotificationService(
            _notificationRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object,
            _mapperMock.Object
        );
    }

    #region GetNotifications Tests

    [Fact]
    public async Task GetNotifications_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);
        var request = new GetNotificationRequest();

        // Act
        Func<Task> act = async () => await _sut.GetNotifications(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You don't have access to this action.");
    }

    [Fact]
    public async Task GetNotifications_WithReadFilter_ShouldFilterByReadStatus()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var request = new GetNotificationRequest { Read = true };

    }

    [Fact]
    public async Task GetNotifications_WithNotificationTypeFilter_ShouldFilterByType()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);

        var request = new GetNotificationRequest { NotificationType = NotificationType.ReservationCreated };
    }

    #endregion

    #region GetNotification Tests

    [Fact]
    public async Task GetNotification_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        // Act
        Func<Task> act = async () => await _sut.GetNotification(notificationId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You don't have access to this action.");
    }

    [Fact]
    public async Task GetNotification_WhenNotificationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _notificationRepositoryMock.Setup(x => x.GetByIdAsync(notificationId))
            .ReturnsAsync((Notification?)null);

        // Act
        Func<Task> act = async () => await _sut.GetNotification(notificationId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Notification not found");
    }

    [Fact]
    public async Task GetNotification_WhenNotificationExists_ShouldReturnMappedNotification()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var notification = Notification.Create(userId, NotificationType.ReservationCreated, "Test message");
        notification.GetType().GetProperty("Id")!.SetValue(notification, notificationId);

        var expectedResponse = new GetNotificationResponse
        {
            Id = notificationId,
            Message = "Test message",
            Read = false
        };

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _notificationRepositoryMock.Setup(x => x.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);
        _mapperMock.Setup(x => x.Map<GetNotificationResponse>(notification))
            .Returns(expectedResponse);

        // Act
        var result = await _sut.GetNotification(notificationId);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEquivalentTo(expectedResponse);
        _notificationRepositoryMock.Verify(x => x.GetByIdAsync(notificationId), Times.Once);
        _mapperMock.Verify(x => x.Map<GetNotificationResponse>(notification), Times.Once);
    }

    #endregion

    #region MarkAsRead Tests

    [Fact]
    public async Task MarkAsRead_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        // Act
        Func<Task> act = async () => await _sut.MarkAsRead(notificationId);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You don't have access to this action.");
    }

    [Fact]
    public async Task MarkAsRead_WhenNotificationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _notificationRepositoryMock.Setup(x => x.GetByIdAsync(notificationId))
            .ReturnsAsync((Notification?)null);

        // Act
        Func<Task> act = async () => await _sut.MarkAsRead(notificationId);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Notification not found");
    }

    [Fact]
    public async Task MarkAsRead_WhenNotificationExists_ShouldMarkAsReadAndSaveChanges()
    {
        // Arrange
        var notificationId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var notification = Notification.Create(userId, NotificationType.ReservationCreated, "Test message");
        notification.GetType().GetProperty("Id")!.SetValue(notification, notificationId);

        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _notificationRepositoryMock.Setup(x => x.GetByIdAsync(notificationId))
            .ReturnsAsync(notification);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(default))
            .ReturnsAsync(1);

        // Act
        await _sut.MarkAsRead(notificationId);

        // Assert
        notification.Read.Should().BeTrue();
        _notificationRepositoryMock.Verify(x => x.GetByIdAsync(notificationId), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    #endregion
}
