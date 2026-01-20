using Microsoft.EntityFrameworkCore;
using NotificationService.Common.Exceptions;
using NotificationService.Domain.DTOs;

namespace NotificationService.Tests.Services;

public class NotificationDisabledServiceTests
{
    private readonly Mock<IRepository<NotificationDisabled>> _notificationDisabledRepositoryMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly NotificationService.Services.NotificationDisabledService _sut;

    public NotificationDisabledServiceTests()
    {
        _notificationDisabledRepositoryMock = new Mock<IRepository<NotificationDisabled>>();
        _currentUserServiceMock = new Mock<ICurrentUserService>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _sut = new NotificationService.Services.NotificationDisabledService(
            _notificationDisabledRepositoryMock.Object,
            _currentUserServiceMock.Object,
            _unitOfWorkMock.Object
        );
    }

    #region DisableNotification Tests

    [Fact]
    public async Task DisableNotification_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);
        var request = new EnableDisableNotificationRequest
        {
            NotificationType = NotificationType.ReservationCreated
        };

        // Act
        Func<Task> act = async () => await _sut.DisableNotification(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You don't have access to this action.");
    }

    [Fact]
    public async Task DisableNotification_WhenUserAuthenticated_ShouldCreateDisabledNotificationAndSave()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

        var request = new EnableDisableNotificationRequest
        {
            NotificationType = NotificationType.ReservationCreated
        };

        NotificationDisabled? capturedNotification = null;
        _notificationDisabledRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<NotificationDisabled>()))
            .Callback<NotificationDisabled>(n => capturedNotification = n)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DisableNotification(request);

        // Assert
        capturedNotification.Should().NotBeNull();
        capturedNotification!.UserId.Should().Be(userId);
        capturedNotification.NotificationType.Should().Be(NotificationType.ReservationCreated);
        _notificationDisabledRepositoryMock.Verify(x => x.AddAsync(It.IsAny<NotificationDisabled>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(default), Times.Once);
    }

    [Theory]
    [InlineData(NotificationType.ReservationCreated)]
    [InlineData(NotificationType.ReservationCanceled)]
    [InlineData(NotificationType.ReservationResponded)]
    [InlineData(NotificationType.HostRated)]
    [InlineData(NotificationType.AccommodationRated)]
    public async Task DisableNotification_WithDifferentNotificationTypes_ShouldCreateCorrectDisabledNotification(
        NotificationType notificationType)
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        _unitOfWorkMock.Setup(x => x.SaveChangesAsync(default)).ReturnsAsync(1);

        var request = new EnableDisableNotificationRequest
        {
            NotificationType = notificationType
        };

        NotificationDisabled? capturedNotification = null;
        _notificationDisabledRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<NotificationDisabled>()))
            .Callback<NotificationDisabled>(n => capturedNotification = n)
            .Returns(Task.CompletedTask);

        // Act
        await _sut.DisableNotification(request);

        // Assert
        capturedNotification.Should().NotBeNull();
        capturedNotification!.NotificationType.Should().Be(notificationType);
    }

    #endregion

    #region EnableNotification Tests

    [Fact]
    public async Task EnableNotification_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);
        var request = new EnableDisableNotificationRequest
        {
            Id = Guid.NewGuid(),
            NotificationType = NotificationType.ReservationCreated
        };

        // Act
        Func<Task> act = async () => await _sut.EnableNotification(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You don't have access to this action.");
    }

    [Fact]
    public async Task EnableNotification_WhenIdIsNull_ShouldThrowUnauthorizedException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _currentUserServiceMock.Setup(x => x.UserId).Returns(userId);
        var request = new EnableDisableNotificationRequest
        {
            Id = null,
            NotificationType = NotificationType.ReservationCreated
        };

        // Act
        Func<Task> act = async () => await _sut.EnableNotification(request);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You don't have access to this action.");
    }

    #endregion

    #region GetDisabledNotifications Tests

    [Fact]
    public async Task GetDisabledNotifications_WhenUserNotAuthenticated_ShouldThrowUnauthorizedException()
    {
        // Arrange
        _currentUserServiceMock.Setup(x => x.UserId).Returns((Guid?)null);

        // Act
        Func<Task> act = async () => await _sut.GetDisabledNotifications();

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("You don't have access to this action.");
    }

    #endregion
}
