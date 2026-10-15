namespace CitySky.CodeAnalysis;

/// <summary>
/// Catálogo central das mensagens pt-BR voltadas ao jogador nos diagnósticos de segurança (F1-100).
/// Único lugar onde o texto dos achados CSK existe — mantém consistência, facilita revisão e prepara a
/// futura externalização para recursos de localização. Os códigos CSK também moram aqui.
/// </summary>
public static class DiagnosticMessages
{
    // Códigos pedagógicos (catálogo de erros; ver F01 §6).
    public const string Unavailable = "CSK1001";      // tipo/membro fora da allowlist
    public const string NoFinalizers = "CSK1002";     // finalizador (~destrutor)
    public const string BannedAttribute = "CSK1003";  // atributo proibido
    public const string LowLevel = "CSK1004";         // unsafe/ponteiro/extern/P-Invoke

    public static string MemberUnavailable(string typeName, string memberName) =>
        $"'{typeName}.{memberName}' não está disponível na cidade.";

    public static string TypeUnavailable(string name) =>
        $"O tipo '{name}' não está disponível na cidade.";

    public const string LowLevelFeature = "Recurso de baixo nível não está disponível na cidade.";
    public const string Finalizers = "Finalizadores não estão disponíveis na cidade.";
    public const string ExternMethods = "Métodos externos não estão disponíveis na cidade.";
    public const string AttributeNotAvailable = "Este atributo não está disponível na cidade.";

    public static string IlBannedType(string name) => $"IL referencia o tipo proibido '{name}'.";

    public static string IlBannedCall(string typeName, string memberName) =>
        $"IL chama '{typeName}.{memberName}', que não está disponível.";

    public const string PInvokeModule = "Referência a módulo externo (P/Invoke) não é permitida.";
    public const string PInvokeMethod = "Método com P/Invoke não é permitido.";
}
