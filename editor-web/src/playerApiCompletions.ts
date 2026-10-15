import * as monaco from 'monaco-editor/editor/editor.api';

// ---------------------------------------------------------------------------------------------
// Sugestões (autocompletar) da API do jogador — City: SkyCode.
//
// Ainda NÃO há IntelliSense semântico: isso exigiria um servidor de linguagem Roslyn analisando
// o código do jogador em tempo real (trabalho de fase futura). Por ora, oferecemos uma lista
// CURADA dos membros públicos atuais da Player API, indexada pelo nome do receptor antes do ponto
// (ex.: `city.` → Log). É barato, determinístico e já cumpre o papel pedagógico de mostrar ao
// aluno o que a cidade oferece. Quando a Player API crescer, basta estender as tabelas abaixo
// (ou, no futuro, alimentá-las a partir dos metadados enviados pelo host).
// ---------------------------------------------------------------------------------------------

interface Member {
  label: string;
  insertText: string;
  detail: string;
  documentation: string;
  kind: monaco.languages.CompletionItemKind;
}

function cityMembers(): Member[] {
  return [
    {
      label: 'Log',
      insertText: 'Log(${1:"mensagem"})',
      detail: 'void City.Log(string mensagem)',
      documentation: 'Escreve uma mensagem no console da cidade.',
      kind: monaco.languages.CompletionItemKind.Method,
    },
  ];
}

// Receptor (texto antes do ponto) → membros oferecidos.
const membersByReceiver: Record<string, Member[]> = {
  CityGame: [
    {
      label: 'Current',
      insertText: 'Current',
      detail: 'City CityGame.Current',
      documentation: 'A cidade que o seu código controla.',
      kind: monaco.languages.CompletionItemKind.Property,
    },
  ],
  Current: cityMembers(), // CityGame.Current é uma City
  city: cityMembers(), // nome usual da variável no código inicial
};

// Sugestões de nível superior (Ctrl+Espaço sem ponto).
const topLevel: Member[] = [
  {
    label: 'CityGame',
    insertText: 'CityGame',
    detail: 'class CityGame',
    documentation: 'Ponto de entrada da API: use CityGame.Current pra pegar a cidade.',
    kind: monaco.languages.CompletionItemKind.Class,
  },
];

const receiverBeforeDot = /([A-Za-z_][A-Za-z0-9_]*)\s*\.\s*[A-Za-z0-9_]*$/;

export function registerPlayerApiCompletions(): monaco.IDisposable {
  return monaco.languages.registerCompletionItemProvider('csharp', {
    triggerCharacters: ['.'],
    provideCompletionItems(model, position) {
      const textUntil = model.getValueInRange({
        startLineNumber: position.lineNumber,
        startColumn: 1,
        endLineNumber: position.lineNumber,
        endColumn: position.column,
      });

      const word = model.getWordUntilPosition(position);
      const range: monaco.IRange = {
        startLineNumber: position.lineNumber,
        endLineNumber: position.lineNumber,
        startColumn: word.startColumn,
        endColumn: word.endColumn,
      };

      const dot = receiverBeforeDot.exec(textUntil);
      const members = dot ? membersByReceiver[dot[1]] : topLevel;
      if (!members) {
        return { suggestions: [] };
      }

      return {
        suggestions: members.map((m) => ({
          label: m.label,
          kind: m.kind,
          insertText: m.insertText,
          insertTextRules: monaco.languages.CompletionItemInsertTextRule.InsertAsSnippet,
          detail: m.detail,
          documentation: m.documentation,
          range,
        })),
      };
    },
  });
}
