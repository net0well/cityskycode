import { defineConfig } from 'vite';

// Bundle local do editor (sem CDN) servido pela WebView2 dentro do jogo. Sai direto no StreamingAssets
// da Unity (gitignored; reproduzível com `npm run build`). Base relativa para carregar via file://.
export default defineConfig({
  base: './',
  build: {
    outDir: '../unity/CitySky/Assets/StreamingAssets/editor-web',
    emptyOutDir: true,
    target: 'esnext',
    sourcemap: false,
  },
});
