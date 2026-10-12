namespace CitySky.CodeAnalysis;

/// <summary>Um achado de análise (código pedagógico CSK, mensagem pt-BR, linha).</summary>
public sealed record Finding(string Code, string Message, int Line);
