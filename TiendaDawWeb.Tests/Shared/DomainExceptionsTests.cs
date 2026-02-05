#nullable disable
using FluentAssertions;
using TiendaDawWeb.Shared.Exceptions;

namespace TiendaDawWeb.Tests.Shared;

public class DomainExceptionsTests
{
    [Test]
    public void NotFoundException_CreatesMessage_WithId()
    {
        var ex = NotFoundException.FromId(123, "Product");
        
        ex.Should().NotBeNull();
        ex.Message.Should().Contain("123");
        ex.Message.Should().Contain("Recurso");
    }

    [Test]
    public void NotFoundException_InheritsFromDomainException()
    {
        var ex = new NotFoundException("Not found");
        ex.Should().BeAssignableTo<DomainException>();
    }

    [Test]
    public void ValidationException_Creates_WithMessage()
    {
        var ex = new ValidationException("Validation failed");
        
        ex.Should().NotBeNull();
        ex.Message.Should().Be("Validation failed");
    }

    [Test]
    public void ValidationException_Creates_WithErrors()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Email", new[] { "Invalid email" } },
            { "Password", new[] { "Too short" } }
        };
        
        var ex = new ValidationException("Validation failed", errors);
        
        ex.ValidationErrors.Should().NotBeNull();
        ex.ValidationErrors.Should().ContainKey("Email");
        ex.ValidationErrors.Should().ContainKey("Password");
    }

    [Test]
    public void ValidationException_WithFieldErrors_StaticMethod()
    {
        var errors = new Dictionary<string, string[]>
        {
            { "Name", new[] { "Required" } }
        };
        
        var ex = ValidationException.WithFieldErrors(errors);
        
        ex.Message.Should().Be("Errores de validación");
        ex.ValidationErrors.Should().ContainKey("Name");
    }

    [Test]
    public void ValidationException_Create_StaticMethod()
    {
        var ex = ValidationException.Create("Custom error");
        
        ex.Message.Should().Be("Custom error");
        ex.ValidationErrors.Should().BeNull();
    }

    [Test]
    public void BusinessException_CreatesMessage()
    {
        var ex = new BusinessException("Business rule violated");
        
        ex.Should().NotBeNull();
        ex.Message.Should().Be("Business rule violated");
    }

    [Test]
    public void BusinessException_InheritsFromDomainException()
    {
        var ex = new BusinessException("Error");
        ex.Should().BeAssignableTo<DomainException>();
    }

    [Test]
    public void UnauthorizedException_CreatesMessage()
    {
        var ex = new UnauthorizedException("Unauthorized");
        
        ex.Should().NotBeNull();
        ex.Message.Should().Be("Unauthorized");
    }

    [Test]
    public void UnauthorizedException_InvalidCredentials_StaticMethod()
    {
        var ex = UnauthorizedException.InvalidCredentials();
        
        ex.Message.Should().Be("Credenciales inválidas");
    }

    [Test]
    public void ForbiddenException_CreatesMessage()
    {
        var ex = new ForbiddenException("Forbidden access");
        
        ex.Should().NotBeNull();
        ex.Message.Should().Be("Forbidden access");
    }

    [Test]
    public void ConflictException_CreatesMessage()
    {
        var ex = new ConflictException("Resource conflict");
        
        ex.Should().NotBeNull();
        ex.Message.Should().Be("Resource conflict");
    }

    [Test]
    public void InternalException_CreatesMessage()
    {
        var ex = new InternalException("Internal error");
        
        ex.Should().NotBeNull();
        ex.Message.Should().Be("Internal error");
    }

    [Test]
    public void DomainException_IsThrowable()
    {
        var action = new System.Action(() => throw new NotFoundException("Test exception"));
        
        action.Should().Throw<NotFoundException>();
    }

    [Test]
    public void AllExceptions_InheritFromDomainException()
    {
        typeof(NotFoundException).Should().BeDerivedFrom<DomainException>();
        typeof(ValidationException).Should().BeDerivedFrom<DomainException>();
        typeof(BusinessException).Should().BeDerivedFrom<DomainException>();
        typeof(UnauthorizedException).Should().BeDerivedFrom<DomainException>();
        typeof(ForbiddenException).Should().BeDerivedFrom<DomainException>();
        typeof(ConflictException).Should().BeDerivedFrom<DomainException>();
        typeof(InternalException).Should().BeDerivedFrom<DomainException>();
    }
}
