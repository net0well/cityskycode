/// <reference types="vite/client" />

declare module 'monaco-editor/languages/definitions/csharp/register';

// Contribuições do editor (efeito colateral: registram controllers). O import "slim" editor.api NÃO
// as inclui, então sem elas a janela de sugestões/hover nunca aparece. Declaradas como módulos sem tipos.
declare module 'monaco-editor/editor/contrib/suggest/browser/suggestController';
declare module 'monaco-editor/editor/contrib/snippet/browser/snippetController2';
declare module 'monaco-editor/editor/contrib/hover/browser/hoverContribution';
declare module 'monaco-editor/editor/contrib/parameterHints/browser/parameterHints';
