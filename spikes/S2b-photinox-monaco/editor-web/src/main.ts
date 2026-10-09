import * as monaco from 'monaco-editor/editor/editor.api';
import 'monaco-editor/languages/definitions/csharp/register';
import EditorWorker from 'monaco-editor/editor/editor.worker?worker&inline';

// ---------------------------------------------------------------------------------------------
// Bridge com o host (.NET): mensagens JSON {type, id?, payload?} pelo canal do Photino.
// ---------------------------------------------------------------------------------------------
type Message = { type: string; id?: number; payload?: any };

interface PhotinoExternal {
  sendMessage(message: string): void;
  receiveMessage(callback: (message: string) => void): void;
}

const external = (window as unknown as { external: PhotinoExternal }).external;
const pending = new Map<number, (payload: any) => void>();
let nextId = 1;

function send(message: Message): void {
  external.sendMessage(JSON.stringify(message));
}

function request(type: string, payload: unknown): Promise<any> {
  const id = nextId++;
  return new Promise((resolve) => {
    pending.set(id, resolve);
    send({ type, id, payload });
  });
}

window.addEventListener('error', (e) => send({ type: 'error', payload: `${e.message} @ ${e.filename}:${e.lineno}` }));
window.addEventListener('unhandledrejection', (e) => send({ type: 'error', payload: String(e.reason) }));
document.addEventListener('securitypolicyviolation', (e) =>
  send({ type: 'cspViolation', payload: { directive: e.violatedDirective, blocked: e.blockedURI } }),
);

// ---------------------------------------------------------------------------------------------
// Editor
// ---------------------------------------------------------------------------------------------
self.MonacoEnvironment = { getWorker: () => new EditorWorker() };

const starterCode = [
  'using CitySky;',
  '',
  'var city = CityGame.Current;',
  'var house = new House();',
  'city.Build(house);',
  'city.Log("Bom dia, cidade!");',
  '',
].join('\n');

const model = monaco.editor.createModel(starterCode, 'csharp', monaco.Uri.parse('inmemory://citysky/Program.cs'));
const editor = monaco.editor.create(document.getElementById('editor')!, {
  model,
  theme: 'vs-dark',
  automaticLayout: true,
  fontSize: 15,
  minimap: { enabled: false },
});

// Completions vindas do Roslyn no host.
monaco.languages.registerCompletionItemProvider('csharp', {
  triggerCharacters: ['.'],
  provideCompletionItems: async (m, position) => {
    const result = await request('completion', { text: m.getValue(), offset: m.getOffsetAt(position) });
    const word = m.getWordUntilPosition(position);
    const range = new monaco.Range(position.lineNumber, word.startColumn, position.lineNumber, word.endColumn);
    return {
      suggestions: (result.items as { label: string; kind: string }[]).map((item) => ({
        label: item.label,
        insertText: item.label,
        kind: kindOf(item.kind),
        range,
      })),
    };
  },
});

function kindOf(tag: string): monaco.languages.CompletionItemKind {
  const k = monaco.languages.CompletionItemKind;
  switch (tag) {
    case 'Method': return k.Method;
    case 'Property': return k.Property;
    case 'Class': return k.Class;
    case 'Field': return k.Field;
    case 'Keyword': return k.Keyword;
    case 'Local': return k.Variable;
    default: return k.Text;
  }
}

// Diagnósticos ao digitar: debounce de 250 ms; respostas antigas são descartadas pela versão do modelo.
const DebounceMs = 250;
let debounce: number | undefined;
let diagnosticsListener: ((version: number) => void) | undefined;

async function requestDiagnostics(): Promise<void> {
  const version = model.getVersionId();
  const result = await request('diagnostics', { text: model.getValue() });
  if (version !== model.getVersionId()) {
    return; // obsoleta
  }

  monaco.editor.setModelMarkers(
    model,
    'roslyn',
    (result.items as any[]).map((d) => ({
      startLineNumber: d.startLine,
      startColumn: d.startColumn,
      endLineNumber: d.endLine,
      endColumn: d.endColumn,
      message: `${d.id}: ${d.message}`,
      severity: d.severity === 'Error' ? monaco.MarkerSeverity.Error : monaco.MarkerSeverity.Warning,
    })),
  );
  diagnosticsListener?.(version);
}

model.onDidChangeContent(() => {
  window.clearTimeout(debounce);
  debounce = window.setTimeout(() => void requestDiagnostics(), DebounceMs);
});

