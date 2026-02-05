using FluentAssertions;
using TiendaDawWeb.Shared.Exceptions;

namespace TiendaDawWeb.Tests.Shared.Exceptions;

public class DomainExceptionTests
{
    [Test]
    public void DomainException_CanBeCreatedWithMessage()
    {
        var exception = new TestDomainException("Test message");

        exception.Message.Should().Be("Test message");
    }

    [Test]
    public void DomainException_InheritsFromException()
    {
        var exception = new TestDomainException("Test");

        exception.Should().BeAssignableTo<Exception>();
    }

    private class TestDomainException : DomainException
    {
        public TestDomainException(string message) : base(message) { }
    }
}

public class NotFoundExceptionTests
{
    [Test]
    public void NotFoundException_CanBeCreatedWithMessage()
    {
        var exception = new NotFoundException("Resource not found");

        exception.Message.Should().Be("Resource not found");
    }

    [Test]
    public void NotFoundException_FromId_FormatsCorrectly()
    {
        var exception = NotFoundException.FromId(123, "Producto");

        exception.Message.Should().Be("Recurso con ID 123 no encontrado");
    }
}

public class ValidationExceptionTests
{
    [Test]
    public void ValidationException_CanBeCreatedWithMessage()
    {
        var exception = new ValidationException("Validation failed");

        exception.Message.Should().Be("Validation failed");
        exception.ValidationErrors.Should().BeNull();
    }

    [Test]
    public void ValidationException_CanBeCreatedWithMessageAndErrors()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Email is required" } },
            { "Password", new[] { "Password too short" } }
        };

        var exception = new ValidationException("Validation failed", errors);

        exception.Message.Should().Be("Validation failed");
        exception.ValidationErrors.Should().ContainKey("Email");
        exception.ValidationErrors.Should().ContainKey("Password");
    }

    [Test]
    public void ValidationException_Create_ReturnsExceptionWithMessage()
    {
        var exception = ValidationException.Create("Error message");

        exception.Message.Should().Be("Error message");
        exception.ValidationErrors.Should().BeNull();
    }

    [Test]
    public void ValidationException_WithFieldErrors_ReturnsExceptionWithErrors()
    {
        var fieldErrors = new Dictionary<string, string[]>
        {
            { "Nombre", new[] { "El nombre es obligatorio" } }
        };

        var exception = ValidationException.WithFieldErrors(fieldErrors);

        exception.Message.Should().Be("Errores de validación");
        exception.ValidationErrors.Should().ContainKey("Nombre");
        exception.ValidationErrors!["Nombre"].Should().Contain("El nombre es obligatorio");
    }
}

public class BusinessExceptionTests
{
    [Test]
    public void BusinessException_CanBeCreatedWithMessage()
    {
        var exception = new BusinessException("Business rule violated");

        exception.Message.Should().Be("Business rule violated");
    }
}

public class UnauthorizedExceptionTests
{
    [Test]
    public void UnauthorizedException_CanBeCreatedWithMessage()
    {
        var exception = new UnauthorizedException("Not authenticated");

        exception.Message.Should().Be("Not authenticated");
    }

    [Test]
    public void UnauthorizedException_InvalidCredentials_ReturnsCorrectMessage()
    {
        var exception = UnauthorizedException.InvalidCredentials();

        exception.Message.Should().Be("Credenciales inválidas");
    }
}

public class ForbiddenExceptionTests
{
    [Test]
    public void ForbiddenException_CanBeCreatedWithMessage()
    {
        var exception = new ForbiddenException("Access denied");

        exception.Message.Should().Be("Access denied");
    }
}

public class ConflictExceptionTests
{
    [Test]
    public void ConflictException_CanBeCreatedWithMessage()
    {
        var exception = new ConflictException("Resource already exists");

        exception.Message.Should().Be("Resource already exists");
    }
}

public class InternalExceptionTests
{
    [Test]
    public void InternalException_CanBeCreatedWithMessage()
    {
        var exception = new InternalException("Server error");

        exception.Message.Should().Be("Server error");
    }
}

public class ExceptionInheritanceTests
{
    [Test]
    public void NotFoundException_InheritsFromDomainException()
    {
        typeof(NotFoundException).Should().BeAssignableTo<DomainException>();
    }

    [Test]
    public void ValidationException_InheritsFromDomainException()
    {
        typeof(ValidationException).Should().BeAssignableTo<DomainException>();
    }

    [Test]
    public void BusinessException_InheritsFromDomainException()
    {
        typeof(BusinessException).Should().BeAssignableTo<DomainException>();
    }

    [Test]
    public void UnauthorizedException_InheritsFromDomainException()
    {
        typeof(UnauthorizedException).Should().BeAssignableTo<DomainException>();
    }

    [Test]
    public void ForbiddenException_InheritsFromDomainException()
    {
        typeof(ForbiddenException).Should().BeAssignableTo<DomainException>();
    }

    [Test]
    public void ConflictException_InheritsFromDomainException()
    {
        typeof(ConflictException).Should().BeAssignableTo<DomainException>();
    }

    [Test]
    public void InternalException_InheritsFromDomainException()
    {
        typeof(InternalException).Should().BeAssignableTo<DomainException>();
    }
}
