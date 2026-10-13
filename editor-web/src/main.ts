import '@fontsource/inter/latin-400.css';
import '@fontsource/inter/latin-600.css';
import '@fontsource/jetbrains-mono/latin-400.css';
import './styles.css';
import * as monaco from 'monaco-editor/editor/editor.api';
import 'monaco-editor/languages/definitions/csharp/register';
import EditorWorker from 'monaco-editor/editor/editor.worker?worker&inline';

// ---------------------------------------------------------------------------------------------
// Ponte com o host (Unity via WebView2): mensagens JSON {type, ...}. F1-74/F1-90.
// ---------------------------------------------------------------------------------------------
type Message = { type: string; [key: string]: unknown };

// O host pode ser PhotinoX (window.external) ou WebView2 cru (window.chrome.webview). Suporta os dois.
interface Photino {
  sendMessage(message: string): void;
  receiveMessage(callback: (message: string) => void): void;
}
interface WebView2 {
  postMessage(message: string): void;
  addEventListener(type: 'message', handler: (event: { data: string }) => void): void;
}

const photino = (window as unknown as { external?: Photino }).external;
const webview = (window as unknown as { chrome?: { webview?: WebView2 } }).chrome?.webview;

function send(message: Message): void {
  const json = JSON.stringify(message);
  if (photino?.sendMessage) {
    photino.sendMessage(json);
  } else if (webview) {
    webview.postMessage(json);
  } else {
    console.log('[editor→host]', json); // modo dev (navegador, sem host)
  }
}

function onHostMessage(handler: (message: Message) => void): void {
  const dispatch = (raw: string): void => {
    try {
      handler(JSON.parse(raw) as Message);
    } catch (error) {
      console.error('mensagem inválida do host', error);
    }
  };
  if (photino?.receiveMessage) {
    photino.receiveMessage(dispatch);
  } else {
    webview?.addEventListener('message', (event) => dispatch(event.data));
  }
}

// ---------------------------------------------------------------------------------------------
// Editor Monaco (tema do jogo)
// ---------------------------------------------------------------------------------------------
self.MonacoEnvironment = { getWorker: () => new EditorWorker() };

monaco.editor.defineTheme('citysky-dusk', {
  base: 'vs-dark',
  inherit: true,
  rules: [
    { token: 'keyword', foreground: '6cb6ff' },
    { token: 'type', foreground: '3cc2b0' },
    { token: 'string', foreground: 'f2b54b' },
    { token: 'number', foreground: 'c4a7ff' },
    { token: 'comment', foreground: '5f7a91', fontStyle: 'italic' },
  ],
  colors: {
    'editor.background': '#111a24',
    'editor.foreground': '#d8e2ec',
    'editorLineNumber.foreground': '#3d5266',
    'editor.lineHighlightBackground': '#16212d',
  },
});

const starter = [
  'using CitySky;',
  '',
  'var city = CityGame.Current;',
  'city.Log("Bom dia, prefeito(a)!");',
  '',
].join('\n');

const model = monaco.editor.createModel(starter, 'csharp', monaco.Uri.parse('inmemory://citysky/Program.cs'));
const editor = monaco.editor.create(document.getElementById('editor')!, {
  model,
  theme: 'citysky-dusk',
  automaticLayout: true,
  fontFamily: "'JetBrains Mono', Consolas, monospace",
  fontSize: 14,
  minimap: { enabled: false },
  scrollBeyondLastLine: false,
});

// ---------------------------------------------------------------------------------------------
// Console
// ---------------------------------------------------------------------------------------------
const consoleLines = document.getElementById('console-lines')!;
const consoleSummary = document.getElementById('console-summary')!;

function setSummary(text: string, state: 'ok' | 'fail' | ''): void {
  consoleSummary.textContent = text;
  consoleSummary.className = 'summary ' + state;
}

