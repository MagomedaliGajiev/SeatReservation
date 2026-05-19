using System.Text.Json.Serialization;

namespace SharedKernel;

public record ErrorMessage(string Code, string Message, string? InvalidField = null);

public record Error
{
    public IReadOnlyList<ErrorMessage> Messages { get; } = [];

    public ErrorType Type { get; }

    public bool IsCritical { get; }

    [JsonConstructor]
    private Error(IReadOnlyList<ErrorMessage> messages, ErrorType type, bool isCritical = false)
    {
        Messages = messages.ToArray();
        Type = type;
        IsCritical = isCritical;
    }

    private Error(IEnumerable<ErrorMessage> messages, ErrorType type, bool isCritical = false)
    {
        Messages = messages.ToArray();
        Type = type;
        IsCritical = isCritical;
    }

    public string GetMessage() => string.Join(";", Messages.Select(m => m.ToString()));

    public static Error Validation(string code, string message, string? invalidField = null, bool isCritical = false) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.VALIDATION, isCritical);

    public static Error NotFound(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.NOT_FOUND);

    public static Error Failure(string code, string message, string? invalidField = null, bool isCritical = false) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.FAILURE, isCritical);

    public static Error Conflict(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.CONFLICT);

    public static Error Authentication(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.AUTHENTICATION);

    public static Error Authorization(string code, string message, string? invalidField = null) =>
        new([new ErrorMessage(code, message, invalidField)], ErrorType.AUTHORIZATION);

    public static Error Validation(IEnumerable<ErrorMessage> messages, bool isCritical = false) =>
        new(messages, ErrorType.VALIDATION, isCritical);

    public static Error NotFound(params IEnumerable<ErrorMessage> messages) =>
        new(messages, ErrorType.NOT_FOUND);

    public static Error Failure(IEnumerable<ErrorMessage> messages, bool isCritical = false) =>
        new(messages, ErrorType.FAILURE, isCritical);

    public static Error Conflict(params IEnumerable<ErrorMessage> messages) =>
        new(messages, ErrorType.CONFLICT);

    public static Error Authentication(params IEnumerable<ErrorMessage> messages) =>
        new(messages, ErrorType.AUTHENTICATION);

    public static Error Authorization(params IEnumerable<ErrorMessage> messages) =>
        new(messages, ErrorType.AUTHORIZATION);

    public Error AsCritical() => new(Messages, Type, isCritical: true);
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum ErrorType
{
    VALIDATION,
    NOT_FOUND,
    FAILURE,
    CONFLICT,
    AUTHENTICATION,
    AUTHORIZATION,
}