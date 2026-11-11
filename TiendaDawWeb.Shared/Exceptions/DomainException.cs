namespace TiendaDawWeb.Shared.Exceptions;

/// <summary>
///     Excepción base para errores de dominio.
/// </summary>
public abstract class DomainException : Exception {
    protected DomainException(string message) : base(message) { }
}

/// <summary>
///     Excepción para recursos no encontrados (HTTP 404).
/// </summary>
public class NotFoundException : DomainException {
    public NotFoundException(string message) : base(message) { }

    public static NotFoundException FromId(long id, string resourceType) {
        return new NotFoundException($"Recurso con ID {id} no encontrado");
    }
}

/// <summary>
///     Excepción para errores de validación (HTTP 400).
/// </summary>
public class ValidationException : DomainException {
    public ValidationException(string message, Dictionary<string, string[]>? errors = null)
        : base(message) {
        ValidationErrors = errors;
    }

    public Dictionary<string, string[]>? ValidationErrors { get; }

    public static ValidationException WithFieldErrors(Dictionary<string, string[]> fieldErrors) {
        return new ValidationException("Errores de validación", fieldErrors);
    }

    public static ValidationException Create(string message) {
        return new ValidationException(message);
    }
}

/// <summary>
///     Excepción para violaciones de reglas de negocio (HTTP 400/422).
/// </summary>
public class BusinessException : DomainException {
    public BusinessException(string message) : base(message) { }
}

/// <summary>
///     Excepción para operaciones no autorizadas (HTTP 401).
/// </summary>
public class UnauthorizedException : DomainException {
    public UnauthorizedException(string message) : base(message) { }

    public static UnauthorizedException InvalidCredentials() {
        return new UnauthorizedException("Credenciales inválidas");
    }
}

/// <summary>
///     Excepción para acceso prohibido (HTTP 403).
/// </summary>
public class ForbiddenException : DomainException {
    public ForbiddenException(string message) : base(message) { }
}

/// <summary>
///     Excepción para conflictos de recursos (HTTP 409).
/// </summary>
public class ConflictException : DomainException {
    public ConflictException(string message) : base(message) { }
}

/// <summary>
///     Excepción para errores internos del servidor (HTTP 500).
/// </summary>
public class InternalException : DomainException {
    public InternalException(string message) : base(message) { }
}