function appendConsole(text: string, kind: 'city' | 'info' | 'error'): void {
  for (const line of text.split('\n')) {
    const li = document.createElement('li');
    li.className = kind;
    li.textContent = line;
    consoleLines.append(li);
  }
}

// ---------------------------------------------------------------------------------------------
// Diagnósticos ao digitar (debounce de 250 ms)
// ---------------------------------------------------------------------------------------------
type Diagnostic = {
  id: string;
  severity: number; // 0 Hidden, 1 Info, 2 Warning, 3 Error (CitySky.Contracts.DiagnosticSeverity)
  message: string;
  startLine: number;
  startColumn: number;
  endLine: number;
  endColumn: number;
};

let debounce: number | undefined;
model.onDidChangeContent(() => {
  window.clearTimeout(debounce);
  debounce = window.setTimeout(() => send({ type: 'diagnostics', source: model.getValue() }), 250);
});

function applyDiagnostics(items: Diagnostic[]): void {
  monaco.editor.setModelMarkers(model, 'citysky', items.map((d) => ({
    startLineNumber: d.startLine,
    startColumn: d.startColumn,
    endLineNumber: d.endLine,
    endColumn: Math.max(d.endColumn, d.startColumn + 1),
    message: `${d.id}: ${d.message}`,
    severity: d.severity >= 3 ? monaco.MarkerSeverity.Error
      : d.severity === 2 ? monaco.MarkerSeverity.Warning
        : monaco.MarkerSeverity.Info,
  })));
  const errors = items.filter((d) => d.severity >= 3).length;
  setSummary(errors === 0 ? 'Sem erros' : `${errors} erro(s)`, errors === 0 ? 'ok' : 'fail');
}

// ---------------------------------------------------------------------------------------------
// Executar
// ---------------------------------------------------------------------------------------------
function run(): void {
  setSummary('Executando…', '');
  send({ type: 'run', source: model.getValue() });
}

document.getElementById('btn-run')!.addEventListener('click', run);
editor.addCommand(monaco.KeyMod.CtrlCmd | monaco.KeyCode.Enter, run);

// ---------------------------------------------------------------------------------------------
// Estados do painel (o host redimensiona a janela-filha ao receber panelState)
// ---------------------------------------------------------------------------------------------
type PanelState = 'minimized' | 'normal' | 'expanded';
function setState(state: PanelState): void {
  document.body.dataset.state = state;
  send({ type: 'panelState', state });
  window.setTimeout(() => editor.layout(), 0);
}
const currentState = (): PanelState => (document.body.dataset.state as PanelState) ?? 'normal';

document.getElementById('btn-minimize')!.addEventListener('click', () => setState('minimized'));
document.getElementById('collapsed-tab')!.addEventListener('click', () => setState('normal'));
document.getElementById('btn-expand')!.addEventListener('click', () =>
  setState(currentState() === 'expanded' ? 'normal' : 'expanded'));

window.addEventListener('keydown', (e) => {
  if (e.ctrlKey && e.code === 'Backquote') {
    e.preventDefault();
    if (e.shiftKey) {
      setState(currentState() === 'expanded' ? 'normal' : 'expanded');
    } else {
      setState(currentState() === 'minimized' ? 'normal' : 'minimized');
    }
  }
});

// ---------------------------------------------------------------------------------------------
// Mensagens do host
// ---------------------------------------------------------------------------------------------
onHostMessage((message) => {
  switch (message.type) {
    case 'runResult': {
      const ok = message.success === true;
      const output = (message.console as string) ?? '';
      if (output) {
        appendConsole(output, 'city');
      }
      if (!ok && message.error) {
        appendConsole(String(message.error), 'error');
      }
      setSummary(ok ? '▶ Executado' : '▶ Falhou', ok ? 'ok' : 'fail');
      break;
    }
    case 'diagnostics':
      applyDiagnostics((message.items as Diagnostic[]) ?? []);
      break;
  }
});

send({ type: 'ready' });