// ---------------------------------------------------------------------------------------------
// Mensagens do host
// ---------------------------------------------------------------------------------------------
external.receiveMessage((raw) => {
  const message = JSON.parse(raw) as Message;
  if (message.id !== undefined && pending.has(message.id)) {
    pending.get(message.id)!(message.payload);
    pending.delete(message.id);
    return;
  }

  switch (message.type) {
    case 'runSelfTest':
      void runSelfTest();
      break;
    case 'prepareTyping':
      prepareTyping();
      break;
    case 'getValue':
      send({ type: 'value', payload: model.getValue() });
      break;
    case 'tryNavigate':
      // Se o host não barrar, a página sai do ar e o host não recebe a resposta (teste negativo).
      window.location.assign('https://example.com/');
      window.setTimeout(() => send({ type: 'navigationResult', payload: location.href }), 1500);
      break;
  }
});

// ---------------------------------------------------------------------------------------------
// Autoteste (spike): latência de completions e diagnósticos, CSP e funcionamento offline.
// ---------------------------------------------------------------------------------------------
function percentile(values: number[], p: number): number {
  const sorted = [...values].sort((a, b) => a - b);
  return sorted[Math.min(sorted.length - 1, Math.floor(p * sorted.length))];
}

function summary(values: number[]) {
  const mean = values.reduce((a, b) => a + b, 0) / values.length;
  return { n: values.length, mean, p50: percentile(values, 0.5), p95: percentile(values, 0.95), max: Math.max(...values) };
}

async function measureCompletions(runs: number) {
  const text = starterCode + 'city.';
  const offset = text.length;
  const roundTrip: number[] = [];
  const server: number[] = [];
  let labels: string[] = [];
  for (let i = 0; i < runs; i++) {
    const start = performance.now();
    const result = await request('completion', { text, offset });
    roundTrip.push(performance.now() - start);
    server.push(result.serverMs);
    labels = (result.items as { label: string }[]).map((x) => x.label);
  }
  return { roundTrip: summary(roundTrip), server: summary(server), hasBuild: labels.includes('Build'), hasLog: labels.includes('Log'), count: labels.length };
}

async function measureDiagnostics(runs: number) {
  const valid = starterCode;
  const invalid = starterCode.replace('city.Log(', 'city.Lgo(');
  const samples: number[] = [];
  let sawError = false;
  for (let i = 0; i < runs; i++) {
    const text = i % 2 === 0 ? invalid : valid;
    const done = new Promise<void>((resolve) => { diagnosticsListener = () => resolve(); });
    const start = performance.now();
    model.setValue(text); // equivale ao fim de uma digitação: dispara o debounce
    await done;
    samples.push(performance.now() - start);
    if (i % 2 === 0) {
      sawError ||= monaco.editor.getModelMarkers({ resource: model.uri }).some((m) => m.message.startsWith('CS1061'));
    }
  }
  diagnosticsListener = undefined;
  model.setValue(starterCode);
  return { endToEnd: summary(samples), debounceMs: DebounceMs, sawCS1061: sawError };
}

async function checkOffline() {
  const resources = performance.getEntriesByType('resource').map((r) => r.name);
  const external = resources.filter((url) => !url.startsWith(location.origin) && !url.startsWith('blob:') && !url.startsWith('data:'));
  let externalFetchBlocked = false;
  try {
    await fetch('https://example.com/', { mode: 'no-cors' });
  } catch {
    externalFetchBlocked = true; // CSP connect-src barrou
  }
  return { origin: location.origin, resourceCount: resources.length, externalResources: external, externalFetchBlocked };
}

async function runSelfTest() {
  const results = {
    completions: await measureCompletions(200),
    diagnostics: await measureDiagnostics(30),
    offline: await checkOffline(),
    userAgent: navigator.userAgent,
  };
  send({ type: 'selfTestResult', payload: results });
}

function prepareTyping() {
  model.setValue('');
  editor.updateOptions({
    autoClosingBrackets: 'never',
    autoClosingQuotes: 'never',
    autoSurround: 'never',
    autoIndent: 'none',
    quickSuggestions: false,
    suggestOnTriggerCharacters: false,
    wordBasedSuggestions: 'off',
    acceptSuggestionOnEnter: 'off',
    formatOnType: false,
  });
  editor.focus();
  send({ type: 'typingReady' });
}

send({ type: 'ready', payload: { href: location.href } });
